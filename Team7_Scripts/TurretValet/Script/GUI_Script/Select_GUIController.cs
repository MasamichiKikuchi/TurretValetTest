//=============================================================================
// <summary>
// Select_GUIController 
// </summary>
// <author> 須永ジン </author>
//=============================================================================
using via;
using via.gui.asset;
using via.gui;
using sound;

namespace app
{
	public class Select_GUIController : GUIBase
    {
        private GUIParamVar<bool> tex_1p;
        private GUIParamVar<bool> tex_1p_selected;
        private GUIParamVar<bool> tex_2p;
        private GUIParamVar<bool> tex_2p_selected;

        SelectCharacterManager_Work _SelectCharacterManager_Work;

        GameObject GameScene;

        int old_select_1p;
        int old_select_2p;
        bool trg_decide_1p;
        bool trg_decide_2p;

        private enum SelectSe
        {
            CrursorMove = 30,   //カーソル移動
            Select,              //決定
            Cancele,             //キャンセル  
        }

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

            _root = _controller.getObject(ChoiceControllerUI.Root);

            tex_1p = _root.getParameter(ChoiceControllerUI.SymbolDef.Root.ParamDef.tex_1p_Visible);
            tex_1p_selected = _root.getParameter(ChoiceControllerUI.SymbolDef.Root.ParamDef.tex_1p_selected_Visible);
            tex_2p = _root.getParameter(ChoiceControllerUI.SymbolDef.Root.ParamDef.tex_2p_Visible);
            tex_2p_selected = _root.getParameter(ChoiceControllerUI.SymbolDef.Root.ParamDef.tex_2p_selected_Visible);

            tex_1p.Value = true;
            tex_1p_selected.Value = false;
            tex_2p.Value = true;
            tex_2p_selected.Value = false;

            old_select_1p = 0;
            old_select_2p = 0;

            trg_decide_1p = true;
            trg_decide_2p = true;

            _SelectCharacterManager_Work = SceneManager.MainScene.findGameObject("SelectCharacter").getComponent<SelectCharacterManager_Work>();

            GameScene = SceneManager.MainScene.findGameObject("GameSystem");
        }



        public override void update()
        {
            int select_1p = _SelectCharacterManager_Work.Select1P;
            int select_2p = _SelectCharacterManager_Work.Select2P;
            bool decide_1p = _SelectCharacterManager_Work.Deide1P;
            bool decide_2p = _SelectCharacterManager_Work.Deide2P;

            
            //切り替え時
            if (old_select_1p != select_1p)
            {
                switch (select_1p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_GoBlue_1P); 
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_GoPink_1P);
                        break;
                }
            }
            if (old_select_2p != select_2p)
            {
                switch (select_2p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_GoBlue_2P);
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_GoPink_2P);
                        break;
                }
            }

            //選択時
            if (decide_1p && trg_decide_1p)
            {
                trg_decide_1p = false;
                switch (select_1p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_StayBlue_Selected_Start_1P); 
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_StayPink_Selected_Start_1P);
                        break;
                }
            }
            if (!decide_1p && !trg_decide_1p)
            {
                trg_decide_1p = true;
                switch (select_1p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_StayBlue_1P);
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_1P_PlayState, ChoiceControllerUI.SymbolDef.PNL_1P.State_StayPink_1P);
                        break;
                }
            }

            if (decide_2p && trg_decide_2p)
            {
                trg_decide_2p = false;
                switch (select_2p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_StayBlue_Selected_Start_2P);
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_StayPink_Selected_Start_2P);
                        break;
                }
            }
            if (!decide_2p && !trg_decide_2p)
            {
                trg_decide_2p = true;
                switch (select_2p)
                {
                    case 0:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_StayBlue_2P);
                        break;
                    case 1:
                        playAnimation(ChoiceControllerUI.SymbolDef.Root.ParamDef.PNL_2P_PlayState, ChoiceControllerUI.SymbolDef.PNL_2P.State_StayPink_2P);
                        break;
                }
            }

            old_select_1p = select_1p;
            old_select_2p = select_2p;

        }
    }
}
