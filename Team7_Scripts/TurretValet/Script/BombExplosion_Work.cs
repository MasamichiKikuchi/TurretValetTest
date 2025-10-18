//=============================================================================
// <summary>
// 爆弾の爆発に関するクラス
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using sound;
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.effect.script;
using via.physics;

namespace app
{
	public class BombExplosion_Work : via.Behavior
	{

        /// <summary>
        /// フィールド
        /// </summary>
        float explosionTimeCount = 0;                                        //爆発継続時間
        protected ObjectEffectController cpObjectEffectController = null;   //エフェクト  
        private SoundPlayer cpSoundPlayer = null;                           //サウンドプレイヤーコンポーネント

        #region 定数
        /// <summary>
        /// エフェクト
        /// </summary>
        private enum Effect
        {
            Explosion, //爆発
        };
        /// <summary>
        /// サウンドSE
        /// </summary>
        private enum BombSe
        {
            Explosion, //爆発
        }
        #endregion

        #region ユーザーデータ
        [DataMember]
        private BombUserData_Work bombUserData = null;
        #endregion

		public override void start()
		{
            //各コンポーネント取得
            cpObjectEffectController = GameObject.getSameComponent<ObjectEffectController>();
            cpSoundPlayer = GameObject.getSameComponent<SoundPlayer>();

            //エフェクト
            EffectID effect_id = new EffectID(0, (int)Effect.Explosion);
            cpObjectEffectController.requestEffect(effect_id, GameObject.Transform.Position, Quaternion.Identity, null);

            //SE
            cpSoundPlayer._Sources[(int)BombSe.Explosion].play();    
        }

		public override void update()
		{
            //爆発時間が終了したら破壊   
            if (explosionTimeCount >= bombUserData.ExplosionTime)
            {
                GameObject.destroy(GameObject);
            }

            //爆発時間加算
            explosionTimeCount += Application.ElapsedSecond;
        }
	}
}
