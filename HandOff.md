# HandOff

다음 세션이 이어받기 위한 인수인계. **작업을 시작하기 전에 이 문서와 [CLAUDE.md](CLAUDE.md)를 먼저 읽는다.**

마지막 갱신: **2026-08-12** / `origin/main` 동기화됨

---

## 지금 상태

**작업 트리 클린, 미푸시 커밋 없음, 실행 중인 프로세스 없음.** 깨끗한 상태에서 재개할 수 있다.

### 도달한 지점

Windows 플레이어 빌드가 통과하고, **투명 배경과 클릭 통과가 실제로 동작한다.**
커스텀 렌더 패스 없이 **프로젝트 설정만으로 투명이 성립**하는 것까지 확인·정리됐다.
상세는 [docs/CHANGELOG.md](docs/CHANGELOG.md).

| 항목 | 상태 |
|---|---|
| advise/001 P0 (1-1 · 1-2 · 1-3 · 1-4 · 1-6) | ✅ 해결 + 빌드/실행 검증 |
| advise/001 1-5 (BlitPass 스택) | ✅ **삭제 완료** — 빌드·실행·투명 육안 확인까지 |
| 투명 배경 | ✅ 실행 확인 (blit 제거 후 재확인) |
| 클릭 통과 | ✅ 실행 확인 |
| 치트 패널 · 콘솔 · 드래그 · 접기 | ✅ 실행 확인 |
| `Assets` 용량 | 789M → 19M |

---

## 다음에 할 일

001 의 P0 는 사실상 소진됐다. 남은 것은 아래 표뿐이며 **우선순위는 002-8 EventSystem** 이다 — 재발 조건이 명확하다.

### 열린 항목

| 항목 | 문서 | 비고 |
|---|---|---|
| `GameScene` 죽은 Build Settings 등록 | advise/001 | 검사기가 Warning으로 알림. 빌드는 안 막음 |
| `EventSystem` 구조적 해법 | [advise/002-8](advise/002-scene-system.md) | **`MenuScene`이 로비 위에 뜨면 재발한다.** 지금은 로딩씬 것만 제거한 임시 조치 |
| `GameProcessManager.RemoveUpdate` 부재 | [advise/004](advise/004-architecture-scale.md) | `CheatPanel`이 null 가드로 우회 중 |
| `Obstacle.prefab` missing script | — | 기존 문제. GUID `37b71252...`가 프로젝트에 없음 |
| `ResourcesManager` 확장자 처리 | — | 확장자 없는 경로 → `ArgumentException`. 호출부에서 우회 중이고 원인은 남아 있음 |

---

## 재개에 필요한 환경 정보

### Unity

```
버전     6000.2.10f1   (ProjectSettings/ProjectVersion.txt 와 일치해야 한다)
설치     E:\Unity\Editor\6000.2.10f1\Editor\Unity.exe
Hub      E:\Unity\Unity Hub\Unity Hub.exe   (Program Files 아님)
```

> 다른 버전으로 열면 에셋 데이터베이스와 `.meta` 재직렬화가 일어나 되돌리기 어렵다.
> `6000.0.23f1`과 `6000.5.5f1`도 설치돼 있으니 **경로를 반드시 확인한다.**

### 빌드

에디터 컴파일 검사:

```bash
"E:/Unity/Editor/6000.2.10f1/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath "E:/Unity/Project/Practice/Practice-UnityWindowApi" -logFile <로그경로>
```

**Development Build** — `-buildWindows64Player`에는 개발 빌드 플래그가 없다.
`Assets/Editor/`에 임시 스크립트를 만들어 `-executeMethod`로 부르고, **끝나면 삭제한다.**

```csharp
BuildPipeline.BuildPlayer(new BuildPlayerOptions {
    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
    locationPathName = "<출력경로>/OverlayTest.exe",
    target = BuildTarget.StandaloneWindows64,
    options = BuildOptions.Development,   // 이게 있어야 CheatPanel 이 포함된다
});
```

`DEVELOPMENT_BUILD`가 정의되지 않으면 `CheatPanel` 파일 전체가 컴파일에서 빠진다.

**2026-08-12 에 실제로 통과한 호출** — `EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1)`을
메서드 끝에 두면 배치 모드 종료 코드로 성공/실패가 그대로 나온다. `-quit` 없이 이 Exit 하나로 끝난다.

```bash
"E:/Unity/Editor/6000.2.10f1/Editor/Unity.exe" -batchmode -nographics \
  -projectPath "E:/Unity/Project/Practice/Practice-UnityWindowApi" \
  -executeMethod TempDevBuild.BuildWindows64Development -logFile <로그경로>
```

> **출력은 프로젝트 밖으로 뺀다.** `.gitignore` 에 빌드 산출물 패턴이 없어서
> 프로젝트 안에 빌드하면 168 MB 가 `git status` 에 그대로 올라온다.
>
> 임시 스크립트는 `Assets/Editor/`에 두고 **빌드 직후 `.cs` 와 `.meta` 를 함께 지운다.**

### 실행 로그

```
%USERPROFILE%\AppData\LocalLow\DefaultCompany\Practice-WindowApi\Player.log
```

**로그 기반 진단이 이 프로젝트에서 가장 효과적이다.** 화면을 볼 수 없는 상태에서도
`[CheatPanel]` · `[WindowNative]` · `[SceneRegistryValidator]` 태그로 상태를 판정할 수 있다.

### 치트 패널 조작

```
O        패널 표시/숨김
1        Set TransparentClick
```

숫자키는 **UI 입력 경로를 우회**한다. "클릭이 안 된다"와 "기능이 안 된다"를 분리해서 판정할 때 쓴다.

> 매핑은 **등록 순서 고정이 아니라 `AddToggle` 호출 순서**다. 치트를 추가하면 번호가 밀린다.
> `Set BitBlitPass` 는 2026-08-12 에 제거됐다(이전 `1`번). 에디터에서는 토글이 0개다.

---

## 이 프로젝트에서 반복해서 물린 함정

다음 세션이 같은 길을 가지 않도록.

**빌드 산출물 폴더는 사용 API의 증거가 아니다.** `D3D12` 폴더는 D3D11 단독 설정에서도 생긴다.
실제 선택 결과는 `Player.log`의 `Direct3D: Version:` 줄이다.

**대량 GUID 대조는 처리 건수를 확인한다.** `xargs`가 조용히 잘려 1574개 중 145개만 검사한 적이 있다.
"참조 없음" 결과를 그대로 믿고 지웠으면 프리팹이 깨졌다.

**Resources는 확장자를 뗀 경로로 색인한다.** 같은 이름의 다른 타입 에셋(`X.uxml` / `X.uss`)이
공존하면 조용히 잘못 로드된다. 파일명을 갈라야 한다.

**숨겨진 UI 요소의 `resolvedStyle`은 0이다.** `display: none` 상태에서 크기를 읽고
"크기가 0이라 안 보인다"고 판단한 적이 있다. 측정은 표시 상태에서 한다.

**`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽은 에디터가 검증하지 않는다.**
배치 모드 컴파일 검사도 이 블록을 건드리지 않는다. **실제 Windows 빌드가 유일한 검증 수단이다.**

**증상과 원인이 계층을 가로지른다.** 이번에 네 번 오진했다 —
파란 틴트를 BlitPass로, 패널 배경 미표시를 알파 합성으로, 클릭 무반응을 EventSystem으로,
클릭 통과 실패를 `hWnd` 문제로. 전부 로그가 바로잡았다.

---

## 참고 문서

| 문서 | 내용 |
|---|---|
| [CLAUDE.md](CLAUDE.md) | 프로젝트 규약. **모델 선택, 확인 절차, 질문 응답 규칙** |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | 시간순 변경 이력. 이번 세션 전체 |
| [docs/models/](docs/models/README.md) | 모델별 작업 규약 |
| [advise/README.md](advise/README.md) | 제언 목록. 현재 **6/10** (10개 넘으면 재정리 먼저) |
| [docs/reference/](docs/reference/) | 외부 구현 분석 |

투명 오버레이 동작 원리를 다이어그램·코드·레퍼런스로 정리한 학습 문서:
**https://claude.ai/code/artifact/13ad6d0c-52be-46bf-afcb-199b4e418b10**

---

## 재개 첫 동작

[CLAUDE.md](CLAUDE.md)의 `연결이 끊긴 뒤 재개할 때` 규칙에 따라, **기억이 아니라 실제 상태를 먼저 읽는다.**

```bash
git status --porcelain
git log --oneline -3
git status -sb | head -1
```

이 문서 기준과 다르면 **그 차이를 먼저 보고한 뒤** 진행한다.
