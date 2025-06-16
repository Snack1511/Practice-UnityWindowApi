using UnityEngine.PlayerLoop;

namespace Manager
{
    public static class GameProcessManager
    {
        public static GameProcess ProcessObject { get; private set; }

        private const string GameProcessPath = "";
        public static void Initialize(Loader loader)
        {
            loader.Load<GameProcess>(GameProcessPath, SetProcessObject);
        }

        private static void SetProcessObject(string key, GameProcess gameProcess)
        {
            ProcessObject = gameProcess;
        }
    }
}