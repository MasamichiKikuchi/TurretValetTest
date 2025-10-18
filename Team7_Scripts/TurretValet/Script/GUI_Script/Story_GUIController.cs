//=============================================================================
// <summary>
// NewBehavior 
// </summary>
// <author>須永ジン</author>
//=============================================================================
using sound;
using via;
using via.attribute;
using via.gui;
using via.gui.asset;
using via.hid;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.GUI)]
    public class StoryGUI_GUIController : GUIBase
    {

        #region 定数
        /// <summary>
        ///ストーリーのSE
        /// </summary>
        private enum StorySe
        {
            CrursorMove = 20,   //決定        
        }
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GUIParamVar<bool> StoryPage_1;
        private GUIParamVar<bool> StoryPage_2;
        private GUIParamVar<bool> StoryPage_3;
        private GUIParamVar<bool> StoryPage_4;
        private GUIParamVar<bool> StoryPage_5;
        private GUIParamVar<bool> StoryPage_6;
        private GUIParamVar<bool> StoryPage_7;
        private GUIParamVar<bool> StoryPage_8;
        private GUIParamVar<bool> StoryPage_9;
        private GUIParamVar<bool> StoryPage_10;
        private GUIParamVar<bool> StoryPage_11;
        private GUIParamVar<bool> StoryPage_12;
        private GUIParamVar<bool> StoryPage_13;
        private GUIParamVar<bool> StoryPage_14;
        private GUIParamVar<bool> StoryPage_15;
        private GUIParamVar<bool> StoryPage_16;
        private GUIParamVar<bool> StoryPage_17;
        private GUIParamVar<bool> StoryPage_18;
        private GUIParamVar<bool> StoryPage_19;
        private GUIParamVar<bool> StoryPage_20;
        private GUIParamVar<bool> StoryPage_21;
        private GUIParamVar<bool> StoryPage_22;
        private GUIParamVar<bool> StoryPage_23;
        private GUIParamVar<bool> StoryPage_24;

        private GameObject GameScene;
        private GameFlowManager_Work GameSceneWork;
        private SoundPlayer cpSoundPlayer = null; //サウンドプレイヤーコンポーネント

        private int page_num;
        private bool nextStep;

        #endregion

        #region プロパティ
        /// <summary>
        /// ストーリーの終了判定
        /// </summary>
        public bool NextStep
        {
            get { return nextStep; }
        }
        #endregion

        public override void awake()
        {
            base.awake();
        }

        public void AllValue_off()
        {
            StoryPage_1.Value = false;
            StoryPage_2.Value = false;
            StoryPage_3.Value = false;
            StoryPage_4.Value = false;
            StoryPage_5.Value = false;
            StoryPage_6.Value = false;
            StoryPage_7.Value = false;
            StoryPage_8.Value = false;
            StoryPage_9.Value = false;
            StoryPage_10.Value = false;
            StoryPage_11.Value = false;
            StoryPage_12.Value = false;
            StoryPage_13.Value = false;
            StoryPage_14.Value = false;
            StoryPage_15.Value = false;
            StoryPage_16.Value = false;
            StoryPage_17.Value = false;
            StoryPage_18.Value = false;
            StoryPage_19.Value = false;
            StoryPage_20.Value = false;
            StoryPage_21.Value = false;
            StoryPage_22.Value = false;
            StoryPage_23.Value = false;
            StoryPage_24.Value = false;
        }

        public override void start()
        {
            base.start();

            _root = _controller.getObject(StoryGUI.Root);

            StoryPage_1 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page1_Visible);
            StoryPage_2 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page2_Visible);
            StoryPage_3 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page3_Visible);
            StoryPage_4 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page4_Visible);
            StoryPage_5 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page5_Visible);
            StoryPage_6 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page6_Visible);
            StoryPage_7 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page7_Visible);
            StoryPage_8 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page8_Visible);
            StoryPage_9 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page9_Visible);
            StoryPage_10 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page10_Visible);
            StoryPage_11 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page11_Visible);
            StoryPage_12 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page12_Visible);
            StoryPage_13 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page13_Visible);
            StoryPage_14 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page14_Visible);
            StoryPage_15 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page15_Visible);
            StoryPage_16 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page16_Visible);
            StoryPage_17 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page17_Visible);
            StoryPage_18 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page18_Visible);
            StoryPage_19 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page19_Visible);
            StoryPage_20 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page20_Visible);
            StoryPage_21 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page21_Visible);
            StoryPage_22 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page22_Visible);
            StoryPage_23 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page23_Visible);
            StoryPage_24 = _root.getParameter(StoryGUI.SymbolDef.Root.ParamDef.PNL_Page24_Visible);

            GameScene = SceneManager.MainScene.findGameObject("GameSystem");
            GameSceneWork = GameScene.getComponent<GameFlowManager_Work>();
            cpSoundPlayer = GameScene.getComponent<SoundPlayer>();

            AllValue_off();
            StoryPage_1.Value = true;
            page_num = 0;
            nextStep = false;
        }




        public void ChangePage(int page_number)
        {
            switch (page_number)
            {
                case 1:
                    StoryPage_2.Value = true;
                    break;
                case 2:
                    StoryPage_3.Value = true;
                    break;
                case 3:
                    StoryPage_4.Value = true;
                    break;
                case 4:
                    StoryPage_5.Value = true;
                    break;
                case 5:
                    StoryPage_6.Value = true;
                    break;
                case 6:
                    StoryPage_7.Value = true;
                    break;
                case 7:
                    StoryPage_8.Value = true;
                    break;
                case 8:
                    StoryPage_9.Value = true;
                    break;
                case 9:
                    StoryPage_10.Value = true;
                    break;
                case 10:
                    StoryPage_11.Value = true;
                    break;
                case 11:
                    StoryPage_12.Value = true;
                    break;
                case 12:
                    StoryPage_13.Value = true;
                    break;
                case 13:
                    StoryPage_14.Value = true;
                    break;
                case 14:
                    StoryPage_15.Value = true;
                    break;
                case 15:
                    StoryPage_16.Value = true;
                    break;
                case 16:
                    StoryPage_17.Value = true;
                    break;
                case 17:
                    StoryPage_18.Value = true;
                    break;
                case 18:
                    StoryPage_19.Value = true;
                    break;
                case 19:
                    StoryPage_20.Value = true;
                    break;
                case 20:
                    StoryPage_21.Value = true;
                    break;
                case 21:
                    StoryPage_22.Value = true;
                    break;
                case 22:
                    StoryPage_23.Value = true;
                    break;
                case 23:
                    StoryPage_24.Value = true;
                    break;
                case 24:
                    //ストーリー終わり
                    nextStep = true;
                    break;
            }
        }

        public override void update()
        {

            if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown))
            {
                cpSoundPlayer._Sources[(int)StorySe.CrursorMove].play();
                page_num++;
                AllValue_off();
                ChangePage(page_num);
            }
        }
    }
}
