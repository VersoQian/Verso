using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public class GameStatsHudView : BaseHudWithModel<GameModel>
    {
        private const string _energyFormat = "{0}/{1}";

        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private TMP_Text _energyTimeText;
        [SerializeField] private Button _addEnergyButton;
        [SerializeField] private TMP_Text _passCardText;  // 原_gemsText
        [SerializeField] private Button _addPassCardButton;  // 原_addGemsButton
        [SerializeField] private TMP_Text _creditText;  // 原_cashText
        [SerializeField] private Button _addCreditButton;  // 原_addCashButton

        public TMP_Text EnergyTimeText => _energyTimeText;
        public Button AddEnergyButton => _addEnergyButton;
        public Button AddPassCardButton => _addPassCardButton;  // 原AddGemsButton
        public Button AddCreditButton => _addCreditButton;  // 原AddCashButton

        public int EnergyMax { get; internal set; }

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(GameModel model)
        {
            _energyText.text = string.Format(_energyFormat, model.Energy, EnergyMax);
            _passCardText.text = MathUtil.NiceCash(model.PassCard);
            _creditText.text = MathUtil.NiceCash(model.Credit);
        }
    }
}

