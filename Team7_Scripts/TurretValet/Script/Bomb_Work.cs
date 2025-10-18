//=============================================================================
// <summary>
// 爆弾に関するクラス
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using sound;
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.behaviortree;
using via.effect.script;
using via.motion;
using via.physics;
using via.render;
using via.uvsequence;

namespace app
{
	public class Bomb_Work : via.Behavior　, IColliders
    {
        #region コンポーネント
        protected Transform cpTransform = null;
        private SoundPlayer cpSoundPlayer = null;
        protected HitController_Work cpHitController = null;
        Collider magicPowerCollider;                                     //魔力玉用コライダー          
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>

        [DataMember]
        private Prefab bombExplisiponPrefab = null;     //爆発のプレハブ
        float countToExplosion = 0;                     //魔力弾球を受けてからの経過時間
        bool explosionStart = false;                    //点火フラグ
        bool countSE = false;                           //SEフラグ
        #endregion

        #region 定数
        /// <summary>
        /// サウンドSE
        /// </summary>
        private enum BombSe
        {
            Count, //カウント
            Spawn, //スポーン
        }

        /// <summary>
        /// コライダー
        /// </summary>
        private enum BombCollider
        {
           MagicPower = 0,    //魔力玉
           Spawn  = 1,        //スポーン
        }
        #endregion

        #region ユーザーデータ
        [DataMember]
        private BombUserData_Work bombUserData = null;
        #endregion

        #region 色変更系
        [DataMember]
        public string MaterialName;
        [DataMember]
        public string VariableName;

        private uint _MaterialNo = 0;
        private uint _VariableNo = 0;

        private Mesh _Mesh;

        private float bombFlushLv0 = 0.0f; //　爆弾の光る光量
        private float bombFlushLv1 = 1.0f; //　爆弾の光る光量
        private float bombFlushLv2 = 2.0f; //　爆弾の光る光量

        private float explotionWarningThreshold = 0.5f; // 爆発前警告時間
        private int flushCycleDuration = 10;
        private int flushOnDuration = 5;
        #endregion

        #region 押し当たり
        private void updatePress()
        {
            if (cpHitController == null)
            {
                return;
            }

            var press = cpHitController.getTotalPressMove();
            if (press != null)
            {
                var pos = cpTransform.Position;
                pos += press.moveDistance;
                cpTransform.Position = pos;
            }
        }
        #endregion

        //爆発の処理
        private void explosionProcess()
        {
            //SE
            if (countSE != true)
            {
                cpSoundPlayer._Sources[(int)BombSe.Count].play();
                countSE = true;
            }       

            //魔力弾を受けた(当たり判定OFF)場合、時間経過に応じて爆発のプロセスを行う
            if (countToExplosion >= bombUserData.TimeToExplosion)
            {
                bombExplisiponPrefab.instantiate(GameObject.Transform.Position);
                GameObject.destroy(GameObject);
            }
            else 
            {             
                //ちかちか爆発警告
                //爆発0.5秒前はさらに光らせる
                if (countToExplosion >= bombUserData.TimeToExplosion - explotionWarningThreshold)
                {
                    _Mesh.setMaterialFloat(_MaterialNo, _VariableNo, bombFlushLv2);
                }
                else if (countToExplosion * flushCycleDuration % flushCycleDuration < flushOnDuration)
                {
                    _Mesh.setMaterialFloat(_MaterialNo, _VariableNo, bombFlushLv1);                  
                }
                else
                {
                    _Mesh.setMaterialFloat(_MaterialNo, _VariableNo, bombFlushLv0);
                }

                //魔力弾球を受けてからの経過時間を加算
                countToExplosion += Application.ElapsedSecond;
            }
        }
        
		public override void start()
		{
            //各コンポーネント取得
            cpTransform = GameObject.Transform;
            cpHitController = GameObject.getSameComponent<HitController_Work>();
            cpSoundPlayer = GameObject.getSameComponent<SoundPlayer>();
            _Mesh = GameObject.getSameComponent<Mesh>();
            var variableNameHash = str.makeHash(VariableName);

            // マテリアルのインデックスとパラメータのインデックスを取得
            var materialNameCount = _Mesh.MaterialNames.Count;
            for (uint materialNo = 0; materialNo < materialNameCount; materialNo++)
            {
                if (MaterialName != _Mesh.MaterialNames[(int)materialNo])
                {
                    continue;
                }

                var variableNo = _Mesh.getMaterialVariableIndex(materialNo, variableNameHash);
                if (variableNo != 0xffu)
                {   // パラメータが見つかった
                    _MaterialNo = materialNo;
                    _VariableNo = variableNo;
                    _Mesh.setMaterialFloat(_MaterialNo, _VariableNo, 0.0f);
                }
            }

            //魔力玉用コライダー取得
            Colliders colliders = GameObject.getSameComponent<Colliders>();
            magicPowerCollider = colliders.getCollider((int)BombCollider.MagicPower);
           
            //SE
            cpSoundPlayer._Sources[(int)BombSe.Spawn].play();
        }

        public override void update()
        {
            //魔力弾を受けたら爆発
            if (explosionStart)
            {
                explosionProcess();
            }

            //押し当り
            updatePress();
        }

        #region 衝突処理
        /// <summary>
        /// 衝突処理
        /// </summary>
        /// <param name="info"></param>
        //衝突した瞬間
        public void onContact(CollisionInfo collision_info)
        {
            //自身の衝突形状
            Collidable self = collision_info.CollidableA;
            //相手の衝突形状
            Collidable other = collision_info.CollidableB;

            //魔力弾と接触したら
            if ((other.FilterInfo.Layer == via.physics.System.getLayerIndex("MagicPower")))
            {
               //起爆フラグON
               explosionStart = true;

               //魔力玉用コライダーOFF
               magicPowerCollider.disable();
            }
        }

        //衝突している間
        public void onOverlapping(CollisionInfo collision_info)
        {
            
        }

        //離れた瞬間
        public void onSeparate(CollisionInfo collision_info)
        {
            
        }
        #endregion
    }
}
