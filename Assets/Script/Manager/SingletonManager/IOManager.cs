using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using pattern;
using Script.Define.ModelDefine;
using Script.Define.SaveDefine;
using UnityEngine;

namespace Script.Manager.SingletonManager
{
    public enum ESaveType
    {
        Test
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
                if (null == saveBase)
                {
                    return false;
                }
                
                string filePath = GetFilePath(saveType);
                string folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                
                string jsonString = JsonUtility.ToJson(saveBase);
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
                string filePath = GetFilePath(saveType);
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    object loadObject = JsonUtility.FromJson(jsonString, GetSaveType(saveType));
                    AddSaveData(saveType, loadObject as SaveBase);
                    saveBase = GetSaveData(saveType);
                    return true;
                }
                else
                {
                    //파일 없으면 신규 데이터 생성
                    if(TryAddNewSaveData(saveType));
                        saveBase = GetSaveData(saveType);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
                return false;
            }

        }

        private Type GetSaveType(ESaveType saveType)
        {
            Type type = (saveType)switch
            {
                ESaveType.Test => typeof(TestSaveData)
            };
            return type;
        }

        public async UniTask SaveDataAsync(ESaveType saveType, float delayDuration = 0.0f)
        {
            try
            {
                SaveBase saveBase = GetSaveData(saveType);
                if (null == saveBase) return;
                
                string filePath = GetFilePath(saveType);
                
                string folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string jsonString = JsonUtility.ToJson(saveBase);
                await UniTask.WaitForSeconds(delayDuration);
                await File.WriteAllTextAsync(filePath, jsonString);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"파일 저장 중 오류 발생: {ex.Message}");
            }
        }
        
        public async UniTask<SaveBase> LoadDataAsync(ESaveType saveType)
        {
            try
            {
                SaveBase saveBase = null;
                string filePath = GetFilePath(saveType);
                if (File.Exists(filePath))
                {
                    string jsonString = await File.ReadAllTextAsync(filePath);
                    object loadObject = JsonUtility.FromJson(jsonString, GetSaveType(saveType));
                    AddSaveData(saveType, loadObject as SaveBase);
                    saveBase = GetSaveData(saveType);
                }
                else
                {
                    //파일 없으면 신규 데이터 생성
                    if (TryAddNewSaveData(saveType))
                        saveBase = GetSaveData(saveType);
                }

                return saveBase;
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

        private SaveBase GetSaveData(ESaveType saveType)
        {
            SaveBase saveBase = saveDict.GetValueOrDefault(saveType, default);
            return saveBase;
        }
        
        private bool TryAddNewSaveData(ESaveType saveType)
        {
            SaveBase saveBase = SaveConstructor.CreateSaveBase(saveType);
            if (null == saveBase) return false;
            AddSaveData(saveType, saveBase);
            return true;
        }
        

        private void AddSaveData(ESaveType saveType, SaveBase saveBase)
        {
            if (!saveDict.ContainsKey(saveType))
            {
                saveDict.Add(saveType, saveBase);
            }
            else
            {
                saveDict[saveType] = saveBase;
            }
        }

    }
}