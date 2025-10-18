//=============================================================================
// <summary>
// カメラ制御を管理するクラス
// </summary>
// <author> 菊池雅道 </author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
    [UpdateOrder((int)AppUpdateOrder_Work.UpdateOrder.CameraManager)]
    public class CameraManager_Work : SingletonRoot_Work<CameraManager_Work>
    {
        #region 定義
        public class CameraParam
        {
            public enum CameraSettingType
            {
                Normal,             //!< 位置、回転
                LookAt,             //!< 位置、注視点
                FollowTarget,        //!< ターゲット追従
            }

            [IgnoreDataMember]
            public CameraSettingType settingType = CameraSettingType.Normal;

            [IgnoreDataMember, ManipulatorInspectBtn]
            public vec3 position = new vec3(0.0f, 8.0f, 8.0f);
            [IgnoreDataMember]
            public vec3 rotation = new vec3(math.deg2rad(-49.0f), 0.0f, 0.0f);
            [IgnoreDataMember]
            public float fov = 40.0f;
            [IgnoreDataMember]
            public float nearClipPlane = 0.1f;
            [IgnoreDataMember]
            public float farClipPlane = 1000.0f;

            [IgnoreDataMember, ManipulatorInspectBtn]
            public vec3 lookAtPosition = new vec3(0.0f, 0.0f, 0.0f);
            [IgnoreDataMember]
            public vec3 lookAtOffset = new vec3(0.0f, 0.0f, 0.0f);

            [IgnoreDataMember]
            public GameObject targetObject = null;
            [IgnoreDataMember]
            public vec3 targetOffset = new vec3(0.0f, 0.0f, 0.0f);
            [IgnoreDataMember]
            public vec3 targetLookAtOffset = new vec3(0.0f, 1.0f, 1.0f);

            /// <summary>
            /// 計算された位置
            /// </summary>
            [IgnoreDataMember, ReadOnly(true)]
            private vec3 calculatedPosition = new vec3(0.0f, 0.0f, 0.0f);

            /// <summary>
            /// 計算された回転
            /// </summary>
            [IgnoreDataMember, ReadOnly(true)]
            private vec3 calculatedRotation = new vec3(0.0f, 0.0f, 0.0f);

            /// <summary>
            /// 毎フレーム呼ばれるカメラ計算処理
            /// </summary>
            public void calc()
            {
                switch (settingType)
                {
                    case CameraSettingType.Normal:
                        calcCameraNormal();
                        break;

                    default:
                        break;
                }
            }

            /// <summary>
            /// オブジェクトへ反映
            /// </summary>
            /// <param name="go"></param>
            public void apply(GameObject go)
            {
                //ゲーム
                go.Transform.Position = calculatedPosition;
                go.Transform.Rotation = quaternion.makeRotateZXY(calculatedRotation);
            }

            /// <summary>
            /// 位置、回転指定
            /// </summary>
            private void calcCameraNormal()
            {
                calculatedPosition = position;
                calculatedRotation = rotation;
            }
        }
        #endregion

        [IgnoreDataMember]
        public bool applyCamera = true;

        private Camera cpCamera = null;

        [IgnoreDataMember]
        private CameraParam currentCameraParam = new CameraParam();

        public override void start()
        {
            cpCamera = GameObject.getSameComponent<Camera>();
            
        }

        public override void update()
        {
            //設定されているカメラの計算
            currentCameraParam.calc();

            //メインカメラへ反映
            if (applyCamera)
            {
                currentCameraParam.apply(GameObject);
            }
        }

        public void setCameraParam(CameraParam param)
        {
            currentCameraParam.settingType = param.settingType;

            currentCameraParam.position = param.position;
            currentCameraParam.rotation = param.rotation;

            currentCameraParam.lookAtPosition = param.lookAtPosition;
            currentCameraParam.lookAtOffset = param.lookAtOffset;

            currentCameraParam.targetObject = param.targetObject;
            currentCameraParam.targetOffset = param.targetOffset;
            currentCameraParam.targetLookAtOffset = param.targetLookAtOffset;

            currentCameraParam.nearClipPlane = param.nearClipPlane;
            currentCameraParam.farClipPlane = param.farClipPlane;
            currentCameraParam.fov = param.fov;
        }

        public void getCurrentCameraParam(out CameraParam param)
        {
            param = new CameraParam();

            param.settingType = currentCameraParam.settingType;

            param.position = currentCameraParam.position;
            param.rotation = currentCameraParam.rotation;

            param.lookAtPosition = currentCameraParam.lookAtPosition;
            param.lookAtOffset = currentCameraParam.lookAtOffset;

            param.targetObject = currentCameraParam.targetObject;
            param.targetOffset = currentCameraParam.targetOffset;
            param.targetLookAtOffset = currentCameraParam.targetLookAtOffset;

            param.nearClipPlane = currentCameraParam.nearClipPlane;
            param.farClipPlane = currentCameraParam.farClipPlane;
            param.fov = currentCameraParam.fov;
        }

        //カメラを目的地点まで線形補間で動かす
        public void moveCameraLerp(vec3 targetPosition,float interpolationCoef)
        {
            currentCameraParam.position = via.vector.lerp(currentCameraParam.position, targetPosition, interpolationCoef);
        }

        //カメラ振動
        System.Collections.IEnumerator cameraShake()
        {
            for (; ; )
            {
                currentCameraParam.position.x += 0.5f;
                //currentCameraParam.apply(GameObject);
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                currentCameraParam.position.x -= 1.0f;
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                currentCameraParam.position.x += 1.0f;
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                currentCameraParam.position.x -= 1.0f;
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                currentCameraParam.position.x += 1.0f;
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                currentCameraParam.position.x -= 0.5f;
                yield return new WaitUntil(TimeSpan.FromMilliseconds(0.001));
                break;
            }
        }

        //カメラ振動開始
        public void startCameraShack()
        {
            startCoroutine(cameraShake());
        }

    }
}