//=============================================================================
// <summary>
// インゲームの設定値に関するユーザーデータ 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class IngameUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("ゲーム制限時間(秒)"), DataMember]
        private float gameTime = 99.0f;             //ゲームの制限時間
        [DisplayName("プレイヤー操作可能開始時間(秒)"), DataMember]
        private float startTransitionTime = 1.0f;   //プレイヤーが操作可能になる時間
        [DisplayName("開始演出時間(秒)"), DataMember]
        private float startGoTime = 1.0f;           //開始演出を行う時間       
        [DisplayName("ゲームオーバー演出時間(秒)"), DataMember]
        [Description("最後の攻撃ヒット時の演出用\n寄りのカメラワーク等")]
        private float gameOverTime = 2.0f;          //ゲームオーバー演出時間
        [DisplayName("試合終了演出遷移猶予時間(秒)"), DataMember]
        [Description("ゲームオーバー演出の後にゲーム終了演出を入れる猶予時間\nUI表示など")]
        private float finishTransitionTime = 3.0f;  //ゲームオーバー演出から終了演出へ遷移する猶予時間     
        [DisplayName("リザルトシーン遷移猶予時間 (秒)"), DataMember]
        [Description("終了演出からリザルトシーンへ遷移する猶予時間\n終了演出を見たいときに入れる")]
        private float resultTransitionTime = 3.0f;  //終了演出からリザルトシーンへ遷移する猶予時間 
        #endregion

        #region プロパティ
        public float GameTime
        {
            get { return gameTime; }
        }

        public float StartTransitionTime
        {
            get { return startTransitionTime; }
        }
       
        public float StartGoTime
        {
            get { return startGoTime; }
        }

        public float GameOverTime
        { 
            get { return gameOverTime; }   
        }

        public float FinishTransitionTime
        {
            get { return finishTransitionTime; }
        }

        public float ResultTransitionTime
        { 
            get { return resultTransitionTime; } 
        }

        #endregion
    }
}
