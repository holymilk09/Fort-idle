using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FortIdle
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Vector2 Value { get; private set; }

        RectTransform baseRect;
        RectTransform knob;
        float travel;
        float deadzone;
        bool dragging;

        public void Configure(RectTransform baseRectTransform, RectTransform knobRect, float travelPixels, float deadzoneValue)
        {
            baseRect = baseRectTransform;
            knob = knobRect;
            travel = travelPixels;
            deadzone = deadzoneValue;
            Value = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (baseRect == null || travel <= 0f)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                baseRect, eventData.position, eventData.pressEventCamera, out Vector2 local);
            Vector2 clamped = Vector2.ClampMagnitude(local, travel);
            knob.anchoredPosition = clamped;
            Vector2 raw = clamped / travel;
            Value = raw.magnitude < deadzone ? Vector2.zero : raw;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
            Value = Vector2.zero;
            if (knob != null)
                knob.anchoredPosition = Vector2.zero;
        }

        public void SetDemoValue(Vector2 value)
        {
            if (dragging || knob == null)
                return;

            Vector2 clamped = Vector2.ClampMagnitude(value, 1f);
            Value = clamped.magnitude < deadzone ? Vector2.zero : clamped;
            knob.anchoredPosition = Value * travel;
        }
    }
}
