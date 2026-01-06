using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Utilities
{
    public class EnumUtils
    {
        private static readonly Dictionary<Type, int> EnumCount = new Dictionary<Type, int>();

        public static int GetCount<T>()
        {
            int result = 0;
            var type = typeof(T);
            if (type.IsEnum)
            {
                if (!EnumCount.TryGetValue(type, out result))
                {
                    result = Enum.GetValues(type).Length;
                    EnumCount.Add(type, result);
                }
            }
            return result;
        }

        public static TEnum[] GetValues<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray<TEnum>();
        }

        public static TEnum Parse<TEnum>(object value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value.ToString(), true);
        }

        public static bool TryParse<TEnum>(object value, out TEnum result)
        {
            if (Enum.GetNames(typeof(TEnum)).Any(x => x.ToLower() == value.ToString().ToLower()))
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), value.ToString(), true);
                return true;
            }

            result = default(TEnum);
            return false;
        }

        public static IEnumerable<Enum> GetFlags(Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }
    }
}
