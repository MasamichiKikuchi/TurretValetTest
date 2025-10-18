//=============================================================================
// <summary>
// HitStopUserData_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class HitStopUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("ヒットストップ速度"), DataMember]
        private float hitStopSpeed = 0.0f;

        [DisplayName("ヒットストップ時間"), DataMember]
        private float hitStopFrame = 0.0f;
        #endregion

        #region プロパティ
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
        #endregion
    }
}
