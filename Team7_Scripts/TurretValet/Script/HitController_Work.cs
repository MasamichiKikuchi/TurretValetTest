//=============================================================================
// <summary>
// キャラクター同士の接触を管理するクラス 
// </summary>
// <author> 菊池 雅道 </author>
//=============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using via;
using via.attribute;
using via.physics;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.HitController)]
    public class HitController_Work : via.Behavior, via.physics.IColliders
    {
        public enum ContactKind
        {
            NONE = 0,
            CONTACT,
            OVERLAPPING,
            SEPARATE,
        };

        /// <summary>
        /// 攻撃情報
        /// </summary>
        public class AttackRecvData
        {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="kind"></param>
            public AttackRecvData(ContactKind kind, CollisionInfo info)
            {
                var colA = info.CollidableA;
                var colB = info.CollidableB;
                var cp = info.ContactPoint;

                this.kind = kind;

                //受けたオブジェクト
                owner = colA.GameObject;

                //ヒットしたGameObject
                attacker = colB.GameObject;

                //UserData情報
                var user_data = colB.UserData as RequestSetColliderUserData;
                if (user_data != null)
                {
                    //AttackUserDataの情報を格納
                    var atk_data = user_data.ParentUserData as AttackUserData_Work;

                    attackUserData.DamageValue = atk_data.DamageValue;
                    attackUserData.DamageType = atk_data.DamageType;
                    attackUserData.HitStopSpeed = atk_data.HitStopSpeed;
                    attackUserData.HitStopFrame = atk_data.HitStopFrame;
                    attackUserData.SameAttackId = atk_data.SameAttackId;
                    attackUserData.HitDisableFrame = atk_data.HitDisableFrame;
                }

                //ヒット位置情報
                contactPosition = cp.Position;
                contactNormal = cp.Normal;
                contactDistance = cp.Distance;
            }

            /// <summary>
            /// 攻撃を受けたオブジェクト
            /// </summary>
            public GameObject owner = null;


            /// <summary>
            /// 攻撃を仕掛けたオブジェクト
            /// </summary>
            public GameObject attacker = null;

            /// <summary>
            /// 衝突の種類
            /// </summary>
            public ContactKind kind = ContactKind.CONTACT;

            /// <summary>
            /// 攻撃情報
            /// </summary>
            public AttackUserData_Work attackUserData = new AttackUserData_Work();

            /// <summary>
            /// ヒット位置
            /// </summary>
            public vec3 contactPosition = new vec3();
            public vec3 contactNormal = new vec3();
            public float contactDistance = 0.0f;
        }

        #region ヒット情報
        /// <summary>
        /// ヒット情報の基底クラス
        /// </summary>
        public class HitInfoBase
        {
            /// <summary>
            /// クリア
            /// </summary>
            public void clear()
            {
                isHit = false;
                owner = null;
                damage = 0;
                damageType = AttackUserData_Work.DamageTypeEnum.NONE;
                hitStopFrame = 0.0f;
                hitStopSpeed = 1.0f;
                attacker = null;
                userData = null;
            }

            /// <summary>
            /// 何かにヒットしたか
            /// </summary>
            public bool isHit = false;

            /// <summary>
            /// ダメージを受けたオブジェクト
            /// </summary>
            public GameObject owner = null;

            /// <summary>
            /// ダメージ量
            /// </summary>
            public float damage = 0;

            /// <summary>
            /// ヒットストップ時間（フレーム数）
            /// </summary>
            public float hitStopFrame = 0.0f;

            /// <summary>
            /// ヒットストップ速度倍率
            /// </summary>
            public float hitStopSpeed = 1.0f;

            /// <summary>
            /// ダメージリアクション（ダメージタイプ）
            /// </summary>
            public AttackUserData_Work.DamageTypeEnum damageType = AttackUserData_Work.DamageTypeEnum.NONE;

            /// <summary>
            /// AttackUserData
            /// </summary>
            public AttackUserData_Work userData;

            /// <summary>
            /// 攻撃したオブジェクト
            /// </summary>
            public GameObject attacker = null;

            /// <summary>
            /// ヒット位置
            /// </summary>
            public vec3 contactPosition = new vec3();

            /// <summary>
            /// ヒット法線
            /// </summary>
            public vec3 contactNormal = new vec3();

            /// <summary>
            /// ヒット距離
            /// </summary>
            public float contactDistance = 0.0f;

        }

        public class DamageInfo : HitInfoBase
        {
            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public DamageInfo() { }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="data"></param>
            public DamageInfo(AttackRecvData data)
            {
                AttackUserData_Work atk_data = data.attackUserData;

                isHit = true;
                owner = data.owner;
                damage = atk_data.DamageValue;
                damageType = atk_data.DamageType;
                hitStopFrame = atk_data.HitStopFrame;
                hitStopSpeed = atk_data.HitStopSpeed;
                userData = atk_data;
                attacker = data.attacker;
                contactPosition = data.contactPosition;
                contactNormal = data.contactNormal;
                contactDistance = data.contactDistance;
            }
        }

        public class AttackHitInfo : HitInfoBase
        {
            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public AttackHitInfo() { }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="attack"></param>
            public AttackHitInfo(AttackHit attack)
            {
                isHit = attack.isHit;
                owner = attack.owner;
                damage = attack.damage;
                damageType = attack.damageType;
                hitStopFrame = attack.hitStopFrame;
                hitStopSpeed = attack.hitStopSpeed;
                attacker = attack.attacker;
                userData = attack.userData;
                contactPosition = attack.contactPosition;
                contactNormal = attack.contactNormal;
                contactDistance = attack.contactDistance;
            }
        }

        private class DamageHistory
        {
            /// <summary>
            /// 更新
            /// </summary>
            /// <param name="delta_frame"></param>
            /// <returns></returns>
            public bool update(float delta_frame)
            {
                hitDisableFrameCounter -= delta_frame;
                if (hitDisableFrameCounter < 0.0f)
                {
                    return false;
                }
                return true;
            }

            [IgnoreDataMember, ReadOnly(true)]
            public float hitDisableFrameCounter = 0.0f;

            [IgnoreDataMember, ReadOnly(true)]
            public DamageInfo damageInfo = null;
        }

        public class AttackHit : HitInfoBase
        {
            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public AttackHit() { }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public AttackHit(DamageInfo info)
            {
                isHit = info.isHit;
                owner = info.owner;
                damage = info.damage;
                damageType = info.damageType;
                hitStopFrame = info.hitStopFrame;
                hitStopSpeed = info.hitStopSpeed;
                attacker = info.attacker;
                userData = info.userData;
                contactPosition = info.contactPosition;
                contactNormal = info.contactNormal;
                contactDistance = info.contactDistance;
            }
        }
        #endregion

        #region ダメージ関連フィールド
        //Colliderの衝突チェックでの攻撃情報
        [IgnoreDataMember, ReadOnly(true)]
        private List<AttackRecvData>[] attackRecvDataList = new List<AttackRecvData>[via.physics.System.MaxWorker];

        /// <summary>
        /// ダメージ情報
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        private List<DamageInfo> damageInfoList = new List<DamageInfo>();

        //受けたダメージの履歴
        [IgnoreDataMember, ReadOnly(true)]
        private List<DamageHistory> damageHistoryList = new List<DamageHistory>();
        #endregion

        #region 押し当たり関連フィールド
        /// <summary>
        /// 押し当たりコリジョン情報
        /// </summary>
        private class PressInfo
        {
            /// <summary>
            /// 相手のオブジェクト
            /// </summary>
            public GameObject targetObject = null;

            /// <summary>
            /// ヒットした位置
            /// </summary>
            public vec3 contactPos = new vec3();

            /// <summary>
            /// ヒットした法線
            /// </summary>
            public vec3 contactNormal = new vec3();

            /// <summary>
            ///    ヒットした距離
            /// </summary>
            public float contactDistance = 0.0f;
        }

        /// <summary>
        /// 押し当たり情報
        /// </summary>
        public class PressMoveInfo
        {
            /// <summary>
            /// 情報をクリア
            /// </summary>
            public void clear()
            {
                direction = vec3.Zero;
                distance = 0.0f;
                moveDistance = vec3.Zero;
            }

            /// <summary>
            /// 補正移動量
            /// </summary>
            public vec3 moveDistance = new vec3();

            /// <summary>
            /// 補正方向
            /// </summary>
            public vec3 direction = new vec3();

            /// <summary>
            /// 補正移動量
            /// </summary>
            public float distance = 0.0f;
        }

        [IgnoreDataMember, ReadOnly(true)]
        private List<PressInfo>[] pressInfoList = new List<PressInfo>[via.physics.System.MaxWorker];

        [IgnoreDataMember, ReadOnly(true)]
        private List<PressMoveInfo> pressMoveInfoList = new List<PressMoveInfo>();

        [IgnoreDataMember, ReadOnly(true)]
        private PressMoveInfo totalPressMove = new PressMoveInfo();

        /// <summary>
        /// 押し当たりの最大履歴数
        /// </summary>
        public static int maxPressMoveHistory = 100;

        /// <summary>
        /// 押し当たりの履歴
        /// </summary>
        [IgnoreDataMember, ReadOnly(true), GridEdit(new string[] { "Direction", "Distance", "MoveDistance" })]
        List<PressMoveInfo> totalPressMoveHistory = new List<PressMoveInfo>(maxPressMoveHistory);
        #endregion

        #region 攻撃側関連フィールド
        [IgnoreDataMember, ReadOnly(true)]
        private List<AttackHit>[] attackHitList = new List<AttackHit>[via.Thread.MaxThreadId];

        /// <summary>
        /// 攻撃情報
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        List<AttackHitInfo> attackHitInfoList = new List<AttackHitInfo>();

        /// <summary>
        /// 攻撃ヒット情報を取得
        /// </summary>
        /// <returns></returns>
        public List<AttackHitInfo> getAttackHitInfo()
        {
            return attackHitInfoList;
        }
        #endregion

        public override void start()
        {
            //ヒット管理の初期化
            initializeHit();

            //押し当たりの初期化
            initializePress();
        }

        public override void update()
        {
            //ヒットした情報を整理
            updateDamageInfo();

            //押し当たりの情報を整理
            updatePressInfo();

            //受けた攻撃の初期化
            clearAttackRecvData();

            //ヒットした押しあたりの初期化
            clearAttackRecvPress();
        }

        public override void lateUpdate()
        {
            //ヒットした攻撃の初期化
            clearAttackHitHistory();
        }

        #region ヒット管理
        /// <summary>
        /// ヒットした攻撃の初期化
        /// </summary>
        private void initializeHit()
        {
            int num = attackRecvDataList.Length;
            for (int i = 0; i < num; i++)
            {
                attackRecvDataList[i] = new List<AttackRecvData>();
            }

            num = attackHitList.Length;
            for (int i = 0; i < num; i++)
            {
                attackHitList[i] = new List<AttackHit>();
            }
        }

        private void updateDamageInfo()
        {
            //ダメージ情報のクリア
            damageInfoList.Clear();

            //履歴の更新
            updateDamageHistory();

            int list_num = attackRecvDataList.Length;
            for (int i = 0; i < list_num; i++)
            {
                int atk_num = attackRecvDataList[i].Count;
                for (int j = 0; j < atk_num; j++)
                {
                    AttackRecvData data = attackRecvDataList[i][j];
                    bool is_find = findDamageInfo(data);

                    if (is_find == false)
                    {
                        AttackUserData_Work atk_data = data.attackUserData;

                        //ダメージ情報を登録
                        DamageInfo dmg_info = new DamageInfo(data);
                        damageInfoList.Add(dmg_info);

                        //攻撃側へも登録
                        if (data.attacker != null)
                        {
                            HitController_Work cp_hc = data.attacker.getSameComponent<HitController_Work>();
                            if (cp_hc != null)
                            {
                                cp_hc.registerAttackHit(dmg_info);
                            }
                        }
                        //履歴に登録
                        registerDamageHistory(dmg_info);
                    }
                }
            }
        }
        #endregion

        #region ダメージ処理メソッド
        /// <summary>
        /// ダメージ情報を取得
        /// </summary>
        public List<DamageInfo> getDamageInfo()
        {
            return damageInfoList;
        }

        /// <summary>
        /// ヒットした攻撃をクリア
        /// </summary>
        private void clearAttackRecvData()
        {
            for (int i = 0; i < attackRecvDataList.Length; i++)
            {
                attackRecvDataList[i].Clear();
            }
        }

        private void registerDamageHistory(DamageInfo info)
        {
            DamageHistory history = new DamageHistory();
            history.damageInfo = info;
            history.hitDisableFrameCounter = info.userData.HitDisableFrame;

            damageHistoryList.Add(history);
        }

        /// <summary>
        /// 履歴に同じ攻撃があるか
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool findDamageInfo(AttackRecvData data)
        {
            int history_num = damageHistoryList.Count;
            for (int i = 0; i < history_num; i++)
            {
                DamageHistory history = damageHistoryList[i];
                if (history == null || history.damageInfo == null)
                {
                    continue;
                }

                //同じ攻撃IDかつ、同じ攻撃者の場合は無効
                if (history.damageInfo.userData.SameAttackId == data.attackUserData.SameAttackId)
                {
                    if (history.damageInfo.attacker == data.attacker)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 履歴情報の更新
        /// </summary>
        private void updateDamageHistory()
        {
            int history_num = damageHistoryList.Count;

            damageHistoryList.RemoveAll((history) =>
            {
                if (history == null)
                {
                    return true;
                }
                //履歴情報の更新
                if (history.update(DeltaTime))
                {
                    //まだ有効
                    return false;
                }
                //無効
                return true;
            });
        }
        #endregion

        #region 押し当たりメソッド
        /// <summary>
        /// 押し当たりの移動量を取得
        /// </summary>
        public PressMoveInfo getTotalPressMove()
        {
            return totalPressMove;
        }

        /// <summary>
        /// ヒットした押し当たりの初期化
        /// </summary>
        private void initializePress()
        {
            int num = pressInfoList.Length;
            for (int i = 0; i < num; i++)
            {
                pressInfoList[i] = new List<PressInfo>();
            }
        }

        /// <summary>
        /// ヒットした押し当たりをクリア
        /// </summary>
        private void clearAttackRecvPress()
        {
            int num = pressInfoList.Length;
            for (int i = 0; i < num; i++)
            {
                pressInfoList[i].Clear();
            }
        }

        /// <summary>
        /// ヒットした攻撃情報を取得
        /// </summary>
        private void clearAttackHitHistory()
        {
            for (int i = 0; i < attackHitList.Length; i++)
            {
                attackHitList[i].Clear();
            }
        }

        /// <summary>
        /// 押し当たり情報を取得
        /// </summary>
        private void updatePressInfo()
        {
            totalPressMove.clear();
            pressMoveInfoList.Clear();

            int num = pressInfoList.Length;
            for (int i = 0; i < num; i++)
            {
                int press_num = pressInfoList[i].Count;
                for (int j = 0; j < press_num; j++)
                {
                    PressInfo press_info = pressInfoList[i][j];
                    if (press_info == null)
                    {
                        continue;
                    }

                    //押し当たり情報を取得
                    vec3 direction = new vec3();
                    float distance = 0.0f;
                    getPressMove(press_info, out direction, out distance);

                    PressMoveInfo move_info = new PressMoveInfo();
                    move_info.direction = direction;
                    move_info.distance = distance;
                    pressMoveInfoList.Add(move_info);
                }
            }

            ///合計の移動量を取得
            int info_num = pressMoveInfoList.Count;
            vec3 move_dir = vec3.Zero;
            for (int i = 0; i < info_num; i++)
            {
                PressMoveInfo info = pressMoveInfoList[i];
                if (info == null)
                {
                    continue;
                }

                //押し当たり情報を取得
                move_dir += info.direction * info.distance;
            }

            //移動量の合計
            totalPressMove.moveDistance = move_dir;
            totalPressMove.direction = vector.normalize(move_dir);
            totalPressMove.distance = vector.length(move_dir);

            //履歴に追加
            if (pressMoveInfoList.Count > 0)
            {
                if (totalPressMoveHistory.Count >= maxPressMoveHistory)
                {
                    totalPressMoveHistory.RemoveAt(totalPressMoveHistory.Count - 1);
                }
                var info = new PressMoveInfo();
                info.distance = totalPressMove.distance;
                info.direction = totalPressMove.direction;
                info.moveDistance = totalPressMove.moveDistance;
                totalPressMoveHistory.Insert(0, info);
            }
        }

        /// <summary>
        /// 押し当たりの移動量を取得
        /// </summary>
        /// <param name="info"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        private void getPressMove(PressInfo info, out vec3 direction, out float distance)
        {
            vec3 dir = info.contactNormal;
            dir *= info.contactDistance * -1.0f;

            //XZ方向の移動量を取得
            dir.y = 0.0f;
            distance = vector.length(dir);
            direction = vector.normalize(dir);
        }
        #endregion

        #region 攻撃側通知メソッド
        private void registerAttackHit(DamageInfo info)
        {
            AttackHit history = new AttackHit(info);

            //並列でコールされることがあるのでスレッドIDで管理
            attackHitList[via.Thread.CurrentThreadId].Add(history);
        }

        /// <summary>
        /// AttackHitInfoList更新
        /// </summary>
        public void updateAttackHitInfo()
        {
            //これまでの情報をクリア
            attackHitInfoList.Clear();

            int list_num = attackHitList.Length;
            for (int i = 0; i < list_num; i++)
            {
                int atk_num = attackHitList[i].Count;
                for (int j = 0; j < atk_num; j++)
                {
                    AttackHit hit = attackHitList[i][j];
                    if (hit == null)
                    {
                        continue;
                    }

                    AttackHitInfo info = new AttackHitInfo(hit);
                    attackHitInfoList.Add(info);
                }
            }
        }
        #endregion

        #region IColliders

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(via.physics.CollisionInfo info)
        {
            
        }

        /// <summary>
        /// 衝突中
        /// </summary>
        /// <param name="info"></param>
        public void onOverlapping(via.physics.CollisionInfo info)
        {
            //--------------------------------------------------------------------------
            //押し当たり
            //--------------------------------------------------------------------------
            if ((info.CollidableA.FilterInfo.Layer == via.physics.System.getLayerIndex("Press")) && (info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Press")))
            {
                PressInfo press_info = new PressInfo();
                press_info.targetObject = info.CollidableB.GameObject;
                press_info.contactPos = info.ContactPoint.Position;
                press_info.contactNormal = info.ContactPoint.Normal;
                press_info.contactDistance = info.ContactPoint.Distance;

                pressInfoList[info.WorkerID].Add(press_info);

            }
            //--------------------------------------------------------------------------
            //爆弾同士の押し当たり
            //--------------------------------------------------------------------------
            if ((info.CollidableA.FilterInfo.Layer == via.physics.System.getLayerIndex("Bomb")) && (info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Bomb")))
            {
                PressInfo press_info = new PressInfo();
                press_info.targetObject = info.CollidableB.GameObject;
                press_info.contactPos = info.ContactPoint.Position;
                press_info.contactNormal = info.ContactPoint.Normal;
                press_info.contactDistance = info.ContactPoint.Distance * 0.5f;

                pressInfoList[info.WorkerID].Add(press_info);

            }
        }

        /// <summary>
        /// 離れた
        /// </summary>
        /// <param name="info"></param>
        public void onSeparate(via.physics.CollisionInfo info)
        {

        }
        #endregion
    }
}
