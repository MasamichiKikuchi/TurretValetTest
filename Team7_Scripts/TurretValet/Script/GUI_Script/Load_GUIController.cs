//=============================================================================
// <summary>
// Load_GUIController 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.gui.asset;
using via.hid;

namespace app
{
	public class Load_GUIController : GUIBase
    {
        
        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GameFlowManager_Work _GameFlowManager_Work;
        #endregion
        public override void start()
        {
            if (!IsReady)
            {
                return;
            }
            _root = _controller.getObject(Title.Root);
            _GameFlowManager_Work = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
        }

        public void Animation()
        {
            playAnimation(LoadingGUI.SymbolDef.Root.ParamDef.PNL_Loading_PlayState, LoadingGUI.SymbolDef.PNL_Loading.State_DEFAULT);
        }
        public void NoAnimation()
        {
            playAnimation(LoadingGUI.SymbolDef.Root.ParamDef.PNL_Loading_PlayState, LoadingGUI.SymbolDef.PNL_Loading.State_NoIcon);
        }
    }
}
