//=============================================================================
// <summary>
// NewBehavior 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using sound;
using System.Reflection;
using via;
using via.gui.asset;

namespace app
{
    public class Result_GUIController : GUIBase
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
            _root = _controller.getObject(ResultGUI.Root);
            _GameFlowManager_Work = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
        }



        public override void update()
        {
            int select;
            select = _GameFlowManager_Work.SelectInResult;

            //切り替え時
            if (old_select != select)
            {
                switch (select)
                {
                    case 0:
                        playAnimation(ResultGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, ResultGUI.SymbolDef.PNL_Choice.State_CharacterSelect);
                        break;
                    case 1:
                        playAnimation(ResultGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, ResultGUI.SymbolDef.PNL_Choice.State_TitleSelect);
                        break;
                    case 2:
                        playAnimation(ResultGUI.SymbolDef.Root.ParamDef.PNL_Choice_PlayState, ResultGUI.SymbolDef.PNL_Choice.State_QuitSelect);
                        break;
                }
            }
            old_select = select;

        }
    }
}