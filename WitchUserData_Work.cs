//=============================================================================
// <summary>
// 魔法少女のステータス数値関連ユーザーデータ 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

public class PlayerUserData_Work : via.UserData
{
    #region フィールド
    [DisplayName("移動速度"), DataMember]
    private float witchMoveSpeed = 0.05f;           //移動速度
    [DisplayName("魔力玉最大保有数"), DataMember]
    private int maxMagicPower = 5;                  //魔力玉最大保有数
    [DisplayName("魔力弾の獲得魔力"), DataMember]
    private int magicPowerPoint = 1;                //魔力弾獲得時の増加魔力数
    [DisplayName("大魔力弾の獲得魔力"), DataMember]
    private int bigMagicPowerPoint = 5;             //大魔力弾獲得時の増加魔力数
    [DisplayName("ヒット時ビーム時間(秒)"), DataMember]
    private float hitBeamTime = 2.5f;               //攻撃ヒット時のビーム照射時間
    [DisplayName("非ヒット時ビーム時間(秒)"), DataMember]
    private float missBeamTime = 1.0f;              //攻撃ヒットがヒットしなかった時のビーム照射時間
    #endregion

    #region プロパティ
    public float WitchMoveSpeed
    {
        get { return witchMoveSpeed; }
    }
    public int MagicPowerPoint
    {
        get { return magicPowerPoint; }
    }
    public int BigMagicPowerPoint
    {
        get { return bigMagicPowerPoint; }
    }
    public int MaxMagicPower
    {
        get { return maxMagicPower; }
    }
    public float HitBeamTime
    {
        get { return hitBeamTime; }
    }
    public float MissBeamTime
    {
        get { return missBeamTime; }
    }

    #endregion
}
