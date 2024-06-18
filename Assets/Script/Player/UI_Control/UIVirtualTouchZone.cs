using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

namespace HaDuyBach
{
    public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class Event : UnityEvent<Vector2> { }

        [Header("Rect References")]
        public RectTransform containerRect;
        public RectTransform handleRect;

        [Header("Settings")]
        public bool clampToMagnitude;
        public float magnitudeMultiplier = 1f;
        public bool invertXOutputValue;
        public bool invertYOutputValue;
        public bool isLook;
        [Tooltip("Có thể bấm xuyên qua")]
        public bool isCanPressThrough = false;

        //Stored Pointer Values
        private Vector2[] pointerDownPosition = new Vector2[10];
        private Vector2[] currentPointerPosition = new Vector2[10];

        private float ClampMoveValue;

        [Header("Output")]
        public Event touchZoneOutputEvent;

        private int _touchid = -1;
        void Start()
        {
            ClampMoveValue = GameObject.FindObjectOfType<ThirdPersonController>().ClampMoveValue;
            pointerDownPosition[0] = Vector2.zero;
            currentPointerPosition[0] = Vector2.zero;
            SetupHandle();
        }

        private void Update()
        {
            //if (isLook) Debug.Log("Số còn đang chạm là: " + _touchCount);
        }

        private void SetupHandle()
        {
            if (handleRect)
            {
                SetObjectActiveState(handleRect.gameObject, false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isLook)
            {
                _touchid = 15;
                for (int i = 0; i < Input.touchCount; i++) {
                    var t = Input.GetTouch(i);
                    if (t.position == eventData.position)
                    {
                        _touchid = t.fingerId;
                        break;
                    }
                }

                if (_touchid >= 10) return;
            }
            else
                _touchid = 0;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition[_touchid]);

            if (handleRect)
            {
                SetObjectActiveState(handleRect.gameObject, true);
                UpdateHandleRectPosition(pointerDownPosition[_touchid]);
            }

        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isLook)
            {
                _touchid = 15;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (t.position == eventData.position)
                    {
                        _touchid = t.fingerId;
                        break;
                    }
                }

                if (_touchid >= 10) return;
            }
            else
                _touchid = 0;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition[_touchid]);

            Vector2 positionDelta = GetDeltaBetweenPositions(pointerDownPosition[_touchid], currentPointerPosition[_touchid]);

            Vector2 clampedPosition = positionDelta;

            if (isLook)
            {
                UpdateHandleRectPosition(currentPointerPosition[_touchid]);

                //điều chỉnh cách rotate của camera
                pointerDownPosition[_touchid] = currentPointerPosition[_touchid];
            }
            else
            {
                // nếu là di chuyển thì sẽ Clamp giá trị lại


                if (clampedPosition.y > ClampMoveValue*1.5f)
                    clampedPosition.x = ClampValuesToMagnitude(clampedPosition, ClampMoveValue).x;
                else
                    clampedPosition = ClampValuesToMagnitude(clampedPosition, ClampMoveValue);
            }

            Vector2 outputPosition = ApplyInversionFilter(clampedPosition);

            OutputPointerEventValue(outputPosition * magnitudeMultiplier);
        }

        // Hàm này thi thoảng sẽ không được gọi vì lag, cần có cách khác để triển khai
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isLook)
            {
                _touchid = 15;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (t.position == eventData.position)
                    {
                        _touchid = t.fingerId;
                        break;
                    }
                }

                if (_touchid >= 10) return;
            }
            else
                _touchid = 0;

            pointerDownPosition[_touchid] = Vector2.zero;
            currentPointerPosition[_touchid] = Vector2.zero;

            OutputPointerEventValue(Vector2.zero);

            if (handleRect)
            {
                SetObjectActiveState(handleRect.gameObject, false);
                UpdateHandleRectPosition(Vector2.zero);
            }
        }

        void OutputPointerEventValue(Vector2 pointerPosition)
        {
            touchZoneOutputEvent.Invoke(pointerPosition);
        }

        void UpdateHandleRectPosition(Vector2 newPosition)
        {
            handleRect.anchoredPosition = newPosition;
        }

        void SetObjectActiveState(GameObject targetObject, bool newState)
        {
            targetObject.SetActive(newState);
        }

        Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
        {
            return secondPosition - firstPosition;
        }

        Vector2 ClampValuesToMagnitude(Vector2 position, float ClampValue)
        {
            return Vector2.ClampMagnitude(position, ClampValue);
        }

        Vector2 ApplyInversionFilter(Vector2 position)
        {
            if (invertXOutputValue)
            {
                position.x = InvertValue(position.x);
            }

            if (invertYOutputValue)
            {
                position.y = InvertValue(position.y);
            }

            return position;
        }

        float InvertValue(float value)
        {
            return -value;
        }

    }

}