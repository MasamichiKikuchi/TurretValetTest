//=============================================================================
// <summary>
// GamePadManager_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.hid;

namespace app
{
	public class GamePlayerManager_Work : SingletonRoot_Work<GamePlayerManager_Work>
	{
        #region フィールド
        // 最大ゲームプレイヤー数
        [IgnoreDataMember]
        public static int maxPlayerNum = 2;
        // ゲームプレイヤー
        [IgnoreDataMember]
		private Player[] playerList = new Player[maxPlayerNum];
        // デバイス数
        private int deviceNum = 0;
        // 入力受付状態
        private bool isInput = true;
        #endregion

        #region プロパティ
        /// <summary>
        /// 接続デバイスの取得
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        public int DeviceNum
        {
            get { return deviceNum; }
            set { deviceNum = value; }
        }

        /// <summary>
        /// 入力受付状態の取得
        /// </summary>
        [IgnoreDataMember, ReadOnly(true)]
        public bool IsInput
        {
            get { return isInput; }
            set { isInput = value; }
        }
        #endregion

        /// <summary>
        /// キャラクターの種類
        /// </summary>
        public enum Character
        {
            Witch,
            Monster,
            Sheena,
            Frula,
            Egi,
            Aari,
            None,
        };

        /// <summary>
        /// プレイヤー取得（操作キャラクターから指定）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Player getPlayer(string name)
        {
            foreach (Player player in playerList)
            {
                string chara = Enum.GetName(typeof(Character), player.Chara);
                if (chara == name)
                {
                    return player;
                }
                string witch = Enum.GetName(typeof(Character), player.Witch);
                if (witch == name)
                {
                    return player;
                }
                string monster = Enum.GetName(typeof(Character), player.Monster);
                if (monster == name)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// プレイヤー取得（プレイヤーIDから指定）
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Player getPlayer(int num)
        {
            foreach (Player player in playerList)
            {
                if((int)player.PlayerIndex == num)
                {
                    return player;
                }
            }
            return null;
        }

        public override void start()
		{
            //プレイヤー割り当て初期化
            for (int i = 0; i < maxPlayerNum; i++)
			{
				//操作可能人数分のゲームプレイヤーを確保
                playerList[i] = new Player((GamePlayerIndex)i);
			}

            //入力受付を可能状態に初期化
            isInput = true;
		}

        public override void onDestroy()
        {
            //プレイヤー割り当て解除・終了
            for (int i = 0; i < maxPlayerNum; i++)
			{
				//プレイヤーからゲームパッドの割り当てを解除
                GamePlayer.unassign(playerList[i].PlayerIndex);
                playerList[i].PlayerDeviceIndex = DeviceIndex.Invalid;
			}
        }

        public override void update()
		{
			//プレイヤーの割り当て確認
            checkPlayer();

            //プレイヤー割り当て
			assignPlayer();

            //プレイヤー入力更新
            updatePlayerInput();
		}

        #region 割り当て関連
        // プレイヤーの割り当て確認
        public void checkPlayer()
		{
            for (int i = 0; i < maxPlayerNum; i++)
			{
				// デバイスが未割り当てのプレイヤー
				if (playerList[i].isAssign())
				{
					continue;
				}

				// デバイスが割り当て済みのプレイヤー
				// 割り当てたデバイス
				var pad = GamePlayer.getPlayer(playerList[i].PlayerIndex).GamePad;
				// デバイスが切断された
				if(pad.DeviceType == DeviceType.Null)
				{
					// 割り当て解除
					GamePlayer.unassign(playerList[i].PlayerIndex);
                    playerList[i].PlayerDeviceIndex = DeviceIndex.Invalid;
				}
			}

        }

		// プレイヤーに割り当て
		private void assignPlayer()
		{
			for(int i = 0; i < maxPlayerNum; i++)
			{
                // プレイヤーにデバイスが割り当てられるを調べる
                if (playerList[i].isAssign())
				{
                    // 接続されたデバイスの１つに自動でプレイヤーを割り当てる
                    // 接続されたデバイスから１つ取得
                    GamePadDevice decideDevice = getAutoGamePad();

                    if (decideDevice != null)
					{
                        // プレイヤーに割り当てる
                        GamePlayer.assign(playerList[i].PlayerIndex, decideDevice);
                        // デバイスのDeviceIndexを取得
                        playerList[i].PlayerDeviceIndex = decideDevice.DeviceIndex;
                    }
				}
			}
		}

        // 決定ボタンが押されたデバイスの取得
        private GamePadDevice getDecideGamePad()
		{
            // 接続デバイス数
            deviceNum = GamePad.ConnectingDevices.Count;
            for (int i = 0; i < deviceNum; i++)
			{
                // デバイス
                GamePadDevice device = GamePad.ConnectingDevices[i];
                // 割り当て済みのデバイスか
                GamePlayerIndex assignedPlayerIndex = GamePlayer.findAssignedPlayerIndex(device.UserIndex, device.DeviceIndex);
                if (assignedPlayerIndex != GamePlayerIndex.Invalid)
                {
                    continue;
                }

                // 決定ボタン（RRight）が押された時、プレイヤーに割り当てる
                if (device.isTrigger(GamePadButton.Decide))
				{
					return device;
				}
            }

			// 全てのデバイスが割り当て済み、もしくは決定ボタンが押されなかった
			return null;
        }

        // 自動で接続されたデバイスから１つ取得
        private GamePadDevice getAutoGamePad()
        {
            // 接続デバイス数
            deviceNum = GamePad.ConnectingDevices.Count;
            for (int i = 0; i < deviceNum; i++)
            {
                // デバイス
                GamePadDevice device = GamePad.ConnectingDevices[i];
                // 割り当て済みのデバイスか
                GamePlayerIndex assignedPlayerIndex = GamePlayer.findAssignedPlayerIndex(device.UserIndex, device.DeviceIndex);
                if (assignedPlayerIndex != GamePlayerIndex.Invalid)
                {
                    continue;
                }

                // 未割り当てのデバイスを返す
                return device;
            }

            // 全てのデバイスが割り当て済み、もしくは決定ボタンが押されなかった
            return null;
        }

        //プレイヤーの入力更新
        private void updatePlayerInput()
        {
            foreach (Player player in playerList)
            {
                //入力受付が可能状態であれば、更新
                if (isInput)
                {
                    //各プレイヤーの入力更新
                    player.updateInputData();
                }
                else
                {
                    //入力受付が不可状態であれば、更新しない（クリア）
                    player.clear();
                }
            }
        }
        #endregion

        #region キャラクター設定
        // プレイヤーの操作するキャラクターの設定
        public void setCharacter(int playerId, int team)
        {
            foreach (Player player in playerList)
            {
                if ((int)player.PlayerIndex == playerId)
                {
                    if(team == 0)
                    {
                        player.Witch = Character.Sheena;
                        player.Monster = Character.Egi;
                    }
                    else if(team == 1)
                    {
                        player.Witch = Character.Frula;
                        player.Monster = Character.Aari;
                    }
                }
            }
        }

        //プレイヤーの操作するキャラクターの解除
        public void removeCharacter(int playerId)
        {
            foreach (Player player in playerList)
            {
                if ((int)player.PlayerIndex == playerId)
                {
                    player.Witch = Character.None;
                    player.Monster = Character.None;
                }
            }
        }
        #endregion

        #region 入力関連
        /// <summary>
        /// 指定ボタンの状態取得（任意のプレイヤー）
        /// </summary>
        /// <param name="gamePadButton"></param>
        /// <returns></returns>
        public bool isAnyPlayerButton(GamePadButton gamePadButton)
        {
            foreach(Player player in playerList)
            {
                //いずれか（任意）のプレイヤーが指定ボタンを押したか確認
                if((player.PadTrigger & gamePadButton) != 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        #endregion

        #region プレイヤー
        public class Player
        {
            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public Player() { }
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="player"></param>
            public Player(GamePlayerIndex player)
            {
                playerIndex = player;
                playerDeviceIndex = DeviceIndex.Invalid;
                chara = Character.None;
                witch = Character.None;
                monster = Character.None;
            }

            // プレイヤーは割り当て可能か
            public bool isAssign()
            {
                return !GamePlayer.getPlayerInfo(playerIndex).Assigned;
            }

            /// <summary>
            /// パッド入力処理
            /// </summary>
            public void updateInputData()
            {
                var gamePad = GamePlayer.getPlayer(playerIndex).GamePad;
                if(gamePad == null)
                {
                    // デバイスがない場合はクリア
                    clear();
                    return;
                }

                // パッド入力情報取得
                if (playerDeviceIndex != DeviceIndex.Invalid)
                {
                    axisL = gamePad.AxisL;
                    axisR = gamePad.AxisR;

                    padOn = gamePad.Button;
                    padTrigger = gamePad.ButtonDown;
                    padRelease = gamePad.ButtonUp;
                }
                else
                {
                    clear();
                }
            }

            /// <summary>
            /// 入力クリア
            /// </summary>
            public void clear()
            {
                axisL = vec2.Zero;
                axisR = vec2.Zero;
                padOn = 0;
                padTrigger = 0;
                padRelease = 0;
            }

            /// <summary>
            /// プレイヤー情報
            /// </summary>
            #region フィールド
            private GamePlayerIndex playerIndex;    // プレイヤーのPlayerIndex（1P,2P,3P,4P）
            private DeviceIndex playerDeviceIndex;  // プレイヤーに割り当てられたゲームパッドのDeviceIndex
            // プレイヤーの操作するキャラクター
            private Character chara;                // 1人1コントローラー用
            private Character witch;                // 2人１コントローラー用（魔法少女）
            private Character monster;              // 2人１コントローラー用（召喚獣）
            #endregion

            #region プロパティ
            // プレイヤーのPlayerIndexの取得
            [IgnoreDataMember, ReadOnly(true)]
            public GamePlayerIndex PlayerIndex
            {
                get { return playerIndex; }
            }

            // プレイヤーに割り当てられたゲームパッドのDeviceIndexの取得、設定
            [IgnoreDataMember, ReadOnly(true)]
            public DeviceIndex PlayerDeviceIndex
            {
                get { return playerDeviceIndex; }
                set { playerDeviceIndex = value; }
            }

            // プレイヤーの操作するキャラクターの取得、設定
            [IgnoreDataMember, ReadOnly(true)]
            public Character Chara
            {
                get { return chara; }
                set { chara = value; }
            }
            [IgnoreDataMember, ReadOnly(true)]
            public Character Witch
            {
                get { return witch; }
                set { witch = value; }
            }
            [IgnoreDataMember, ReadOnly(true)]
            public Character Monster
            {
                get { return monster; }
                set { monster = value; }
            }
            #endregion

            /// <summary>
            /// パッド情報
            /// </summary>
            #region フィールド
            private bool padConnect = false;
            private GamePadButton padOn = 0;
            private GamePadButton padTrigger = 0;
            private GamePadButton padRelease = 0;
            private vec2 axisL = new vec2();
            private vec2 axisR = new vec2();
            #endregion

            #region プロパティ
            [IgnoreDataMember, ReadOnly(true)]
            public bool PadConnect
            {
                get
                {
                    return padConnect;
                }
            }

            [IgnoreDataMember, ReadOnly(true)]
            public GamePadButton PadOn
            {
                get
                {
                    return padOn;
                }
            }

            [IgnoreDataMember, ReadOnly(true)]
            public GamePadButton PadTrigger
            {
                get
                {
                    return padTrigger;
                }
            }

            [IgnoreDataMember, ReadOnly(true)]
            public GamePadButton PadRelease
            {
                get
                {
                    return padRelease;
                }
            }

            [IgnoreDataMember, ReadOnly(true)]
            public vec2 AxisL
            {
                get
                {
                    return axisL;
                }
            }

            [IgnoreDataMember, ReadOnly(true)]
            public vec2 AxisR
            {
                get
                {
                    return axisR;
                }
            }
            #endregion
        }
        #endregion
    }
}
