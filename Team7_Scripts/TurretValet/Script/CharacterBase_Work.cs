//=============================================================================
// <summary>
// キャラクターの共通処理を扱うクラス
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.effect.script;
using via.gui;
using via.motion;
using via.physics;

namespace app
{
    public class CharacterBase_Work : via.Behavior
    {
        #region コンポーネント
        protected Transform cpTransform = null;
        protected Motion cpMotion = null;
        protected CharacterController cpCharacterController = null;
        protected HitController_Work cpHitController = null;
        protected RequestSetCollider cpRequestSetCollider = null;
        protected ObjectEffectController cpObjectEffectController = null;
        private FrameSpeedController_Work cpFrameSpeedController = null;
        #endregion

        #region 座標更新
        #region 回転
        /// <summary>
        /// ターゲットのY回転角度
        /// </summary>
        private float targetRotationY = 0.0f;

        /// <summary>
        /// 今のY回転角度
        /// </summary>
        private float nowRotationY = 0.0f;

        /// <summary>
        /// 1フレームの回転量
        /// </summary>
        private float rotationYSpeed = 0.1f;
        #endregion

        #region 移動  
        private vec3 moveSpeed = vec3.Zero;
        [IgnoreDataMember, ReadOnly(true)]
        public vec3 MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }     
        #endregion
        #endregion

        #region ユーザーデータ
        [DataMember]
        private HitStopUserData_Work hitStopUserData = null;
        #endregion


        public override void awake()
        {
            cpTransform = GameObject.Transform;
            cpMotion = GameObject.getSameComponent<Motion>();
            cpCharacterController = GameObject.getSameComponent<CharacterController>();
            cpHitController = GameObject.getSameComponent<HitController_Work>();
            cpRequestSetCollider = GameObject.getSameComponent<RequestSetCollider>();
            cpObjectEffectController = GameObject.getSameComponent<ObjectEffectController>();
            cpFrameSpeedController = GameObject.getComponent<FrameSpeedController_Work>();

            resetRotationY();
        }

        #region 座標更新メソッド
        protected void updatePosition()
        {
            //Y軸の回転を更新
            updateRotationY();

            //キャラクター押しあたり
            updatePress();

            //重力を反映
            updateMove();
        }

        /// <summary>
        /// Y軸の回転をセット
        /// </summary>
        /// <param name="rotation_y"></param>
        /// <param name="speed"></param>
        public void setRotationY(float rotation_y, float speed = math.PI)
        {
            targetRotationY = rotation_y;
            rotationYSpeed = speed;
        }

        /// <summary>
        /// Y軸の回転をリセット
        /// </summary>
        private void resetRotationY()
        {
            var angleY = cpTransform.EulerAngle.y;
            nowRotationY = angleY;
            targetRotationY = angleY;
        }

        /// <summary>
        /// Y軸の回転を更新
        /// </summary>
        public void updateRotationY()
        {
            //±180度に正規化
            float target_rotation_y = MathEx.normalizeRad(targetRotationY);
            float now_rotation_y = MathEx.normalizeRad(nowRotationY);

            float diff = target_rotation_y - now_rotation_y;
            float diff_abs = math.abs(diff);
            float add = rotationYSpeed * DeltaTime;
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
            if (diff_abs < rotationYSpeed)
            {
                nowRotationY = targetRotationY;
            }
            else
            {
                nowRotationY += add;
            }

            //回転を適用
            vec3 rot = cpTransform.EulerAngle;
            rot.y = nowRotationY;
            cpTransform.EulerAngle = rot;
        }

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

        #region 移動
        private void updateMove()
        {           
            var pos = cpTransform.Position;
            pos += MoveSpeed;
            cpTransform.Position = pos;
           
        }
        #endregion

        #region アクション関連
        protected ActionController_Work actionCtrl = new ActionController_Work();

        /// <summary>
        /// インスペクターでアクションを選択するための列挙型
        /// </summary>
        private static string[] EnumName = new string[] { "ACTION_00", "ACTION_01", "ACTION_02", "ACTION_03", "ACTION_04" };
        private static int[] EnumValue = new int[] { 0, 1, 2, 3, 4 };

        protected virtual string[] getActionEnumName()
        {
            return EnumName;
        }

        protected virtual int[] getActionEnumValue()
        {
            return EnumValue;
        }

        [IgnoreDataMember, Browsable(false)]
        private string[] ActionEnumName
        {
            get
            {
                var list = getActionEnumName();
                return list;
            }
        }

        [IgnoreDataMember, Browsable(false)]
        private int[] ActionEnumValue
        {
            get
            {
                var list = getActionEnumValue();
                return list;
            }
        }

        [IgnoreDataMember, EnumSelector(ItemsSourceName = nameof(ActionEnumName), ItemsValueName = nameof(ActionEnumValue)), DisplayOrder(0)]
        protected int ActionNo
        {
            get { return actionCtrl.ActionNo; }
        }

        [IgnoreDataMember, EnumSelector(ItemsSourceName = nameof(ActionEnumName), ItemsValueName = nameof(ActionEnumValue)), DisplayOrder(1)]
        protected int PrevActionNo
        {
            get { return actionCtrl.PrevActionNo; }
        }

        protected void requestAction(int action_no, bool force_reset = false)
        {
            actionCtrl.requestAction(action_no, force_reset);
        }

        /// <summary>
        /// 列挙型の値をint配列に変換
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        protected int[] getEnumValuesToIntArray(Type enumType)
        {
            var enum_values = Enum.GetValues(enumType);
            int count = enum_values.Length;
            int[] ints = new int[count];
            for (int i = 0; i < count; i++)
            {
                ints[i] = (int)enum_values.GetValue(i);
            }
            return ints;
        }
        #endregion

        #endregion

        #region ヒットストップ
        /// <summary>
        /// ヒットストップのセット
        /// </summary>
        /// <param name="effectCnt"></param>
        public void setHitStop(int effectCnt)
        {
            if (cpFrameSpeedController != null)
            {
                //モーションとエフェクトをヒットストップ
                cpFrameSpeedController.requestFrameSpeed(hitStopUserData.HitStopSpeed, hitStopUserData.HitStopFrame, effectCnt);
            }

            //移動もヒットストップ
            MoveSpeed *= hitStopUserData.HitStopSpeed;
        }
        #endregion

        #region エフェクト
        /// <summary>
        /// エフェクトのセット
        /// </summary>
        /// <param name="container"></param>
        /// <param name="element"></param>
        public void setEffect(uint element)
        {
            if (cpObjectEffectController != null)
            {
                //エフェクトID
                EffectID effect_id = new EffectID(0, element);

                //エフェクト発生のリクエスト
                cpObjectEffectController.requestEffect(effect_id, vec3.Zero, cpTransform.Rotation, GameObject);
            }
        }

        /// <summary>
        /// エフェクトの終了
        /// </summary>
        /// <param name="element"></param>
        public void finishEffect(uint element)
        {
            //エフェクトIDで指定の発生中のエフェクトをリストとして取得
            var list = cpObjectEffectController.getCreatedEffectFromEffectID(new EffectID(0, element));

            if (list != null)
            {
                //発生中のエフェクトIDが同じエフェクトを終了
                for(int i = 0; i < list.Count; i++)
                {
                    list[i].finishAll();
                }
            }
        }

        /// <summary>
        /// エフェクトの消去
        /// </summary>
        /// <param name="element"></param>
        public void killEffect(uint element)
        {
            //エフェクトIDで指定の発生中のエフェクトをリストとして取得
            var list = cpObjectEffectController.getCreatedEffectFromEffectID(new EffectID(0, element));

            if (list != null)
            {
                //発生中のエフェクトIDが同じエフェクトを終了
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].killAll();
                }
            }
        }

        /// <summary>
        /// 指定したエフェクトが終了したか
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool isFinishedEffect(uint element)
        {
            //エフェクトIDで指定の発生中のエフェクトをリストとして取得
            var list = cpObjectEffectController.getCreatedEffectFromEffectID(new EffectID(0, element));

            if(list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].isFinished)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}

