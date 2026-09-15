using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Script.EditorTools
{
    /// <summary>
    /// 에디터 메인 툴바에 이 프로젝트의 검증 동작을 올린다.
    ///
    /// 런타임 치트 패널(<c>CheatPanel</c>)과는 역할이 다르다 —
    /// 치트는 **빌드된 창**을 조작하므로 런타임에 남고, 여기에는 **에디터가 하는 일**만 둔다.
    /// 클릭 통과·모니터 이동 같은 Win32 치트를 에디터에서 켜면 에디터 창 자체가 대상이 된다.
    ///
    /// 패키지 : com.paps.unity-toolbar-extender-ui-toolkit
    /// </summary>
    internal static class DevToolbarPaths
    {
        /// <summary>플레이어 로그. 에디터의 <c>persistentDataPath</c> 가 플레이어와 같은 LocalLow 경로다.</summary>
        public static string PlayerLog => Path.Combine(Application.persistentDataPath, "Player.log");

        /// <summary>
        /// 빌드 산출 exe 경로. Unity 의 Build Settings 창이 쓰는 값과 같은 곳에 저장되므로
        /// 툴바에서 정한 경로가 빌드 창에도 그대로 보인다.
        /// </summary>
        public static string GetBuildExe(bool askIfMissing)
        {
            string saved = EditorUserBuildSettings.GetBuildLocation(BuildTarget.StandaloneWindows64);

            // 버전에 따라 exe 경로를 주기도 하고 폴더를 주기도 한다. 둘 다 받는다.
            if (!string.IsNullOrEmpty(saved))
            {
                if (saved.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    return saved;

                return Path.Combine(saved, Application.productName + ".exe");
            }

            if (!askIfMissing)
                return null;

            string picked = EditorUtility.SaveFilePanel(
                "빌드 산출 위치", "", Application.productName, "exe");

            if (string.IsNullOrEmpty(picked))
                return null;

            EditorUserBuildSettings.SetBuildLocation(BuildTarget.StandaloneWindows64, picked);
            return picked;
        }
    }

    /// <summary>
    /// 에디터에서 런타임 치트 패널을 켤지 정한다.
    ///
    /// 에디터 기본값은 꺼짐 — 치트가 전부 빌드 창을 대상으로 하고, 에디터에서는 게임 뷰를 가리기만 한다.
    /// 개발 빌드는 이 토글과 무관하게 항상 켜진다.
    /// </summary>
    [Paps.UnityToolbarExtenderUIToolkit.MainToolbarElement(id: "CheatPanelEnabled", order: 4)]
    public class CheatPanelToggle : EditorToolbarToggle
    {
        [UnityEditor.Toolbars.MainToolbarElement("CheatPanelEnabled", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static UnityEditor.Toolbars.MainToolbarElement CreateDummy() => null;

        public void InitializeElement()
        {
            text = "Cheat Panel";
            tooltip = "에디터 플레이에서 치트 패널을 만들지 여부. 개발 빌드는 항상 켜진다.\n"
                      + "플레이 중에 바꾸면 다음 플레이부터 반영된다.";

            SetValueWithoutNotify(Script.GameContent.UI.CheatPanel.IsEnabled);

            this.RegisterValueChangedCallback(evt =>
            {
                PlayerPrefs.SetInt(Script.GameContent.UI.CheatPanel.EnabledPrefKey, evt.newValue ? 1 : 0);
                PlayerPrefs.Save();

                Debug.Log($"[DevToolbar] 에디터 치트 패널 = {evt.newValue}");
            });
        }
    }

    /// <summary>Development Build 를 만들고 바로 실행한다. 임시 빌드 스크립트를 만들 필요가 없어진다.</summary>
    [Paps.UnityToolbarExtenderUIToolkit.MainToolbarElement(id: "DevBuildAndRun", order: 0)]
    public class DevBuildAndRunButton : EditorToolbarButton
    {
        [UnityEditor.Toolbars.MainToolbarElement("DevBuildAndRun", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static UnityEditor.Toolbars.MainToolbarElement CreateDummy() => null;

        public void InitializeElement()
        {
            text = "Dev Build";
            icon = (Texture2D)EditorGUIUtility.IconContent("PlayButton@2x").image;
            tooltip = "Windows64 Development Build 를 만들고 실행한다";
            clicked += BuildAndRun;
        }

        private static void BuildAndRun()
        {
            string exe = DevToolbarPaths.GetBuildExe(askIfMissing: true);
            if (string.IsNullOrEmpty(exe))
                return;

            List<string> scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,

                // DEVELOPMENT_BUILD 가 없으면 CheatPanel 파일 전체가 컴파일에서 빠진다.
                options = BuildOptions.Development | BuildOptions.AutoRunPlayer,
            });

            Debug.Log($"[DevToolbar] 빌드 {report.summary.result} : {exe}");
        }
    }

    /// <summary>플레이어 로그를 기본 앱으로 연다. 이 프로젝트에서 가장 효과적인 진단 수단이다.</summary>
    [Paps.UnityToolbarExtenderUIToolkit.MainToolbarElement(id: "OpenPlayerLog", order: 1)]
    public class OpenPlayerLogButton : EditorToolbarButton
    {
        [UnityEditor.Toolbars.MainToolbarElement("OpenPlayerLog", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static UnityEditor.Toolbars.MainToolbarElement CreateDummy() => null;

        public void InitializeElement()
        {
            text = "Player.log";
            icon = (Texture2D)EditorGUIUtility.IconContent("console.infoicon@2x").image;
            tooltip = DevToolbarPaths.PlayerLog;
            clicked += Open;
        }

        private static void Open()
        {
            string path = DevToolbarPaths.PlayerLog;

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[DevToolbar] 플레이어 로그가 없습니다. 빌드를 한 번 실행해야 생깁니다 : {path}");
                return;
            }

            EditorUtility.OpenWithDefaultApp(path);
        }
    }

    /// <summary>빌드 산출 폴더를 연다.</summary>
    [Paps.UnityToolbarExtenderUIToolkit.MainToolbarElement(id: "OpenBuildFolder", order: 2)]
    public class OpenBuildFolderButton : EditorToolbarButton
    {
        [UnityEditor.Toolbars.MainToolbarElement("OpenBuildFolder", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static UnityEditor.Toolbars.MainToolbarElement CreateDummy() => null;

        public void InitializeElement()
        {
            icon = (Texture2D)EditorGUIUtility.IconContent("Folder Icon").image;
            tooltip = "빌드 산출 폴더를 연다";
            clicked += Open;
        }

        private static void Open()
        {
            string exe = DevToolbarPaths.GetBuildExe(askIfMissing: false);
            string folder = string.IsNullOrEmpty(exe) ? null : Path.GetDirectoryName(exe);

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                Debug.LogWarning("[DevToolbar] 빌드 산출 폴더가 아직 없습니다. Dev Build 를 한 번 돌리면 정해집니다.");
                return;
            }

            EditorUtility.RevealInFinder(folder);
        }
    }

    /// <summary>
    /// Build Settings 에 등록된 씬으로 전환한다.
    /// 씬 추가가 코드 3곳 + 에디터 설정 2곳이라 등록 누락이 런타임 조용한 실패로 나타나는데,
    /// 이 목록이 곧 등록된 씬이라 누락이 눈에 띈다.
    /// </summary>
    [Paps.UnityToolbarExtenderUIToolkit.MainToolbarElement(id: "SceneSwitch", order: 3)]
    public class SceneSwitchDropdown : EditorToolbarDropdown
    {
        public void InitializeElement()
        {
            tooltip = "Build Settings 에 등록된 씬으로 전환";
            RefreshLabel();

            clicked += ShowMenu;
            EditorSceneManager.sceneOpened += (scene, mode) => RefreshLabel();
        }

        [UnityEditor.Toolbars.MainToolbarElement("SceneSwitch", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static UnityEditor.Toolbars.MainToolbarElement CreateDummy() => null;

        private void RefreshLabel()
        {
            Scene active = SceneManager.GetActiveScene();
            text = string.IsNullOrEmpty(active.name) ? "Scene" : active.name;
        }

        private void ShowMenu()
        {
            GenericMenu menu = new GenericMenu();
            string activePath = SceneManager.GetActiveScene().path;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                string path = scene.path;

                // 죽은 등록(파일이 없는 경로)이 남아 있으므로 비활성으로 보여준다 — 지워야 할 것이 보인다.
                if (!File.Exists(path))
                {
                    menu.AddDisabledItem(new GUIContent(Path.GetFileNameWithoutExtension(path) + " (파일 없음)"));
                    continue;
                }

                string label = Path.GetFileNameWithoutExtension(path) + (scene.enabled ? "" : " (비활성)");
                menu.AddItem(new GUIContent(label), path == activePath, () => Open(path));
            }

            if (EditorBuildSettings.scenes.Length == 0)
                menu.AddDisabledItem(new GUIContent("등록된 씬이 없습니다"));

            menu.ShowAsContext();
        }

        private static void Open(string path)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }
}
