
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace W
{
    /// <summary>
    /// 地图点击交互
    /// 和UI输入有关，会用到相机
    /// </summary>
    [ExecuteAfter(typeof(UI))]
    public class MapTapping : MonoBehaviour
    {
        public static MapTapping I => i;
        private static MapTapping i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }
        private void Update() {
            UpdateIndicatorCursor();
        }


        [SerializeField]
        private Transform IndicatorCursor;
        private void UpdateIndicatorCursor() {
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                IndicatorCursor.gameObject.SetActive(true);

                Vector3 pos = CameraControl.I.Camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int pos2 = Floor(pos);
                IndicatorCursor.transform.position = new Vector3(pos2.x, pos2.y, 0);
            }
        }



        private Vector2Int tapDownWorldPosition;
        private Vector2 tapDownScreenPosition;
        private Vector2Int Floor(Vector3 v) => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        private Vector2Int WorldPos(PointerEventData data) => Floor(CameraControl.I.Camera.ScreenToWorldPoint(data.position));
        private const float screenDistanceToIgnoreTap = 10f;
        private void AwakeInstance() {

            PointerEventReceiver receiver = UI.I.PointerEventReceiver;
            receiver.Down += (PointerEventData data) => {
                tapDownScreenPosition = data.position;
                tapDownWorldPosition = WorldPos(data);
            };
            receiver.Up += (PointerEventData data) => {
                if ((tapDownScreenPosition - data.position).sqrMagnitude > screenDistanceToIgnoreTap * screenDistanceToIgnoreTap) {
                    return;
                }
                if (tapDownWorldPosition != WorldPos(data)) {
                    return;
                }
                TapPosition(tapDownWorldPosition);
            };
        }


        public Action OnTap;

        [SerializeField]
        private Transform IndicatorTap;
        private void TapPosition(Vector2Int pos) {
            X = pos.x;
            Y = pos.y;

            OnTap?.Invoke();
        }

        public void ShowIndicatorAt(int x, int y) {
            IndicatorTap.gameObject.SetActive(true);
            IndicatorTap.position = new Vector3(x, y, 0);
        }
        public void HideIndicator() {
            IndicatorTap.gameObject.SetActive(false);
        }


        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
