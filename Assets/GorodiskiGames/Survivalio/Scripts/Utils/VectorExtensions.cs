using UnityEngine;

namespace Utilities
{
    public static class VectorExtensions
    {
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector2 CutToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static bool IsCollided(Vector3 posA, float radiusA, Vector3 posB, float radiusB)
        {
            float distanceSqr = (posA - posB).sqrMagnitude;
            float combinedRadius = radiusA + radiusB;
            return distanceSqr <= combinedRadius * combinedRadius;
        }
    }
}