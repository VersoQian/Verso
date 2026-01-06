using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public class SkillPauseSlotModel : Observable
    {
        public Sprite Icon;
        public int Count;

        public SkillPauseSlotModel(Sprite icon, int count)
        {
            Icon = icon;
            Count = count;
        }
    }

    public sealed class SkillPauseSlotView : BaseHudWithModel<SkillPauseSlotModel>
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image[] _stars;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(SkillPauseSlotModel model)
        {
            var icon = model.Icon;
            var count = model.Count;

            _icon.gameObject.SetActive(icon != null);
            _icon.sprite = icon;

            count--;
            foreach (var star in _stars)
            {
                var index = _stars.ToList().IndexOf(star);

                var fade = 1f;
                if (index > count)
                    fade = 0f;

                var color = star.color;
                star.color = new Color(color.r, color.g, color.b, fade);
            }
        }
    }
}

