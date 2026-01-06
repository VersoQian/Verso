using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class ExtendedScrollbar : Scrollbar
    {
        public override void OnPointerDown(PointerEventData eventData)
        {
            var screenPosition = eventData.pointerPressRaycast.screenPosition;

            var handle = GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(handle, screenPosition,
                eventData.enterEventCamera, out var localMousePos);

            value = Mathf.Clamp01((localMousePos.y - handle.rect.yMin) / handle.rect.height);
        }
    }
}