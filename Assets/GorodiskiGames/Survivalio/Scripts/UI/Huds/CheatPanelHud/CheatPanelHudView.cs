using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class CheatPanelHudView : BaseHudWithModel<GameModel>
    {
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private TMP_Text _gemsText;
        [SerializeField] private TMP_Text _cashText;
        [SerializeField] private Button _addEnergyButton;
        [SerializeField] private TMP_Text _addEnergyButtonText;
        [SerializeField] private Button _removeEnergyButton;
        [SerializeField] private TMP_Text _removeEnergyButtonText;
        [SerializeField] private Button _addGemsButton;
        [SerializeField] private TMP_Text _addGemsButtonText;
        [SerializeField] private Button _addCashButton;
        [SerializeField] private TMP_Text _addCashButtonText;

        public Button ResetButton => _resetButton;
        public Button CloseButton => _closeButton;
        public Button AddEnergyButton => _addEnergyButton;
        public TMP_Text AddEnergyButtonText => _addEnergyButtonText;
        public Button RemoveEnergyButton => _removeEnergyButton;
        public TMP_Text RemoveEnergyButtonText => _removeEnergyButtonText;
        public Button AddGemsButton => _addGemsButton;
        public TMP_Text AddGemsButtonText => _addGemsButtonText;
        public Button AddCashButton => _addCashButton;
        public TMP_Text AddCashButtonText => _addCashButtonText;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(GameModel model)
        {
            _energyText.text = MathUtil.NiceCash(model.Energy);
            _gemsText.text = MathUtil.NiceCash(model.Gems);
            _cashText.text = MathUtil.NiceCash(model.Cash);
        }
    }
}