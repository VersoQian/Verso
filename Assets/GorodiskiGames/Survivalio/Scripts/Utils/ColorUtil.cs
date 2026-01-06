using  UnityEngine;

namespace Utilities
{
    public static class ColorUtil
    {
        public static Color GetDisabledColor(Color c)
        {
            return new Color(c.r, c.g, c.b, .5f);
        }

        public static Color GetEnabledColor(Color c)
        {
            return new Color(c.r, c.g, c.b, 1f);
        }

        public static string ColorString(string text, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
        }

        public static Color HEXToColor(string color)
        {
            ColorUtility.TryParseHtmlString(color, out Color result);
            return result;
        }

        public static Color HEXToColor(string color, float alpha)
        {
            var result = HEXToColor(color);
            result.a = alpha;
            return result;
        }
    }
}