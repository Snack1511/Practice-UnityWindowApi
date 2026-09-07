using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.GameFlow.GameScene;
using UnityEditor;
using UnityEngine;

namespace Script.EditorTools
{
    /// <summary>
    /// ESceneType enum 과 Build Settings 씬 목록의 정합성, 그리고 씬에 EventSystem 이 남아 있는지를 검사한다.
    /// 씬 추가 시 한쪽만 갱신하거나 씬에 EventSystem 이 딸려 들어오면 컴파일 에러 없이 런타임에 조용히 실패하므로 에디터 재컴파일마다 자동으로 확인한다.
    /// Editor 폴더 스크립트라 플레이어 빌드에 포함되지 않는다. 런타임 비용 없음.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneRegistryValidator
    {
        static SceneRegistryValidator()
        {
            Validate();
        }

        [MenuItem("Tools/Validate Scene Registry")]
        public static void Validate()
        {
            // Build Settings 에 실제로 포함되는(enabled) 항목만 대상으로 한다.
            HashSet<string> buildScenes = new HashSet<string>(
                EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => Path.GetFileNameWithoutExtension(scene.path)));

            HashSet<string> enumScenes = new HashSet<string>(
                Enum.GetNames(typeof(ESceneType))
                    .Where(name => name != ESceneType.None.ToString()));

            // (a) enum 에는 있는데 Build Settings 에 없음 -> 전환 시 로드 실패
            foreach (string name in enumScenes.Where(name => !buildScenes.Contains(name)))
            {
                Debug.LogError($"[SceneRegistryValidator] ESceneType.{name} 이 Build Settings 에 없습니다. 해당 씬으로 전환하면 런타임에 실패합니다.");
            }

            // (b) Build Settings 에는 있는데 enum 에 없음 -> 죽은 등록
            foreach (string name in buildScenes.Where(name => !enumScenes.Contains(name)))
            {
                Debug.LogWarning($"[SceneRegistryValidator] Build Settings 의 '{name}' 에 대응하는 ESceneType 항목이 없습니다. 죽은 등록입니다.");
            }

            ValidateNoSceneEventSystem();
        }

        /// <summary>
        /// EventSystem 은 UIManager 가 부팅 시 띄우는 UIRoot 프리팹 안에 하나만 있고 DontDestroyOnLoad 로 유지된다.
        /// 씬에 EventSystem 이 들어 있으면 Additive 로드 시 중복이 되고, 먼저 로드된 쪽이 언로드될 때 활성 인스턴스가 0개가 되어 UI 입력이 죽는다.
        /// Canvas 를 추가하면 에디터가 EventSystem 을 자동으로 만들어 주므로 씬 작업 중에 다시 섞여 들어오기 쉽다.
        /// 씬을 열지 않고 에셋 텍스트만 확인한다.
        /// </summary>
        private static void ValidateNoSceneEventSystem()
        {
            const string eventSystemTypeMark = "UnityEngine.EventSystems.EventSystem";

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(scene => scene.enabled))
            {
                if (!File.Exists(scene.path))
                    continue;

                if (!File.ReadAllText(scene.path).Contains(eventSystemTypeMark))
                    continue;

                Debug.LogWarning($"[SceneRegistryValidator] '{scene.path}' 에 EventSystem 이 있습니다. UIRoot 의 EventSystem 과 중복되어 UI 입력이 죽을 수 있으니 씬에서 제거하세요.");
            }
        }
    }
}
