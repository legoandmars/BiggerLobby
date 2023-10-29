using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BigLobby
{
    public static class Helper
    {
        public static T[] ResizeArray<T>(T[] oldArray, int newSize) {
            var newArray = new T[newSize];
            oldArray.CopyTo(newArray, 0);
            return newArray;
        }
    }
}