//=============================================================================
// <summary>
// タイトルシーンのユーザーデータ 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class TitleUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("次シーン遷移時間(秒)"), DataMember]
        private float tilteTranslateTime = 1.0f;  //タイトルから次のシーンへの遷移時
       
        #endregion

        #region プロパティ
        public float TilteTranslateTime
        {
            get { return tilteTranslateTime; }
        }
       
        #endregion
    }
}
