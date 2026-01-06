using System;
using UnityEngine;

namespace Game.Config
{
    public enum MenuHudType
    {
        Shop = 0,
        Equipment = 1,
        Battle = 2,
        Third = 3,
        Fourth = 4
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/MenuHudConfig")]
    public sealed class MenuHudConfig : ScriptableObject
    {
        public MenuHudType Type;
        [Tooltip("The level at which the menu will be unlocked.")]
        [Min(0)] public int UnlockAtLevel;
        public string Label;
        public Sprite Icon;
    }
}

