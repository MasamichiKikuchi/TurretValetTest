//=============================================================================
// <summary>
// キャラクターのアクションを管理するクラス 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.effect.script;
using via.motion;
using via.physics;

namespace app
{
    public class ActionController_Work
    {
        public class ActionBase
        {
            protected GameObject cpGameObject = null;
            protected Transform cpTransform = null;
            protected Motion cpMotion = null;
            protected CharacterController cpCharacterController = null;
            protected ObjectEffectController cpObjectEffectController = null;
            protected CharacterBase_Work cpCharacterBase = null;
            protected ActionController_Work actionController = null;

            protected int phase = 0;
            protected float timer = 0.0f;

            private bool isEnd = false;
        
            /// <summary>
            /// アクションが終了しているか
            /// </summary>
            public bool IsEnd
            {
                get { return isEnd; }
            }

            /// <summary>
            /// デルタタイムを取得
            /// </summary>
            protected float DeltaTime
            {
                get
                {
                    return cpCharacterBase.DeltaTime;
                }
            }

            protected vec3 MoveSpeed
            {
                set
                {
                    cpCharacterBase.MoveSpeed = value;
                }
                get
                {
                    return cpCharacterBase.MoveSpeed;
                }
            }

            /// <summary>
            /// アクションの初期化
            /// </summary>
            public void initialize(GameObject owner, ActionController_Work parent, ActionData data)
            {
                cpGameObject = owner;
                cpTransform = cpGameObject.Transform;
                cpMotion = cpGameObject.getSameComponent<Motion>();
                cpCharacterController = cpGameObject.getSameComponent<CharacterController>();
                cpObjectEffectController = cpGameObject.getSameComponent<ObjectEffectController>();
                cpCharacterBase = cpGameObject.getComponent<CharacterBase_Work>();
                actionController = parent;
            }

            /// <summary>
            /// リセット
            /// </summary>
            public void reset()
            {
                phase = 0;
                timer = 0.0f;
                isEnd = false;
            }

            /// <summary>
            /// アクションの終了を通知
            /// </summary>
            protected void notifyActionEnd()
            {
                isEnd = true;
            }

            /// <summary>
            /// アクションのリクエスト
            /// </summary>
            /// <param name="action_no"></param>
            /// <param name="reset"></param>
            protected void requestAction(int action_no, bool reset = false)
            {
                actionController.requestAction(action_no, reset);
            }

            #region アクション独自処理
            /// <summary>
            /// スタート処理
            /// </summary>
            public virtual void start() { }

            /// <summary>
            /// アップデート処理
            /// </summary>
            public virtual void update() { }

            public virtual void lateUpdate() { }

            /// <summary>
            /// 終了処理
            /// </summary>
            public virtual void finalize() { }
            #endregion

            #region モーション
            /// <summary>
            /// モーションをセット
            /// </summary>
            public void setMotion(int layer_idx, uint bank_id, uint motion_id, float start_frame = 0.0f, float interpolation_frame = 20.0f, bool force_set = false)
            {
                var layer = cpMotion.Layer[layer_idx];

                //現在と違うモーションがセットいる場合はセット
                if (layer.MotionBankID != bank_id || layer.MotionID != motion_id || force_set)
                {
                    layer.changeMotion(bank_id, motion_id, start_frame, interpolation_frame, InterpolationMode.CrossFade, InterpolationCurve.Linear);
                }
            }

            /// <summary>
            /// モーションが終了したか
            /// </summary>
            public bool isEndMotion(int layer_idx)
            {
                return cpMotion.Layer[layer_idx].StateEndOfMotion;
            }
            #endregion

            #region 回転
            public float getTargetRotY(vec3 target_dir)
            {
                target_dir.y = 0.0f;
                target_dir = vector.normalizeFast(target_dir);
                return math.atan2(target_dir.x, target_dir.z);
            }

            /// <summary>
            /// Y軸の回転をセット
            /// </summary>
            protected void setRotationY(float rotation_y, float speed = math.PI)
            {
                cpCharacterBase.setRotationY(rotation_y, speed);
            }
            #endregion
        }

        /// <summary>
        /// アクションデータ
        /// </summary>
        public class ActionData
        {
            public int actionNo = 0;
            public ActionBase actionProc = null;
            public bool isAlwaysThink = false;
        }

        private List<ActionData> actionList = new List<ActionData>();

        /// <summary>
        /// 現在のアクション
        /// </summary>
        private int nowActionNo = -1;
        private ActionBase nowAction = null;

        public int ActionNo
        {
            get { return nowActionNo; }
        }

        /// <summary>
        /// 前回のアクション
        /// </summary>
        private int prevActionNo = -1;
        private ActionBase prevAction = null;

        public int PrevActionNo
        {
            get { return prevActionNo; }
        }

        /// <summary>
        /// リクエストされたアクション番号
        /// </summary>
        private int requestActionNo = -1;
        private bool resetAction = false;

        public void initialize(GameObject owner, List<ActionData> action_list)
        {
            actionList = action_list;

            int action_count = actionList.Count;
            for (int i = 0; i < action_count; i++)
            {
                var data = actionList[i];
                data.actionProc.initialize(owner, this, data);
            }
        }

        public void update()
        {
            // アクションの切り替え
            if (requestActionNo != -1)
            {
                bool is_change_action = false;
                if ((requestActionNo != nowActionNo) || resetAction)
                {
                    //現在のアクションと違うアクションがリクエストされた。またはリセットしなおす。
                    is_change_action = true;
                }

                if (is_change_action)
                {
                    if (nowAction != null)
                    {
                        //現在のアクションの終了処理
                        nowAction.finalize();
                    }

                    //前回のアクションを保存
                    prevActionNo = nowActionNo;
                    prevAction = nowAction;

                    nowActionNo = requestActionNo;
                    nowAction = actionList.Find(x => x.actionNo == requestActionNo).actionProc;

                    if (nowAction != null)
                    {
                        //新しいアクションの初期
                        nowAction.reset();
                        nowAction.start();
                    }
                }
                requestActionNo = -1;
                resetAction = false;
            }

            if (nowAction != null)
            {
                nowAction.update();
            }
        }
        public void lateUpdate()
        {
            if (nowAction != null)
            {
                nowAction.lateUpdate();
            }
        }

        public void requestAction(int action_no, bool reset = false)
        {
            requestActionNo = action_no;
            resetAction = reset;
        }

        /// <summary>
        /// アクションが終了しているか
        /// </summary>
        /// <returns></returns>
        public bool isActionEnd()
        {
            if (nowAction == null)
            {
                return true;
            }
            return nowAction.IsEnd;
        }

        public bool isRequestAction()
        {
            return requestActionNo != -1;
        }
    }
}
