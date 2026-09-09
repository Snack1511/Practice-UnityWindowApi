# 검증 명령

> **이 프로젝트 고유 규약이다.** 어느 프로젝트에서든 통하는 절차는 [`process/`](../process/README.md)에 있다.
> 절차와 값이 갈릴 때는 여기가 이 프로젝트의 정본이다.

절차 5단계는 [process/00-process.md](../process/00-process.md) §6. 여기에는 **이 프로젝트의 명령과 한계**만 둔다.

## 로컬 경로는 `CLAUDE.local.md` 에 있다

아래 명령의 `<...>` 는 머신마다 다른 값이라 저장소에 커밋하지 않는다.
루트 `CLAUDE.local.md` 에 두고 `.gitignore` 로 제외한다. **매 세션 자동으로 읽히므로 따로 열 필요 없다.**

| 플레이스홀더 | 뜻 |
|---|---|
| `<UNITY_EDITOR>` | 프로젝트 버전과 일치하는 Unity 실행 파일 경로 |
| `<PROJECT>` | 이 프로젝트 루트의 절대 경로 |
| `<BUILD_OUT>` | 빌드 산출 폴더. **프로젝트 밖이어야 한다** |

**클론 직후에는 이 파일이 없다.** 아래를 루트에 `CLAUDE.local.md` 로 만든다.

```markdown
# 로컬 환경 (이 머신 전용)

| 항목 | 값 |
|---|---|
| `<UNITY_EDITOR>` | (Unity 설치 경로)/Editor/Unity.exe |
| `<PROJECT>`      | (클론한 경로) |
| `<BUILD_OUT>`    | (프로젝트 밖 임의 폴더) |

검증 절차는 Meta/roles/verification.md 참조. 여기엔 값만 둔다.
```

에디터 버전은 `ProjectSettings/ProjectVersion.txt` 와 **정확히 일치**해야 한다.

## 컴파일 검사 (2단계)

```bash
"<UNITY_EDITOR>" -batchmode -quit -nographics \
  -projectPath "<PROJECT>" \
  -logFile - 2>&1 | grep -iE "error CS|Compilation failed"
```

에디터 플랫폼 기준 컴파일만 검사한다. **`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 블록은 여기서 컴파일되지 않으므로 검증되지 않는다.**

> **에디터 버전이 일치해야 한다.** `ProjectSettings/ProjectVersion.txt`의 버전과 다른 에디터로 프로젝트를 열면 **에셋 데이터베이스와 `.meta` 재직렬화가 일어나고 되돌리기 어렵다.** 버전이 없으면 실행하지 말고 사용자에게 알린다. 임의로 다른 버전으로 열지 않는다.

## Windows 스탠드얼론 빌드 (3단계)

`ResolutionManager`, `DebuggingComponent`, `WindowNativeManager` 등 `#if UNITY_STANDALONE_WIN` 안쪽을 고쳤다면 **실제 빌드가 유일한 검증 수단이다.** 2단계를 통과해도 여기서 터질 수 있다.

**`-buildWindows64Player` 에는 개발 빌드 플래그가 없다.** `Assets/Editor/` 에 임시 스크립트를 만들어
`-executeMethod` 로 부르고 **끝나면 `.cs` 와 `.meta` 를 함께 지운다.**

```csharp
// Assets/Editor/TempDevBuild.cs — 실행 후 삭제
BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
    locationPathName = "<BUILD_OUT>/OverlayTest.exe",
    target = BuildTarget.StandaloneWindows64,
    options = BuildOptions.Development,   // 이게 있어야 CheatPanel 이 포함된다
});
EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
```

```bash
"<UNITY_EDITOR>" -batchmode -nographics   -projectPath "<PROJECT>"   -executeMethod TempDevBuild.BuildWindows64Development -logFile <로그경로>
```

메서드 끝의 `EditorApplication.Exit` 하나로 종료 코드가 성공/실패를 그대로 준다. `-quit` 불필요.

- `DEVELOPMENT_BUILD` 가 정의되지 않으면 **`CheatPanel` 파일 전체가 컴파일에서 빠진다.**
- **출력은 프로젝트 밖으로 뺀다.** `.gitignore` 에 빌드 산출물 패턴이 없어서 프로젝트 안에 빌드하면
  168 MB 가 `git status` 에 그대로 올라온다.

## 실행 로그

```
%USERPROFILE%\AppData\LocalLow\DefaultCompany\Practice-WindowApi\Player.log
```

**화면을 못 보는 상태에서는 로그 기반 진단이 가장 효과적이다.**
`[UIManager]` · `[CheatPanel]` · `[WindowNative]` · `[SceneRegistryValidator]` 태그로 상태를 판정한다.

`There can be only one active Event System` 이 나오면 씬에 `EventSystem` 이 섞여 들어온 것이다.

## 육안 확인 (4단계)

투명 배경, 클릭 통과 전환, 창 위치·크기는 자동 검증이 불가능하다. 무엇을 눈으로 확인해야 하는지 목록으로 제시한다.

## 자동 테스트

**없다.** 테스트 코드 0개, asmdef 0개다. `TestScene.cs` 는 테스트가 아니라 **씬**이다.
테스트 러너를 찾지 말 것 — 검증은 위 2~4단계가 전부다.

도입 제언은 [advise/004](../advise/004-architecture-scale.md) 4-7.
