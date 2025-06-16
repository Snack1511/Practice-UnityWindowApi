
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Extension.Collection;
using UnityEditor.iOS;
using UnityEngine;

namespace Manager
{
    using System;

    public interface ILoader
    {
        public UniTask RequestLoad(CancellationToken token);
    }

    public class Loader<T> : ILoader  where T : UnityEngine.Object
    {
        private string key;
        private string loadPath;
        private Action<string, T> completeCallback;
        public void Load(string loadPath, Action<string, T> callback)
        {
            key = loadPath;
            completeCallback = callback;
            
            LoadManager.RegistLoadTask(key, this);
        }
        
        UniTask ILoader.RequestLoad(CancellationToken token)
        {
            return LoadProcess(token);
        }
        
        private async UniTask LoadProcess(CancellationToken token)
        {
            ResourceRequest request = Resources.LoadAsync(loadPath);
            await UniTask.WaitUntil(()=>request.isDone, cancellationToken: token);
            
            completeCallback?.Invoke(loadPath, request.asset as T);
        }
    }

    public static class LoadManager
    {
        private static Dictionary<string, CancellationTokenSource> taskMap;
        public static void Release()
        {
            if (taskMap.IsNullOrEmpty())
                return;
            
            foreach (var cancellationTokenSource in taskMap)
            {
                cancellationTokenSource.Value.Cancel();
                cancellationTokenSource.Value.Dispose();
            }
            taskMap.Clear();
        }

        public static void RegistLoadTask(string key, ILoader task)
        {
            taskMap ??= new();
            taskMap.Add(key, new CancellationTokenSource());
            task.RequestLoad(taskMap[key].Token).Forget();
            //TODO : 캔슬토큰은 어디서 관리하냐;;
        }

    }
}