
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace W
{
    public class PointerEventReceiver : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        public event Action<PointerEventData> Down;
        public void OnPointerDown(PointerEventData eventData) {
            Down?.Invoke(eventData);
        }

        public event Action<PointerEventData> Up;
        public void OnPointerUp(PointerEventData eventData) {
            Up?.Invoke(eventData);
        }

        public event Action<PointerEventData> Move;
        public void OnPointerMove(PointerEventData eventData) {
            Move?.Invoke(eventData);
        }
    }
}
