using UnityEngine;

namespace pattern
{
    public class Singleton<T>  where T : new()
    {
        public static T Instance
        {
            get
            {
                if(null == instance)
                    instance =  new T();
                return instance;
            }
        }
        
        private static T instance;
    }
    
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance
        {
            get
            {
                if (null == instance)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<T>();
                }
                return instance;
            }
        }
        
        private static T instance;
    }
}