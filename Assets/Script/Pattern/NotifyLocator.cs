using System;
using System.Collections.Generic;
using UnityEngine;

// TODO : 여기부터
// 일단 OutboundNotifier 마무리 짓고
// 대충 NotifyLocator를 통해 타이밍과 액션 관리
// 장애물 오브젝트 풀링 처리 제대로 되는지 확인해봐야함
namespace pattern
{
    public interface INotifiedValue
    {
    }

    public static class NotifyLocator
    {
        public delegate void NotifiedAction (INotifiedValue  notifiedValue);
        private static Dictionary<System.Type, NotifiedAction> locators = new Dictionary<System.Type, NotifiedAction>();

        public static void RegistLocate<T>(NotifiedAction delg) where T : class, INotifiedValue
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