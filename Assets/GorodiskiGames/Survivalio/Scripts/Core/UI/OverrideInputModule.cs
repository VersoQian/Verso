using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Core
{
    public sealed class OverrideInputModule : StandaloneInputModule
    {
        public void ClickAt(float x, float y)
        {
            Input.simulateMouseWithTouches = true;

            var pointerData = GetTouchPointerEventData(new Touch()
            {
                position = new Vector2(x, y),
            }, out bool b, out bool bb);

            ProcessTouchPress(pointerData, true, true);
        }

        public void DragAtTo(float x, float y, float x1, float y1, GameObject gameObject)
        {
            Input.simulateMouseWithTouches = true;

            var pointerData = GetTouchPointerEventData(new Touch()
            {
                position = new Vector2(x, y),
            }, out bool b, out bool bb);

            pointerData.delta = new Vector2(x + x1, y + y1);
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.pressPosition = new Vector2(x + x1, y + y1);

            pointerData.pointerDrag = gameObject;
            pointerData.pointerEnter = gameObject;

            ProcessDrag(pointerData);
        }

        public void EndDrag(float x, float y, float x1, float y1, GameObject gameObject)
        {
            var pointerData = GetTouchPointerEventData(new Touch()
            {
                position = new Vector2(x, y),
            }, out bool b, out bool bb);

            pointerData.delta = new Vector2(x + x1, y + y1);
            pointerData.pressPosition = new Vector2(x + x1, y + y1);

            pointerData.dragging = true;
            pointerData.useDragThreshold = true;

            pointerData.pointerId = -1;
            pointerData.eligibleForClick = false;

            pointerData.pointerDrag = gameObject;
            pointerData.pointerEnter = gameObject;

            ProcessTouchPress(pointerData, false, true);
        }
    }
}