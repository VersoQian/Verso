using UnityEngine;

namespace Game.Level
{
    public sealed class GroundView : MonoBehaviour
    {
        public int Index { get; internal set; }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
    }
}

