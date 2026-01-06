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
        [SerializeField] private TMP_Text _gemsText;
        [SerializeField] private Button _addGemsButton;
        [SerializeField] private TMP_Text _cashText;
        [SerializeField] private Button _addCashButton;

        public TMP_Text EnergyTimeText => _energyTimeText;
        public Button AddEnergyButton => _addEnergyButton;
        public Button AddGemsButton => _addGemsButton;
        public Button AddCashButton => _addCashButton;

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
            _gemsText.text = MathUtil.NiceCash(model.Gems);
            _cashText.text = MathUtil.NiceCash(model.Cash);
            _cashText.text = model.Cash.ToString();
        }
    }
}

