
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace W
{
    /// <summary>
    /// 相机控制
    /// </summary>
    public class CameraControl : MonoBehaviour
    {
        public static CameraControl I => i;
        private static CameraControl i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }

        private void AwakeInstance() {
            trans = transform;
            initialPositionZ = trans.position.z;
        }

        [SerializeField]
        private Camera cam;
        public Camera Camera => cam;
        public Vector2 Position {
            get => new Vector2(trans.position.x, trans.position.y);
            set {
                trans.position = new Vector3(value.x, value.y, initialPositionZ);
            }
        }

        [SerializeField]
        private PixelPerfectCamera ppCam;

        private Transform trans;
        private float initialPositionZ;

        public void TranslateInScreenSpace(Vector2 delta) {
            trans.position -= new Vector3(delta.x * ppCam.refResolutionX / Screen.width, delta.y * ppCam.refResolutionY / Screen.height, 0) / ppCam.assetsPPU;
            ClampCameraPosition();
        }

        public void TranslateInWorldSpace(Vector2 delta) {
            trans.position += new Vector3(delta.x, delta.y, 0);
            ClampCameraPosition();
        }

        public void ClampCameraPosition() {
            float x = M.Clamp(Area.xMin, Area.xMax, trans.position.x);
            float y = M.Clamp(Area.yMin, Area.yMax, trans.position.y);
            trans.position = new Vector3(x, y, initialPositionZ);
        }

        public Rect Area = new Rect(-20, -11.25f, 40, 22.5f);




        const int zoomBase = 3;
        const int zoomLength = 3;
        public int Zoom {
            get {
                float value = ppCam.assetsPPU;
                int result;
                if (value <= 8) {
                    result = zoomBase + 0;
                }
                else if (value <= 16) {
                    result = zoomBase + 1;
                }
                else if (value <= 32) {
                    result = zoomBase + 2;
                }
                else if (value <= 64) {
                    result = zoomBase + 3;
                }
                else {
                    result = zoomBase + 3;
                }
                return result;
            }
            set {
                // 8 最小
                // 16 偏小
                // 32 偏大
                // 64 最大
                value = M.Clamp(zoomBase, zoomBase + zoomLength, value);
                int result = 32;
                switch (value) {
                    case zoomBase + 0:
                        result = 8;
                        break;
                    case zoomBase + 1:
                        result = 16;
                        break;
                    case zoomBase + 2:
                        result = 32;
                        break;
                    case zoomBase + 3:
                        result = 64;
                        break;
                    default:
                        break;
                }
                ppCam.assetsPPU = result;
            }
        }
    }
}
