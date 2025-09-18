using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using pattern;
using Script.Define.ModelDefine;
using Script.Define.SaveDefine;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Script.Manager.SingletonManager
{
    public enum ESaveType
    {
    }

    //컨텐츠 별로
    public class IOManager : Singleton<IOManager>
    {
        private Dictionary<ESaveType, SaveBase> saveDict = new Dictionary<ESaveType, SaveBase>();
        public void Initialize()
        {
            
        }
        public void Release()
        {
            saveDict?.Clear();
        }

        public bool TrySaveData(ESaveType saveType)
        {
            try
            {
                SaveBase saveBase = saveDict.GetValueOrDefault(saveType, default);
                if (null == saveBase) return false;
                
                string filePath = GetFilePath(saveType);
                string folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                string jsonString = JsonConvert.SerializeObject(saveBase, Formatting.Indented);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
                return false;
            }
        }
        
        public bool TryLoadData(ESaveType saveType, out SaveBase saveBase)
        {
            saveBase = null;
            try
            {
                if (null == saveBase) return false;

                string filePath = GetFilePath(saveType);
                string jsonString = File.ReadAllText(filePath);
                object loadObject = JsonConvert.DeserializeObject(jsonString);
                if (!saveDict.ContainsKey(saveType))
                {
                    saveDict.Add(saveType, loadObject as SaveBase);
                }
                else
                {
                    saveDict[saveType] = loadObject as SaveBase;
                }

                saveBase = saveDict[saveType];
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
                return false;
            }

        }
        
        public async UniTask SaveDataAsync(ESaveType saveType, float delayDuration = 0.0f)
        {
            try
            {
                SaveBase saveBase = saveDict.GetValueOrDefault(saveType, default);
                if (null == saveBase) return;
                
                string filePath = GetFilePath(saveType);
                
                string folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string jsonString = JsonConvert.SerializeObject(saveBase, Formatting.Indented);
                await UniTask.WaitForSeconds(delayDuration);
                await File.WriteAllTextAsync(filePath, jsonString);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
            }
        }
        
        public async UniTask<SaveBase> LoadDataAsync(ESaveType saveType, float delayDuration = 0.0f)
        {
            try
            {
                SaveBase saveBase = saveDict.GetValueOrDefault(saveType, default);
                if (null == saveBase) return null;
                
                await UniTask.WaitForSeconds(delayDuration);
                string filePath = GetFilePath(saveType);
                string jsonString = await File.ReadAllTextAsync(filePath);
                object loadObject = JsonConvert.DeserializeObject(jsonString);
                if (!saveDict.ContainsKey(saveType))
                {
                    saveDict.Add(saveType, loadObject as SaveBase);
                }
                else
                {
                    saveDict[saveType] = loadObject as SaveBase;
                }

                return saveDict[saveType];
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        private string GetFilePath(ESaveType saveType)
        {
            string path = Application.dataPath;
            path = Path.Combine(path, "SaveData");
            path = Path.Combine(path, saveType.ToString());
            return path;
        }
    }
}