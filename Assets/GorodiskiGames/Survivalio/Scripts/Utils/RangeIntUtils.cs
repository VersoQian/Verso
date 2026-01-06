using UnityEngine;

namespace Game.Utilities
{
    public static class RangeIntUtils
    {
        public static bool RangeContains(this RangeInt range, int target)
        {
            if (target >= range.start && target <= range.end)
            {
                return true;
            }
            return false;
        }
    }
}