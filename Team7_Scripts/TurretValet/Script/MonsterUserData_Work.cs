//=============================================================================
// <summary>
// MonsterUserData_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class MonsterUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("通常の移動速度（倍率）"), DataMember]
        private float monsterMoveSpeed = 0.05f;
        [DisplayName("チャージ中の移動速度（倍率）"), DataMember]
        private float chargeMoveSpeed = 0.025f;
        [DisplayName("ラリアット中の移動速度（倍率）"), DataMember]
        private float[] lariatMoveSpeed = new float[3] { 0.05f, 0.075f, 0.1f };
        [DisplayName("チャージ時間（フレーム数）"), DataMember]
        private int chargeTime = 120;
        [DisplayName("筋力ゲージ1単位のホールド時間（フレーム数）"), DataMember]
        private int holdOneTime = 20;
        [DisplayName("最大ヒットポイント"), DataMember]
        private int maxHitPoint = 5;
        [DisplayName("筋肉ゲージ最大値"), DataMember]
        private int maxMuscleGauge = 15;
        [DisplayName("ムキムキレベル１筋力ゲージ最大値"), DataMember]
        private int level1_MaxMuscleGauge = 10;
        [DisplayName("ムキムキレベルノーマル筋力ゲージ最大値"), DataMember]
        private int nomal_MaxMuscleGauge = 5;
        [DisplayName("体力を見せる時間"), DataMember]
        private int monsterShowLifeCnt = 60;
        #endregion

        #region プロパティ
        //通常の移動速度（倍率）のゲッター
        public float MonsterMoveSpeed
        {
            get { return monsterMoveSpeed; }
        }
        //チャージ中の移動速度（倍率）のゲッター
        public float ChargeMoveSpeed
        {
            get { return chargeMoveSpeed; }
        }
        //ラリアット中の移動速度（倍率）のメソッド
        public float LariatMoveSpeed(int index)
        {
            return lariatMoveSpeed[index];
        }
        //チャージ時間（フレーム数）のゲッター
        public int ChargeTime
        {
            get { return chargeTime; }
        }
        //筋力ゲージ1単位のホールド時間（フレーム数）のゲッター
        public int HoldOneTime
        {
            get { return holdOneTime; }
        }
        //最大ヒットポイントのゲッター
        public int MaxHitPoint
        { 
            get { return maxHitPoint; }
        }
        //筋肉ゲージ最大値のゲッター
        public int MaxMuscleGauge
        {
            get{  return maxMuscleGauge; }
        }
        //ムキムキレベル１筋力ゲージ最大値のゲッター
        public int Level1_MaxMuscleGauge
        {
            get{ return level1_MaxMuscleGauge; }
        }
        //ムキムキレベル１筋力ゲージ最大値のゲッター
        public int Nomal_MaxMuscleGauge
        {
            get { return nomal_MaxMuscleGauge; }
        }
        //召喚獣がダメージを受けたときのHPが出る長さ
        public int MonsterShowLifeCnt
        {
            get { return monsterShowLifeCnt; }
        }
        #endregion
    }
}
