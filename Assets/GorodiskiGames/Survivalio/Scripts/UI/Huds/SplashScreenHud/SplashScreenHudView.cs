using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SplashScreenHudView : BaseHud
    {
        [SerializeField] private GameObject _fillBarHolder;
        [SerializeField] private Image _fillBarImage;
        [SerializeField] private TMP_Text _deviceIDText;
        [SerializeField] private Image _imageBg;
        [SerializeField] private AspectRatioFitter _imageBgAspectRatio;
        [SerializeField] private Sprite _icon16x9;
        [SerializeField] private Sprite _icon1x1;

        public GameObject FillBarHolder => _fillBarHolder;
        public Image FillBarImage => _fillBarImage;
        public TMP_Text DeviceIDText => _deviceIDText;
        public Image ImageBg => _imageBg;
        public AspectRatioFitter ImageBgAspectRatio => _imageBgAspectRatio;
        public Sprite Icon16x9 => _icon16x9;
        public Sprite Icon1x1 => _icon1x1;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {
            
        }
    }
}