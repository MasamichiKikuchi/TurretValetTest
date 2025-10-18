//=============================================================================
// <summary>
// Title_GUIController 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using sound;
using via;
using via.gui;
using via.gui.asset;
using via.hid;

namespace app
{
	public class Title_GUIController : GUIBase
    {

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GameFlowManager_Work _GameFlowManager_Work;
        private GUIParamVar<bool> Credit;
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
            _root = _controller.getObject(Title.Root);
            Credit = _root.getParameter(Title.SymbolDef.Root.ParamDef.PNL_Credit_Visible);

            _GameFlowManager_Work = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();

            Credit.Value = false;
        }



        public override void update()
        {
            int select;
            select = _GameFlowManager_Work.SelectInTitle;

            //切り替え時
            if (old_select != select)
            {
                switch (select)
                {
                    case 0:
                        playAnimation(Title.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, Title.SymbolDef.PNL_Choice.State_FastStart_select);
                        break;
                    case 1:
                        playAnimation(Title.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, Title.SymbolDef.PNL_Choice.State_Begin_select);
                        break;
                    case 2:
                        playAnimation(Title.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, Title.SymbolDef.PNL_Choice.State_Quit_select);
                        break;
                }
            }

            if (_GameFlowManager_Work.TitleTransition)
            {
                playAnimation(Title.SymbolDef.Root.ParamDef.PNL_Fade_PlayState, Title.SymbolDef.PNL_Fade.State_FADE_OUT);
            }
            if (_GameFlowManager_Work.CreditSelected)
            {
                Credit.Value = true;
            }
            else
            {
                Credit.Value = false;
            }

            old_select = select;

        }
    }
}
