using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public class LeaveGameplayHudView : BaseHud
    {
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Button _continueButton;

        public Button LeaveButton => _leaveButton;
        public Button ContinueButton => _continueButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}

