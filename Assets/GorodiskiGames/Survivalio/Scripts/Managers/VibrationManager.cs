using Game.Core;

namespace Game.Managers
{
    public sealed class VibrationManager
    {
        private bool _isVibration;

        public void Initialize(bool isVibration)
        {

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            Vibration.Init();
#endif

            SetVibration(isVibration);
        }

        public void SetVibration(bool isVibration)
        {
            _isVibration = isVibration;
        }

        public void VibrateNope()
        {
            if (!_isVibration)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            Vibration.VibrateNope();
#elif UNITY_IOS && !UNITY_EDITOR
            Vibration.VibrateIOS(NotificationFeedbackStyle.Error);
#endif
        }

        public void Vibrate()
        {
            if (!_isVibration)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            Vibration.VibrateAndroid(5);
#elif UNITY_IOS && !UNITY_EDITOR
            Vibration.VibrateIOS(ImpactFeedbackStyle.Light);
#endif
        }
    }
}

