//=============================================================================
// <summary>
// SelectCharacterManager_Work 
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
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.SelectCharacter)]
    public class SelectCharacterManager_Work : via.Behavior
	{
        #region フィールド
        /// <summary>
		/// チーム数
		/// </summary>
		public PlayerTeam[] teams = new PlayerTeam[GamePlayerManager_Work.maxPlayerNum];

        /// <summary>
        /// 入力情報
        /// </summary>
        private GamePlayerManager_Work.Player inputData = null;
        #endregion

		/// <summary>
		/// チームの種類
		/// </summary>
		public enum Team
		{
			SheenaEgi,
			FrulaAari,
		}

		#region プロパティ
		//1Pの選択チームの取得
		public int Select1P
		{
			get { return (int)teams[0].Select; }
		}
		//2Pの選択チームの取得
		public int Select2P
		{
			get { return (int)teams[1].Select; }
		}
		//1Pの決定状態の取得
		public bool Deide1P
		{
			get { return teams[0].Decide; }
		}
		//2Pの決定状態の取得
		public bool Deide2P
		{
			get { return teams[1].Decide; }
		}
        #endregion

		public override void start()
		{
			base.start();
		}

		public override void update()
		{
			for(int i = 0; i < GamePlayerManager_Work.maxPlayerNum; i++)
			{
				inputData = GamePlayerManager_Work.Instance.getPlayer(i);

				//チーム選択
				selectTeam(i);

				//チーム決定
				decideTeam(i);
			}
		}

        #region チーム選択
        //チーム選択
        private void selectTeam(int playerId)
		{
            //選択チーム（位置）の取得
			int pos = (int)teams[playerId].Select;
			int prePos = pos;

			//決定しているか確認
			if (!teams[playerId].Decide)
			{
                //右移動か確認
                if (isRight())
                {
                    //右に移動
                    pos++;
                    if (pos >= Enum.GetValues(typeof(Team)).Length)
                    {
                        pos = prePos;
                    }
                }

                //左移動か確認
				if (isLeft())
                {
                    //左に移動
                    pos--;
                    if (pos < 0)
                    {
                        pos = prePos;
                    }
                }

                //選択してるチームが前と異なるか確認
                if (pos != prePos)
                {
                    //選択後のチームに変更
                    teams[playerId].Select = (Team)Enum.ToObject(typeof(Team), pos);
                }
            }
        }

		//チーム決定
		private void decideTeam(int playerId)
		{
			//決定ボタンを押したか
			if(isDecide() && !teams[playerId].Decide)
			{
                for (int i = 0; i < GamePlayerManager_Work.maxPlayerNum; i++)
				{
					if(playerId != i)
					{
						//他のプレイヤーチームが同じチームを決定していないか確認
						if (!((teams[playerId].Select == teams[i].Select) && teams[i].Decide))
						{
							//決定状態に変更
							teams[playerId].Decide = true;

							//プレイヤーにキャラクターの設定
							GamePlayerManager_Work.Instance.setCharacter(playerId, (int)teams[playerId].Select);
                        }
					}
				}
			}

			//キャンセルボタンを押したか
			if(isCancel() && teams[playerId].Decide)
			{
				//キャンセル（決定待ち）状態に変更
				teams[playerId].Decide = false;

				//プレイヤーのキャラクター解除
				GamePlayerManager_Work.Instance.removeCharacter(playerId);
            }
		}
        #endregion

        #region 入力関連
        //左左ボタン・スティックの状態取得
        private bool isLeft()
		{
			//十字の左ボタン（左スティック用と左左ボタン）を押したか
			if((inputData.PadTrigger & GamePadButton.EmuLleft) != 0 || (inputData.PadTrigger & GamePadButton.LLeft) != 0)
			{
				return true;
			}
			return false;
		}

        //左右ボタン・スティックの状態取得
        private bool isRight()
        {
            //十字の右ボタン（左スティック用と左右ボタン）を押したか
            if ((inputData.PadTrigger & GamePadButton.EmuLright) != 0 || (inputData.PadTrigger & GamePadButton.LRight) != 0)
            {
                return true;
            }
            return false;
        }

        //決定ボタンの状態取得
        private bool isDecide()
		{
			//右の下ボタン（Aボタン）を押したか
			if((inputData.PadTrigger & GamePadButton.RDown) != 0)
			{
				return true;
			}
			return false;
		}

		//キャンセルボタンの状態取得
		private bool isCancel()
		{
            //右の右ボタン（Bボタン）を押したか
			if ((inputData.PadTrigger & GamePadButton.RRight) != 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region プレイヤーチーム
        public class PlayerTeam
		{
            /// <summary>
			/// デフォルトコンストラクタ
			/// </summary>
			public PlayerTeam()
			{
				select = Team.SheenaEgi;
				decide = false;
			}
			
			#region フィールド
            /// <summary>
            /// 選択チーム
            /// </summary>
            private Team select;

            /// <summary>
            /// 決定したか
            /// </summary>
            private bool decide;
			#endregion

			#region プロパティ
			[IgnoreDataMember, ReadOnly(true)]
			//選択チームの取得
			public Team Select
			{
				get { return select; }
				set { select = value; }
			}
            //決定状態の取得
            [IgnoreDataMember, ReadOnly(true)]
            public bool Decide
			{
				get { return decide; }
				set { decide = value; }
			}
            #endregion
        }
        #endregion
    }
}
