using System.Collections.Generic;
using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;
using Game.Managers;
using Utilities;

namespace Game.Modules
{
    public sealed class UINotificationModule : Module<UINotificationModuleView>
    {
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;

        private readonly Dictionary<UINotificationColorType, Color> _colorMap;

        public UINotificationModule(UINotificationModuleView view) : base(view)
        {
            _colorMap = new Dictionary<UINotificationColorType, Color>();
        }

        public override void Initialize()
        {
            _colorMap[UINotificationColorType.White] = Color.white;
            _colorMap[UINotificationColorType.Yellow] = ColorUtil.HEXToColor(GameConstants.YellowColorHex);
            _colorMap[UINotificationColorType.Red] = ColorUtil.HEXToColor(GameConstants.RedColorHex);

            _gameManager.ON_SPAWN_NOTIFICATION_POPUP += OnSpawnNotificationPopUp;
        }

        public override void Dispose()
        {
            _gameManager.ON_SPAWN_NOTIFICATION_POPUP -= OnSpawnNotificationPopUp;

            _view.PopupPool.ReleaseAllInstances();
            _colorMap.Clear();
        }

        private void OnSpawnNotificationPopUp(string info, Vector3 position, UINotificationColorType colorType)
        {
            var view = _view.PopupPool.Get<UINotificationPopupView>();

            var positionResult = _gameView.Camera.Camera.WorldToScreenPoint(position);
            var color = _colorMap[colorType];

            view.ON_END += OnEnd;
            view.Initialize(info, color, positionResult);
        }

        private void OnEnd(UINotificationPopupView view)
        {
            _view.PopupPool.Release(view);
        }
    }
}

