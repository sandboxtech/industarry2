
using UnityEngine;
using UnityEngine.EventSystems;

namespace W
{
    /// <summary>
    /// 相机移动，和UI输入有关
    /// </summary>
    [ExecuteAfter(typeof(CameraControl)), ExecuteAfter(typeof(UI))]
    public class CameraMovement : MonoBehaviour
    {
        public static CameraMovement I => i;
        private static CameraMovement i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }

        private void AwakeInstance() {
            PointerEventReceiver receiver = UI.I.PointerEventReceiver;
            receiver.Down += PointerDown;
            receiver.Up += PointerUp;
            receiver.Move += PointerMove;
        }

        private bool tapping = false;
        private void PointerDown(PointerEventData eventData) {
            tapping = true;
        }

        private void PointerUp(PointerEventData eventData) {
            tapping = false;
        }

        private void PointerMove(PointerEventData eventData) {
            if (!tapping) return;
            CameraControl.I.TranslateInScreenSpace(eventData.delta);
        }

        private void Update() {
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f) {
                    CameraControl.I.Zoom++;
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f) {
                    CameraControl.I.Zoom--;
                }

                Vector3 movement = Vector3.zero;
                if (Input.GetKey(KeyCode.W)) {
                    movement += Vector3.up;
                }
                if (Input.GetKey(KeyCode.S)) {
                    movement += Vector3.down;
                }
                if (Input.GetKey(KeyCode.A)) {
                    movement += Vector3.left;
                }
                if (Input.GetKey(KeyCode.D)) {
                    movement += Vector3.right;
                }
                CameraControl.I.TranslateInScreenSpace(-movement * Time.deltaTime * 300f);
            }
        }
    }
}
