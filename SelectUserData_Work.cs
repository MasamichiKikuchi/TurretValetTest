//=============================================================================
// <summary>
// SelectUserData_Work 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class SelectUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("決定時、自動で次シーンへの遷移時間(秒)"), DataMember]
        private float autoSelectSkip = 2.0f;  //タイトルから次のシーンへの遷移時

        #endregion

        #region プロパティ
        public float AutoSelectSkip
        {
            get { return autoSelectSkip; }
        }

        #endregion
    }
}
