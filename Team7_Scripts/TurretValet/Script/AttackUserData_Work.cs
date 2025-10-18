//=============================================================================
// <summary>
// AttackUserData_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class AttackUserData_Work : via.physics.RequestSetColliderUserData
	{
        public enum DamageTypeEnum
        {
            [DisplayName("なし")]
            NONE,       //なし

            [DisplayName("ビーム")]
            BEAM,       //ビーム

            [DisplayName("爆弾")]
            BOMB,       //爆弾
        }
        
        #region フィールド
        [DisplayName("ダメージ量"), DataMember]
        private float damageValue = 0.0f;

        [DisplayName("ダメージタイプ"), DataMember]
        private DamageTypeEnum damageType = DamageTypeEnum.NONE;

        [DisplayName("ヒットストップ速度"), DataMember]
        private float hitStopSpeed = 0.0f;

        [DisplayName("ヒットストップ時間"), DataMember]
        private float hitStopFrame = 0.0f;

        [DisplayName("同じ攻撃扱いとするID"), DataMember]
        [Description("攻撃ヒット後の無敵用。「同じ攻撃の無効時間」とセットで設定。\n攻撃ヒット時、同じIDの無効時間が残っていた場合は、ヒット扱いにならなくなる。\n値はリソース内で、自由に設定可能。\n0未満：このIDの攻撃でのみ無効判定")]
        private int sameAttackId = -1;

        [DisplayName("同じ攻撃の無効時間（フレーム）"), DataMember]
        [Description("攻撃ヒット後の無敵用。「同じ攻撃扱いとするID」とセットで設定。\n攻撃ヒット（ダメージ確定）時に、この時間内では同じIDの攻撃は無効判定となる")]
        private float hitDisableFrame = 0.0f;
        #endregion

        #region プロパティ
        public float DamageValue
        {
            get { return damageValue; }
            set { damageValue = value; }
        }

        public DamageTypeEnum DamageType
        {
            get { return damageType; }
            set { damageType = value; }
        }

        public float HitStopSpeed
        {
            get { return hitStopSpeed; }
            set { hitStopSpeed = value; }
        }

        public float HitStopFrame
        {
            get { return hitStopFrame; }
            set { hitStopFrame = value; }
        }

        public int SameAttackId
        {
            get { return sameAttackId; }
            set { sameAttackId = value; }
        }

        public float HitDisableFrame
        {
            get { return hitDisableFrame; }
            set { hitDisableFrame = value; }
        }
        #endregion
    }
}
