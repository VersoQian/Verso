using UnityEngine;

namespace Utilities
{
    public static class PlatformUtils
    {
        /// <summary>
        /// Returns true if the game is running on a mobile platform (Android or iOS).
        /// </summary>
        public static bool IsMobile
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
                return false;
#endif
            }
        }
    }
}

