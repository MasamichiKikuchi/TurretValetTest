//=============================================================================
// <summary>
// Monster_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using sound;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using via;
using via.attribute;
using via.effect.script;
using via.hid;
using via.landscape.spline;
using via.motion;
using via.physics;
using static via.physics.ColliderGroupSet;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.Monster)]
    public class Monster_Work : app.CharacterBase_Work, IColliders
    {
        /// <summary>
        /// フィールド
        /// </summary>
        private int hitPoint;               //ヒットポイント
        private int muscleGauge;            //筋力ゲージ
        private MuscleLevel muscleLevel;    //ムキムキレベル
        private bool holding;               //相手をホールドしているか
        private bool holded;                //相手にホールドされているか
        private bool charge;                //ラリアットのチャージをしているか
        private bool lariat;                //ラリアットをしているか
        private bool isDamage;              //ダメージを受けたか
        private bool finishedHolding;       //ホールドを終了させるか
        private float slideMoveCharaCon;    //滑り移動値
        private int holdTime;               //ホールド時間

        /// <summary>
        /// コンポーネント
        /// </summary>
        private SoundPlayer cpSoundPlayer = null;

        /// <summary>
        /// ムキムキレベル
        /// </summary>
        private enum MuscleLevel
        {
           Nomal,
           Level1,
           Level2,
        };

        /// <summary>
        /// エフェクト
        /// </summary>
        private enum Effect
        { 
            get,                //魔力ゲット
            lariatCharge,       //ラリアットチャージ中
            lariatChargeFin,    //ラリアットチャージ完了
            Damage,             //ダメージ
        };

        /// <summary>
        /// SE
        /// </summary>
        private enum Sound : int
        {
            MagicGet = 0,
            MuscleUp = 1,
            Charge = 2,
            ChargeFinish = 3,
            Lariat = 4,
            Hit = 5,
            Damage = 6,
            LastAttack = 7,
        }

        /// <summary>
        /// 入力情報
        /// </summary>
        private GamePlayerManager_Work.Player inputData = null;

        /// <summary>
        /// カメラ情報
        /// </summary>
        private float cameraAngle = 0.0f;

        #region アクションフィールド
        /// <summary>
        /// アクション
        /// </summary>
        private enum Action
        {
            IDLE,       //待機
            MOVE,       //移動
            CHARGE,     //チャージ
            ATTACK,     //攻撃
            HOLDING,    //ホールドしている
            HOLDEG,     //ホールドされている
            DAMAGE,     //ダメージ
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
            new ActionController_Work.ActionData(){actionNo = (int)Action.IDLE, actionProc = new MonsterActIdle()},
            new ActionController_Work.ActionData(){actionNo = (int)Action.MOVE, actionProc = new MonsterActMove()},
            new ActionController_Work.ActionData(){actionNo = (int)Action.ATTACK, actionProc = new MonsterActAttack()},
            new ActionController_Work.ActionData(){actionNo = (int)Action.HOLDING, actionProc = new MonsterActHolding()},
            new ActionController_Work.ActionData(){actionNo = (int)Action.HOLDEG, actionProc = new MonsterActHolded()},
            new ActionController_Work.ActionData(){actionNo = (int)Action.DAMAGE, actionProc = new MonsterActDamage()},
        };
        #endregion

        #region モーションシーケンス
        [IgnoreDataMember, GroupSeparator, DisplayName("モーションシーケンス")]
        public bool groupSeparatorMotionSequence = false;

        [IgnoreDataMember, ReadOnly(true)]
        [DisplayName("ジャンプ開始")]
        public bool jumpStart = true;

        [IgnoreDataMember, ReadOnly(true)]
        [DisplayName("全行動開始")]
        public bool allStart = true;

        [IgnoreDataMember, GroupEndSeparator, DisplayName("モーションシーケンス")]
        public bool groupEndSeparatorMotionSequence = false;
        #endregion

        #region ユーザーデータ
        [DataMember]
        private MonsterUserData_Work monsterUserData = null;
        #endregion

        #region プロパティ
        //ヒットポイントの取得
        public int HitPoint
        {
            get { return hitPoint; }
        }
        //ホールドしている状態の取得
        public bool Hold
        {
            get { return holding; }
        }
        //ホールドされている状態の取得
        public bool Holded
        {
            get { return holded; }
        }
        //チャージ状態の取得
        public bool Charge
        {
            get { return charge; }
        }
        //筋力ゲージの取得
        public int MuscleGauge
        {
            get { return muscleGauge; }
        }
        //ダメージ状態の取得
        public bool IsDamage
        {
            get { return isDamage; }
        }
        //ホールドを終了させる状態の取得、設定
        public bool FinishedHolding
        {
            get { return finishedHolding; }
            set { finishedHolding = value; }
        }
        //ホールド時間の取得
        public int HoldTime
        {
            get { return holdTime; }
        }
        #endregion

        #region Utility（計算）
        /// <summary>
        /// ±πに正規化
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
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

        public override void awake()
        {
            base.awake();

            //アクションの初期化
            actionCtrl.initialize(GameObject, actionDataList);
            actionCtrl.requestAction((int)Action.IDLE);

            //サウンドSEの初期化
            cpSoundPlayer = GameObject.getComponent<SoundPlayer>();

            //ヒットポイントの初期化
            hitPoint = monsterUserData.MaxHitPoint;

            //筋力ゲージの初期化
            muscleGauge = 0;

            //ムキムキレベルの初期化
            muscleLevel = MuscleLevel.Nomal;

            //ホールド状態の初期化
            holding = false;
            holded = false;

            //チャージ状態の初期化
            charge = false;

            //ラリアット状態の初期化
            lariat = false;

            //ダメージ状態の初期化
            isDamage = false;

            //ホールドを終了させる状態の初期化
            finishedHolding = false;

            //滑り移動値の初期化
            slideMoveCharaCon = cpCharacterController.SlideMovementLimit;

            //ホールド時間の初期化
            holdTime = 0;
        }

        public override void update()
        {
            //キャラクターごとに設定したプレイヤーのパッド情報を取得
            inputData = GamePlayerManager_Work.Instance.getPlayer(GameObject.Name);

            //モーションシーケンス情報更新
            updateMotionSequence();

            //カメラアングル更新
            updateCameraAngle();

            //ムキムキレベルを設定
            checkMuscleLevel();

            //ダメージを与えたかチェック
            updateHitAttack();

            //ダメージチェック
            updateDamage();

            //ActionNoから対応しているモーションに反映
            checkAction();

            //アクション切り替え、アクション振る舞い処理
            actionCtrl.update();

            //CharacterBaseの位置更新処理
            updatePosition();

            //ダメージあたりをセット
            cpRequestSetCollider.registerRequestSet(0, 1);

            //押し当たりをセット
            cpRequestSetCollider.registerRequestSet(0, 0);
        }

        public override void lateUpdate()
        {
            actionCtrl.lateUpdate();
        }

        public override void onDestroy()
        {
            base.onDestroy();
        }

        #region 筋力ゲージ
        /// <summary>
        /// 筋力ゲージに応じてムキムキレベルを設定
        /// </summary>
        private void checkMuscleLevel()
        {
            MuscleLevel preMuscleLevel = muscleLevel;
            
            if (muscleGauge <= monsterUserData.Nomal_MaxMuscleGauge)
            {
                //ムキムキレベルノーマル
                muscleLevel = MuscleLevel.Nomal;
                cpTransform.LocalScale = vec3.One;
            }
            else if (muscleGauge <= monsterUserData.Level1_MaxMuscleGauge)
            {
                //ムキムキレベル1
                muscleLevel = MuscleLevel.Level1;
            }
            else if (muscleGauge <= monsterUserData.MaxMuscleGauge)
            {
                //ムキムキレベル2
                muscleLevel = MuscleLevel.Level2;
            }

            //ムキムキレベルが上がったかチェック
            if(muscleLevel != preMuscleLevel)
            {
                //スケール変更
                cpTransform.LocalScale = vec3.One + vec3.One * 0.5f * (float)muscleLevel;

                if (muscleLevel != MuscleLevel.Nomal)
                {
                    //ムキムキレベルアップSE再生
                    cpSoundPlayer._Sources[(int)Sound.MuscleUp].play();
                }
            }
        }
        #endregion

        #region ダメージ
        /// <summary>
        /// ダメージの更新
        /// </summary>
        private void updateDamage()
        {
            if(!isDamage)
            {
                return;
            }

            //ヒットポイントを1減らす
            hitPoint -= 1;

            //ヒットストップ
            setHitStop(System.Enum.GetValues(typeof(Effect)).Length);
        }

        /// <summary>
        /// 相手がダメージを受けた時、ヒットストップ
        /// </summary>
        private void updateHitAttack()
        {
            GameObject other = null;
            //相手のGameObjectを取得
            if (GameObject.Name == "Egi")
            {
                other = SceneManager.MainScene.findGameObject("Aari");
            }
            else if (GameObject.Name == "Aari")
            {
                other = SceneManager.MainScene.findGameObject("Egi");
            }

            bool otherDamage = false;
            //相手のダメージ状況を取得
            if(other != null)
            {
                otherDamage = other.getComponent<Monster_Work>().isDamage;
            }

            if(otherDamage)
            {
                //ヒットストップ
                setHitStop(System.Enum.GetValues(typeof(Effect)).Length);
            }
        }
        #endregion

        #region アクション
        private void checkAction()
        {
            if (isDamage)
            {
                //ダメージ
                actionCtrl.requestAction((int)Action.DAMAGE);
                ignoreCheckAction = true;

                //ダメージクリア
                isDamage = false;
            }
            else
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
            }
            ignoreCheckAction = false;
        }

        /// <summary>
        /// 召喚獣のアクション基底クラス
        /// </summary>
        private class MonsterActBase : ActionController_Work.ActionBase
        {
            protected Monster_Work monster = null;

            //相手の召喚獣
            protected Monster_Work other = null;

            public override void start()
            {
                monster = cpCharacterBase as Monster_Work;

                //相手の召喚獣を取得
                if (monster.GameObject.Name == "Egi")
                {
                    other = SceneManager.MainScene.findGameObject("Aari").getComponent<Monster_Work>();
                }
                else if (monster.GameObject.Name == "Aari")
                {
                    other = SceneManager.MainScene.findGameObject("Egi").getComponent<Monster_Work>();
                }
            }

            public override void update()
            {
                
            }
        }

        #region 待機アクション
        private class MonsterActIdle : MonsterActBase
        {
            public override void start()
            {
                base.start();

                //滑り移動値の設定
                cpCharacterController.SlideMovementLimit = monster.slideMoveCharaCon;
            }
            public override void update()
            {
                //待機
                if (phase == 0)
                {
                    //待機モーションをセット
                    setMotion(0, 0, (uint)Action.IDLE);
                    phase = 1;
                }

                //移動しない
                monster.MoveSpeed = vec3.Zero;
            }
        }
        #endregion

        #region 移動アクション
        public enum MoveType
        {
            Idle = 0,
            Move,
        }

        public MoveType checkMoveType()
        {
            //スティックの移動量を取得
            float move = getStickMove();
            
            if(move > MoveThreshold)
            {
                return MoveType.Move;
            }
            return MoveType.Idle;
        }

        /// <summary>
        /// 移動アクション
        /// </summary>
        private class MonsterActMove : MonsterActBase
        {
            //以前のモーションタイプ
            MoveType prevMoveType = MoveType.Idle;

            public override void start()
            {
                base.start();

                //滑り移動値の設定
                cpCharacterController.SlideMovementLimit = monster.slideMoveCharaCon;
            }

            public override void update()
            {
                if (phase == 0)
                {
                    prevMoveType = MoveType.Idle;
                    phase = 1;
                }

                //モーションをチェック
                MoveType type = monster.checkMoveType();

                if (prevMoveType != type)
                {
                    switch (type)
                    {
                        case MoveType.Move:
                            //移動モーションをセット
                            setMotion(0, 0, (uint)Action.MOVE);
                            break;

                        default:
                            break;
                    }
                    prevMoveType = type;
                }

                if (type == MoveType.Idle)
                {
                    //移動しない
                    monster.MoveSpeed = vec3.Zero;
                    //待機に戻る
                    requestAction((int)Action.IDLE);
                }
                else
                {
                    //移動方向に応じて向きを変える
                    float angle = monster.getMoveAngle();

                    //回転を反映
                    setRotationY(angle);

                    //移動ベクトル
                    vec3 move = vec3.Zero;

                    //移動方向から移動ベクトルを取得
                    move = new vec3(math.sin(angle), 0.0f, math.cos(angle));

                    //移動速度を設定
                    move *= monster.monsterUserData.MonsterMoveSpeed;

                    //移動を反映
                    monster.MoveSpeed = move;
                }
            }
        }
        #endregion

        #region 攻撃（ラリアット）アクション
        private class MonsterActAttack : MonsterActBase
        {
            //ラリアット時の回転角度
            private float lariatAngle = 0.0f;
            //召喚獣の攻撃時の移動速度
            private float attackMoveSpeed = 0.0f;
            //攻撃時間
            int time = 0;

            public override void start()
            {
                base.start();

                //時間初期化
                time = 0;
            }

            public override void update()
            {
                //攻撃中はアクションチェックを無視
                monster.IgnoreCheckAction = true;

                //パッド入力情報から攻撃アクションの状態チェック
                if(monster.isCharging())
                {
                    monster.charge = true;
                }
                else if(monster.isLariat())
                {
                    monster.charge = false;
                }

                //スティックの移動量を取得
                float stickMove = monster.getStickMove();

                switch (phase)
                {
                    //チャージ開始
                    case 0:
                        //チャージ開始モーションをセット
                        setMotion(0, 0, (uint)Action.CHARGE);

                        //チャージエフェクトをセット
                        monster.setEffect((uint)Effect.lariatCharge);

                        //チャージSE再生
                        monster.cpSoundPlayer._Sources[(int)Sound.Charge].Loop = true;
                        monster.cpSoundPlayer._Sources[(int)Sound.Charge].play();

                        phase = 1;
                        break;
                    case 1:
                        //チャージ中
                        if (monster.charge)
                        {
                            //スティックの移動量から、移動か静止かを確認
                            if(stickMove > MoveThreshold)
                            {
                                //移動
                                //移動方向に応じて向きを変える
                                float angle = monster.getMoveAngle();
                                lariatAngle = angle;

                                //チャージ中の移動速度を設定
                                attackMoveSpeed = monster.monsterUserData.ChargeMoveSpeed;
                            }
                            else
                            {
                                //静止
                                //静止中は召喚獣の向きを維持
                                lariatAngle = monster.cpTransform.EulerAngle.y;
                                
                                //チャージ中の移動速度を設定
                                attackMoveSpeed = 0.0f;
                            }
                            
                            //チャージ完了
                            if(time == monster.monsterUserData.ChargeTime)
                            {
                                //チャージエフェクトを解除
                                monster.finishEffect((uint)Effect.lariatCharge);

                                //チャージSE停止
                                monster.cpSoundPlayer._Sources[(int)Sound.Charge].Loop = false;
                                monster.cpSoundPlayer._Sources[(int)Sound.Charge].stop();

                                //チャージ完了エフェクトをセット
                                monster.setEffect((uint)Effect.lariatChargeFin);

                                //チャージ完了SE再生
                                monster.cpSoundPlayer._Sources[(int)Sound.ChargeFinish].play();
                            }

                            time++;
                        }
                        else
                        {
                            if(time >= monster.monsterUserData.ChargeTime)
                            {
                                //ラリアット発動
                                //ラリアット時間をムキムキレベルから参照設定
                                time = 60 + 30 * (int)monster.muscleLevel;

                                //ラリアットモーションをセット
                                setMotion(0, 0, (uint)Action.ATTACK);

                                //チャージ完了エフェクトを解除
                                monster.finishEffect((uint)Effect.lariatChargeFin);

                                //ラリアットSE再生
                                monster.cpSoundPlayer._Sources[(int)Sound.Lariat].play();

                                //ラリアット状態を更新
                                monster.lariat = true;

                                phase = 2;
                            }
                            else
                            {
                                //チャージ不十分のため、ラリアット不発
                                //チャージエフェクトを解除
                                monster.finishEffect((uint)Effect.lariatCharge);

                                //チャージSE停止
                                monster.cpSoundPlayer._Sources[(int)Sound.Charge].Loop = false;
                                monster.cpSoundPlayer._Sources[(int)Sound.Charge].stop();

                                //待機に戻る
                                requestAction((int)Action.IDLE);
                            }
                        }
                        break;
                    case 2:
                        // ラリアット中
                        if (time > 0)
                        {
                            //ラリアット中の移動速度を設定
                            attackMoveSpeed = monster.monsterUserData.LariatMoveSpeed((int)monster.muscleLevel);

                            //攻撃コライダーをセット
                            monster.cpRequestSetCollider.registerRequestSet(0, 2);

                            time--;
                        }
                        else
                        {
                            //ラリアット終了
                            //ラリアットSE停止
                            monster.cpSoundPlayer._Sources[(int)Sound.Lariat].stop();

                            //待機に戻る
                            requestAction((int)Action.IDLE);
                        }
                        break;
                }

                //回転を反映
                setRotationY(lariatAngle);

                //移動ベクトル
                vec3 move = vec3.Zero;

                //移動方向から移動ベクトルを取得
                move = new vec3(math.sin(lariatAngle), 0.0f, math.cos(lariatAngle));

                //移動速度を設定
                move *= attackMoveSpeed;

                //移動を反映
                monster.MoveSpeed = move;
            }

            public override void finalize()
            {
                base.finalize();

                //チャージエフェクトを解除
                monster.finishEffect((uint)Effect.lariatCharge);
                //チャージSE停止
                monster.cpSoundPlayer._Sources[(int)Sound.Charge].Loop = false;
                monster.cpSoundPlayer._Sources[(int)Sound.Charge].stop();

                //チャージ完了エフェクトを解除
                monster.finishEffect((uint)Effect.lariatChargeFin);
                //チャージ完了SE停止
                monster.cpSoundPlayer._Sources[(int)Sound.ChargeFinish].stop();

                //ラリアットSE停止
                monster.cpSoundPlayer._Sources[(int)Sound.Lariat].stop();

                if (monster.charge)
                {
                    monster.charge = false;
                }
                if(monster.lariat)
                {
                    monster.lariat = false;

                    if(!monster.holding)
                    {
                        //ラリアット状態であれば、筋力ゲージをリセット
                        monster.muscleGauge = 0;

                        monster.holdTime = 0;
                    }
                }

                time = 0;
            }
        }
        #endregion

        #region ホールドしているアクション
        private class MonsterActHolding : MonsterActBase
        {
            //以前の角度
            float preAngle = 0.0f;
            //相手の以前の壁との衝突
            bool preOtherHitWall = false;
            //以前の座標
            vec3 prePosition = vec3.Zero;
            vec3 preOtherPosition = vec3.Zero;

            public override void start()
            {
                base.start();

                //ホールドを終了させる状態の初期化
                monster.finishedHolding = false;

                //滑り移動値の設定（ホールドしている間は滑らない）
                cpCharacterController.SlideMovementLimit = 0.0f;
            }

            public override void update()
            {
                base.update();
                
                //アクションチェックを無視
                monster.IgnoreCheckAction = true;

                //相手の位置情報
                int otherActionNo = other.ActionNo;

                //回転角度
                float angle = monster.cpTransform.EulerAngle.y;

                //以前の情報を更新
                preAngle = monster.cpTransform.EulerAngle.y;
                prePosition = monster.cpTransform.Position;
                preOtherPosition = other.cpTransform.Position;

                if (monster.holdTime < 0 || monster.finishedHolding)
                {
                    //待機に戻る
                    requestAction((int)Action.IDLE);

                    monster.finishedHolding = false;
                }

                if(phase == 0)
                {
                    //相手の方向の角度を取得
                    angle = monster.getDirection(other.cpTransform);

                    //相手の以前の壁との衝突を初期化
                    preOtherHitWall = false;

                    //ホールドしているモーションをセット
                    setMotion(0, 0, (uint)Action.HOLDING);

                    //ラリアットヒットSE再生
                    monster.cpSoundPlayer._Sources[(int)Sound.Hit].play();

                    phase = 1;
                }
                else
                {
                    //相手がダメージアクション中は操作不可
                    if (otherActionNo != (int)Action.DAMAGE)
                    {
                        //ホールドしている召喚獣の処理
                        angle = monster.swingMonster();
                    }

                    //方向からY軸の回転量を取得
                    float nowRoteY =  monster.getRotationY(angle, preAngle);

                    //ホールドされている召喚獣の処理
                    other.swungMonster(nowRoteY, monster.cpTransform.Position);
                }

                //回転を反映
                setRotationY(angle);

                //移動方向と速度のベクトル
                vec3 move = vec3.Zero;

                //移動を反映
                monster.MoveSpeed = move;

                monster.holdTime--;
            }

            public override void lateUpdate()
            {
                base.lateUpdate();

                bool isHitWall = other.cpCharacterController.Wall;
                if (isHitWall)
                {
                    //相手が以前も壁と総突していた場合
                    if(preOtherHitWall)
                    {
                        //以前の位置に戻す
                        monster.cpTransform.Position = prePosition;
                        other.cpTransform.Position = preOtherPosition;
                    }
                    
                    //相手の方向の角度を取得
                    float angle = monster.getDirection(other.cpTransform);

                    //回転を反映
                    setRotationY(angle);

                    //Y軸回転を更新
                    monster.updateRotationY();
                }

                //相手の以前の壁との衝突を更新
                preOtherHitWall = isHitWall;
            }

            public override void finalize()
            {
                base.finalize();

                //ラリアットヒットSE停止
                monster.cpSoundPlayer._Sources[(int)Sound.Hit].stop();

                //ホールド状態をリセット
                monster.holding = false;

                //筋力ゲージをリセット
                monster.muscleGauge = 0;

                //ホールド時間をリセット
                monster.holdTime = 0;
            }
        }

        #region ホールドしている関連処理
        /// <summary>
        /// 相手の方向を取得
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private float getDirection(Transform other)
        {
            //相手の位置情報を取得
            vec3 otherPosition = other.Position;

            //相手の方向ベクトルを取得
            vec3 dir = otherPosition - cpTransform.Position;

            //相手の方向を正面とする
            float angle = math.atan2(dir.x, dir.z);

            return angle;
        }

        /// <summary>
        /// ホールドしている召喚獣の処理（ぶん回す）
        /// </summary>
        /// <returns></returns>
        private float swingMonster()
        {
            if (getStickMove() > MoveThreshold)
            {
                //スティックから向きを取得
                return getMoveAngle();
            }
            else
            {
                //静止中は向きを維持
                return cpTransform.EulerAngle.y;
            }
        }

        /// <summary>
        /// Y軸回転量の取得
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        private float getRotationY(float angle, float preAngle, float speed = math.PI)
        {
            float nowRotationY = preAngle;
            
            //±180度に正規化
            float target_rotation_y = MathEx.normalizeRad(angle);
            float now_rotation_y = MathEx.normalizeRad(nowRotationY);

            float diff = target_rotation_y - now_rotation_y;
            float diff_abs = math.abs(diff);
            float add = speed * DeltaTime;
            if (diff < 0.0f)
            {
                add *= -1.0f;
            }
            if (diff_abs > math.PI)
            {
                //±180度を超える場合は逆回転
                float dir = -1.0f;
                if (add < 0.0f)
                {
                    dir = 1.0f;
                }

                //移動量を反転
                add = math.PI2 - diff_abs;
                add *= dir;
            }
            if (diff_abs < speed)
            {
                nowRotationY = angle;
            }
            else
            {
                nowRotationY += add;
            }

            return nowRotationY;
        }
        #endregion

        #endregion

        #region ホールドされているアクション
        private class MonsterActHolded : MonsterActBase
        {
            public override void start()
            {
                base.start();

                //滑り移動値の設定（ホールドされている間は滑らない）
                cpCharacterController.SlideMovementLimit = 0.0f;
            }

            public override void update()
            {
                base.update();
                
                //アクションチェックを無視
                monster.IgnoreCheckAction = true;

                //相手のアクション状態（ホールド）
                bool otherHolding = true;

                //相手のホールドしている状態と位置情報の取得
                otherHolding = other.Hold;

                //相手のホールドが終了したか確認
                if (!otherHolding)
                {
                    //待機に戻る
                    requestAction((int)Action.IDLE);
                }

                if(phase == 0)
                {
                    //ホールドされているモーションをセット
                    setMotion(0, 0, (uint)Action.HOLDEG);

                    phase = 1;
                }

                //スティックから向きを取得
                float angle = monster.getMoveAngle();

                //現在の回転を反映
                setRotationY(angle);

                //移動方向と速度のベクトル
                vec3 move = vec3.Zero;

                //移動を反映
                monster.MoveSpeed = move;
            }

            public override void finalize()
            {
                base.finalize();

                //ホールドされている状態をリセット
                monster.holded = false;
            }
        }

        #region ホールドされている関連処理
        /// <summary>
        /// ホールドされている召喚獣の処理（ぶん回される）
        /// </summary>
        /// <param name="rad"></param>
        /// <param name="otherPosition"></param>
        private void swungMonster(float rad, vec3 otherPosition)
        {
            //ホールドしている召喚獣の位置を原点にした自分の位置
            vec3 pos = cpTransform.Position - otherPosition;

            //平行移動ベクトルを取得
            vec3 transLatePos = vec3.Zero;
            transLatePos.x = math.sin(rad);
            transLatePos.y = 0.0f;
            transLatePos.z = math.cos(rad);

            //ベクトルを正規化
            transLatePos = vector.normalize(transLatePos);

            //ホールドしている召喚獣との距離を取得
            float dis = pos.length();

            //ベクトルに距離をセット
            transLatePos = vector.setLength(transLatePos, dis);

            //位置を更新
            cpTransform.Position = otherPosition + transLatePos;
        }
        #endregion

        #endregion

        #region ダメージアクション
        private class MonsterActDamage : MonsterActBase
        {
            public override void start()
            {
                base.start();

                //滑り移動値の設定（ダメージ中は滑らない）
                cpCharacterController.SlideMovementLimit = 0.0f;
            }

            public override void update()
            {
                //アクションチェックを無視
                monster.IgnoreCheckAction = true;

                //ダメージ
                if (phase == 0)
                {
                    //ダメージモーションをセット
                    setMotion(0, 0, (uint)Action.DAMAGE);

                    //ダメージエフェクトをセット
                    monster.setEffect((uint)Effect.Damage);

                    if(monster.hitPoint > 0)
                    {
                        //ダメージSE再生
                        monster.cpSoundPlayer._Sources[(int)Sound.Damage].play();
                    }
                    else
                    {
                        //ラストダメージSE再生
                        monster.cpSoundPlayer._Sources[(int)Sound.LastAttack].play();
                    }

                    phase = 1;
                }
                else
                {
                    if (isEndMotion(0))
                    {
                        //待機に戻る
                        requestAction((int)Action.IDLE);

                        if(other.Hold)
                        {
                            //相手のホールドを終了させる状態を更新
                            other.finishedHolding = true;
                        }
                    }
                }

                //移動方向と速度のベクトル
                vec3 move = vec3.Zero;

                //移動を反映
                monster.MoveSpeed = move;
            }
        }
        #endregion

        #endregion

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
        /// 攻撃アクションチェック
        /// </summary>
        /// <returns></returns>
        private bool isAttackAction()
        {
            if(inputData == null)
            {
                return false;
            }
            
            if((inputData.PadTrigger & GamePadButton.RTrigTop) != 0)
            {
                //筋力ゲージが0じゃないか
                if(muscleGauge > 0)
                {
                    //ボタンを押した時、チャージ状態になる
                    //チャージ中は移動可能（移動状態の時よりは遅い）
                    charge = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// チャージ状態チェック
        /// </summary>
        /// <returns></returns>
        private bool isCharging()
        {
            if (inputData == null)
            {
                return false;
            }

            if ((inputData.PadOn & GamePadButton.RTrigTop) != 0)
            {
                //ボタンを押している間、チャージ状態
                return true;
            }
            return false;
        }

        /// <summary>
        /// ラリアット発動チェック
        /// </summary>
        /// <returns></returns>
        private bool isLariat()
        {
            if (inputData == null)
            {
                return false;
            }

            if ((inputData.PadRelease & GamePadButton.RTrigTop) != 0)
            {
                //ボタンを離した時、ラリアット状態になる
                charge = false;
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
            float x_abs = math.abs(inputData.AxisR.x);
            float y_abs = math.abs(inputData.AxisR.y);

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
            vec2 axis = inputData.AxisR;

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
        private void updateMotionSequence()
        {
            if(cpMotion == null)
            {
                return;
            }
            var node = cpMotion.Layer[0].HighestWeightMotionNode;
            if(node != null )
            {
                //MotionデータからMotionSequenceに登録されたSequenceを取得する
                var count = node.SequenceTracksCount;
                for(uint i = 0; i < count; i++)
                {
                    var tracktype = node.getSequenceTracksTypeinfo(i);
                }
            }
        }
        #endregion

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

            //相手のMonster_Workを取得
            Monster_Work otherMonster = null;
            if (GameObject.Name == "Egi")
            {
                otherMonster = SceneManager.MainScene.findGameObject("Aari").getComponent<Monster_Work>();
            }
            else if (GameObject.Name == "Aari")
            {
                otherMonster = SceneManager.MainScene.findGameObject("Egi").getComponent<Monster_Work>();
            }

            //魔女からの攻撃を受けたら
            if ((other.FilterInfo.Layer == via.physics.System.getLayerIndex("WitchAttack")))
            {
                //ダメージを受けた状態にする
                isDamage = true;
            }

            //魔力弾を受けたら
            if ((other.FilterInfo.Layer == via.physics.System.getLayerIndex("MagicPower")))
            {
                //筋力ゲージが最大でない場合
                if (muscleGauge < monsterUserData.MaxMuscleGauge)
                {
                    //筋力ゲージを増やす
                    muscleGauge += 1;

                    //筋力ゲージに応じて、ホールド時間を設定（筋力ゲージ1につき、20フレーム）
                    holdTime += monsterUserData.HoldOneTime;

                    //魔力ゲットのエフェクトをセット
                    setEffect((uint)Effect.get);
                    
                    //魔力ゲットSE再生
                    cpSoundPlayer._Sources[(int)Sound.MagicGet].play();
                }
            }

            //自分のラリアットが相手にヒット
            //自分の攻撃コリジョンと相手のダメージコリジョンが接触している間
            if ((self.FilterInfo.Layer == via.physics.System.getLayerIndex("MonsterAttack")) && (other.FilterInfo.Layer == via.physics.System.getLayerIndex("Damage")))
            {
                //自分のムキムキレベルが相手より高い場合
                if (!(otherMonster.ActionNo == (int)Action.ATTACK && !otherMonster.charge && muscleLevel <= otherMonster.muscleLevel))
                {
                    //ホールドしている
                    holding = true;
                    //ホールドしているアクション
                    actionCtrl.requestAction((int)Action.HOLDING);
                    ignoreCheckAction = true;

                    //ラリアットSEを停止
                    cpSoundPlayer._Sources[(int)Sound.Lariat].FadeOutTimeSec = 0.5f;
                    cpSoundPlayer._Sources[(int)Sound.Lariat].fadeOutAndStop();
                }
            }

            //相手のラリアットが自分にヒット
            if ((self.FilterInfo.Layer == via.physics.System.getLayerIndex("Damage")) && (other.FilterInfo.Layer == via.physics.System.getLayerIndex("MonsterAttack")))
            {
                //相手が攻撃（ラリアット）中かつ、相手のムキムキレベルが自分より高い場合
                if(!(ActionNo == (int)Action.ATTACK && !charge && muscleLevel >= otherMonster.muscleLevel))
                {
                    //ホールドされている
                    holded = true;
                    //ホールドされているアクション
                    actionCtrl.requestAction((int)Action.HOLDEG);
                    ignoreCheckAction = true;
                }
            }

            //爆発にヒットしたら
            if((self.FilterInfo.Layer == via.physics.System.getLayerIndex("Damage")) && (other.FilterInfo.Layer == via.physics.System.getLayerIndex("BombAttack")))
            {
                //ダメージを受けた状態にする
                isDamage = true;
            }
        }

        //衝突している間
        public void onOverlapping(CollisionInfo collision_info)
        {
            //自身の衝突形状
            Collidable self = collision_info.CollidableA;
            //相手の衝突形状
            Collidable other = collision_info.CollidableB;
        }

        //離れた瞬間
        public void onSeparate(CollisionInfo collision_info)
        {
            //自身の衝突形状
            Collidable self = collision_info.CollidableA;
            //相手の衝突形状
            Collidable other = collision_info.CollidableB;
        }
        #endregion
    }
}
