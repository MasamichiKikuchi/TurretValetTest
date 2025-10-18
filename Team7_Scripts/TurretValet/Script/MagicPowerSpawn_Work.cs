//=============================================================================
// <summary>
// ステージ上にスポーンする魔力弾のクラス 
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.effect.script;
using via.physics;

namespace app
{
	public class MagicPowerSpawn_Work : via.Behavior,IColliders
    {
        #region コンポーネント
        protected ObjectEffectController cpObjectEffectController = null;
        #endregion

        #region 定数
        /// <summary>
        /// エフェクト
        /// </summary>
        private enum Effect
        {
            spawn,      //スポーン
            exist,      //存在してる間ループ再生するエフェクト
        };
        #endregion

        public override void start()
		{
            //エフェクト発生
            cpObjectEffectController = GameObject.getSameComponent<ObjectEffectController>();
            EffectID spawnEffect_id = new EffectID(0, (int)Effect.spawn);
            cpObjectEffectController.requestEffect(spawnEffect_id, GameObject.Transform.Position, Quaternion.Identity ,null);
            EffectID existEffect_id2 = new EffectID(0, (int)Effect.exist);
            cpObjectEffectController.requestEffect(existEffect_id2, GameObject.Transform.Position, Quaternion.Identity, null);
        }

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(CollisionInfo collision_info)
        {
           //魔法少女と接触したら、自分を破壊
            if ((collision_info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Witch")))
            {
                GameObject.destroy(this.GameObject);
            }       
        }

        public void onOverlapping(CollisionInfo collision_info)
        {
            
        }

        public void onSeparate(CollisionInfo collision_info)
        {
            
        }
    }
}
