using Game.Player;
using TMPro;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class AttributeSlotView : MonoBehaviour
    {
        [SerializeField] private UnitAttributeType _type;
        [SerializeField] private TMP_Text _infoText;

        public UnitAttributeType Type => _type;

        public void SetValue(float value)
        {
            var result = (value % 1f == 0f)
                ? value.ToString("0")
                : value.ToString("0.0");

            _infoText.text = result;
        }
    }
}

