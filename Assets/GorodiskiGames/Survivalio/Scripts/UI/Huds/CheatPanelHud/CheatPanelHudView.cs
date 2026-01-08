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
        [SerializeField] private TMP_Text _passCardText;  // 原_gemsText
        [SerializeField] private TMP_Text _creditText;    // 原_cashText
        [SerializeField] private Button _addEnergyButton;
        [SerializeField] private TMP_Text _addEnergyButtonText;
        [SerializeField] private Button _removeEnergyButton;
        [SerializeField] private TMP_Text _removeEnergyButtonText;
        [SerializeField] private Button _addPassCardButton;  // 原_addGemsButton
        [SerializeField] private TMP_Text _addPassCardButtonText;  // 原_addGemsButtonText
        [SerializeField] private Button _addCreditButton;  // 原_addCashButton
        [SerializeField] private TMP_Text _addCreditButtonText;  // 原_addCashButtonText

        public Button ResetButton => _resetButton;
        public Button CloseButton => _closeButton;
        public Button AddEnergyButton => _addEnergyButton;
        public TMP_Text AddEnergyButtonText => _addEnergyButtonText;
        public Button RemoveEnergyButton => _removeEnergyButton;
        public TMP_Text RemoveEnergyButtonText => _removeEnergyButtonText;
        public Button AddPassCardButton => _addPassCardButton;  // 原AddGemsButton
        public TMP_Text AddPassCardButtonText => _addPassCardButtonText;  // 原AddGemsButtonText
        public Button AddCreditButton => _addCreditButton;  // 原AddCashButton
        public TMP_Text AddCreditButtonText => _addCreditButtonText;  // 原AddCashButtonText

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(GameModel model)
        {
            _energyText.text = MathUtil.NiceCash(model.Energy);
            _passCardText.text = MathUtil.NiceCash(model.PassCard);
            _creditText.text = MathUtil.NiceCash(model.Credit);
        }
    }
}