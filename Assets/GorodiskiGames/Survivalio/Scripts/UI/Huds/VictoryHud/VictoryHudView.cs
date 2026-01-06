using Game.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class VictoryHudView : BaseHudWithModel<LevelModel>
    {
        private const string _levelFormat = "LEVEL {0}";
        private const string _enemiesKilledFormat = "{0} {1}";

        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _enemiesKilledText;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GridLayoutGroup _layoutGroup;

        public RectTransform Content => _content;
        public GridLayoutGroup LayoutGroup => _layoutGroup;
        public Button ConfirmButton => _confirmButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(LevelModel model)
        {
            var levelNice = model.Level + 1;
            _levelText.text = string.Format(_levelFormat, levelNice);
            _enemiesKilledText.text = string.Format(_enemiesKilledFormat, GameConstants.EnemyIcon, model.EnemiesKilled);
        }
    }
}

