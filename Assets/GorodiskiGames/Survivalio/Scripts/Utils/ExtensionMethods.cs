using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Game.Utilities
{
    public static class ExtensionMethods
    {
        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            var count = collection.Count();
            if (count != 0)
            {
                return collection.ElementAt(MathUtil.Random(0, count - 1));
            }
            return default(T);
        }

        public static T GetRandom<T>(this IEnumerable<T> collection, Random random)
        {
            var count = collection.Count();
            if (count != 0)
            {
                return collection.ElementAt(random.Next(0, count));
            }
            return default(T);
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Queue<T> lastElements = new Queue<T>();
            foreach (T element in source)
            {
                lastElements.Enqueue(element);
                if (lastElements.Count > count)
                {
                    lastElements.Dequeue();
                }
            }

            return lastElements;
        }
    }
}