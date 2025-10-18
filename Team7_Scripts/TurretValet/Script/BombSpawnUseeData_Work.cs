//=============================================================================
// <summary>
// 爆弾の発生に関するユーザーデータ
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class BombSpawnUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("爆弾発生開始時間(秒)"), DataMember]
        private int spawnStartTime = 30;            //爆弾発生開始時間
        [DisplayName("爆弾の発生間隔(秒)"), DataMember]
        private int spawnIntervsal = 5;             //爆弾の発生間隔
        [DisplayName("発生地点を探す最大回数"), DataMember]
        [Description("爆弾生成時、まだ爆弾がない地点を探す回数の上限　\n数が多いと精度が上がるが、処理が重くなる")]
        private int maxPositionSearchCount = 99;    //発生地点を探す最大回数
        [DisplayName("爆弾生成方向の分割数"), DataMember]
        [Description("ステージを円とした時、中心からみてステージを分割する数　\n数が多いと爆弾生成地点の候補が増えるが、計算コストが増える")]
        private int stageDivideNum = 32;            //爆弾生成方向の候補数(ステージを円とした時の、中心からみた分割数)

        #endregion

        #region プロパティ
        public int SpawnStartTime
        {
            get { return spawnStartTime; }
        }

        public int SpawnIntervsal
        {
            get { return spawnIntervsal; }
        }

        public int MaxPositionSearchCount
        {
            get { return maxPositionSearchCount; }
        }

        public int StageDivideNum
        { 
            get { return stageDivideNum; } 
        }
        #endregion
    }
}
