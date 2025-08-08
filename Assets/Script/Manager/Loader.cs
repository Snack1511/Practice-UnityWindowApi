
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Extension.Collection;
using UnityEditor.iOS;
using UnityEngine;

namespace Manager
{
    using System;
    using pattern;
    
    public interface ILoader
    {
        public UniTask AsyncLoad();
        public void Clear();
    }
    public class Loader<T> : ILoader  where T : UnityEngine.Object
    {
        
        public string loadPath;
        public Action<string, T> completeCallback;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        
        public async UniTask AsyncLoad()
        {
            string dirName = Path.GetDirectoryName(loadPath);
            string cutOffExt = Path.GetFileNameWithoutExtension(loadPath);
            string path = Path.Combine(dirName, cutOffExt);
            
            ResourceRequest request = Resources.LoadAsync(path, typeof(T));
            await UniTask.WaitUntil(()=>request.isDone, cancellationToken: cancellationTokenSource.Token);
            
            completeCallback?.Invoke(path, request.asset as T);
            completeCallback = null;
            //LoadManager.RemoveLoad(loadPath);
            Clear();
        }

        public void Clear()
        {
            if (null == cancellationTokenSource) return;
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

    // public static class LoadManager// : Singleton<LoadManager>
    // {
    //     private static Dictionary<string, ILoader> taskMap;
    //     //private CancellationTokenSource  cancellationTokenSource;
    //     public static void Initialize()
    //     {
    //         taskMap = new();
    //         //cancellationTokenSource = new CancellationTokenSource();
    //     }
    //
    //     public static void Release()
    //     {
    //         if (taskMap.IsNullOrEmpty())
    //             return;
    //         
    //         foreach (var loader in taskMap)
    //         {
    //             loader.Value.Clear();
    //         }
    //         taskMap.Clear();
    //     }
    //
    //     public static void RequestLoad<T>(Loader<T> loader) where T : UnityEngine.Object
    //     {
    //         if (!taskMap.ContainsKey(loader.loadPath))
    //         {
    //             taskMap.Add(loader.loadPath, loader);
    //         }
    //
    //         taskMap[loader.loadPath].AsyncLoad().Forget();
    //     }
    //
    //     public static void RemoveLoad(string path)
    //     {
    //         if(taskMap.ContainsKey(path))
    //             taskMap.Remove(path);
    //     }
    // }
}