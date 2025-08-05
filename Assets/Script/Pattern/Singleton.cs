namespace pattern
{
    public class Singleton<T>  where T : new()
    {
        public T Instance
        {
            get
            {
                if(null == instance)
                    instance =  new T();
                return instance;
            }
        }
        
        private T instance;
    }
}