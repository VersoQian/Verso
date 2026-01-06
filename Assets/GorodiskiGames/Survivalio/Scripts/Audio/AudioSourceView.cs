using UnityEngine;

namespace Game.Audio
{
    public sealed class AudioSourceView : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;

        public AudioSource Source => _source;
    }
}

