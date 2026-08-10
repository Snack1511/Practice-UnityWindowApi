using Cysharp.Threading.Tasks;

namespace Script.GameFlow.Command
{
    /// <summary>
    /// 큐에 실려 순차 실행되는 명령. 씬 로딩 중 다음 씬에 필요한 오브젝트를 만드는 데 쓴다.
    /// 로딩은 비동기라 반환형이 UniTask 다.
    /// </summary>
    public interface ICmdBase
    {
        UniTask Execute();
    }

    /// <summary>
    /// 공통 처리가 생길 자리. 지금은 비어 있으므로 인터페이스만 구현해도 무방하다.
    /// </summary>
    public abstract class CmdBase : ICmdBase
    {
        public abstract UniTask Execute();
    }
}
