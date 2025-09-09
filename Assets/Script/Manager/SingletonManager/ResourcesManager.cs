using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using pattern;
using UnityEngine;

namespace Script.Manager.SingletonManager
{
    public class ResourcesManager : MonoSingleton<ResourcesManager>
    {
        private Dictionary<Type, Dictionary<string, UnityEngine.Object>> dictionary = new();

        public void Initialize()
        {

        }

        public void Release()
        {
            dictionary?.Clear();
        }

        public async UniTask<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            string filePath = path;
            Type type = typeof(T);
            if (!dictionary.ContainsKey(type))
            {
                dictionary.Add(type, new Dictionary<string, UnityEngine.Object>());
            }

            string ext = Path.GetExtension(filePath);
            string loadPath = filePath.Replace(ext, "");
            string assetName = Path.GetFileNameWithoutExtension(filePath);
            if (!dictionary[type].ContainsKey(assetName))
            {

                var request = Resources.LoadAsync<T>(loadPath);
                await request;

                if (null == request.asset)
                {
                    Debug.LogError($"Not found resource by path : {path}");
                    return null;
                }
                else
                {
                    dictionary[type].Add(assetName, request.asset);
                }
            }


            return dictionary[type][assetName] as T;
        }


    }
}