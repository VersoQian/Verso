using System.Collections.Generic;
using Game.Controls;
using Game.Inputs;
using Game.Player;
using Game.UI.Hud;
using Game.Weapon;
using UnityEngine;

namespace Game.UI
{
    public sealed class GameView : MonoBehaviour
    {
        public BaseHud[] Huds;
        public Canvas Canvas;
        public Joystick Joystick;
        public PlayerView PlayerView;
        public WeaponView WeaponView;
        public Transform BarsHolder;
        public CameraController Camera;
        public GameObject MenuLight;
        public GameObject LevelLight;
        public GameInput GameInput;

        public IEnumerable<IHud> AllHuds()
        {
            foreach (var hud in Huds)
            {
                yield return hud;
            }
        }
    }
}

