//=============================================================================
// <summary>
// 魔女の操作に関するクラス
// </summary>
// <author>菊池雅道</author>
//=============================================================================
using sound;
using System;
using System.Collections;
using System.Collections.Generic;
using via;
using via.attribute;
using via.dynamics;
using via.effect.script;
using via.gui;
using via.hid;
using via.motion;
using via.physics;
using static via.dynamics.Vehicle2;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.Witch)]
    public class Witch_Work : app.CharacterBase_Work, IColliders
    {
        #region コンポーネント
        /// <summary>
        /// コンポーネント
        /// </summary>
        private SoundPlayer cpSoundPlayer = null;   //サウンドコンポーネント
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private int magicPowerCount = 0;    //所持魔力数
        bool beam = false;                  //ビーム発射状態か
        bool beamStandby = false;           //ビーム発射可能状態か
        bool auraEffect = false;            //オーラエフェクトが出ているか
        bool beamNoticeEffect = false;      //ビーム予測線が出ているか
        bool hitMonster = false;            //召喚獣に攻撃が当たったか
        float beamTime = 0;                 //ビームの攻撃時間
        float beamHeghtOffset = 0.9f;       //ビームの高さオフセット値
        float beamfrontOffset = 1.0f;       //ビームの前方オフセット値
        float magicPowerHeghtOffset = 0.9f; //魔力玉の高さオフセット値
        float magicPowerFrontOffset = 1.0f; //魔力玉の前方オフセット値
        float moveEffectBackOffset = 0.9f;  //移動エフェクトの後方オフセット値
        float witchRotationSpeed = 0.3f;    //旋回速度
        float witchScale = 1.5f;            //魔法少女のローカルスケール
        float stickThreshold = 0.001f;      //スティックの入力閾値        
        Collider beamCollider;              //ビームコライダー
        private string inGameLocationFolderPath = "GameContents/InGame/Location";   //インゲームフォルダパス
        private Folder inGameLocationFolder = null;     //インゲームフォルダ     
        #endregion

        /// <summary>
        /// プレハブ
        /// </summary>
        [DataMember]
        //魔力弾のプレハブ
        private Prefab magicPowerPrefab = null;

        #region ユーザーデータ
        [DataMember]
        private PlayerUserData_Work witchUserData = null;   //魔法少女関連ユーザーデータ
        #endregion 

        /// <summary>
        /// 入力情報
        /// </summary>
        private GamePlayerManager_Work.Player inputData = null;

        /// <summary>
        /// カメラ情報
        /// </summary>
        private float cameraAngle = 0.0f;

        #region 定数
        /// <summary>
        /// エフェクト
        /// </summary>
        private enum Effect
        {
            beam,        //ビーム
            star,        //移動時の星
            aura,        //オーラ
            beamNotice,  //ビーム予測線
        };

        /// <summary>
        /// サウンドSE
        /// </summary>
        private enum Sound
        {
            MagicGet = 0,      //魔力ゲット
            MagicShot = 1,     //魔力弾発射
            Beam = 2,          //ビーム
        }

        /// <summary>
        /// コライダー
        /// </summary>
        private enum WitchCollider
        {
            Witch = 0,     //アイテム取得
            Press = 1,     //押し当たり
            Attack = 2,    //攻撃
        }
        #endregion

        #region アクションフィールド
        /// <summary>
        /// アクション
        /// </summary>
        enum Action
        {
            IDLE,   //待機
            MOVE,   //移動
            ATTACK,   //攻撃
        };

        /// <summary>
        /// アクションチェックを無視するか
        /// </summary>
        private bool ignoreCheckAction = false;

        [IgnoreDataMember, ReadOnly(true), DisplayOrder(1)]
        public bool IgnoreCheckAction
        {
            get { return ignoreCheckAction; }
            set { ignoreCheckAction = value; }
        }

        private List<ActionController_Work.ActionData> actionDataList = new List<ActionController_Work.ActionData>()
        {
            new ActionController_Work.ActionData() { actionNo = (int)Action.IDLE, actionProc = new WitchActIdle() },
            new ActionController_Work.ActionData() { actionNo = (int)Action.MOVE, actionProc = new WitchActMove() },
            new ActionController_Work.ActionData() { actionNo = (int)Action.ATTACK, actionProc = new WitchActAttack() },
        };
        #endregion

        #region プロパティ
        public float MagicPowerCount
        {
            get { return magicPowerCount; }
        }

        public bool BeamStandby
        {
            get { return beamStandby; }
        }
        public bool Beam
        {
            get { return beam; }
        }
        #endregion

        #region 計算
        public static float normalizeRadian(float rad)
        {
            //±πに正規化
            if (rad < -math.PI)
            {
                rad += math.PI2;
            }
            else if (rad > math.PI)
            {
                rad -= math.PI2;
            }

            return rad;
        }

        /// <summary>
        /// vec3の各要素を±πに正規化
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static vec3 normalizeRadian(vec3 vec)
        {
            vec.x = normalizeRadian(vec.x);
            vec.y = normalizeRadian(vec.y);
            vec.z = normalizeRadian(vec.z);

            return vec;
        }
        #endregion

        //モーションをセット
        private void setMotion(int layer_idx, uint bank_id, uint motion_id, float start_frame = 0.0f, float interpolation_frame = 20.0f, bool force_set = false)
        {
            var layer = cpMotion.Layer[layer_idx];

            //現在と違うモーションがセットしている場合はセット
            if (layer.MotionBankID != bank_id || layer.MotionID != motion_id || force_set)
            {
                layer.changeMotion(bank_id, motion_id, start_frame, interpolation_frame, InterpolationMode.CrossFade, InterpolationCurve.Linear);
            }
        }

        //ビーム発射可能状態の判定と処理
        private void checkBeamCondition()
        {
            //召喚獣のホールド状態を確認
            bool egi = SceneManager.MainScene.findGameObject("Egi").getComponent<Monster_Work>().Hold;
            bool aari = SceneManager.MainScene.findGameObject("Aari").getComponent<Monster_Work>().Hold;

            //魔法少女のビーム発射可能状態を判定
            bool sheenaBeem = GameObject.Name == "Sheena" && egi;
            bool frulaBeem = GameObject.Name == "Frula" && aari;
            
            //ビーム発射可能状態
            if ((sheenaBeem || frulaBeem) && beam != true)
            {
                beamStandby = true;

                //オーラエフェクトがセットされていなければセットする
                if (auraEffect != true)
                {
                    EffectID effect_id = new EffectID(0, (int)Effect.aura);
                    cpObjectEffectController.requestEffect(effect_id, vec3.Zero, cpTransform.Rotation, mat4.Zero, GameObject, null);                  
                    auraEffect = true;
                }
                //ビーム照準エフェクトがセットされていなければセットする
                if (beamNoticeEffect != true)
                {
                    //ビームの発射位置を設定                    
                    EffectID predictionEffect_id = new EffectID(0, (int)Effect.beamNotice);
                    vec3 beamPosition = new vec3();                  
                    beamPosition.y += beamHeghtOffset / witchScale;
                    beamPosition.z += beamfrontOffset / witchScale;
                    cpObjectEffectController.requestEffect(predictionEffect_id, vec3.Zero, Quaternion.Identity, via.matrix.makeTranslate(beamPosition), GameObject, null);
                    beamNoticeEffect = true;
                }
            }
            else
            {
                //ビーム発射不能
                beamStandby = false;

                //オーラエフェクトがセットされている場合、解除する
                if (auraEffect == true)
                {
                    killEffect((uint)Effect.aura);
                    auraEffect = false;
                }
                //ビーム照準エフェクトがセットされている場合、解除する
                if (beamNoticeEffect == true)
                {
                    killEffect((uint)Effect.beamNotice);
                    beamNoticeEffect = false;
                }
            }
        }

        //ビーム発射後の処理
        private void ResetBeam()
        {
            beamCollider.Enabled = false;
            beam = false;
            beamTime = 0.0f;
        }

        public override void awake()
        {
            base.awake();

            //アクションの初期化
            actionCtrl.initialize(GameObject, actionDataList);
            actionCtrl.requestAction((int)Action.IDLE);

            //各コンポーネントを取得
            var motion = GameObject.getComponent<via.motion.Motion>();
            cpSoundPlayer = GameObject.getComponent<SoundPlayer>();
            Colliders colliders = GameObject.getSameComponent<Colliders>();
            
            //ビームコライダー取得
            beamCollider = colliders.getCollider((int)WitchCollider.Attack);
            beamCollider.disable();
            
            //フォルダ取得
            inGameLocationFolder = SceneManager.CurrentScene.findFolder(inGameLocationFolderPath);

            //オブジェクトスケール設定
            cpTransform.LocalScale = vec3.One * witchScale;
           
            //待機モーションをセット
            setMotion(0, 0, 0);
        }

        public override void update()
        {
            //キャラクターごとに設定したプレイヤーのパッド情報を取得
            inputData = GamePlayerManager_Work.Instance.getPlayer(GameObject.Name);

            //モーションシーケンス情報更新
            updateMotionSequecne();

            //カメラアングル更新
            updateCameraAngle();

            //アクションリクエスト
            checkAction();

            //ビーム発射可能状態の確認
            checkBeamCondition();

            //アクション切り替え、アクション振る舞い処理
            actionCtrl.update();

            //ビーム発射中は移動しない
            if (!beam)
            {
                //CharacterBaseの位置更新処理
                updatePosition();
            }        
        }

        #region アクション
        private void checkAction()
        {
            if (ignoreCheckAction == false)
            {
                if (isAttackAction())
                {
                    //攻撃アクション
                    actionCtrl.requestAction((int)Action.ATTACK);
                }
                else if (isMoveAction())
                {
                    //移動アクション
                    actionCtrl.requestAction((int)Action.MOVE);
                }
            }
            else
            {
                //アクションチェック中は移動しない
                MoveSpeed = vec3.Zero;
            }
            
            ignoreCheckAction = false;
        }

        private class WitchActBase : ActionController_Work.ActionBase
        {
            //魔法少女を取得
            protected Witch_Work witch = null;
            public override void start()
            {
                witch = cpCharacterBase as Witch_Work;
            }
        }

        #region 待機アクション
        private class WitchActIdle : WitchActBase
        {
           
        }
        #endregion

        #endregion

        #region 移動アクション
        public enum MoveType
		{
			Idle,
			Move,
		}

        //移動状態確認
		public MoveType checkMoveType()
		{
            //入力があれば移動
			float move = getStickMove();

			if (move > MoveThreshold)
			{
				return MoveType.Move;
			}
			return MoveType.Idle;
		}

        /// <summary>
        /// 移動アクション
        /// </summary>
        private class WitchActMove : WitchActBase
        {
            MoveType prevMoveType = MoveType.Idle;

            public override void start()
            {
                base.start();

                //魔法少女の後ろに移動エフェクトをセット
                vec3 moveEffectPosition = new vec3(0.0f, 0.0f, -witch.moveEffectBackOffset);
                Quaternion moveEffectQuaternion = cpTransform.Rotation;
                moveEffectQuaternion.y = cpTransform.Rotation.y + math.rad2deg(math.PI);
                EffectID moveEffect_id = new EffectID(0, (int)Effect.star);              
                cpObjectEffectController.requestEffect(moveEffect_id, moveEffectPosition, moveEffectQuaternion , mat4.Zero, cpGameObject, null);
            }

            public override void update()
            {
               
                //移動状態をチェック
                MoveType type = witch.checkMoveType();
                if (prevMoveType != type)
                {
                    prevMoveType = type;
                }

                //移動状態に応じて移動もしくは待機
                if (type == MoveType.Idle)
                {                    
                    //移動しない
                    witch.MoveSpeed = vec3.Zero;
                    //待機に戻る
                    requestAction((int)Action.IDLE);
                }
                else
                {
                    //移動方向に応じて向きを変える
                    float angle = witch.getMoveAngle();
                    setRotationY(angle,witch.witchRotationSpeed);
                    
                    //移動方向と速度を設定
                    vec3 move = vec3.Zero;
                    move = new vec3(math.sin(angle), 0.0f, math.cos(angle));                 
                    move *= witch.witchUserData.WitchMoveSpeed;                       
                     
                    //移動を反映
                    witch.MoveSpeed = move;
                }
            }

            public override void finalize()
            {
                base.finalize();

                //エフェクトを消す
                witch.finishEffect((uint)Effect.star);
            }
        }
        #endregion

        #region 攻撃アクション
        private class WitchActAttack : WitchActBase
        {
            public override void start()
            {
                base.start();

                //ビーム発射可能状態の識別
                bool egi = SceneManager.MainScene.findGameObject("Egi").getComponent<Monster_Work>().Hold;
                bool aari = SceneManager.MainScene.findGameObject("Aari").getComponent<Monster_Work>().Hold;
                bool sheenaBeem = witch.GameObject.Name == "Sheena" && egi;
                bool frulaBeem = witch.GameObject.Name == "Frula" && aari;

                //ビーム発射
                if (sheenaBeem || frulaBeem)
                {
                    //ビームコリジョン
                    witch.beamCollider.Enabled = true;

                    //ビームエフェクトを発生
                    if (cpObjectEffectController != null)
                    {                      
                        EffectID beamEffect_id = new EffectID(0, (int)Effect.beam);
                        //ビームの発射位置を設定
                        vec3 beamPosition = witch.cpTransform.Position;
                        beamPosition.y += witch.beamHeghtOffset;
                        beamPosition = beamPosition + vector.setLength(witch.GameObject.Transform.AxisZ, witch.beamfrontOffset);
                        cpObjectEffectController.requestEffect(beamEffect_id, beamPosition, witch.cpTransform.Rotation);

                        //SE
                        witch.cpSoundPlayer._Sources[(int)Sound.Beam].play();
                    }

                    //ビーム状態設定
                    witch.beam = true;
                }
                //魔力玉発射
                else if (witch.magicPowerCount > 0)
                {
                    //魔力弾消費
                    witch.magicPowerCount--;

                    //魔力玉の発射位置を設定
                    vec3 magicPowePosition = witch.cpTransform.Position;
                    magicPowePosition.y += witch.magicPowerHeghtOffset;
                    magicPowePosition = magicPowePosition + vector.setLength(witch.GameObject.Transform.AxisZ, witch.magicPowerFrontOffset);

                    //魔力弾を作製
                    var magicPower = witch.magicPowerPrefab.instantiate(magicPowePosition, witch.inGameLocationFolder);
                    //魔力弾の向きを設定
                    magicPower.Transform.Rotation = cpTransform.Rotation;
                    //ポーズタグ設定
                    magicPower.Tag = "Pausable";

                    //魔力弾の発射SEを再生
                    witch.cpSoundPlayer._Sources[(int)Sound.MagicShot].play();

                    //待機に戻る
                    requestAction((int)Action.IDLE);
                }   
            }

            public override void update()
            {
                //攻撃中はアクションチェックを無視	
                witch.ignoreCheckAction = true;

                //ビーム発生中は一定時間動かない　それ以外はすぐに動ける
                if (witch.beam == true)
                {   
                    witch.beamTime += Application.ElapsedSecond;

                    //一定時間経過したらビーム終了 当たった場合と外れた場合で時間が変わる
                    if (witch.hitMonster == true)
                    {
                        if (witch.beamTime > witch.witchUserData.HitBeamTime)
                        {
                            //ビームの後処理
                            witch.ResetBeam();

                            //攻撃判定フラグOFF
                            witch.hitMonster = false;

                            //待機に戻る
                            requestAction((int)Action.IDLE);
                        }
                    }
                    else if (witch.beamTime > witch.witchUserData.MissBeamTime)
                    {
                        //ビームの後処理
                        witch.ResetBeam();

                        //待機に戻る
                        requestAction((int)Action.IDLE);
                    }                    
                }
                else
                {
                    //待機に戻る
                    requestAction((int)Action.IDLE);
                }
            }
        }
        #endregion

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(CollisionInfo collision_info)
        {
            //魔力に接触した場合
            if ((collision_info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Item")))
            {              
                if (collision_info.CollidableB.GameObject.Tag == "BigMagicPower")
                {
                     //魔力を増やす
                     magicPowerCount += witchUserData.BigMagicPowerPoint;
                }
                else
                {
                     //魔力を増やす
                     magicPowerCount += witchUserData.MagicPowerPoint;
                }
                //魔力が最大値の場合、処理しない
                if (magicPowerCount >= witchUserData.MaxMagicPower)
                {
                    magicPowerCount = witchUserData.MaxMagicPower;
                }               

                //SE
                cpSoundPlayer._Sources[(int)Sound.MagicGet].play();
               
            }
            //召喚獣に接触した場合
            if ((collision_info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Damage")))
            {
                //攻撃判定フラグON
                hitMonster = true;

                //スローモーション
                setHitStop(System.Enum.GetValues(typeof(Effect)).Length);

                beamCollider.disable();
            }
        }

        public void onOverlapping(CollisionInfo collision_info)
        {
         
        }

        public void onSeparate(CollisionInfo collision_info)
        {
          
        }

        #region 入力関連
        /// <summary>
        /// 移動アクションチェック
        /// </summary>
        static readonly float MoveThreshold = 0.3f;

        /// <summary>
        /// 移動アクションチェック
        /// </summary>
        /// <returns></returns>
        private bool isMoveAction()
        {
            if (getStickMove() > MoveThreshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ジャンプアクションチェック
        /// </summary>
        /// <returns></returns>
        private bool isJumpAction()
        {
            if (inputData == null)
            {
                return false;
            }

            if ((inputData.PadTrigger & GamePadButton.RDown) != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 攻撃アクションチェック
        /// </summary>
        /// <returns></returns>
        private bool isAttackAction()
        {
            if (inputData == null)
            {
                return false;
            }

            if ((inputData.PadTrigger & GamePadButton.LTrigTop) != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// スティックの移動量を取得
        /// </summary>
        /// <returns></returns>
        private float getStickMove()
        {
            if (inputData == null)
            {
                return 0.0f;
            }

            //XY軸それぞれで一定以上の入力があった場合
            float x_abs = 0.0f;
            float y_abs = 0.0f;          
            if (stickThreshold < math.abs(inputData.AxisL.x))
            {
                x_abs = math.abs(inputData.AxisL.x);
            }
            if (stickThreshold < math.abs(inputData.AxisL.y))
            {
                y_abs = math.abs(inputData.AxisL.y);
            }
            
            return math.sqrt(x_abs * x_abs + y_abs * y_abs);
        }

        /// <summary>
        /// カメラの向きを考慮した移動方向を取得
        /// </summary>
        /// <returns></returns>
        private float getMoveAngle()
        {
            if (inputData == null)
            {
                return cameraAngle;
            }

            //入力方向を取得
            vec2 axis = inputData.AxisL;

            //カメラの向きを考慮
            float angle = math.atan2(axis.x, axis.y);
            angle = cameraAngle - angle;

            //±πに正規化
            angle = normalizeRadian(angle);

            return angle;
        }
        #endregion

        #region カメラ
        /// <summary>
        /// カメラのアングル
        /// </summary>
        private void updateCameraAngle()
        {
            if (SceneManager.MainView.PrimaryCamera != null)
            {
                var go = SceneManager.MainView.PrimaryCamera.GameObject;

                vec3 forward = go.Transform.AxisZ;
                forward.y = 0.0f;
                cameraAngle = math.atan2(forward.x, forward.z);
                cameraAngle += math.PI;

                //±πに正規化
                cameraAngle = normalizeRadian(cameraAngle);
            }
        }
        #endregion

        #region モーションシーケンス
        /// <summary>
        /// 更新処理
        /// </summary>
        private void updateMotionSequecne()
        {
            if (cpMotion == null)
            {
                return;
            }
            var node = cpMotion.Layer[0].HighestWeightMotionNode;
            if (node != null)
            {
                //MotionデータからMotionSequenceに登録されたSequenceを取得する
                var count = node.SequenceTracksCount;
                for (uint i = 0; i < count; i++)
                {
                    var tracktype = node.getSequenceTracksTypeinfo(i);
                }
            }
        }
        #endregion
    }
}
