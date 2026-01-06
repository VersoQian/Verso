using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public enum SkillType
    {
        Weapon,
        Ability
    }

    public sealed class SkillSelectionSlotView : ButtonSelectableView
    {
        private const float _fade = 0.5f;

        [SerializeField] private Sprite _weaponBGIcon;
        [SerializeField] private Sprite _abilityBGIcon;
        [SerializeField] private Image _iconBG;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Image[] _stars;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {
            KillTween();
        }

        public void Initialize(SkillType type, Sprite icon, string label, int count)
        {
            var iconBG = _weaponBGIcon;
            if(type == SkillType.Ability)
                iconBG = _abilityBGIcon;

            _iconBG.sprite = iconBG;
            _icon.sprite = icon;
            _labelText.text = label;

            var countResult = count - 1;

            foreach (var star in _stars)
            {
                var index = _stars.ToList().IndexOf(star);

                var fade = 1f;
                if (index > countResult)
                    fade = 0f;

                var color = star.color;
                star.color = new Color(color.r, color.g, color.b, fade);

                if (index == countResult)
                {
                    var image = star.GetComponent<Image>();
                    image.DOFade(0f, _fade).SetLoops(-1, LoopType.Yoyo).SetId(this);
                }
            }
        }

        private void KillTween()
        {
            DOTween.Kill(this);
        }
    }
}

