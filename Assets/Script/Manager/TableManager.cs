using System.Linq;
using System.Text.RegularExpressions;

namespace Manager
{
    using pattern;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public abstract class TableDataBase
    {
        // CSV의 'index' 컬럼과 매칭됩니다.
        public int index;
    }

    public class TableManager : Singleton<TableManager>
    {
        private readonly Dictionary<Type, Dictionary<int, TableDataBase>> _tables = new();

        public void Initialize()
        {
        }
        
        public void Release()
        {
            foreach (var table in _tables.Values)
            {
                table.Clear();
            }
            
            _tables.Clear();
        }

        const string pattern = @"""[^""]*""|[^,]+";
        /// <summary>
        /// 지정된 경로의 CSV 파일을 로드하고 파싱하여 테이블 데이터를 채웁니다.
        /// </summary>
        /// <typeparam name="T">TableDataBase를 상속하고 new() 제약 조건을 갖는 테이블 데이터 타입</typeparam>
        /// <param name="tablePath">ResourcesManager를 통해 로드할 에셋 경로</param>
        public async UniTask LoadTableData<T>(string tablePath) where T : TableDataBase, new()
        {
            TextAsset textAsset = await ResourcesManager.Instance.LoadAsync<TextAsset>(tablePath);
            if (textAsset == null)
            {
                Debug.LogError($"[TableManager] Failed to load table asset from path: {tablePath}");
                return;
            }
            
            // --- CSV 파싱 코드 시작 ---
            Type tableType = typeof(T);
            if (!_tables.ContainsKey(tableType))
            {
                _tables[tableType] = new Dictionary<int, TableDataBase>();
            }
            var table = _tables[tableType];

            // 줄바꿈 문자를 기준으로 텍스트를 나눕니다.
            string[] lines = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Debug.LogError($"[TableManager] Table file is empty or has no data rows: {tablePath}");
                return;
            }

            // 첫 줄(헤더)을 파싱하여 필드 정보를 준비합니다.
            //string[] headers = lines[0].Split(',');
            string[] headers = Regex.Matches(lines[0], pattern).Cast<Match>()
                .Select(match => match.Value.Trim('"'))
                .ToArray();
            
            FieldInfo[] fields = tableType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            // 데이터 줄(두 번째 줄부터)을 파싱합니다.
            for (int i = 1; i < lines.Length; i++)
            {
                //string[] values = lines[i].Split(',');
                string[] values = Regex.Matches(lines[i], pattern).Cast<Match>()
                    .Select(match => match.Value.Trim('"'))
                    .ToArray();
                if (values.Length != headers.Length)
                {
                    Debug.LogWarning($"[TableManager] Row {i + 1} in '{tablePath}' has incorrect column count. Skipping row.");
                    continue;
                }

                T dataEntry = new T();
                for (int j = 0; j < headers.Length; j++)
                {
                    string header = headers[j].Trim();
                    FieldInfo field = Array.Find(fields, f => f.Name == header);

                    if (field != null)
                    {
                        try
                        {
                            // string -> "1, 2, 3, 4" / "1.0, 2.3, 3.5" / "text, text2, text3"
                            if (field.FieldType.IsArray)
                            {
                                Type itemType = field.FieldType.GetElementType();
                                if (string.IsNullOrEmpty(values[j]))
                                {
                                    field.SetValue(dataEntry, Array.CreateInstance(itemType, 0));
                                }
                                else
                                {
                                    string stringValue = values[j];
                                    stringValue = stringValue.Trim();
                                    string[] items = stringValue.Split(',');
                                    Array arrayInst = Array.CreateInstance(itemType, items.Length);
                                    for (int k = 0; k < arrayInst.Length; ++k)
                                    {
                                        object itemValue = Convert.ChangeType(items[k], itemType);
                                        arrayInst.SetValue(itemValue, k);
                                    }

                                    field.SetValue(dataEntry, arrayInst);
                                }
                            }
                            else
                            {
                                // 문자열 값을 필드의 실제 타입으로 변환하여 할당합니다.
                                object value = Convert.ChangeType(values[j], field.FieldType);
                                field.SetValue(dataEntry, value);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[TableManager] Failed to parse value '{values[j]}' for field '{header}' in '{tablePath}'. Error: {e.Message}");
                        }
                    }
                }
                
                if (table.ContainsKey(dataEntry.index))
                {
                    Debug.LogWarning($"[TableManager] Duplicate index {dataEntry.index} found in table {tableType.Name}. Overwriting.");
                }
                table[dataEntry.index] = dataEntry;
            }
            // --- CSV 파싱 코드 종료 ---
        }

        public static bool TryGetData<T>(int index, out T tableData) where T : TableDataBase
        {
            tableData = default;
            if (Instance._tables.TryGetValue(typeof(T), out var table) && table.TryGetValue(index, out var data))
            {
                tableData = data as T;
                return tableData != null;
            }
            return false;
        }
    }
}
