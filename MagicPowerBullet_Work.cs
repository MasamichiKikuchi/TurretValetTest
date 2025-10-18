//=============================================================================
// <summary>
// 発射された魔力弾のクラス 
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

    public class MagicPower_Work : via.Behavior, IColliders
    {
        #region フィールド
        public vec3 starPosition = vec3.Zero;   //発射地点
        #endregion

        #region ユーザーデータ
        [DataMember]
        private MagicPowerUserData_Work magicPowerUserData = null;  //魔力玉設定に関するユーザーデータ
        #endregion

        //移動処理
        private void move()
        {
            //前方に移動
            vec3 move = vector.setLength(GameObject.Transform.AxisZ, magicPowerUserData.MagicPowerMoveSpeed);
            GameObject.Transform.Position += move;
        }

        //消滅処理
        private void destroyProcess()
        {
            //一定距離移動したら消滅
            vec3 movedVector = starPosition - GameObject.Transform.Position;
            float movedDistance = movedVector.length();
            if (movedDistance > magicPowerUserData.MagicPowerMoveDistance)
            {
                GameObject.destroy(this.GameObject);
            }
        }
       
		public override void start()
		{
            starPosition = GameObject.Transform.Position;
        }

		public override void update()
		{
            //移動
            move();

            //消滅
            destroyProcess();          
        }

        /// <summary>
        /// 衝突した
        /// </summary>
        /// <param name="info"></param>
        public void onContact(CollisionInfo collision_info)
        {
           
            //召喚獣と接触したら消える
            if ((collision_info.CollidableB.FilterInfo.Layer == via.physics.System.getLayerIndex("Damage")))
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
