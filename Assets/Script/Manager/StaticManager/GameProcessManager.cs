using Cysharp.Threading.Tasks;
using Manager;
using Script.GameFlow;

namespace Script.Manager.StaticManager
{
    public static class GameProcessManager
    {
        public static GameProcess ProcessObject { get; private set; }

        //TODO : ScriptableObject 로드 처리하기
        private const string GameProcessPath = "Prefabs/GameProcess.prefab";
        public static void Initialize()
        {
            Loader<GameProcess> loader = new()
            {
                loadPath =  GameProcessPath,
                completeCallback = SetProcessObject
            };
            loader.AsyncLoad().Forget();
        }

        private static void SetProcessObject(string key, GameProcess gameProcess)
        {
            GameProcess processObject= UnityEngine.GameObject.Instantiate<GameProcess>(gameProcess);
            ProcessObject = processObject;
        }
    }
}