//=============================================================================
// <summary>
// UpdateOrderの順番を格納するためのクラス 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
namespace app
{
    public class AppUpdateOrder_Work
    {
        /// <summary>
        /// UpdateOrder
        /// </summary>
        public enum UpdateOrder
        {
            InputManager = 1,       //入力
            CameraManager,          //カメラ      
            GameFlowManager,        //ゲームフロー
            SelectCharacter,        //キャラクター選択
            HitController,          //ヒットコントローラー
            Gimmick,                //ステージギミック
            Witch,                  //魔法少女
            Monster,                //召喚獣
            FrameSpeedController,　 //フレーム速度調整
            GUI,                    //UI
        }
    }
}

