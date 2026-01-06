using System;
using Game.Core;
using Injection;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace Game.Managers
{
    public sealed class iOSAuthorizationTrackingManager : IDisposable
    {
        [Inject] private Timer _timer;

        public delegate void TrackingAuthorizationStatusHandler(int status);

#if UNITY_IOS
        public event TrackingAuthorizationStatusHandler OnTrackingAuthorizationStatusReceived;
#endif
        public void Initialize()
        {
            RequestTrackingAuthorization();
        }

        public void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        public void RequestTrackingAuthorization()
        {
#if UNITY_IOS

            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                _timer.TICK += OnTick;
            }
            else
            {
                OnTrackingAuthorizationStatusReceived?.Invoke((int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
            }
#endif
        }

        private void OnTick()
        {
#if UNITY_IOS
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if(status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                return;

            _timer.TICK -= OnTick;

            OnTrackingAuthorizationStatusReceived?.Invoke((int)ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
#endif
        }
    }
}
