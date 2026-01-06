using Game.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class PlayerStatsHudView : BaseHudWithModel<PlayerModel>
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _playerLabelText;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(PlayerModel model)
        {
            _icon.sprite = model.Icon;
            _playerLabelText.text = model.Label;
        }
    }
}

