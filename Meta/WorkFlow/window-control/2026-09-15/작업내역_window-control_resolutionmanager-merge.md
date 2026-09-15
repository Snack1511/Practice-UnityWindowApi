# 창 제어 : ResolutionManager 를 WindowNativeManager 에 합침 (2026-09-15)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.
> 같은 날 [서브모듈 분리](작업내역_window-control_windownative-submodule.md) 직후의 후속 작업이다.

## 목표

두 클래스의 목적이 겹친다. `ResolutionManager` 는 자기 로직이 없고 **전부 `WindowNativeManager` 호출**이다.

| 메서드 | 본문 |
|---|---|
| `Initialize` | `TryGetPrimaryMonitor` → `SetWindowFrame` (+ 실패 시 예비 경로) |
| `OnApplicationFocusChanged` | `TryGetMonitorForWindow` → `IsWindowFittedTo` → `SetWindowToMonitor` |
| `Release` | 빈 함수 |

래퍼와 그 래퍼를 부르는 정책이 파일만 갈라져 있었다. 한 클래스로 합친다.

## 바뀐 것

`ResolutionManager.cs` (53줄) 삭제. 세 메서드를 `WindowNativeManager` 로 옮겼다.

| 전 | 후 |
|---|---|
| `WindowNative.ResolutionManager.Initialize()` | `WindowNative.WindowNativeManager.Initialize()` |
| `WindowNative.ResolutionManager.OnApplicationFocusChanged(isFocus)` | `WindowNative.WindowNativeManager.OnApplicationFocusChanged(isFocus)` |
| `WindowNative.ResolutionManager.Release()` | `WindowNative.WindowNativeManager.Release()` |

호출부는 `MainProcess.cs` 3곳뿐이다.

로그 태그는 `[ResolutionManager]` → `[WindowNative]` 로 통일했다.
개선 메모 안의 `ResolutionManager.OnApplicationFocusChanged` 참조도 같은 클래스 안을 가리키도록 고쳤다.

### 조건부 컴파일 — 합치면서 틀리기 쉬운 부분

`WindowNativeManager` 본체는 전부 `#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || DEBUG` 안에 있다.
`|| DEBUG` 가 붙은 건 에디터·개발 빌드의 `CheatPanel` 과 `DebuggingComponent` 가 이 타입을 참조하기 때문이다.

옮긴 세 메서드는 **그 블록 밖**에 뒀다.

```
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || DEBUG
    ... P/Invoke 래퍼 ...
#endif

    // 생명주기 — 메서드는 항상 존재, 본문만 #if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    public static void Initialize() { ... }
```

`MainProcess` 가 플랫폼 분기 없이 부르므로 **메서드 자체는 항상 존재해야 한다.**
안쪽에 넣었다면 Windows 가 아닌 릴리스 빌드에서 메서드가 사라져 컴파일이 깨진다.
기존 `ResolutionManager` 의 구조(클래스는 무조건, 본문만 `#if`)를 그대로 옮긴 것이다.

### `Release()` 를 남긴 이유

빈 함수지만 지웠다가 다시 만들 자리다.

- `roles/coding.md` — 매니저는 `MainProcess` 의 `Initialize` 와 `Release` **양쪽**에 등록한다.
- 개선 메모의 창 프로시저 서브클래싱을 도입하면 **여기서 원래 프로시저를 복원해야 한다.**
  빠뜨리면 에디터 도메인 리로드 때 죽은 프로시저가 남는다.

## 검증

| 단계 | 결과 |
|---|---|
| 에디터 배치 모드 컴파일 | `error CS` 0건 |
| Windows Development Build | `Succeeded`, `error CS` 0건 |
| 실행 예외 | 0건 |
| `Initialize` | 창 크기가 작업 영역과 **정확히 일치** (`0,0 1920x1032` == `rcWork`) |
| 창 스타일 | `WS_POPUP` 적용, `WS_CAPTION` · `WS_THICKFRAME` 제거, `WS_EX_TOPMOST` |
| 모니터 열거 | `[CheatPanel] 화면 설정 : 모니터 1 개 등록` |
| 클릭 통과 | `[WindowNative] SetTransparentClick(True) ... 0x8 -> 0x80028` |
| 투명 배경 | 스크린샷 육안 확인 |

> **검증 중 한 번 오판할 뻔했다.** 첫 시도에서 클릭 통과가 안 켜져 회귀로 보였으나,
> 로그에 `O` 키 기록 자체가 없었다 — 합성 입력이 창에 안 들어간 것이었다.
> 창을 먼저 클릭해 포커스를 확실히 준 뒤 다시 넣으니 정상 동작했다.
> **합성 입력 검증은 "동작 안 함"과 "입력이 안 들어감"을 먼저 갈라야 한다.**

## 문서 갱신

현재 동작을 기술하는 문서만 고쳤다. `WorkFlow/` 의 과거 기록과 `advise/001` 은 당시 상태라 두었다.

| 문서 | 변경 |
|---|---|
| `CLAUDE.md` | 서브모듈 표 — 클래스 1개로 |
| `docs/window-native.md` | 2절 제목·경로, 전체 조합 다이어그램, `#if` 설명 |
| `docs/architecture.md` | 두 항목을 하나로 합치고 서브모듈임을 명시 |
| `docs/README.md` | 부트 시퀀스 다이어그램 |
| `docs/roadmap.md` | 윈도우 컨트롤 행 |
| `docs/reference/desktop-overlay-unity6.md` | 호출 시점 비교표 |
| `roles/verification.md` | 빌드 검증이 필요한 대상 목록 |
| `advise/005` · `006` | 파일 참조 |
| 서브모듈 `README.md` | 파일 표 제거, 생명주기 API 와 `#if` 위치 설명 추가 |
