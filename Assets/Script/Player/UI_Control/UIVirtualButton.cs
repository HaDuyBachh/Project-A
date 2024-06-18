using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

namespace HaDuyBach
{

    public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IDragHandler
    {
        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }
        [System.Serializable]
        public class Event : UnityEvent { }

        [Header("Output")]
        public BoolEvent buttonStateOutputEvent;
        public Event buttonClickOutputEvent;

        [Header("Setting")]
        public bool CanDragScreen = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            OutputButtonStateValue(true);

            if (CanDragScreen)
            {
                if (GetCurrentDrop<UIVirtualTouchZone>(eventData, out var p) && p.isLook)
                {
                    p.OnPointerDown(eventData);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OutputButtonStateValue(false);

            if (CanDragScreen)
            {
                if (GetCurrentDrop<UIVirtualTouchZone>(eventData, out var p) && p.isLook)
                {
                    p.OnPointerUp(eventData);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (CanDragScreen)
            {
                if (GetCurrentDrop<UIVirtualTouchZone>(eventData, out var p) && p.isLook)
                {
                    p.OnDrag(eventData);
                }
            }

        }

        // Sử dụng để thể hiện đã click vào nút này
        public void OnPointerClick(PointerEventData eventData)
        {
            OutputButtonClickEvent();
        }

        // Sử dụng để truyền tham số bool thể hiện nút có đang được click hay không
        void OutputButtonStateValue(bool buttonState)
        {
            buttonStateOutputEvent.Invoke(buttonState);
        }

        void OutputButtonClickEvent()
        {
            buttonClickOutputEvent.Invoke();
        }

        private bool GetCurrentDrop<T>(PointerEventData eventData, out T component)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (var CurrentDrop in raycastResults)
            {
                if (CurrentDrop.gameObject.TryGetComponent(out component))
                {
                    return true;
                }
            }

            component = default(T);
            return false;
        }

    }

}
