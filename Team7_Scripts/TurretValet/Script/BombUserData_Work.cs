//=============================================================================
// <summary>
// 爆弾のユーザーデータ 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class BombUserData_Work : via.UserData
	{
        #region フィールド
        [DisplayName("爆発猶予時間(秒)"), DataMember]
        private float timeToExplosion = 3.0f;       //魔力弾を受けて爆発するまでの時間
        [DisplayName("爆風持続時間(秒)"), DataMember]
        private float explosionTime = 3.0f;         //爆発の持続時間
        #endregion

        #region プロパティ
        public float TimeToExplosion
        {
            get { return timeToExplosion; }
        }

        public float ExplosionTime
        {
            get { return explosionTime; }
        }
        #endregion
    }
}
