using System.Collections.Generic;

namespace MazeGamePillaPilla
{
    static class ExtensionMethods
    {
        public static void AddIfNotNull<T>(this IList<T> list, T item)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
    }
}
