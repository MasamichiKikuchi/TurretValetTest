//=============================================================================
// <summary>
// インゲームのカメラに関するユーザーデータ
// </summary>
// <author> 菊池 雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
	public class IngameCameraUserData_Work : via.UserData
	{
        #region フィールド      
        [DisplayName("ゲーム開始時のカメラ位置(X,Y,Z)"), DataMember]
        private vec3 ingameCameStartPosition = new vec3(0.0f, 8.0f, 8.0f);  //ゲーム開始時のカメラ位置
        [DisplayName("ゲーム開始時のカメラ回転(X,Y,Z)"), IgnoreDataMember]
        private vec3 ingameCameStartRotation = new vec3(math.deg2rad(-49.0f), math.deg2rad(0.0f), math.deg2rad(0.0f));  //ゲーム開始時のカメラローテーション
        [DisplayName("ターゲット位置のオフセット値(X,Y,Z)"), DataMember]
        [Description("ゲーム終了時、ズーム対象からオフセットをかけた位置に移動する")]
        private vec3 targetPositionOffset = new vec3(0.0f, 2.0f, 2.0f);     //ターゲット位置のオフセット値
        [DisplayName("ズームイン時の補間係数(最大1)"), DataMember]
        [Description("カメラ移動の補間係数　高いほど補完される")]
        private float zoomInInterpolationCoef = 0.2f;                       //ズームイン時の補間係数
        [DisplayName("ズームアウト時の補間係数(最大1)"), DataMember]
        [Description("カメラ移動の補間係数　高いほど補完される")]
        private float zoomOutInterpolationCoef = 0.5f;                      //ズームアウト時の補間係数
        #endregion

        #region プロパティ
        public vec3 IngameCameStartPosition
        {
            get { return ingameCameStartPosition; }
            set { ingameCameStartPosition = value; }
        }

        public vec3 IngameCameStartRotation
        {
            get { return ingameCameStartRotation; }
            set { ingameCameStartRotation = value; }
        }

        public vec3 TargetPositionOffset
        {
            get { return targetPositionOffset; }
            set { targetPositionOffset = value; }
        }
        public float ZoomInInterpolationCoef
        {
            get { return zoomInInterpolationCoef; }
            set { zoomInInterpolationCoef = value; }
        }
        public float ZoomOutInterpolationCoef
        {
            get { return zoomOutInterpolationCoef; }
            set { zoomOutInterpolationCoef = value; }
        }
        #endregion
    }
}
