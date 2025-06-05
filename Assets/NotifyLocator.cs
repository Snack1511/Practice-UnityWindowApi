using System;
using System.Collections.Generic;

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

        public static void RegistLocate(System.Type type, Action<INotifiedValue> delg)
        {
            if(!locators.ContainsKey(type))
                locators.Add(type, );
        }

        public static void UnRegistLocate(System.Type type, NotifiedAction delg)
        {
        }
    }
}