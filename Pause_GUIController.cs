//=============================================================================
// <summary>
// Pause_GUIController 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.gui.asset;

namespace app
{
	public class Pause_GUIController : GUIBase
    {

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GameFlowManager_Work _GameFlowManager_Work;
        private int old_select;
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
            _root = _controller.getObject(PauseGUI.Root);
            _GameFlowManager_Work = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
            playAnimation(PauseGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, PauseGUI.SymbolDef.PNL_Choice.State_TitleSelect);
        }



        public override void update()
        {
            int select;
            select = _GameFlowManager_Work.SelectInPause;

            //切り替え時
            if (old_select != select)
            {
                switch (select)
                {
                    case 0:
                        playAnimation(PauseGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, PauseGUI.SymbolDef.PNL_Choice.State_TitleSelect);
                        break;
                    case 1:
                        playAnimation(PauseGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, PauseGUI.SymbolDef.PNL_Choice.State_ReturnSelect);
                        break;
                    case 2:
                        playAnimation(PauseGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, PauseGUI.SymbolDef.PNL_Choice.State_QuitSelect);
                        break;
                }
            }

            //現在の値を最後に保存
            old_select = select;

        }
    }
}
