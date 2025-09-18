using System;
using System.Collections.Generic;
using UnityEngine;

namespace pattern
{
    public interface INotifiedValue
    {
    }
    
    // ServiceLocator의 형태로, EventBus시스템을 구현...너무 전역적인대..
    public static class EventBusLocater
    {
        public delegate void NotifiedAction (INotifiedValue  notifiedValue);
        private static Dictionary<System.Type, NotifiedAction> locators = new Dictionary<System.Type, NotifiedAction>();

        public static void RegistService<T>(NotifiedAction delg) where T : class, INotifiedValue
        {
            var targetType = typeof(T);
            if(!locators.ContainsKey(targetType))
                locators.Add(targetType , null);

            locators[targetType] += delg;
        }

        public static void UnRegistLocate<T>(NotifiedAction delg) where T : class, INotifiedValue
        {
            var targetType = typeof(T);
            if(locators.ContainsKey(targetType))
                locators[targetType] -= delg;
        }

        public static void Notify(INotifiedValue notifiedValue)
        {
            Type targetType = notifiedValue.GetType();
            Debug.Log(targetType.FullName + " Notify");
            if(locators.ContainsKey(targetType))
                locators[targetType].Invoke(notifiedValue);
        }
    }
}