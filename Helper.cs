using System.Collections.Generic;
using System.Linq;

namespace BiggerLobby
{
    public static class Helper
    {
        public static T[] ResizeArray<T>(T[] oldArray, int newSize) {
            var newArray = new T[newSize];
            oldArray.CopyTo(newArray, 0);
            return newArray;
        }
        public static void ResizeList<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }
    }
}
