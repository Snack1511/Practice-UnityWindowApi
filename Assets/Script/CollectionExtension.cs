using System.Collections.Generic;
using UnityEngine;

namespace CollectionExtension
{
    public static class CollectionExtention
    {
        public static bool IsInRange(this System.Collections.IList list, int index)
        {
            if (list.IsNullOrEmpty())
                return false;
            
            return list.Count > index && index >= 0;
        }
        public static bool IsNullOrEmpty(this System.Collections.ICollection collection)
        {
            return collection is null || collection.Count <= 0;
        }
    }

}