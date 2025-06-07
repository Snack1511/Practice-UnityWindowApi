using System.Collections.Generic;
using UnityEngine;

namespace CollectionExtension
{
    public static class CollectionExtention
    {
        public static bool IsInRange<T>(this List<T> list, int index)
        {
            if (list.IsNullOrEmpty())
                return false;
            
            return list.Count > index && index >= 0;
        }
        public static bool IsNullOrEmpty(this System.Collections.ICollection collection)
        {
            return collection is null || collection.Count <= 0;
        }

        public static T Random<T>(this List<T> list, out int targetIndex)
        {
            targetIndex = -1;
            if(list.IsNullOrEmpty())
                return default;
            
            targetIndex = UnityEngine.Random.Range(0, list.Count);
            return (T)list[targetIndex];
        }
    }

}