//=============================================================================
// <summary>
// ゲームフローを管理するクラス
// </summary>
// <author> 菊池 雅道 </author>
//=============================================================================
using sound;
using System;
using System.Reflection;
using via;
using via.attribute;
using via.audiorender;
using via.dialog;
using via.gui;
using via.gui.asset;
using via.hid;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.GameFlowManager)]
    public class GameFlowManager_Work : SingletonRoot_Work<GameFlowManager_Work>
    {
        #region コンポーネント
        /// <summary>
        /// コンポーネント
        /// </summary>  
        private SoundPlayer cpSoundPlayer = null; //サウンドプレイヤーコンポーネント
        #endregion

        #region フィールド
        /// <summary>
        /// フィールド
        /// </summary>
        /// 
        [IgnoreDataMember, ReadOnly(true)]
        private int winner = (int)Team.None; //勝者

        [IgnoreDataMember]
        private bool pause = false; //ポーズフラグ

        [IgnoreDataMember]
        private bool gameOver = false; //ゲームオーバー

        [IgnoreDataMember, ReadOnly(true)]
        private GameState gameState = GameState.Title; //ゲームシーン状態

        private float titleTransitionTimer          = 0.0f;     //タイトルシーンの遷移時間
        private float gameStartTimer                = 0.0f;     //ゲーム開始時タイマー
        private float gameOverTimer                 = 0.0f;     //ゲームオーバ演出タイマー
        private float finishDirectionWaitTimer      = 0.0f;     //終了演出待機タイマー
        private float resultTransitionTimer         = 0.0f;     //終了演出からリザルトへの待機時間タイマー
        private float ingameTimer                   = 0.0f;     //インゲームの経過時間
        private float selectSuccessTimer            = 0.0f;     //セレクト画面で2人が選択した時のタイマー
    
        private GUI loadGUI = null;
        private GUI pauseGUI = null;
        private GUI resultGUI = null;

        private Load_GUIController load_GUIController = null;

        private int[] titleOption = new int[] {  (int)TitleOption.Battle, (int)TitleOption.Story_Rule, (int)TitleOption.Exit }; //タイトルシーンの選択肢
        private int selectInTitle = (int)TitleOption.Story_Rule;   //タイトルシーンで選択している内容

        private int[] resultOption = new int[] { (int)ResultOption.Select, (int)ResultOption.Title, (int)ResultOption.Exit }; //リザルトシーンの選択肢
        private int selectInResult = (int)ResultOption.Select;   //リザルトシーンで選択している内容

        private int[] pauseOption = new int[] { (int)PauseOption.Title, (int)PauseOption.Return, (int)PauseOption.Exit };   //ポーズ画面の選択肢
        private int selectInPause = (int)PauseOption.Return;    //ポーズシーンで選択している内容

        private int teamsCnt = GamePlayerManager_Work.maxPlayerNum; //チーム数
        private bool[] preDecide = new bool[] { false, false };     //前回の各プレイヤーのキャラクター決定状況
        private int[] preSelect = new int[] { 0, 0 };               //前回の各プレイヤーのキャラクター選択状況

        //ゲームオブジェクト
        GameObject egi = null;
        GameObject aari = null;
        GameObject sheena = null;
        GameObject frula = null;
        GameObject loseMonster = null;

        via.movie.Movie preGameMovie = null;
        via.movie.Movie ResultMovie = null;

        //召喚獣ヒットポイント
        private int egiHitPoint =0;     //イーギ
        private int aariHitPoint = 0;   //アーリ

        //フラグ関係
        private bool gameStart = false;         //インゲーム開始フラグ
        private bool finishDirection = false;   //終了演出フラグ
        private bool titleSelected = false;     //タイトル画面で選択を決定したフラグ
        private bool titleTransition = false;   //タイトル遷移フラグ
        private bool creditSelected = false;    //クレジット表示フラグ
        private bool pauseSelected = false;     //ボーズ画面で選択を決定したフラグ
        private bool resultSelected = false;    //リザルト画面で選択を決定したフラグ
        private bool resultSkip = false;        //リザルト画面で動画をスキップしたフラグ
        private bool playBgm = false;           //BGMフラグ

        /// <summary>
        /// シーンフォルダーのパス
        /// </summary>       
        private string titleFolderPath = "GameContents/Title/Title";
        private string storyFolderPath = "GameContents/Story/Story";
        private string selectFolderPath = "GameContents/Select/SelectCharacter";
        private string preGameFolderPath = "GameContents/PreGame/PreGame";
        private string inGameLocationFolderPath = "GameContents/InGame/Location";
        private string resultFolderPath = "GameContents/Result/Result";

        /// <summary>
        /// シーンフォルダー
        /// </summary>      
        private Folder titleFolder = null;
        private Folder storyFolder = null;
        private Folder selectFolder = null;
        private Folder preGameFolder = null;
        private Folder inGameLocationFolder = null;
        private Folder resultFolder = null;
        private Folder nowFolder = null;
        #endregion

        #region ユーザーデータ
        [DataMember]
        private TitleUserData_Work titleUserData = null;                 //タイトルシーン設定に関するユーザーデータ
        [DataMember]
        private SelectUserData_Work selectUserData = null;               //セレクトシーン設定に関するユーザーデータ
        [DataMember]
        private IngameUserData_Work ingameUserData = null;               //インゲーム設定に関するユーザーデータ
        [DataMember]
        private IngameCameraUserData_Work ingameCameraUserData = null;   //インゲームのカメラ設定に関するユーザーデータ
        #endregion

        #region 定数
        /// <summary>
        /// ゲームシーンの状態
        /// </summary>
        public enum GameState
        {
            Title,     // タイトル
            Story,     // ストーリー
            Select,    // キャラクター選択
            PreGame,   // 対戦前
            InGame,    // インゲーム
            Result,    // リザルト
        }

        // BGMの番号
        private enum Bgm
        {
            Title,  // タイトル
            InGame, // インゲーム
            Story,  // ストーリー
            Select, // キャラクター選択
            Result, // リザルト
            PreGame,
        }

        // タイトルシーンSE
        private enum TitleSe
        { 
            CrursorMove = 10,   //カーソル移動
            TitleSelect,
        }

        // ストーリーシーンSE
        private enum Story
        {
            CrursorMove = 20,   //決定        
        }

        // セレクトシーンSE
        private enum SelectSe
        {
            CrursorMove  = 30,   //カーソル移動
            Select,              //決定
            Cancele,             //キャンセル  
        }

        // インゲームシーンSE
        private enum InGameSe
        {
            Ready = 40,         //開始時（準備）
            Go,                 //開始時（スタート）
            Finigh,             //終了時
        }
        // リサルトシーンSE
        private enum ResultSe
        {
            CrursorMove = 50,   //カーソル移動
            Select,             //決定
            Jingle,             // 始まったときに流れるジングル
        }
        // ポーズシーンSE
        private enum PauseSe
        {
            CrursorMove = 60,   //カーソル移動
            Select,             //決定
        }
        //チーム
        private enum Team
        {
            SheenaAndEgi, // シーナ＆イーギ
            FrulaAndAari, // フルーラ＆アーリ
            None          // なし
        }

        //タイトルシーン選択肢
        private enum TitleOption
        {
            Battle,       // 対戦
            Story_Rule,   // ストーリー・ルール説明
            Exit,         // ゲーム終了
        }

        //タイトルシーン選択肢
        private enum ResultOption
        {
            Select,       // キャラクター選択
            Title,        // タイトル
            Exit,         // ゲーム終了
        }

        //ポーズ画面選択肢
        private enum PauseOption
        {           
            Title,        // タイトル
            Return,       // ゲームに戻る
            Exit,         // ゲーム終了
        }
        #endregion

        #region プロパティ

        /// <summary>
        /// ゲーム時間
        /// </summary>
        public float IngameTimer
        {
            get
            {
                return ingameTimer;
            }
        }

        /// <summary>
        /// ポーズ
        /// </summary>
        public bool Pause
        {
            get
            {
                return pause;
            }
        }
        /// <summary>
        /// ポーズの選択肢
        /// </summary>
        public int SelectInPause
        {
            get
            {
                return selectInPause;
            }
        }

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public bool GameStart
        {
            get
            {
                return gameStart;
            }
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public bool GameOver
        {
            get
            {
                return gameOver;
            }
        }

        /// <summary>
        /// 終了演出
        /// </summary>
        public bool FinishDirection
        {
            get
            {
                return finishDirection;
            }
        }

        /// <summary>
        /// 勝者
        /// </summary>
        public int Winner
        {
            get
            {
                return winner;
            }
        }

        /// <summary>
        /// タイトルの選択肢
        /// </summary>
        public int SelectInTitle
        {
            get
            {
                return selectInTitle;
            }
        }

        /// <summary>
        /// タイトルの終了
        /// </summary>
        public bool TitleTransition
        {
            get
            {
                return titleTransition;
            }
        }

        /// <summary>
        /// リザルトの選択肢
        /// </summary>
        public int SelectInResult
        {
            get
            {
                return selectInResult;
            }
        }

        /// <summary>
        /// リザルトの選択肢
        /// </summary>
        public bool CreditSelected
        {
            get
            {
                return creditSelected;
            }
        }
        #endregion
      
        #region ポーズ関係     
        //ポーズをかける
        [Action]
        private void setPause()
        {
            //Pausableタグがついているゲームオブジェクト、フォルダの更新処理を無効化
            SceneManager.MainScene.setUpdate("Pausable", false);

            //フラグを設定
            pause = true;
            pauseSelected = false;
            pauseGUI.Enabled = true;

            //初期値設定
            selectInPause = (int)PauseOption.Return;
        }

        //ポーズを解除
        [Action]
        private void cancelePause()
        {
            //Pausableタグがついているゲームオブジェクト、フォルダの更新処理を有効化
            SceneManager.MainScene.setUpdate("Pausable", true);

            pause = false;
            pauseGUI.Enabled = false;
        }

        //ポーズ選択肢
        private void PauseSelect()
        {
            //選択肢
            if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLleft) || GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LLeft))
            {
                //選択肢を入力
                selectInPause--;
                if (selectInPause < 0)
                {
                    selectInPause = 0;
                }
                else
                {
                    cpSoundPlayer._Sources[(int)PauseSe.CrursorMove].play();
                }
                selectInPause = titleOption[selectInPause];
             
            }
            else if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLright) || GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LRight))
            {
                //選択肢を入力
                int oldSelsect = selectInPause;
                selectInPause++;
                if (selectInPause >= Enum.GetValues(typeof(PauseOption)).Length)
                {
                    selectInPause = oldSelsect;
                }
                else
                {
                    cpSoundPlayer._Sources[(int)PauseSe.CrursorMove].play();
                }
                selectInPause = titleOption[selectInPause];           
            }

            //ボタンを押したら処理を行う
            if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown))
            {
                //多重処理防止
                if (pauseSelected == true)
                {
                    return;
                }
                else 
                { 
                    pauseSelected = true;
                }

                //選択SE      
                cpSoundPlayer._Sources[(int)PauseSe.Select].play();

                //選択に応じて処理
                switch (selectInPause)
                {
                    case (int)PauseOption.Title:

                        //タイトルへ移行
                        PauseTransitionTitle();
                        
                        break;

                    case (int)PauseOption.Return:

                        //ポーズ解除
                        cancelePause();
                       
                        break;

                    case (int)PauseOption.Exit:

                        //ゲーム終了
                        via.Application.exit(0);

                        break;
                }              
            }
        }

        //ポーズ画面からタイトルへの遷移
        private void PauseTransitionTitle()
        {
            //各シーンを終了
            switch (gameState)
            {
                //タイトルシーン
                case GameState.Title:

                    break;

                //ストーリーシーン
                case GameState.Story:
                    //ストーリーフォルダ非アクティブ
                    nowFolder = storyFolder;
                    storyFolder.deactivate();

                    //BGM終了
                    if (playBgm)
                    {
                        cpSoundPlayer._Sources[(int)Bgm.Story].stop();
                        playBgm = false;
                    }

                    //ロード画像を表示
                    load_GUIController.NoAnimation();
                    loadGUI.Enabled = true;

                    break;

                //キャラクター選択シーン
                case GameState.Select:

                    //キャラクター選択フォルダ非アクティブ
                    nowFolder = selectFolder;
                    selectFolder.deactivate();

                    //BGM終了
                    if (playBgm)
                    {
                        cpSoundPlayer._Sources[(int)Bgm.Select].stop();
                        playBgm = false;
                    }

                    //ロード画像を表示
                    load_GUIController.NoAnimation();
                    loadGUI.Enabled = true;

                    break;

                //対戦前シーン
                case GameState.PreGame:

                    //対戦前フォルダ非アクティブ
                    nowFolder = preGameFolder;
                    preGameFolder.deactivate();

                    //BGM終了
                    if (playBgm)
                    {
                        cpSoundPlayer._Sources[(int)Bgm.PreGame].stop();
                        playBgm = false;
                    }

                    //ロード画像を表示
                    load_GUIController.NoAnimation();
                    loadGUI.Enabled = true;

                    break;

                //インゲームシーン
                case GameState.InGame:

                    //ストーリーフォルダ非アクティブ
                    nowFolder = inGameLocationFolder;
                    inGameLocationFolder.deactivate();

                    //BGM終了
                    if (playBgm)
                    {
                        cpSoundPlayer._Sources[(int)Bgm.InGame].stop();
                        playBgm = false;
                    }

                    //ロード画像を表示
                    load_GUIController.NoAnimation();
                    loadGUI.Enabled = true;

                    break;

                //リザルトシーン
                case GameState.Result:

                    break;
            }

            //現在のシーンを終了し、タイトルへ
            if (nowFolder != null)
            {
                if (nowFolder.Activating == false)
                {
                    //ポーズ解除
                    cancelePause();

                    //タイトルシーンへ
                    gameState = GameState.Title;
                    initializeTitle();
                }
            }
        }

        #endregion

        public override void start()
        {
            base.start();

            //サウンドプレイヤー
            cpSoundPlayer = GameObject.getComponent<SoundPlayer>();

            //各シーンのフォルダを取得
            titleFolder = SceneManager.CurrentScene.findFolder(titleFolderPath);
            storyFolder = SceneManager.CurrentScene.findFolder(storyFolderPath);
            selectFolder = SceneManager.CurrentScene.findFolder(selectFolderPath);
            preGameFolder = SceneManager.CurrentScene.findFolder(preGameFolderPath);
            inGameLocationFolder = SceneManager.CurrentScene.findFolder(inGameLocationFolderPath);
            resultFolder = SceneManager.CurrentScene.findFolder(resultFolderPath);

            //ロード画像のコンポーネントを取得
            loadGUI = SceneManager.MainScene.findGameObject("LoadGUI").getComponent<GUI>();
            load_GUIController = SceneManager.MainScene.findGameObject("LoadGUI").getComponent<Load_GUIController>();
            loadGUI.Enabled = false;

            //ポーズGUIのコンポーネントを取得
            pauseGUI = SceneManager.MainScene.findGameObject("PauseGUI").getComponent<GUI>();
            pauseGUI.Enabled = false;
        }

        public override void update()
        {
            base.update();

            //各シーンのupdateを実行
            switch (gameState)
            {
                //タイトルシーン
                case GameState.Title:
                    updateTitle();
                    break;

                //ストーリーシーン
                case GameState.Story:
                    updateStory();
                    break;

                //キャラクター選択シーン
                case GameState.Select:
                    updateSelect();
                    break;
                
                //キャラクター選択シーン
                case GameState.PreGame:
                    updatePreGame();
                    break;

                //インゲームシーン
                case GameState.InGame:
                    updateInGame();
                    break;

                //リザルトシーン
                case GameState.Result:
                    updateResult();
                    break;
            }
        }

        #region Title
        /// <summary>
        /// タイトルフェーズ
        /// </summary>
        private enum TitlePhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            TITLE,
            WAIT_FADE_OUT,
            WAIT_DEACTIVATE,
            EXIT
        }

        [IgnoreDataMember, ReadOnly(true)]
        private TitlePhase titlePhase = TitlePhase.ACTIVATE;

        /// <summary>
        /// タイトル初期化
        /// </summary>
        private void initializeTitle()
        {
            titlePhase = TitlePhase.ACTIVATE;
            selectInTitle = titleOption[(int)TitleOption.Story_Rule];
        }

        /// <summary>
        /// タイトル
        ///　</summary>
        private void updateTitle()
        {
            switch (titlePhase)
            {
                //シーンをactivate
                case TitlePhase.ACTIVATE:

                    //タイトルフォルダをactivate
                    if (titleFolder != null)
                    {
                        titleFolder.activate();
                    }

                    //フェーズを進める
                    titlePhase = TitlePhase.WAIT_ACTIVATE;
                    break;

                //activate待ち
                case TitlePhase.WAIT_ACTIVATE:

                    //タイトルフォルダのactivate完了
                    if (titleFolder.Activating == false)
                    {
                        //ロード画像を非表示
                        loadGUI.Enabled = false;

                        if (titleFolder.Active)
                        {
                            //フェーズを進める
                            titlePhase = TitlePhase.TITLE;
                        }
                    }
                    break;

                //タイトルシーン
                case TitlePhase.TITLE:

                    //BGM再生
                    if (!playBgm)
                    {
                        playBgm = true;
                        cpSoundPlayer._Sources[(int)Bgm.Title].play();
                        cpSoundPlayer._Sources[(int)Bgm.Title].Loop = true;
                    }

                    //SEが鳴り終わったら、次のシーンへ遷移する
                    if (titleTransition)
                    {
                        if (titleTransitionTimer >= titleUserData.TilteTranslateTime)
                        {
                            //BGM終了
                            if (playBgm)
                            {
                                cpSoundPlayer._Sources[(int)Bgm.Title].stop();
                                playBgm = false;
                            }

                            //タイトルフォルダ非アクティブ
                            titleFolder.deactivate();
                            //フェーズを進める
                            titlePhase = TitlePhase.WAIT_DEACTIVATE;
                        }

                        //シーン遷移時間加算
                        titleTransitionTimer += Application.ElapsedSecond;
                    }

                    //クレジット状態解除
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight) && creditSelected)
                    {
                        titleSelected = false;
                        creditSelected = false;
                        return;
                    }

                    //選択が確定したら処理をしない
                    if (titleSelected == true)
                    { 
                        return;
                    }

                    //選択肢
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLleft)|| GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LLeft))
                    {
                        //選択肢を入力
                        selectInTitle--;                    
                        if (selectInTitle < 0)
                        {
                            selectInTitle = 0;
                        }
                        else
                        {
                            //SE
                            cpSoundPlayer._Sources[(int)TitleSe.CrursorMove].play();
                        }
                        selectInTitle = titleOption[selectInTitle];
                    }
                    else if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLright) || GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LRight))
                    {
                        //選択肢を入力
                        int oldSelsect = selectInTitle;
                        selectInTitle++;
                        if (selectInTitle >= Enum.GetValues(typeof(TitleOption)).Length)
                        {
                            selectInTitle = oldSelsect;
                        }
                        else
                        {
                            //SE
                            cpSoundPlayer._Sources[(int)TitleSe.CrursorMove].play();
                        }
                        selectInTitle = titleOption[selectInTitle];
                    }

                    //クレジット決定
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight))
                    {
                        titleSelected = true;
                        creditSelected = true;
                    }


                    //ボタンを押したら次のシーンへ
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown))
                    {
                        //SE
                        cpSoundPlayer._Sources[(int)TitleSe.TitleSelect].play();

                        //シーン遷移フラグ設定
                        titleTransition = true;

                        //選択肢決定フラグ設定
                        titleSelected = true;

                    }
                   
                    break;

                //deactivate待ち
                case TitlePhase.WAIT_DEACTIVATE:

                    //タイトルフォルダ非アクティブ完了
                    if (titleFolder.Activating == false)
                    {
                        //フラグリセット
                        titleTransition = false;
                        titleSelected = false;

                        //ロード画像を表示
                        load_GUIController.NoAnimation();
                        loadGUI.Enabled = true;
                        titleTransitionTimer = 0.0f;
                        //フェーズを進める
                        titlePhase = TitlePhase.EXIT;                                                                  
                    }
                
                    break;

                //シーン終了
                case TitlePhase.EXIT:

                    switch (selectInTitle)
                    {
                        case (int)TitleOption.Battle:

                            //キャラクター選択シーンへ
                            gameState = GameState.Select;
                            initializeSelect();

                            break;

                        case (int)TitleOption.Story_Rule:

                            //ストーリー・ルールシーンへ
                            gameState = GameState.Story;
                            initializeStory();

                            break;

                        case (int)TitleOption.Exit:

                            //ゲーム終了
                            via.Application.exit(0);

                            break;
                    }

                    break;
            }

        }
        #endregion

        #region Story
        /// <summary>
        /// ストーリーフェーズ
        /// </summary>
        private enum StoryPhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            STORY,
            WAIT_FADE_OUT,
            WAIT_DEACTIVATE,
            EXIT
        }

        [IgnoreDataMember, ReadOnly(true)]
        private StoryPhase storyPhase = StoryPhase.ACTIVATE;

        /// <summary>
        /// ストーリー初期化
        /// </summary>
        private void initializeStory()
        {
            storyPhase = StoryPhase.ACTIVATE;
        }

        /// <summary>
        /// ストーリー
        ///　</summary>
        private void updateStory()
        {
            switch (storyPhase)
            {
                //シーンをactivate
                case StoryPhase.ACTIVATE:

                    //ストーリーフォルダをactivate
                    if (storyFolder != null)
                    {
                        storyFolder.activate();
                    }

                    //フェーズを進める
                    storyPhase = StoryPhase.WAIT_ACTIVATE;
                    break;

                //activate待ち
                case StoryPhase.WAIT_ACTIVATE:

                    //ストーリーフォルダのactivate完了
                    if (storyFolder.Activating == false)
                    {
                        if (storyFolder.Active)
                        {
                            //ロード画像を非表示
                            loadGUI.Enabled = false;
                            //フェーズを進める
                            storyPhase = StoryPhase.STORY;
                        }
                    }
                    break;

                //ストーリーシーン
                case StoryPhase.STORY:

                    //BGM再生
                    if (!playBgm)
                    {
                        playBgm = true;
                        cpSoundPlayer._Sources[(int)Bgm.Story].play();
                        cpSoundPlayer._Sources[(int)Bgm.Story].Loop = true;
                    }

                    //ポーズ
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight))
                    {
                        if (!pause)
                        {
                            setPause();
                        }
                        else 
                        {
                            cancelePause();
                        }
                    }
                    //ポーズ画面選択肢
                    if (pause)
                    {
                        PauseSelect();           
                    }                

                    //ボタンを押したらページを進める。最後まで再生したら次のシーンへ
                    bool nextstep = SceneManager.MainScene.findGameObject("story_GUIController").getComponent<StoryGUI_GUIController>().NextStep;
                    if (nextstep)
                    {
                        //BGM終了
                        if (playBgm)
                        {
                            cpSoundPlayer._Sources[(int)Bgm.Story].stop();
                            playBgm = false;
                        }
                        //ストーリーフォルダ非アクティブ
                        storyFolder.deactivate();
                        //ロード画像を表示
                        load_GUIController.NoAnimation();
                        loadGUI.Enabled = true;
                        //フェーズを進める
                        storyPhase = StoryPhase.WAIT_DEACTIVATE;
                    }
                    break;

                //deactivate待ち
                case StoryPhase.WAIT_DEACTIVATE:

                    //ストーリーフォルダ非アクティブ完了
                    if (storyFolder.Activating == false)
                    {
                        //フェーズを進める
                        storyPhase = StoryPhase.EXIT;
                    }
                    break;

                //シーン終了
                case StoryPhase.EXIT:

                    //キャラクター選択シーンへ
                    gameState = GameState.Select;
                    initializeSelect();

                    break;
            }

        }
        #endregion

        #region Select
        /// <summary>
        /// キャラクター選択フェーズ
        /// </summary>
        private enum SelectPhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            SELECT,
            WAIT_DEACTIVATE,
            EXIT
        }

        [IgnoreDataMember, ReadOnly(true)]
        private SelectPhase selectPhase = SelectPhase.ACTIVATE;

        /// <summary>
        /// キャラクター選択初期化
        /// </summary>
        private void initializeSelect()
        {
            selectPhase = SelectPhase.ACTIVATE;

            selectSuccessTimer = 0;

            for (int i = 0; i < teamsCnt; i++)
            {
                //前回の各プレイヤーの選択・決定状況の初期化
                preDecide[i] = false;
                preSelect[i] = 0;
            }
        }

        private void updateSelect()
        {
            switch (selectPhase)
            {
                //シーンをactivate
                case SelectPhase.ACTIVATE:
                    //キャラクター選択シーンActivate
                    if (selectFolder != null)
                    {
                        selectFolder.activate();
                    }

                    //フェーズを進める
                    selectPhase = SelectPhase.WAIT_ACTIVATE;
                    break;

                //activate待ち
                case SelectPhase.WAIT_ACTIVATE:
                    //タイトルフォルダのactivate完了
                    if (selectFolder.Activating == false)
                    {
                        if (selectFolder.Active)
                        {
                            //ロード画像を非表示
                            loadGUI.Enabled = false;
                            //フェーズを進める
                            selectPhase = SelectPhase.SELECT;

                            preDecide = new bool[] { false, false };     //前回の各プレイヤーのキャラクター決定状況
                        }
                    }
                    break;

                //キャラクター選択シーン
                case SelectPhase.SELECT:
                    
                    //BGM再生
                    if (!playBgm)
                    {
                        playBgm = true;
                        cpSoundPlayer._Sources[(int)Bgm.Select].play();
                        cpSoundPlayer._Sources[(int)Bgm.Select].Loop = true;
                    }          

                    //各プレイヤーの選択状況
                    bool[] nowDecide = new bool[]
                    {
                        SceneManager.MainScene.findGameObject("SelectCharacter").getComponent<SelectCharacterManager_Work>().Deide1P,
                        SceneManager.MainScene.findGameObject("SelectCharacter").getComponent<SelectCharacterManager_Work>().Deide2P
                    };

                    int[] nowSelect = new int[]
                    {
                        SceneManager.MainScene.findGameObject("SelectCharacter").getComponent<SelectCharacterManager_Work>().Select1P,
                        SceneManager.MainScene.findGameObject("SelectCharacter").getComponent<SelectCharacterManager_Work>().Select2P
                    };

                    for (int i = 0; i < teamsCnt; i++)
                    {
                        if (nowSelect[i] != preSelect[i])
                        {
                            //キャラクター選択SE再生
                            cpSoundPlayer._Sources[(int)SelectSe.CrursorMove].play();
                        }
                        
                        if (nowDecide[i] && !preDecide[i])
                        {
                            //キャラクター選択決定SE再生
                            cpSoundPlayer._Sources[(int)SelectSe.Select].play();
                        }

                        if (!nowDecide[i] && preDecide[i])
                        {
                            //キャラクター選択解除SE再生
                            cpSoundPlayer._Sources[(int)SelectSe.Cancele].play();
                        }
                    }

                    for (int i = 0; i < teamsCnt; i++)
                    {
                        //前回の各プレイヤーの選択・決定状況の更新
                        preDecide[i] = nowDecide[i];
                        preSelect[i] = nowSelect[i];
                    }

                    //全プレイヤーがキャラクターを決定したか確認
                    if (nowDecide[0] && nowDecide[1] && !pause)
                    {
                        //ボタンを押したら次のシーンへ
                        if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown) || selectSuccessTimer >= selectUserData.AutoSelectSkip)
                        {
                            //キャラクター選択フォルダ非アクティブ
                            selectFolder.deactivate();
                            //フェーズを進める
                            selectPhase = SelectPhase.WAIT_DEACTIVATE;

                            //ロード画像を表示
                            load_GUIController.NoAnimation();
                            loadGUI.Enabled = true;
                        }

                        //シーン遷移時間加算
                        selectSuccessTimer += Application.ElapsedSecond;
                    }
                    else
                    {
                        selectSuccessTimer = 0;
                    }
                    
                    //ポーズ
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight))
                    {
                        if (!pause)
                        {
                            setPause();
                        }
                        else
                        {
                            cancelePause();
                        }
                    }
                    //ポーズ画面選択肢
                    if (pause)
                    {
                        PauseSelect();
                    }

                    break;

                //deactivate待ち
                case SelectPhase.WAIT_DEACTIVATE:
                    //キャラクター選択フォルダ非アクティブ完了
                    if (selectFolder.Activating == false)
                    {
                        //BGM終了
                        if (playBgm)
                        {
                            cpSoundPlayer._Sources[(int)Bgm.Select].stop();
                            playBgm = false;
                        }

                        //フェーズを進める
                        selectPhase = SelectPhase.EXIT;
                    }
                    break;

                //シーン終了
                case SelectPhase.EXIT:
                    //対戦前シーンへ
                    gameState = GameState.PreGame;
                    initializePreGame();
                    break;
            }
        }
        #endregion

        #region  PreGame
        /// <summary>
        /// 対戦前フェーズ
        /// </summary>
        private enum PreGamePhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            PREGAME,
            WAIT_FADE_OUT,
            WAIT_DEACTIVATE,
            EXIT
        }

        [IgnoreDataMember, ReadOnly(true)]
        private PreGamePhase preGamePhase = PreGamePhase.ACTIVATE;

        /// <summary>
        /// 対戦前初期化
        /// </summary>
        private void initializePreGame()
        {
            preGamePhase = PreGamePhase.ACTIVATE;
        }

        /// <summary>
        /// 対戦前
        ///　</summary>
        private void updatePreGame()
        {
            switch (preGamePhase)
            {
                //シーンをactivate
                case PreGamePhase.ACTIVATE:

                    //対戦前フォルダをactivate
                    if (preGameFolder != null)
                    {
                        preGameFolder.activate();
                    }

                    //フェーズを進める
                    preGamePhase = PreGamePhase.WAIT_ACTIVATE;
                    break;

                //activate待ち
                case PreGamePhase.WAIT_ACTIVATE:

                    //対戦前フォルダのactivate完了
                    if (preGameFolder.Activating == false)
                    {
                        if (preGameFolder.Active)
                        {
                            preGameMovie = SceneManager.MainScene.findGameObject("Movie").getComponent<via.movie.Movie>();
                            preGameMovie.reset();
                            preGameMovie.fillTexture();
                            if (preGameMovie.State == via.movie.Movie.CosmeticState.Ready)
                            {
                                //フェーズを進める
                                preGamePhase = PreGamePhase.PREGAME;
                            }
                        }
                    }
                    break;

                //対戦前シーン
                case PreGamePhase.PREGAME:

                    //ボタンを押したら次のシーンへ
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown) && pause == false)
                    {
                        //ロード画像を表示
                        load_GUIController.Animation();
                        loadGUI.Enabled = true;

                        //試合前フォルダ非アクティブ
                        preGameFolder.deactivate();
                        //フェーズを進める
                        preGamePhase = PreGamePhase.WAIT_DEACTIVATE;
                    }

                    //ポーズ
                    if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight))
                    {
                        if (!pause)
                        {
                            preGameMovie.pause();
                            cpSoundPlayer._Sources[(int)Bgm.PreGame].pause();
                            setPause();
                        }
                        else
                        {
                            cancelePause();
                        }
                    }
                    //ポーズ解除時音楽と動画の再生再開
                    if (preGameMovie.State == via.movie.Movie.CosmeticState.Paused && !pause)
                    {
                        preGameMovie.play();
                        cpSoundPlayer._Sources[(int)Bgm.PreGame].play();
                    }
                    //ポーズ画面選択肢
                    if (pause)
                    {
                        PauseSelect();
                    }

                    //試合前ムービーを再生し、終了したらインゲームへ
                    if (preGameMovie.State == via.movie.Movie.CosmeticState.Ready && preGameMovie.State != via.movie.Movie.CosmeticState.Preparing)
                    {
                        //ロード画像を非表示
                        loadGUI.Enabled = false;
                        //BGM再生
                        if (!playBgm)
                        {
                            playBgm = true;
                            cpSoundPlayer._Sources[(int)Bgm.PreGame].play();
                        }
                        preGameMovie.play();
                    }
                    else if (preGameMovie.State == via.movie.Movie.CosmeticState.Finished)
                    {
                        preGameMovie.reset();
                        preGameMovie.fillTexture();
                        //ロード画像を表示
                        load_GUIController.Animation();
                        loadGUI.Enabled = true;
                        //試合前フォルダ非アクティブ
                        preGameFolder.deactivate();
                        //フェーズを進める
                        preGamePhase = PreGamePhase.WAIT_DEACTIVATE;
                    }
                    
                    break;

                //deactivate待ち
                case PreGamePhase.WAIT_DEACTIVATE:

                    //対戦前フォルダ非アクティブ完了
                    if (preGameFolder.Activating == false)
                    {
                        //BGM終了
                        if (playBgm)
                        {
                            cpSoundPlayer._Sources[(int)Bgm.PreGame].stop();
                            playBgm = false;
                        }
                        //フェーズを進める
                        preGamePhase = PreGamePhase.EXIT;
                    }
                    break;

                //シーン終了
                case PreGamePhase.EXIT:

                    //インゲームシーンへ
                    gameState = GameState.InGame;
                    initializeInGame();

                    break;
            }

        }
        #endregion
       
        #region InGame
        /// <summary>
        /// インゲームフェーズ
        /// </summary>
        private enum InGamePhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            INGAME,
            PAUSE,
            GAME_OVER,
            WAIT_DEACTIVATE,
            EXIT
        }

        /// <summary>
        /// インゲームフェーズ
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        private InGamePhase inGamePhase = InGamePhase.ACTIVATE;
     
        //Game Over状態にする
        [Action]
        public void setGameOver()
        {
            if (gameOver == false)
            {
                gameOver = true;
            }
        }

        //Game Over状態をリセット
        public void resetGameOver()
        {
            if (gameOver == true)
            {
                gameOver = false;
            }
        }

        /// <summary>
        /// インゲーム初期化
        /// </summary>
        private void initializeInGame()
        {
            //勝者・敗者情報の初期化
            winner = (int)Team.None;
            loseMonster = null;

            //ゲームオーバー状態の初期化
            resetGameOver();

            //終了演出の初期化
            finishDirection = false;

            //ゲーム開始フラグの初期化
            gameStart = false;

            //タイマー初期化
            ingameTimer = ingameUserData.GameTime;
            gameStartTimer = 0.0f;
            gameOverTimer = 0.0f;
            finishDirectionWaitTimer = 0.0f;
            resultTransitionTimer = 0.0f;
        
            //カメラ初期化
            CameraManager_Work.CameraParam param = new CameraManager_Work.CameraParam();
            CameraManager_Work.Instance.getCurrentCameraParam(out param);
            param.position = ingameCameraUserData.IngameCameStartPosition;
            param.rotation = ingameCameraUserData.IngameCameStartRotation;
            CameraManager_Work.Instance.setCameraParam(param);

            egiHitPoint = 0;
            aariHitPoint = 0;

            //フェーズを進める
            inGamePhase = InGamePhase.ACTIVATE;

            //入力を受け付けない
            GamePlayerManager_Work.Instance.IsInput = false;
        }
        /// <summary>
        /// インゲーム
        /// </summary>
        private void updateInGame()
        {
            switch (inGamePhase)
            {
                //シーンをactivate
                case InGamePhase.ACTIVATE:

                    //インゲームフォルダをactivate
                    if (inGameLocationFolder != null)
                    {
                        inGameLocationFolder.activate();
                    }

                    //フェーズを進める
                    inGamePhase = InGamePhase.WAIT_ACTIVATE;
                    break;
                //activate待ち
                case InGamePhase.WAIT_ACTIVATE:

                    //インゲームフォルダのactivate完了
                    if (inGameLocationFolder.Activating == false)
                    {
                        //ロード画像を非表示
                        loadGUI.Enabled = false;

                        //プレイヤーのゲームオブジェクトを取得
                        sheena = SceneManager.MainScene.findGameObject("Sheena");
                        frula = SceneManager.MainScene.findGameObject("Frula");
                        egi = SceneManager.MainScene.findGameObject("Egi");
                        aari = SceneManager.MainScene.findGameObject("Aari");
                        
                        //フェーズを進める
                        inGamePhase = InGamePhase.INGAME;
                    }
                    break;
                //インゲーム
                case InGamePhase.INGAME:

                    float oldStartTimer = gameStartTimer;

                    //ゲーム開始演出のタイマーが０の時、SEを鳴らす
                    if (Math.Truncate(gameStartTimer) == 0 && gameStartTimer % 1.0f== 0)
                    {
                        //準備SE
                        cpSoundPlayer._Sources[(int)InGameSe.Ready].play();
                    }

                    //ゲーム開始演出のタイマーを進める
                    if (!gameOver)
                    {
                        gameStartTimer += Application.ElapsedSecond;
                    }

                    //経過時間に応じて開始演出を進める
                    if (gameStartTimer < ingameUserData.StartTransitionTime)
                    {                           
                        if (Math.Truncate(gameStartTimer) == Math.Truncate(ingameUserData.StartGoTime) && Math.Truncate(gameStartTimer) != Math.Truncate(oldStartTimer))
                        {
                            //開始SE
                            cpSoundPlayer._Sources[(int)InGameSe.Go].play();
                        }
                    }
                    else if (Math.Truncate(gameStartTimer) == Math.Truncate(ingameUserData.StartTransitionTime) && Math.Truncate(gameStartTimer) != Math.Truncate(oldStartTimer))
                    {                     
                        //入力を受け付ける
                        GamePlayerManager_Work.Instance.IsInput = true;

                        //ゲーム開始フラグ有効
                        gameStart = true;

                        //BGM再生
                        if (!playBgm)
                        {
                            playBgm = true;
                            cpSoundPlayer._Sources[(int)Bgm.InGame].play();
                            cpSoundPlayer._Sources[(int)Bgm.InGame].Loop = true;
                        }
                    }
                    else 
                    {
                        //開始演出が終了したら、ポーズ可能になる
                        //ポーズ
                        if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.CRight))
                        {
                            if (!pause)
                            {
                                setPause();
                            }
                            else
                            {
                                cancelePause();
                            }
                        }
                        //ポーズ画面選択肢
                        if (pause)
                        {
                            PauseSelect();
                        }
                    }

                    float oldGameTimer = ingameTimer;
                    //ゲーム時間のカウントダウン
                    if (pause == false && gameOver == false　&& gameStart)
                    {
                        ingameTimer -= Application.ElapsedSecond;
                    }

                    //召喚獣のヒットポイントを取得
                    int oldEgiHitPoint = egiHitPoint;
                    int oldAarihitPoint = aariHitPoint;

                    //召喚獣のヒットポイントを取得
                    egiHitPoint = egi.getComponent<Monster_Work>().HitPoint;
                    aariHitPoint = aari.getComponent<Monster_Work>().HitPoint;

                    //召喚獣ダメージ時カメラ振動
                    if (oldEgiHitPoint > egiHitPoint || oldAarihitPoint > aariHitPoint)
                    {                       
                        CameraManager_Work.Instance.startCameraShack();
                    }

                    //召喚獣のヒットポイントがなくなったら、勝敗設定とゲームオーバー処理を行う
                    if (egiHitPoint <= 0 && aariHitPoint > 0)
                    {
                        winner = (int)Team.FrulaAndAari;

                        setGameOver();

                        loseMonster = egi;

                    }
                    else if (aariHitPoint <= 0 && egiHitPoint > 0)
                    {
                        winner = (int)Team.SheenaAndEgi;

                        setGameOver();

                        loseMonster = aari;
                    }
                    else if(egiHitPoint <= 0 && aariHitPoint <= 0)
                    {
                        winner = (int)Team.None;

                        setGameOver();

                        loseMonster = null;
                    }

                    //ゲーム時間が無くなったらゲームオーバー
                    if (ingameTimer <= 0.0f && gameOver == false)
                    {
                        //ヒットポイントに応じて勝者判定を行う
                        if (aariHitPoint > egiHitPoint)
                        {
                            winner = (int)Team.FrulaAndAari;
                        }
                        else if (egiHitPoint > aariHitPoint)
                        {
                            winner = (int)Team.SheenaAndEgi;
                        }
                        else
                        {
                            winner = (int)Team.None;
                        }

                        setGameOver();
                    }

                    //ゲームオーバーかBボタンでリザルトへ
                    if (gameOver)
                    {
                        //時間切れの場合、終了演出
                        if (ingameTimer <= 0.0f)
                        {
                            if (finishDirection == false)
                            {
                                //終了演出
                                finishDirection = true;
                                //終了SE
                                cpSoundPlayer._Sources[(int)InGameSe.Finigh].play();
                            }
                           
                            finishDirectionWaitTimer += Application.ElapsedSecond;
                        }
                       
                        gameOverTimer += Application.ElapsedSecond;

                        //ゲームオーバー時、カメラ演出を行う　一定時間経過したら終了演出を出す
                        if (gameOverTimer > ingameUserData.GameOverTime)
                        {

                            //終了演出時間加算
                            finishDirectionWaitTimer += Application.ElapsedSecond;

                            //終了演出
                            if (finishDirection == false)
                            {
                                //終了演出
                                finishDirection = true;
                                //終了SE
                                cpSoundPlayer._Sources[(int)InGameSe.Finigh].play();
                            }
                        }
                        else
                        {
                            //負けた召喚獣にカメラを寄せる
                            if (loseMonster != null)
                            {
                                vec3 targetPosition = loseMonster.getComponent<Transform>().Position;
                                targetPosition = targetPosition + ingameCameraUserData.TargetPositionOffset;
                                CameraManager_Work.Instance.moveCameraLerp(targetPosition, ingameCameraUserData.ZoomInInterpolationCoef);
                            }                     
                        }

                        //終了演出を出し、一定時間経過したらリザルトへ
                        if (finishDirectionWaitTimer > ingameUserData.FinishTransitionTime)
                        { 
                            //シーン遷移時間加算
                            resultTransitionTimer += Application.ElapsedSecond;

                            if (resultTransitionTimer > ingameUserData.ResultTransitionTime)
                            {
                                //フェーズを進める
                                inGamePhase = InGamePhase.GAME_OVER;
                            }
                        }
   
                        break;
                    }

                    break;

                //ゲームオーバー
                case InGamePhase.GAME_OVER:

                    //インゲームフォルダ終了
                    inGameLocationFolder.deactivate();

                    //フェーズを進める
                    inGamePhase = InGamePhase.WAIT_DEACTIVATE;

                    //ロード画像を表示
                    load_GUIController.NoAnimation();
                    loadGUI.Enabled = true;

                    break;

                //deactivate待ち
                case InGamePhase.WAIT_DEACTIVATE:

                    //インゲームフォルダのdeactivate完了
                    if (inGameLocationFolder.Activating == false)
                    {
                        //Pausableタグがついているゲームオブジェクト、フォルダの更新処理を有効化
                        SceneManager.MainScene.setUpdate("Pausable", true);

                        //BGM終了
                        if (playBgm)
                        {
                            cpSoundPlayer._Sources[(int)Bgm.InGame].stop();
                            playBgm = false;
                        }

                        //フェーズを進める
                        inGamePhase = InGamePhase.EXIT;
                    }

                    break;

                //シーン終了
                case InGamePhase.EXIT:

                    //リザルトシーンへ
                    gameState = GameState.Result;
                    initializeResult();

                    break;
            }
        }

        #endregion

        #region Result
        /// <summary>
        /// リザルトフェーズ
        /// </summary>
        private enum ResultPhase
        {
            ACTIVATE,
            WAIT_ACTIVATE,
            RESULT,
            WAIT_DEACTIVATE,
            EXIT
        }

        /// <summary>
        /// リザルトフェーズ
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        private ResultPhase resultPhase = ResultPhase.ACTIVATE;

        //リザルト選択肢
        private void ResultSelect()
        {
            //選択肢
            if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLup) || GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LUp))
            {
                //選択肢を入力
                selectInResult--;
                if (selectInResult < 0)
                {
                    selectInResult = 0;
                }
                else
                {
                    cpSoundPlayer._Sources[(int)ResultSe.CrursorMove].play();
                }
                selectInResult = resultOption[selectInResult];
            }
            else if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.EmuLdown) || GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.LDown))
            {
                //選択肢を入力
                int oldSelsect = selectInResult;
                selectInResult++;
                if (selectInResult >= Enum.GetValues(typeof(ResultOption)).Length)
                {
                    selectInResult = oldSelsect;
                }
                else
                {
                    cpSoundPlayer._Sources[(int)ResultSe.CrursorMove].play();
                }
                selectInResult = resultOption[selectInResult];
            }

            //ボタンを押したら処理を行う
            if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown))
            {
                //多重処理防止
                if (resultSelected == true)
                {
                    return;
                }
                else
                {
                    resultSelected = true;
                }

                //選択SE      
                cpSoundPlayer._Sources[(int)ResultSe.Select].play();

                //BGM終了
                if (playBgm)
                {
                    cpSoundPlayer._Sources[(int)Bgm.Result].stop();
                    playBgm = false;
                }
                //ジングル終了
                cpSoundPlayer._Sources[(int)ResultSe.Jingle].stop();
                //選択に応じて処理
                switch (selectInResult)
                {

                    case (int)ResultOption.Select:

                        //リザルトシーンフォルダをdeactivate
                        resultFolder.deactivate();

                        //ロード画像を表示
                        load_GUIController.NoAnimation();
                        loadGUI.Enabled = true;

                        break;

                    case (int)ResultOption.Title:

                        //リザルトシーンフォルダをdeactivate
                        resultFolder.deactivate();

                        //ロード画像を表示
                        load_GUIController.NoAnimation();
                        loadGUI.Enabled = true;

                        //フェーズを進める(タイトルへ移行)
                        resultPhase = ResultPhase.WAIT_DEACTIVATE;

                        break;

                    case (int)ResultOption.Exit:

                        //ゲーム終了
                        via.Application.exit(0);

                        break;
                }

                //キャラクター選択への移行
                if (selectInResult == (int)ResultOption.Select && resultSelected == true)
                {
                    //リザルトシーンフォルダのdeactivate完了
                    //キャラクター選択シーンへ
                    gameState = GameState.Select;
                    initializeSelect();                  
                }
            }
        }

        /// <summary>
        /// リザルト初期化
        /// </summary>
        private void initializeResult()
        {
            //フェーズを進める
            resultPhase = ResultPhase.ACTIVATE;
            
            //フラグリセット
            resultSelected = false;
            resultSkip = false;

            //初期値設定
            selectInResult = (int)ResultOption.Select;
        }
        /// <summary>
        /// リザルト
        /// </summary>
        private void updateResult()
        {
            switch (resultPhase)
            {
                //シーンをactivate
                case ResultPhase.ACTIVATE:

                    //リザルトシーンフォルダをactivate
                    if (resultFolder != null)
                    {
                        resultFolder.activate();
                    }

                    //フェーズを進める
                    resultPhase = ResultPhase.WAIT_ACTIVATE;

                    break;

                //activate待ち
                case ResultPhase.WAIT_ACTIVATE:

                    //リザルトシーンフォルダのactivate完了
                    if (resultFolder.Activating == false)
                    {
                        if (resultFolder.Active)
                        {
                            ResultMovie = null;
                            switch (winner)
                            {
                                case (int)Team.SheenaAndEgi:
                                    //青チーム勝ち
                                    ResultMovie = SceneManager.MainScene.findGameObject("MovieSheenaWin").getComponent<via.movie.Movie>();
                                    break;
                                case (int)Team.FrulaAndAari:
                                    //ピンクチーム勝ち
                                    ResultMovie = SceneManager.MainScene.findGameObject("MovieFrulaWin").getComponent<via.movie.Movie>();
                                    break;
                                case (int)Team.None:
                                    //引き分け
                                    ResultMovie = SceneManager.MainScene.findGameObject("MovieDraw").getComponent<via.movie.Movie>();
                                    break;
                            }

                            ResultMovie.reset();
                            ResultMovie.fillTexture();
                            if (ResultMovie.State == via.movie.Movie.CosmeticState.Ready)
                            {
                                //ロード画像を非表示
                                loadGUI.Enabled = false;
                                resultPhase = ResultPhase.RESULT;
                            }

                            resultGUI = SceneManager.MainScene.findGameObject("Result_GUIController").getComponent<GUI>();
                            resultGUI.Enabled = false;
                        }
                    }
                    break;

                //リザルト
                case ResultPhase.RESULT:

                    //勝利ムービーを再生し、終了したらリザルト処理へ
                    if (ResultMovie.State == via.movie.Movie.CosmeticState.Ready && ResultMovie.State != via.movie.Movie.CosmeticState.Preparing)
                    {
                        ResultMovie.play();
                        if(winner != (int)Team.None)
                        {
                            cpSoundPlayer._Sources[(int)ResultSe.Jingle].play();
                        }
                    }

                    //BGM再生
                    if ((ResultMovie.State == via.movie.Movie.CosmeticState.Finished || winner == (int)Team.None) && !playBgm)
                    {
                        playBgm = true;
                        cpSoundPlayer._Sources[(int)Bgm.Result].play();
                        cpSoundPlayer._Sources[(int)Bgm.Result].Loop = true;
                    }

                    if (ResultMovie.State == via.movie.Movie.CosmeticState.Finished || resultSkip)
                    {
                        //リザルト選択肢
                        ResultSelect();
                        resultGUI.Enabled = true;
                    }
                    else
                    {
                        //ボタンを押したらリザルト画面表示
                        if (GamePlayerManager_Work.Instance.isAnyPlayerButton(GamePadButton.RDown))
                        {
                            resultSkip = true;
                        }
                    }
                    break;

                //deactivate待ち
                case ResultPhase.WAIT_DEACTIVATE:

                    //リザルトシーンフォルダのdeactivate完了
                    if (resultFolder.Activating == false)
                    {
                        //フェーズを進める
                        resultPhase = ResultPhase.EXIT;
                    }

                    break;

                //シーン終了
                case ResultPhase.EXIT:

                    //タイトルシーンへ
                    gameState = GameState.Title;
                    initializeTitle();
                    break;
            }
        }
        #endregion
    }
}