//=============================================================================
// <summary>
// FrameSpeedController_Work 
// </summary>
// <author> 廣山将太郎 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.effect;
using via.effect.script;

namespace app
{
	[UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.FrameSpeedController)]
	public class FrameSpeedController_Work : via.Behavior
	{
		[Action]
		private void setFrameSpeed()
		{
			requestFrameSpeed(frameSpeedRate, effectFrame);

            effectCount = 0;
        }

		[IgnoreDataMember]
		private float frameSpeedRate = 1.0f;

		[IgnoreDataMember]
		private float effectFrame = 0.0f;

		[IgnoreDataMember, ReadOnly(true)]
		private float frameCounter = 0.0f;

		private int effectCount = 0;

		/// <summary>
		/// コンポーネント
		/// </summary>
		private Transform cpTransform = null;												//位置情報
		private via.motion.Motion cpMotion = null;											//モーション
		private via.effect.script.ObjectEffectController cpObjectEffectController = null;	//エフェクト

		public override void start()
		{
			cpTransform = GameObject.Transform;
			cpMotion = GameObject.getSameComponent<via.motion.Motion>();
			cpObjectEffectController = GameObject.getComponent<via.effect.script.ObjectEffectController>();
		}

		public override void update()
		{
			float rate = 1.0f;
			frameCounter -= DeltaTime;

			//frameCounter中はヒットストップ
			if(frameCounter >= 0.0f)
			{
				rate = frameSpeedRate;
			}
			else
			{
				rate = 1.0f;
				frameCounter = 0.0f;
			}

			//最終的なフレームレート
			int layer_num = cpMotion.Layer.Count;
			for(int i = 0; i < layer_num; i++)
			{
				//位置情報のスピードをセット
				
				//モーションのスピードをセット
				cpMotion.Layer[i].Speed = rate;

				if(effectCount > 0)
				{
					//エフェクトのスピードをセット
					setEffectSpeed(rate);
				}
			}
		}

		/// <summary>
		/// FrameSpeed変更申請（モーションのみ）
		/// </summary>
		/// <param name="rate"></param>
		/// <param name="frame"></param>
		public void requestFrameSpeed(float rate, float frame)
		{
			frameSpeedRate = rate;
			effectFrame = frame;
			frameCounter = frame;
		}

		/// <summary>
		/// FrameSpeed変更申請（モーション、エフェクト）
		/// </summary>
		/// <param name="rate"></param>
		/// <param name="frame"></param>
		/// <param name="effectCnt"></param>
		public void requestFrameSpeed(float rate, float frame, int effectCnt)
		{
            frameSpeedRate = rate;
            effectFrame = frame;
            frameCounter = frame;
			effectCount = effectCnt;
        }

		/// <summary>
		/// オブジェクトから発生しているエフェクトのスピードをセット
		/// </summary>
		/// <param name="speedRate"></param>
		public void setEffectSpeed(float speedRate)
		{
			for(uint element = 0; element < effectCount; element++)
			{
                //エフェクトIDで指定の発生中のエフェクトをリストとして取得
                var list = cpObjectEffectController.getCreatedEffectFromEffectID(new EffectID(0, element));

				if(list != null)
				{
                    for (int i = 0; i < list.Count; i++)
					{
						//エフェクトの再生スピードを変更
						list[i].setPlaySpeed(speedRate);
					}
                }
            }
		}
	}
}
