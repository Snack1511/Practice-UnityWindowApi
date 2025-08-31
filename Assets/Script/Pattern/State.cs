namespace pattern
{
    public interface IState
    {
        bool Enter();
        void Exit();
        void Update();
        
        void EnterAsync();
        void ExitAsync();
    }
}