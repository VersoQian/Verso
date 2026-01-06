using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class RawCameraView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private RectTransform _spawnTransform;

        public Camera Camera => _camera;

        public Vector3 AnchorToWorldPosition(float anchorX, float anchorY)
        {
            var resultX = Mathf.Lerp(0f, _spawnTransform.rect.max.x, anchorX);
            var resultY = Mathf.Lerp(0f, _spawnTransform.rect.max.y, anchorY);
            var anchorPosition = new Vector2(resultX, resultY);

            return _spawnTransform.TransformPoint(anchorPosition);
        }
    }
}

