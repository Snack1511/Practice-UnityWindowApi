using UnityEngine;

namespace MonoExtensions
{
    

    public static class ComponentExtension
    {

        public static T AddOrGetComponent<T>(this Component callerComponent) where T : Component
        {
            if (callerComponent is null || callerComponent.gameObject is null)
                return null;

            var targetObject = callerComponent.gameObject;
            
            var targetComponent = targetObject.GetComponent<T>() ?? targetObject.AddComponent<T>();

            return targetComponent;
        }
    }

}