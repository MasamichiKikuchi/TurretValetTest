//=============================================================================
// <summary>
// 魔力玉の設定に関するユーザーデータ
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class MagicPowerUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("ランダム時間最大値(秒)"), DataMember]
        private int magicPowerRandomTimeMax = 20;       //最初の魔力弾が出る時間の最大値
        [DisplayName("ランダム時間最小値(秒)"), DataMember]
        private int magicPowerRandomTimeMin = 0;        //最初の魔力弾が出る時間の最小値
        [DisplayName("魔力弾の発生間隔(秒)"), DataMember]
        private int magicPowerSpawnIntervalTime = 1;    //魔力弾の発生間隔
        [DisplayName("大魔力弾が出る確率※最初(0~100％)"), DataMember]
        private int firstBigMagicPowerProbability = 10;      //大魔力弾が出る確率(0~100)
        [DisplayName("大魔力弾が出る確率※最後(0~100％)"), DataMember]
        private int lastBigMagicPowerProbability = 50;      //大魔力弾が出る確率(0~100)
        [DisplayName("大魔力弾が出る確率を変える時間(秒)"), DataMember]
        private int  bigMagicPowerProbabilityChangeTime = 30;
        [DisplayName("魔力弾の移動速度"), DataMember]
        private float magicPowerMoveSpeed = 0.05f;      //魔力弾の移動速度
        [DisplayName("魔力弾の移動距離"), DataMember]
        private float magicPowerMoveDistance = 10.0f;   //魔力弾の移動距離     
        #endregion

        #region プロパティ
        public int MagicPowerRandomTimeMax
        {
            get { return magicPowerRandomTimeMax; }
        }

        public int MagicPowerRandomTimeMin
        {
            get { return magicPowerRandomTimeMin; }
        }

        public int MagicPowerSpawnIntervalTime
        {
            get { return magicPowerSpawnIntervalTime; }
        }

        public int FirstBigMagicPowerProbability
        {
            get { return firstBigMagicPowerProbability; }
        }

        public int LastBigMagicPowerProbability
        {
            get { return lastBigMagicPowerProbability; }
        }

        public int BigMagicPowerProbabilityChangeTime
        {
            get { return bigMagicPowerProbabilityChangeTime; }
        }

        public float MagicPowerMoveSpeed
        {
            get { return magicPowerMoveSpeed; }
        }
       
        public float MagicPowerMoveDistance
        {
            get { return magicPowerMoveDistance; }
        }
        #endregion
    }
}
