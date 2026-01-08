using System;
using Game.Managers;
using Game.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class GamePlayHudView : BaseHudWithModel<LevelModel>
    {
        private const string _levelWord = "LVL";
        private const string _levelFormat = "{0} {1}";

        [SerializeField] private Button _pauseButton;
        [SerializeField] private TMP_Text _elapsedText;
        [SerializeField] private TMP_Text _creditText;  // 原_cashText
        [SerializeField] private TMP_Text _enemiesKilledText;
        [SerializeField] private GameObject _progressHolder;
        [SerializeField] private Image _progressImage;
        [SerializeField] private TMP_Text _levelText;

        public Button PauseButton => _pauseButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(LevelModel model)
        {
            var isReachSpawnBossTime = model.IsReachSpawnBossTime;
            _elapsedText.gameObject.SetActive(!isReachSpawnBossTime);
            _progressHolder.gameObject.SetActive(!isReachSpawnBossTime);

            var elapsed = TimeSpan.FromSeconds(model.Elapsed);
            _elapsedText.text = elapsed.DateToMMSS();
            _creditText.text = MathUtil.NiceCash(model.Credit);
            _enemiesKilledText.text = model.EnemiesKilled.ToString();
            _progressImage.fillAmount = (float)model.GemsGreen / model.ResourcesAmount;
            var levelNice = model.Level + 1;
            _levelText.text = string.Format(_levelFormat, _levelWord, levelNice);
        }
    }
}

