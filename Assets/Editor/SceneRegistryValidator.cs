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
    /// ESceneType enum 과 Build Settings 씬 목록의 정합성을 검사한다.
    /// 씬 추가 시 한쪽만 갱신하면 컴파일 에러 없이 런타임에 조용히 실패하므로 에디터 재컴파일마다 자동으로 확인한다.
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
        }
    }
}
