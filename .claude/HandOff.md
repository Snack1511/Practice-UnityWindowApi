# HandOff

다음 세션이 이어받기 위한 인수인계. **작업을 시작하기 전에 이 문서와 [CLAUDE.md](../CLAUDE.md)를 먼저 읽는다.**

마지막 갱신: **2026-09-08** / 브랜치 `feat/uiroot-and-cheat-panel` (main 대비 13커밋, **미푸시**)

> **다음 세션은 아래 「대기 중인 결정」을 하나씩 순차로 물어본다.** 한꺼번에 나열하지 않는다.
> 항목마다 선택지와 "결정 안 하면 무엇이 막히는지"를 적어뒀으니 그대로 제시하고 답을 받은 뒤 다음으로 넘어간다.

---

## 지금 상태

**작업 트리 클린, 실행 중인 프로세스 없음.**
`feat/uiroot-and-cheat-panel` 브랜치에 커밋이 쌓여 있고 **`main` 반영도 푸시도 안 됐다.**
백업 브랜치는 삭제했다(B-1). 문서는 전부 `.claude/` 아래로 옮겨졌다.

### 도달한 지점

Windows 플레이어 빌드가 통과하고, **투명 배경과 클릭 통과가 실제로 동작한다.**
커스텀 렌더 패스 없이 **프로젝트 설정만으로 투명이 성립**하는 것까지 확인·정리됐다.
UI 는 씬이 아니라 **전역 `UIRoot` 밑**에 만든다 — `EventSystem` 중복 문제가 구조적으로 닫혔다.
상세는 [docs/CHANGELOG.md](docs/CHANGELOG.md).


| 항목                                          | 상태                             |
| ------------------------------------------- | ------------------------------ |
| advise/001 P0 (1-1 · 1-2 · 1-3 · 1-4 · 1-6) | ✅ 해결 + 빌드/실행 검증                |
| advise/001 1-5 (BlitPass 스택)                | ✅ **삭제 완료** — 빌드·실행·투명 육안 확인까지 |
| 투명 배경                                       | ✅ 실행 확인 (blit 제거 후 재확인)        |
| 클릭 통과                                       | ✅ 실행 확인                        |
| 치트 패널 · 콘솔 · 드래그 · 접기                       | ✅ 실행 확인                        |
| `Assets` 용량                                 | 789M → 19M                     |
| advise/002-8 (EventSystem 구조적 해법)        | ✅ **해결** — 전역 `UIRoot` + `UIManager`, 씬 UI 전부 이전 |
| 창 하단 1px 빈칸                              | ✅ 해결 — 작업 표시줄 높이 상수 가정 제거, 작업 영역 직접 조회 |
| 치트 패널 종료 버튼 · 화면 설정 세부 패널          | ✅ 합성 클릭으로 실동작 확인 |
| 포커스 시 작업 영역 재계산                        | ✅ 실행 확인 — 현재 모니터 기준, 주 모니터로 안 끌려옴 |
| `process/` 작업 규약 통합본 (en/ko)             | ✅ 도입 — 세션 프로세스·git·문서·메모리·모델 규약 |
| `docs/roadmap.md` 두 축 로드맵                 | ✅ 구조 축(시스템·모듈·오브젝트) + 시간 축(0~9단계) + 단계별 닫힘 기준 |
| 커밋 트레일러 소급 제거                          | ✅ 13커밋 전부. 트리 해시 대조로 내용 무변경 확인 |


---

## 다음에 할 일

001 의 P0 와 002-8 은 끝났다.

**먼저 할 것** — 아래 「대기 중인 결정」의 **A 그룹부터 하나씩 물어본다.** 그 뒤에 아래 열린 항목 표를 본다.

### 열린 항목


| 항목                                   | 문서                                             | 비고                                                                   |
| ------------------------------------ | ---------------------------------------------- | -------------------------------------------------------------------- |
| `GameScene` 죽은 Build Settings 등록     | advise/001                                     | 검사기가 Warning으로 알림. 빌드는 안 막음                                          |
| `GameProcessManager.RemoveUpdate` 부재 | [advise/004](advise/004-architecture-scale.md) | `CheatPanel`이 null 가드로 우회 중                                          |
| `Obstacle.prefab` missing script     | —                                              | 기존 문제. GUID `37b71252...`가 프로젝트에 없음 -&gt; 기존 테스트용 코드와 프리팹이므로 제거해도 무방 |
| `ResourcesManager` 확장자 처리            | —                                              | 확장자 없는 경로 → `ArgumentException`. 호출부에서 우회 중이고 원인은 남아 있음              |
| 로비 패널 육안 확인                        | [docs/CHANGELOG.md](docs/CHANGELOG.md)         | 2026-09-06 UIRoot 이전 후 위치·크기 미확인. 실행해서 한 번 보면 끝난다                     |
| `UIRoot` 빈 레이어 3개                     | [advise/002-8](advise/002-scene-system.md)     | `Popup`·`CanvasEffect`·`Overlay` 는 쓰는 쪽이 없다. 채울 때 채운다                        |


---

## 대기 중인 결정 (15건 / A 3 · B 4 · C 5 · D 2 · E 1)

**사용자가 "다음 세션에 하나씩 순차 처리하겠다"고 지정했다.**
한 번에 하나만 묻고, 답을 받은 뒤 다음으로 넘어간다. 목록 전체를 나열하지 않는다.

각 항목은 **묻는 내용 / 선택지 / 안 정하면 막히는 것** 순으로 적혀 있다. 그대로 제시하면 된다.
처리한 항목은 `~~취소선~~` + `(결정: 내용, 날짜)` 로 남긴다 — 지우지 않는다.

### A. 작업이 막혀 있는 것 — 먼저 묻는다

~~**A-1. `/init` 개선 7건 적용 여부**~~ ✅ **(결정: 전부 적용, 2026-09-08)**

7건 모두 반영했다.

| 고친 것 | 어디 |
|---|---|
| 파일 수 39 → **43** | `CLAUDE.md` |
| `roadmap.md` 링크 추가 | `CLAUDE.md` 문서 표 |
| Sonnet 5 도입가 만료 반영 ($2/$10 → $3/$15) | `roles/model.md` |
| 컴파일 명령 플레이스홀더 → 실제 경로 | `roles/verification.md` |
| **Development Build 명령 추가** (임시 스크립트 + `-executeMethod`) | `roles/verification.md` |
| `Player.log` 경로와 진단 태그 추가 | `roles/verification.md` |
| **자동 테스트 0개** 명시 (`TestScene.cs` 는 씬이다) | `roles/verification.md` |

**A-2. 아키텍처 요약을 `CLAUDE.md`에 넣을지**

(`roles/` 분리로 `CLAUDE.md`가 55줄이 됐다. 3~4줄 추가 여지는 더 커졌다.)

`/init`은 "여러 파일을 읽어야 알 수 있는 큰 그림"을 `CLAUDE.md`에 넣으라고 한다.
지금은 `docs/architecture.md` 링크만 있다. `process/en/03-docs.md`의 **"정본 1곳 + 나머지는 델타만"** 규칙과 충돌한다.

넣는다면 3~4줄, 구조의 *모양*만:

```
진입점은 씬이 아니라 [RuntimeInitializeOnLoadMethod] 정적 훅(MainProcess)이다.
매니저는 Static -> Singleton -> MonoSingleton 3계층, 해제는 역순.
Unity Update 진입점은 GameProcess 하나뿐 — 나머지는 GameProcessManager.AddUpdate 로 등록.
```

선택지 — 넣는다 / 링크만 유지 (정본 1곳 규칙 우선)
안 정하면 — 새 세션이 구조를 알려고 매번 `docs/architecture.md`를 연다

**A-3. 다음 실작업 선택**

선택지 — (a) roadmap §7 미정 4건 결정 (5단계 선행) / (b) 0단계 닫기 = `advise/002` 2-1·2-2·2-3 씬 전환 불변식 통일
안 정하면 — 코드 작업이 시작되지 않는다. 이번 세션은 전부 문서였다

### B. 정리 대기 — 안 해도 굴러간다

~~**B-1. `backup/pre-trailer-strip` 브랜치 삭제 여부**~~ ✅ **(결정: 삭제, 2026-09-08)**

삭제 근거 — tip 트리(`1ebb354`)가 브랜치 내 대응 커밋 `1f94920` 과 동일하고,
`git cherry` 로 백업의 모든 커밋이 HEAD 에 patch-equivalent 로 존재함을 확인했다.
백업이 유일하게 가진 건 의도적으로 지운 커밋 트레일러뿐이라 잃을 내용이 없었다.

> `.git/refs/original/refs/heads/feat/uiroot-and-cheat-panel` 이 남아 있다.
> `filter-branch` 가 만든 원본 ref 다. 지우려면
> `git update-ref -d refs/original/refs/heads/feat/uiroot-and-cheat-panel` 후 `git gc --prune=now`.

**B-2. `main` 반영 + 푸시**
13커밋 미반영. `git checkout main && git merge --ff-only feat/uiroot-and-cheat-panel`.
**푸시는 사용자 명시 요청이 있을 때만** 한다 → [process/en/02-git.md](process/en/02-git.md) §2
안 정하면 — 작업이 브랜치에만 남는다

**B-3. `docs/model-guides/` 4개 파일도 범용본으로 뽑을지**
Desktop 통합 때 "모델별 개별 규약은 이 프로젝트 관찰 기반이라 범용성이 낮다"고 판단해 제외했다. 공통 원칙만 `process/*/06`에 들어갔다.
안 정하면 — 현행 유지. 실질 손해 없음

**B-4. `~/.claude/commands/*.md` 전역 슬래시 커맨드 정리 여부**
`fable5` / `opus5` / `opus48` / `sonnet5` 4개. 전역 설정이라 프로젝트 통합본에서 제외했다.
안 정하면 — 현행 유지. 실질 손해 없음

### C. 설계 결정 — 5단계 착수 전 필수

전부 [docs/roadmap.md](docs/roadmap.md) §7에 근거가 있다. **5단계(인게임 컨텐츠)를 시작하려면 이 4건이 먼저다.**

**C-1.** 컨텐츠 3종(`ContentDungeon`·`ContentVillage`·`ContentMine`)의 역할과 모듈 소유
**C-2.** 배낭이 UI인가 오브젝트인가 — 인벤토리 모듈과의 경계
**C-3.** RNG 재현성 정책 — 시드를 저장하는가 (저장하면 세이브 스키마에 들어간다)
**C-4.** 경제의 단일 자원 vs 복수 자원 — 보상·소모 모듈이 공유할 정의
**C-5.** 오브젝트를 씬 배치 vs 런타임 생성

안 정하면 — 5단계 착수 시 뒤집힌다. 모듈 5종·오브젝트 7종이 전부 여기 걸려 있다

### D. 정보 요청

**D-1. OpenWiki 스킬**
사용자가 있다고 했으나 프로젝트 `.agents/skills/`, 전역 `~/.claude/skills/`, `installed_plugins.json`, 공식 마켓 카탈로그 **네 군데 모두 없었다.**
정확한 이름이나 어디서 봤는지 확인 필요.

**D-2. `UILobbyPanel`에 보이는 요소를 넣어 `Window` 레이어 렌더링을 증명할지**
`UILobbyPanel`은 `RectTransform` + `HorizontalLayoutGroup`뿐이라 그릴 게 없다.
"컨텐츠 올라오면 재확인"으로 육안 확인 자체는 보류됐지만, **지금 임시 요소를 넣어 레이어 동작만 증명하는 것**은 별개 제안이다.

### E. 반쯤 답변됨

**E-1. "`CLAUDE.md` 편집은 항상 영어로 저장" 규칙 적용 여부**
`Desktop/Claude/TXT/Extra Rules.txt`에 있는 규칙. `process/`는 en/ko 분리로 해결했으나
**프로젝트 `CLAUDE.md` 자체는 여전히 한국어다.** 이 규칙을 이 프로젝트에 적용할지 미결.

---

## 조건부 보류 — 물어볼 필요 없다

답을 이미 받았고 재개 조건이 정해진 것들. **조건이 충족되기 전에는 묻지 않는다.**

| 항목 | 재개 조건 |
|---|---|
| 로비 패널 육안 확인 | 컨텐츠가 올라온 뒤 |
| 8단계 네트워크 닫힘 기준 | 5단계에서 게임 루프 확정 후 |
| `Loader<T>` 를 `ResourcesManager`로 흡수할지 | 2단계 착수 시 |

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
O            패널 표시/숨김
1            Set TransparentClick
[화면 설정]   세부 치트 패널을 연다 (버튼, 숫자키 없음)
[Quit]       Application.Quit (버튼, 숫자키 없음)
```

세부 치트 패널 「화면 설정」에는 연결된 모니터가 버튼으로 나온다. 누르면 창이 그 모니터의 **작업 영역**으로 옮겨간다.

숫자키는 **UI 입력 경로를 우회**한다. "클릭이 안 된다"와 "기능이 안 된다"를 분리해서 판정할 때 쓴다.

> 매핑은 **등록 순서 고정이 아니라 `AddToggle` 호출 순서**다. 치트를 추가하면 번호가 밀린다.
> `AddButton` 으로 만든 버튼은 `toggles` 에 들어가지 않으므로 숫자키 번호를 밀지 않는다.
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

`**#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽은 에디터가 검증하지 않는다.**
배치 모드 컴파일 검사도 이 블록을 건드리지 않는다. **실제 Windows 빌드가 유일한 검증 수단이다.**

**증상과 원인이 계층을 가로지른다.** 이번에 네 번 오진했다 —
파란 틴트를 BlitPass로, 패널 배경 미표시를 알파 합성으로, 클릭 무반응을 EventSystem으로,
클릭 통과 실패를 `hWnd` 문제로. 전부 로그가 바로잡았다.

---

## 참고 문서


| 문서                                     | 내용                                  |
| -------------------------------------- | ----------------------------------- |
| [CLAUDE.md](../CLAUDE.md)                 | 프로젝트 규약. **모델 선택, 확인 절차, 질문 응답 규칙** |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | 시간순 변경 이력. 이번 세션 전체                 |
| [docs/model-guides/](docs/model-guides/README.md)  | 모델별 작업 규약                           |
| [advise/README.md](advise/README.md)   | 제언 목록. 현재 **6/10** (10개 넘으면 재정리 먼저) |
| [docs/reference/](docs/reference/)     | 외부 구현 분석                            |


투명 오버레이 동작 원리를 다이어그램·코드·레퍼런스로 정리한 학습 문서:
[**https://claude.ai/code/artifact/13ad6d0c-52be-46bf-afcb-199b4e418b10**](https://claude.ai/code/artifact/13ad6d0c-52be-46bf-afcb-199b4e418b10)

---

## 재개 첫 동작

[CLAUDE.md](../CLAUDE.md)의 `연결이 끊긴 뒤 재개할 때` 규칙에 따라, **기억이 아니라 실제 상태를 먼저 읽는다.**

```bash
git status --porcelain
git log --oneline -3
git status -sb | head -1
```

이 문서 기준과 다르면 **그 차이를 먼저 보고한 뒤** 진행한다.