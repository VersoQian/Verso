using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public abstract class RawCameraHudMediator<T> : Mediator<T> where T : IHud
    {
        private const float _height = 3000f;

        [Inject] protected ResourcesManager _resourcesManager;

        protected RawCameraView _rawCamera;
        private RawImage _rawImage;

        public void SetViewFromCamera(RawImage rawImage, out RawCameraView rawCameraView)
        {
            _rawImage = rawImage;

            var prefab = _resourcesManager.LoadRawCameraPrefab();
            var result = GameObject.Instantiate(prefab).GetComponent<RawCameraView>();

            var rawImagePosition = rawImage.rectTransform.position;
            result.transform.position = rawImagePosition + Vector3.up * _height;

            var texture = new RenderTexture(Screen.width, Screen.height, 24);
            result.Camera.targetTexture = texture;

            rawCameraView = result;

            rawImage.texture = _rawCamera.Camera.targetTexture;
            rawImage.gameObject.SetActive(true);
        }

        public void DestroyRawCamera()
        {
            _rawImage.gameObject.SetActive(false);
            GameObject.Destroy(_rawCamera.gameObject);
        }
    }
}

