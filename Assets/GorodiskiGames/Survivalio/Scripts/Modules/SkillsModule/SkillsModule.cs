using Game.Core;
using Game.Managers;
using Game.UI.Hud;
using Injection;

namespace Game.Modules
{
    public sealed class SkillsModule : Module
    {
        [Inject] private LevelManager _levelManager;
        [Inject] private HudManager _hudManager;

        public override void Initialize()
        {
            _levelManager.ON_REACH_SKILL_MILESTONE += OnReachSkillMilestone;
        }

        public override void Dispose()
        {
            _levelManager.ON_REACH_SKILL_MILESTONE -= OnReachSkillMilestone;
        }

        private void OnReachSkillMilestone()
        {
            _hudManager.ShowSingle<SkillSelectionHudMediator>();
        }
    }
}

