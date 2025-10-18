//=============================================================================
// <summary>
// 爆弾の発生を管理するクラス 
// </summary>
// <author>	菊池雅道 </author>
//=============================================================================
using sound;
using System;
using System.Collections;
using System.Collections.Generic;
using via;
using via.attribute;
using via.behaviortree.action;
using via.effect.script;
using via.physics;
using via.uvsequence;

namespace app
{
	public class BombSpawnManager_Work : via.Behavior, IColliders
    {
        #region コンポーネント
        protected Transform cpTransform = null;                             
        protected HitController_Work cpHitController = null;                
        protected ObjectEffectController cpObjectEffectController = null;
        private SoundPlayer cpSoundPlayer = null;
        #endregion
        /// <summary>
        /// フィールド
        /// </summary>
        /// 
        private string inGameLocationFolderPath = "GameContents/InGame/Location";   //所属フォルダパス
        private Folder inGameLocationFolder = null;                                 //所属フォルダ         
        private float inGameTimer = 0.0f;                                           //ゲーム内タイマー
        private GameFlowManager_Work gameFlowManager = null;                        //ゲームフロー管理クラス
        private float heghtOffset = 1.0f;                                           //高さ調整
        private int nowPositionSearchCount = 0;                                     //現在の発生地点捜索回数
        private int stageRadius = 4;                                                //ステージの大きさ(半径)
        System.Random random = new System.Random();                                 //ランダムクラス
        private float dividDegree = 0;                                              //爆弾生成方向間の角度
        private vec3 bombSpawnPosition = new vec3();                                //爆弾スポーン地点
        private int bombSpawnEffectTime = 1600;                                     //爆弾スポーンエフェクト時間

        #region ユーザーデータ
        [DataMember]
        private BombSpawnUserData_Work bombSpawnUserData = null;
        #endregion

        #region 定数
        /// <summary>
        /// エフェクト
        /// </summary>
        private enum Effect
        {
            BombSpawn,        //爆弾スポーン
        };
        // SE
        private enum BombSe
        {
            BombSpawn, ////爆弾スポーン
        }
        #endregion

        /// <summary>
        /// プレハブ
        /// </summary>
        [DataMember]
        //爆弾のプレハブ
        private Prefab bombPrefab = null;
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

        //ゲームオブジェクトのポジション初期化
        private void PositionInit()
        {
            GameObject.Transform.EulerAngle = vec3.Zero;
            GameObject.Transform.Position = new vec3(0f, heghtOffset, 0f);
        }

        //爆弾発生地点の設定
        private void setSpawnPosition()
        {
            //ポジション初期化
            PositionInit();

            //ステージを円として、中心からランダムな方向を設定
            vec3 rot = GameObject.Transform.EulerAngle;
            float randomDegree = (float)random.Next(0, bombSpawnUserData.StageDivideNum) * dividDegree;
            rot.y = via.math.rad2deg(randomDegree);
            GameObject.Transform.EulerAngle = rot;
            //ランダムな距離を設定
            GameObject.Transform.Position = new vec3(0f, heghtOffset, 0f) + vector.setLength(GameObject.Transform.AxisZ, (float)random.Next(0, stageRadius));
        }

        //爆弾を発生させる
        private void bombSpawn()
        {      
            //爆弾発生
            var bomb = bombPrefab.instantiate(bombSpawnPosition, inGameLocationFolder);
            bomb.Tag = "Pausable";

            //探索回数リセット
            nowPositionSearchCount = 0;
        }

        //エフェクト発生から爆弾発生の処理
        System.Collections.IEnumerator bombSpawnProcess()
        {
            for (; ; )
            {
                bombSpawnPosition = GameObject.Transform.Position;
                EffectID effect_id = new EffectID(0, (int)Effect.BombSpawn);
                cpObjectEffectController.requestEffect(effect_id, bombSpawnPosition,cpTransform.Rotation);
                cpSoundPlayer._Sources[(int)BombSe.BombSpawn].play();
                yield return new WaitUntil(TimeSpan.FromMilliseconds(bombSpawnEffectTime));
                bombSpawn();

                break;
            }
        }

        public override void awake()
		{
            //各コンポーネント取得
            cpTransform = GameObject.Transform;
            cpHitController = GameObject.getSameComponent<HitController_Work>();
            cpObjectEffectController = GameObject.getComponent<ObjectEffectController>();
            cpSoundPlayer = GameObject.getSameComponent<SoundPlayer>();
            gameFlowManager = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
           
            //フォルダ取得
            inGameLocationFolder = SceneManager.CurrentScene.findFolder(inGameLocationFolderPath);

            //爆弾生成方向の候補数(ステージの分割数)から、各方向の間の角度を求める
            dividDegree = 360.0f / bombSpawnUserData.StageDivideNum;

            //最初のスポーン位置設定
            setSpawnPosition();
        }

        public override void lateUpdate()
		{      
            //ゲーム内の時間を取得
            float oldTimer = inGameTimer;
            inGameTimer = gameFlowManager.IngameTimer;
            
            //爆弾発生条件
            bool gameStarted = gameFlowManager.GameStart;
            bool spawnTime = (int)Math.Truncate(inGameTimer) % bombSpawnUserData.SpawnIntervsal == 0;
            bool differentTime = Math.Truncate(inGameTimer) != Math.Truncate(oldTimer);


            //既定の時間で爆弾発生開始
            if (bombSpawnUserData.SpawnStartTime >= (int)Math.Truncate(inGameTimer))
            {
                //条件を満たしたら爆弾発生
                if (gameStarted && spawnTime && differentTime)
                {
                    startCoroutine(bombSpawnProcess());
                }
            }
        }

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(CollisionInfo collision_info)
        {
            
        }

        void IColliders.onOverlapping(CollisionInfo collision_info)
        {
            //設定回数に達していなければ、次の地点を探す
            if (nowPositionSearchCount <= bombSpawnUserData.MaxPositionSearchCount)
            {
                setSpawnPosition();

                nowPositionSearchCount += 1;
            }           
        }

        void IColliders.onSeparate(CollisionInfo collision_info)
        {
           
        }
    }
}
