using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Manager;
using Script.GameFlow;

namespace Script.Manager.StaticManager
{
    
    
    public static class GameProcessManager
    {
        public static GameProcess ProcessObject { get; private set; }
        
        private static Dictionary<string, Action> updaters = new Dictionary<string, Action>();
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

        public static void Release()
        {
        }

        private static void SetProcessObject(string key, GameProcess gameProcess)
        {
            GameProcess processObject= UnityEngine.GameObject.Instantiate<GameProcess>(gameProcess);
            ProcessObject = processObject;
        }

        public static void AddUpdate(string key, Action update)
        {
            updaters.TryAdd(key, update);
        }

        public static void Update()
        {
            foreach (var keyValuePair in updaters)
            {
                keyValuePair.Value.Invoke();
            }
        }
    }
}