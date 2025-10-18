//=============================================================================
// <summary>
// ResultMovieGUI_Controller 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using static via.motion.JointRemapValue.RemapValueItem;
using via.gui.asset;
using sound;

namespace app
{
	public class ResultMovieGUI_Controller : GUIBase
    {
        #region 定数
        /// <summary>
        ///リザルトのSE
        /// </summary>
        private enum ResultSe
        {
            Telop = 55,      // テロップが出た時のSE
        }
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GameFlowManager_Work _GameFlowManager_Work;
        private GameObject GameScene;
        private SoundPlayer cpSoundPlayer = null; //サウンドプレイヤーコンポーネント

        private int Stop_cnt;
        #endregion

        public override void awake()
		{
            base.awake();
        }

		public override void start()
        {
            if (!IsReady)
            {
                return;
            }
            _root = _controller.getObject(ResultGUI.Root);
            _GameFlowManager_Work = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
            GameScene = SceneManager.MainScene.findGameObject("GameSystem");

            //サウンドプレイヤー
            cpSoundPlayer = GameScene.getComponent<SoundPlayer>();

            playAnimation(ResultMovieGUI.SymbolDef.Root.ParamDef.PNL_Popup_PlayState, ResultMovieGUI.SymbolDef.PNL_popup.State_DEFAULT);


            switch (_GameFlowManager_Work.Winner)
            {
                case 0:
                    //青チーム勝ち
                    Stop_cnt = 300;
                    break;
                case 1:
                    //ピンクチーム勝ち
                    Stop_cnt = 300;
                    break;
                case 2:
                    //引き分け
                    Stop_cnt = 0;
                    break;
            }
        }

		public override void update()
        {

            if(Stop_cnt == 0)
            {
                
                switch (_GameFlowManager_Work.Winner)
                {
                    case 0:
                        //青チーム勝ち
                        cpSoundPlayer._Sources[(int)ResultSe.Telop].play();
                        playAnimation(ResultMovieGUI.SymbolDef.Root.ParamDef.PNL_Popup_PlayState, ResultMovieGUI.SymbolDef.PNL_popup.State_SheenaWin);
                        break;
                    case 1:
                        //ピンクチーム勝ち
                        cpSoundPlayer._Sources[(int)ResultSe.Telop].play();
                        playAnimation(ResultMovieGUI.SymbolDef.Root.ParamDef.PNL_Popup_PlayState, ResultMovieGUI.SymbolDef.PNL_popup.State_FluraWin);
                        break;
                    case 2:
                        //引き分け
                        playAnimation(ResultMovieGUI.SymbolDef.Root.ParamDef.PNL_Popup_PlayState, ResultMovieGUI.SymbolDef.PNL_popup.State_DROW);
                        break;
                }

                Stop_cnt = -1;
            }
            else if (Stop_cnt > 0)
            {
                Stop_cnt--;
            }

        }
	}
}
