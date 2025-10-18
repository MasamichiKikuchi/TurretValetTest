//=============================================================================
// <summary>
// NewBehavior 
// </summary>
// <author> 須永ジン </author>
//=============================================================================
using System.Threading;
using via;
using via.attribute;
using via.gui;
using via.gui.asset;
using static app.GameFlowManager_Work;
using static app.SelectCharacterManager_Work;

namespace app
{
    public class InGame_GUIController : GUIBase
    {

        #region ユーザーデータ
        [DataMember]
        private MonsterUserData_Work monsterUserData = null;
        #endregion

        #region 残り時間
        private GUIParamVar<bool> time_st1_0;
        private GUIParamVar<bool> time_st1_1;
        private GUIParamVar<bool> time_st1_2;
        private GUIParamVar<bool> time_st1_3;
        private GUIParamVar<bool> time_st1_4;
        private GUIParamVar<bool> time_st1_5;
        private GUIParamVar<bool> time_st1_6;
        private GUIParamVar<bool> time_st1_7;
        private GUIParamVar<bool> time_st1_8;
        private GUIParamVar<bool> time_st1_9;

        private GUIParamVar<bool> time_st2_0;
        private GUIParamVar<bool> time_st2_1;
        private GUIParamVar<bool> time_st2_2;
        private GUIParamVar<bool> time_st2_3;
        private GUIParamVar<bool> time_st2_4;
        private GUIParamVar<bool> time_st2_5;
        private GUIParamVar<bool> time_st2_6;
        private GUIParamVar<bool> time_st2_7;
        private GUIParamVar<bool> time_st2_8;
        private GUIParamVar<bool> time_st2_9;
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        private GUIParamVar<Float2> Blue_bar_w_float2;
        private GUIParamVar<int> Blue_bar_w_int;
        private GUIParamVar<Float2> Pink_bar_w_float2;
        private GUIParamVar<int> Pink_bar_w_int;

        private GUIParamVar<Float3> Blue_Pos;
        private GUIParamVar<bool> EgiHPVisible;
        private GUIParamVar<Float3> Pink_Pos;
        private GUIParamVar<bool> AariHPVisible;

        private int EgiMoveHPcnt;
        private int AariMoveHPcnt;

        private float old_Sheena_magicpower;
        private float old_frula_magicpower;
        private int old_egi_HP;
        private int old_aari_HP;

        private bool GameOverTrg;

        private float Sheena_magicpower;
        private float frula_magicpower;
        private int egi_HP;
        private int aari_HP;
        private int egi_muscleGauge;
        private int aari_muscleGauge;
        private bool sheena_canbeam;
        private bool frula_canbeam;

        private float EgiPosX;
        private float EgiPosZ;
        private float AariPosX;
        private float AariPosZ;
        private float EgiPosAddZ;
        private float AariPosAddZ;

        private int MoveHPVisibleMax = 20;      //HPが見える最大時間
        private float GaugeWidth = 422.0f;      //ゲージの横幅
        private float GaugeHight = 36.0f;

        private GameObject Sheena_GameObject;
        private GameObject Frula_GameObject;
        private GameObject Egi_GameObject;
        private GameObject Aari_GameObject;

        private Witch_Work Sheena_Witch_Work;
        private Witch_Work Frula_Witch_Work;
        private Monster_Work Egi_Monster_Work;
        private Monster_Work Aari_Monster_Work;

        private GameFlowManager_Work _gameFlowManager = null;
        #endregion

        private void ResetTime()
        {
            time_st1_0.Value = false;
            time_st1_1.Value = false;
            time_st1_2.Value = false;
            time_st1_3.Value = false;
            time_st1_4.Value = false;
            time_st1_5.Value = false;
            time_st1_6.Value = false;
            time_st1_7.Value = false;
            time_st1_8.Value = false;
            time_st1_9.Value = false;
            time_st2_0.Value = false;
            time_st2_1.Value = false;
            time_st2_2.Value = false;
            time_st2_3.Value = false;
            time_st2_4.Value = false;
            time_st2_5.Value = false;
            time_st2_6.Value = false;
            time_st2_7.Value = false;
            time_st2_8.Value = false;
            time_st2_9.Value = false;
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

            _root = _controller.getObject(InGameGUI.Root);

            Blue_bar_w_int = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_Bar_B_RectW);
            Blue_bar_w_float2 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_Bar_B_Size);
            Pink_bar_w_int = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_Bar_P_RectW);
            Pink_bar_w_float2 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_Bar_P_Size);

            Blue_Pos = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_Position);
            Pink_Pos = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_Position);

            #region 残り時間
            time_st1_0 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_0_Visible);
            time_st1_1 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_1_Visible);
            time_st1_2 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_2_Visible);
            time_st1_3 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_3_Visible);
            time_st1_4 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_4_Visible);
            time_st1_5 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_5_Visible);
            time_st1_6 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_6_Visible);
            time_st1_7 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_7_Visible);
            time_st1_8 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_8_Visible);
            time_st1_9 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_1st_9_Visible);

            time_st2_0 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_0_Visible);
            time_st2_1 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_1_Visible);
            time_st2_2 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_2_Visible);
            time_st2_3 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_3_Visible);
            time_st2_4 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_4_Visible);
            time_st2_5 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_5_Visible);
            time_st2_6 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_6_Visible);
            time_st2_7 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_7_Visible);
            time_st2_8 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_8_Visible);
            time_st2_9 = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.tex_2st_9_Visible);
            #endregion

            EgiHPVisible = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_Visible);
            AariHPVisible = _root.getParameter(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_Visible);

            Sheena_GameObject = SceneManager.MainScene.findGameObject("Sheena");
            Frula_GameObject = SceneManager.MainScene.findGameObject("Frula");
            Egi_GameObject = SceneManager.MainScene.findGameObject("Egi");
            Aari_GameObject = SceneManager.MainScene.findGameObject("Aari");

            Sheena_Witch_Work = Sheena_GameObject.getComponent<Witch_Work>();
            Frula_Witch_Work = Frula_GameObject.getComponent<Witch_Work>();
            Egi_Monster_Work = Egi_GameObject.getComponent<Monster_Work>();
            Aari_Monster_Work = Aari_GameObject.getComponent<Monster_Work>();

            _gameFlowManager = SceneManager.MainScene.findGameObject("GameSystem").getComponent<GameFlowManager_Work>();
            GameOverTrg = true;

            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP0);
            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP0);

            EgiMoveHPcnt = 0;
            AariMoveHPcnt = 0;

            EgiHPVisible.Value = false;
            AariHPVisible.Value = false;

            old_egi_HP = Egi_Monster_Work.HitPoint;
            old_aari_HP = Aari_Monster_Work.HitPoint;
        }

        

        public override void update()
        {
            Sheena_magicpower = Sheena_Witch_Work.MagicPowerCount;
            frula_magicpower = Frula_Witch_Work.MagicPowerCount;
            egi_HP = Egi_Monster_Work.HitPoint;
            aari_HP = Aari_Monster_Work.HitPoint;
            egi_muscleGauge = Egi_Monster_Work.MuscleGauge;
            aari_muscleGauge = Aari_Monster_Work.MuscleGauge;
            sheena_canbeam = Sheena_Witch_Work.BeamStandby;
            frula_canbeam = Frula_Witch_Work.BeamStandby;

            

            if (old_Sheena_magicpower != Sheena_magicpower)
            {
                switch (Sheena_magicpower)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Sheena_MP_PlayState, InGameGUI.SymbolDef.PNL_Blue_MP.State_MP5);
                        break;
                }
            }
            if(old_frula_magicpower != frula_magicpower)
            {
                switch (frula_magicpower)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_MP_PlayState, InGameGUI.SymbolDef.PNL_Pink_MP.State_MP5);
                        break;
                }
            }

            if (old_egi_HP != egi_HP)
            {
                switch (egi_HP)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Egi_HP_PlayState, InGameGUI.SymbolDef.PNL_Blue_HP.State_Life5);
                        break;
                }

                //動くHP関連
                EgiMoveHPcnt = monsterUserData.MonsterShowLifeCnt;
                switch (old_egi_HP)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife5);
                        break;
                }
            }
            if (old_aari_HP != aari_HP)
            {
                switch (aari_HP)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Aari_HP_PlayState, InGameGUI.SymbolDef.PNL_Pink_HP.State_Life5);
                        break;
                }

                //動くHP関連
                AariMoveHPcnt = monsterUserData.MonsterShowLifeCnt;

                switch (old_aari_HP)
                {
                    case 0:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife0);
                        break;
                    case 1:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife1);
                        break;
                    case 2:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife2);
                        break;
                    case 3:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife3);
                        break;
                    case 4:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife4);
                        break;
                    case 5:
                        playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife5);
                        break;
                }
            }

            if (EgiMoveHPcnt > 0 && egi_HP != 0)
            {
                EgiHPVisible.Value = true;
                EgiPosX = Egi_GameObject.Transform.Position.x;
                EgiPosZ = Egi_GameObject.Transform.Position.z;
                EgiPosAddZ = 2 * (3.0f + EgiPosZ);
                //動くHP
                Blue_Pos.Value = new(EgiPosX * 95 + 960, EgiPosZ * (55 + EgiPosAddZ) + 400, 0.0f);
                //カウントダウン
                EgiMoveHPcnt--;
                if(EgiMoveHPcnt > (int)monsterUserData.MonsterShowLifeCnt / 2)
                {
                    if(EgiMoveHPcnt % MoveHPVisibleMax > (MoveHPVisibleMax / 2))
                    {
                        switch (egi_HP + 1)
                        {
                            case 1:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife1);
                                break;
                            case 2:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife2);
                                break;
                            case 3:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife3);
                                break;
                            case 4:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife4);
                                break;
                            case 5:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife5);
                                break;
                        }
                    }
                    else
                    {
                        switch (egi_HP)
                        {
                            case 1:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife1);
                                break;
                            case 2:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife2);
                                break;
                            case 3:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife3);
                                break;
                            case 4:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife4);
                                break;
                        }
                    }
                }
                else if(EgiMoveHPcnt == (int)monsterUserData.MonsterShowLifeCnt / 2)
                {
                    switch (egi_HP)
                    {
                        case 1:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife1);
                            break;
                        case 2:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife2);
                            break;
                        case 3:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife3);
                            break;
                        case 4:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Egi_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Egi.State_MoveLife4);
                            break;
                    }
                }
            }
            else
            {
                EgiHPVisible.Value = false;
            }
            if (AariMoveHPcnt > 0 && aari_HP != 0)
            {
                AariHPVisible.Value = true;
                AariPosX = Aari_GameObject.Transform.Position.x;
                AariPosZ = Aari_GameObject.Transform.Position.z;
                AariPosAddZ = 2 * (3.0f + AariPosZ);
                //動くHP
                Pink_Pos.Value = new(AariPosX * 95 + 960, AariPosZ * (55 + AariPosAddZ) + 400, 0.0f);
                //カウントダウン
                AariMoveHPcnt--;
                if (AariMoveHPcnt > (int)monsterUserData.MonsterShowLifeCnt / 2)
                {
                    if (AariMoveHPcnt % MoveHPVisibleMax > (MoveHPVisibleMax / 2))
                    {
                        switch (aari_HP + 1)
                        {
                            case 1:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife1);
                                break;
                            case 2:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife2);
                                break;
                            case 3:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife3);
                                break;
                            case 4:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife4);
                                break;
                            case 5:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife5);
                                break;
                        }
                    }
                    else
                    {
                        switch (aari_HP)
                        {
                            case 1:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife1);
                                break;
                            case 2:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife2);
                                break;
                            case 3:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife3);
                                break;
                            case 4:
                                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife4);
                                break;
                        }
                    }

                }
                else if (AariMoveHPcnt == (int)monsterUserData.MonsterShowLifeCnt / 2)
                {
                    switch (aari_HP)
                    {
                        case 1:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife1);
                            break;
                        case 2:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife2);
                            break;
                        case 3:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife3);
                            break;
                        case 4:
                            playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_MoveHP_Aari_PlayState, InGameGUI.SymbolDef.PNL_MoveHP_Aari.State_MoveLife4);
                            break;
                    }
                }
            }
            else
            {
                AariHPVisible.Value = false;
            }

            //ゲージ
            Blue_bar_w_int.Value = ((int)GaugeWidth * Egi_Monster_Work.HoldTime) / (monsterUserData.MaxMuscleGauge * monsterUserData.HoldOneTime);
            Blue_bar_w_float2.Value = new((GaugeWidth * Egi_Monster_Work.HoldTime) / (monsterUserData.MaxMuscleGauge * monsterUserData.HoldOneTime), GaugeHight);
            Pink_bar_w_int.Value = ((int)GaugeWidth * Aari_Monster_Work.HoldTime) / (monsterUserData.MaxMuscleGauge * monsterUserData.HoldOneTime);
            Pink_bar_w_float2.Value = new((GaugeWidth * Aari_Monster_Work.HoldTime) / (monsterUserData.MaxMuscleGauge * monsterUserData.HoldOneTime), GaugeHight);

            if (sheena_canbeam)
            {
                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Seena_Beam_PlayState, InGameGUI.SymbolDef.PNL_BlueBeam.State_charge);
            }
            else
            {
                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Seena_Beam_PlayState, InGameGUI.SymbolDef.PNL_BlueBeam.State_DEFAULT);
            }
            if (frula_canbeam)
            {
                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_Beam_PlayState, InGameGUI.SymbolDef.PNL_PinkBeam.State_charge);
            }
            else
            {
                playAnimation(InGameGUI.SymbolDef.Root.ParamDef.PNL_Frula_Beam_PlayState, InGameGUI.SymbolDef.PNL_PinkBeam.State_DEFAULT);
            }

            #region 残り時間

            //Time = gameFlowManager.IngameTimer / 60;
            float timeFloat = _gameFlowManager.IngameTimer;
            int tens = (int)timeFloat / 10;
            int ones = (int)timeFloat % 10;
            ResetTime();

            //2桁目
            switch (tens)
            {
                case 0:
                    time_st2_0.Value = true;
                    break;
                case 1:
                    time_st2_1.Value = true;
                    break;
                case 2:
                    time_st2_2.Value = true;
                    break;
                case 3:
                    time_st2_3.Value = true;
                    break;
                case 4:
                    time_st2_4.Value = true;
                    break;
                case 5:
                    time_st2_5.Value = true;
                    break;
                case 6:
                    time_st2_6.Value = true;
                    break;
                case 7:
                    time_st2_7.Value = true;
                    break;
                case 8:
                    time_st2_8.Value = true;
                    break;
                case 9:
                    time_st2_9.Value = true;
                    break;
            }
            
            //1桁目
            switch (ones)
            {
                case 0:
                    time_st1_0.Value = true;
                    break;
                case 1:
                    time_st1_1.Value = true;
                    break;
                case 2:
                    time_st1_2.Value = true;
                    break;
                case 3:
                    time_st1_3.Value = true;
                    break;
                case 4:
                    time_st1_4.Value = true;
                    break;
                case 5:
                    time_st1_5.Value = true;
                    break;
                case 6:
                    time_st1_6.Value = true;
                    break;
                case 7:
                    time_st1_7.Value = true;
                    break;
                case 8:
                    time_st1_8.Value = true;
                    break;
                case 9:
                    time_st1_9.Value = true;
                    break;
            }
            #endregion

            #region ゲームオーバーUI

            bool _GameOver = _gameFlowManager.FinishDirection;

            if (_GameOver && GameOverTrg)
            {
                GameOverTrg = false;
                playAnimation(test_InGameUI.SymbolDef.Root.ParamDef.PNL_Terop_PlayState, test_InGameUI.SymbolDef.PNL_Terop.State_Finish);
            }

            #endregion

            //現在の値を最後に保存
            old_Sheena_magicpower = Sheena_magicpower;
            old_frula_magicpower = frula_magicpower;
            old_egi_HP = egi_HP;
            old_aari_HP = aari_HP;

        }
    }
}