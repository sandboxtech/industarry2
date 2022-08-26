
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace W
{
    public partial class UI : MonoBehaviour
    {
        public static UI I => i;
        private static UI i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }

        private void AwakeInstance() {

            PointerEventReceiver.Down += (PointerEventData data) => {
                PointerEventReceiver.transform.SetAsLastSibling();
            };
            PointerEventReceiver.Up += (PointerEventData data) => {
                // scroll.SetAsLastSibling();
                PointerEventReceiver.transform.SetAsFirstSibling();
            };
        }

        private void Start() {
            canvas.worldCamera = CameraControl.I.Camera;
        }

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private PointerEventReceiver pointerEventReceiver;
        public PointerEventReceiver PointerEventReceiver => pointerEventReceiver;


        [SerializeField]
        private ItemControl itemControl;

        [SerializeField]
        private RectTransform scroll;
        [SerializeField]
        private RectTransform content;


        public bool ScrollVisible {
            set {
                StopAllCoroutines();
                StartCoroutine(TweenScroll(!value));
            }
        }
        private const float hiddenPosition = -160f;
        private const float shownPosition = 0f;
        private const float tweenDuration = 0.125f;
        private IEnumerator TweenScroll(bool hide) {
            if (!hide) scroll.gameObject.SetActive(true);
            float startTime = Time.time;
            float startPosition = scroll.anchoredPosition.x;
            float targetPosition = hide ? hiddenPosition : shownPosition;
            if (targetPosition == startPosition) {
                yield break;
            }
            yield return null;
            while (true) {
                float t = (Time.time - startTime) / tweenDuration;
                if (t >= 1) {
                    scroll.anchoredPosition = new Vector2(targetPosition, 0);
                    break;
                }
                t = - (t - 1) * (t - 1) + 1;
                scroll.anchoredPosition = new Vector2(M.Lerp(startPosition, targetPosition, t), 0);
                yield return null;
            }
            if (hide) scroll.gameObject.SetActive(false);
        }

        private void Clear() {
            ClearTransform();
        }
        private void ClearTransform() {
            int count = content.childCount;
            for (int i = count - 1; i >= 0; i--) {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        private void AddItem(ref Item item) {
            ItemControl control = Instantiate(itemControl, content);
            control.EnableText(ref item);

            switch (item.Type) {
                case ItemType.Space:
                    control.EnableSpace(ref item);
                    break;
                case ItemType.Text:
                    break;
                case ItemType.Button:
                    control.EnableButton(ref item);
                    break;
                case ItemType.Progress:
                    control.EnableProgress(ref item);
                    break;
                case ItemType.Slider:
                    control.EnableSlider(ref item);
                    break;
                default:
                    break;
            }
        }


        public static void Hide() {
            i.ScrollVisible = false;
        }

        public static List<Item> Items { get => items; set => items = value; }
        private static List<Item> items;

        private static List<Item> itemsPrepared = new List<Item>();
        public static void Prepare() {
            itemsPrepared.Clear();
            items = itemsPrepared;
        }

        public static void Overwrite(int index, Item item) {
            A.Assert(index >= 0 && index < items.Count);
            items[index] = item;
        }

        public static void Show() {
            Show(items);
        }

        public static void Show(List<Item> items) {
            i.Clear();
            i.ScrollVisible = true;

            if (items == null) return;
            UI.items = items;

            for (int ii = 0; ii < items.Count; ii++) {
                Item item = items[ii];
                i.AddItem(ref item);
            }
            UI.items = null;
        }


        [Header("Buttons")]
        [SerializeField]
        public Button Button0;
        [SerializeField]
        public Button Button1;
        [SerializeField]
        public Button Button2;
    }
}
