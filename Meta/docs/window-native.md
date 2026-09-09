# 윈도우 네이티브 · 투명 렌더링

프로젝트 이름이 가리키는 핵심 주제. **"테두리 없는 최상위 전체화면 창 + 픽셀 단위 투명"** 조합으로 데스크톱 오버레이를 만드는 것이 목표다.
두 축이 맞물려야 동작한다: **Win32 창 스타일 조작**(창 껍데기) + **URP Blit 패스**(렌더 결과의 알파).

## 1. `WindowNativeManager` — Win32 P/Invoke

`Assets/Script/Manager/StaticManager/WindowNativeManager.cs`

### 컴파일 조건

```csharp
#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || DEBUG
```

`|| DEBUG` 때문에 **에디터와 개발 빌드에서도 이 코드가 컴파일된다.** 에디터에서 `GetActiveWindow()`는 유니티 에디터 창 핸들을 돌려주므로, 에디터에서 `SetWindowFrame`을 호출하면 **에디터 창 자체의 스타일이 바뀐다.** 호출부(`ResolutionManager`)는 별도로 `UNITY_STANDALONE_WIN && !UNITY_EDITOR`로 막아두었다.

### 임포트 목록

| DLL | 함수 | 용도 |
|---|---|---|
| User32 | `GetActiveWindow`, `FindWindowA`, `GetForegroundWindow` | 창 핸들 획득 |
| User32 | `GetWindowLong` / `SetWindowLong` | `GWL_STYLE`(-16), `GWL_EXSTYLE`(-20) 플래그 조작 |
| User32 | `SetWindowPos` | 위치·크기·Z오더 |
| User32 | `GetWindowText` | 창 제목 조회 |
| Dwmapi | `DwmExtendFrameIntoClientArea` | 클라이언트 영역까지 프레임 확장(유리 효과) |

상수는 중첩 struct로 묶여 있다: `GWL`, `WS`, `WS_EX`, `SWP`, `LWA`, `MARGINS`, `HWND_TOPMOST` 등.

### `SetWindowFrame(x, y, w, h)` — `:87`

```csharp
hWnd = GetActiveWindow();

// 1) 타이틀바 · 리사이즈 핸들 · 최소/최대화 · 시스템 메뉴 제거, POPUP 스타일로 전환
uint sFlag = GetWindowLong(hWnd, GWL.STYLE);
sFlag &= ~(WS.CAPTION | WS.THICKFRAME | WS.MINIMIZEBOX | WS.MAXIMIZEBOX | WS.SYSMENU);
sFlag |= WS.POPUP;
SetWindowLong(hWnd, GWL.STYLE, sFlag);

// 2) 최상위로 올리고 위치·크기 지정 (FRAMECHANGED로 프레임 재계산 강제)
SetWindowPos(hWnd, HWND_TOPMOST, x, y, w, h, SWP.FRAMECHANGED);

// 3) DWM 프레임을 클라이언트 영역 전체로 확장 → 창 배경 제거
MARGINS margins = new MARGINS { cxLeftWidth = -1 };
DwmExtendFrameIntoClientArea(hWnd, ref margins);

return GetWindowText(...);   // 창 제목 반환
```

주석 처리되어 남아 있는 두 갈래(둘 다 "클릭 통과 + 알파" 계열):
- `WS_EX.LAYERED` 부여 — `SetLayeredWindowAttributes`로 컬러키/알파 처리를 하려던 경로
- `SetLayeredWindowAttributes(hWnd, 0, 0, LWA.ALPHA)` — 렌더 결과 전체 투명화

현재는 그 대신 **URP 셰이더로 알파를 강제하는 방식**(아래 3절)을 택하고 있다.

### `SetTransparentClick(bool)` — `:135`

`WS_EX_TRANSPARENT`를 확장 스타일에 OR로 넣어 **마우스 입력이 창을 통과**하게 만든다(클릭 스루).
`IsTransparentClick` 프로퍼티에 상태를 기록하지만, **인자가 false여도 플래그를 제거하지 않는다** — 켜기만 되고 끄기가 안 되는 상태. → [advise/001](../advise/001-build-blockers.md)

> 오버레이의 핵심 UX인 "그려진 곳만 클릭이 먹고 빈 곳은 통과"는 **수동 토글로는 만들 수 없고 히트 테스트가 필요하다.** 성숙한 구현의 접근(Raycast / Opacity 두 방식)과 이 프로젝트에 맞는 설계는 [reference/uniwindowcontroller.md](reference/uniwindowcontroller.md) 및 [advise/006](../advise/006-design-references.md) 참조.

### `GetWindowName()`
활성 창 제목 조회. `DebuggingComponent`가 화면에 띄우는 용도로 사용.

### `GetMonitors()` / `TryGetPrimaryMonitor(out MonitorInfo)`

`EnumDisplayMonitors` + `GetMonitorInfo` 로 연결된 모니터를 열거한다.
`MonitorInfo` 는 **전체 영역(`rcMonitor`)과 작업 영역(`rcWork`)을 둘 다** 들고 있다. 작업 영역은 작업 표시줄을 제외한 사각형이다.

```csharp
public struct MonitorInfo
{
    public string device;      // \.\DISPLAY1
    public bool isPrimary;
    public int x, y, width, height;                  // 전체 영역
    public int workX, workY, workWidth, workHeight;  // 작업 영역
}
```

콜백 델리게이트는 **지역 변수로 붙들고 `GC.KeepAlive`** 를 건다. 네이티브 호출 도중 수집되면 크래시다.

### `SetWindowToMonitor(MonitorInfo)`

창을 해당 모니터의 **작업 영역**에 꽉 채운다. 치트 패널의 「화면 설정」 세부 패널이 이 함수를 부른다.

### `TryGetMonitorForWindow(out MonitorInfo)` / `IsWindowFittedTo(MonitorInfo)`

`MonitorFromWindow(MONITOR_DEFAULTTONEAREST)` 로 **창이 지금 올라가 있는 모니터**를 찾는다.
주 모니터가 아니라 이쪽을 기준으로 잡아야, 치트 패널로 다른 모니터에 옮겨둔 창이 도로 끌려오지 않는다.

`IsWindowFittedTo` 는 `GetWindowRect` 결과가 이미 그 작업 영역과 같은지 본다. 불필요한 `SetWindowPos` 를 건너뛰는 용도.

> 파일 안에 **개선 메모 주석**이 있다. `WM_SETTINGCHANGE` / `WM_DISPLAYCHANGE` / `WM_DPICHANGED` 를 창 서브클래싱으로 받는 정석 방법과, 도입 전에 확인할 것(델리게이트 수명, 프레임 경계, 프로시저 복원)을 적어뒀다. 지금은 넣지 않는다.

## 2. `ResolutionManager` — 적용 지점

`Assets/Script/Manager/StaticManager/ResolutionManager.cs`

```csharp
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    if (WindowNativeManager.TryGetPrimaryMonitor(out var primary))
    {
        WindowNativeManager.SetWindowFrame(primary.workX, primary.workY, primary.workWidth, primary.workHeight);
        return;
    }

    // 열거 실패 시에만 쓰는 예비 경로
    WindowNativeManager.SetWindowFrame(0, 0, Display.main.systemWidth, Display.main.systemHeight - 49);
#endif
```

`MainProcess.OnCallBeforeSplashScreen`에서 호출된다.

**작업 영역을 OS에서 직접 읽는다.** 예전에는 `systemHeight - 49` 로 작업 표시줄 높이를 상수 가정했는데,
실측 작업 표시줄이 48px 이라 **창 하단에 1px 이 비었다**. 그 자리로 배경이 그대로 보인다.
DPI 배율·작업 표시줄 위치·자동 숨김에 따라 값이 달라지므로 상수로 둘 수 없다.

| 환경 | 전체 | 작업 영역 |
|---|---|---|
| DISPLAY1 (주) | 2560x1440 | 2560x1392 |
| DISPLAY2 | 1920x1080 | 1920x1032 |

### 포커스를 얻을 때 다시 맞춘다 — `OnApplicationFocusChanged(bool)`

부팅 때 읽은 작업 영역은 **실행 중에 작업 표시줄을 옮기거나 자동 숨김을 켜면 낡는다.**
`MainProcess.OnApplicationChangedFocus` 훅(원래 비어 있던 자리)에서 창이 포커스를 얻을 때 다시 읽는다.

```csharp
if (!isFocus) return;                                        // 잃을 때는 아무것도 안 한다
if (!TryGetMonitorForWindow(out var monitor)) return;        // 주 모니터가 아니라 "창이 있는" 모니터
if (IsWindowFittedTo(monitor)) return;                       // 이미 맞으면 건너뛴다
SetWindowToMonitor(monitor);
```

실측 동작 — 외부에서 창을 DISPLAY2 에 `1000x800`(틀린 크기)으로 옮긴 뒤:

| 시점 | 창 |
|---|---|
| 포커스 상실 | `2560,0 1000x800` (그대로) |
| 포커스 복귀 | `2560,0 1920x1032` — **DISPLAY2 작업 영역** |

주 모니터로 끌려오지 않는다.

## 3. URP 투명 렌더링 — 렌더 피처 없이 설정만으로 성립한다

창을 뚫었어도 유니티가 그리는 화면은 기본적으로 불투명하다.
투명은 **커스텀 렌더 패스가 아니라 프로젝트 설정 다섯 개의 조합**으로 나온다. 하나라도 어긋나면 나머지가 무의미하다.

| 항목 | 값 | 어디서 | 왜 |
|---|---|---|---|
| `preserveFramebufferAlpha` | `1` | `ProjectSettings.asset` | 꺼져 있으면 Unity 가 최종 프레임버퍼 알파를 1 로 밀어버린다. **다른 걸 다 맞춰도 투명이 원천 불가능** |
| 그래픽 API | **D3D11 단독** | Player Settings | D3D12 는 배경 투명 자체가 불가능 |
| URP HDR | off | `PC_RPAsset.asset` | HDR 버퍼는 알파 처리가 다르다 |
| 카메라 `clearFlags` | `SolidColor` | 씬 카메라 | 스카이박스는 불투명, `Nothing` 은 이전 프레임 잔상 |
| 카메라 배경색 | `(0, 0, 0, 0)` | 씬 카메라 | RGB 도 검정이어야 한다 — 아래 참조 |

**알파만 0 으로 맞추는 것으로는 부족하다.** DWM 은 미리 곱해진 알파(premultiplied)로 합성하는데 Unity 는 스트레이트 알파로 쓴다.
그래서 알파가 0 이어도 RGB 가 남아 있으면 그 색이 그대로 화면 전체에 더해진다 — Unity 기본 배경색으로 두면 전체가 파랗게 물든다.

> **삭제된 것** — `BlitFeature` / `BlitPass` / `MakeTransparent.shader` 로 화면 전체 알파를 상수로 덮어쓰던 스택이 있었다.
> 치트 패널로 런타임에 껐을 때 투명이 그대로 유지되는 것을 확인하고 **2026-08-12 에 제거했다**([CHANGELOG](CHANGELOG.md)).
> 두 방식은 결과가 다르다 — 지금은 "오브젝트만 또렷 + 배경 투명", blit 이 있으면 "화면 전체 반투명"이었다.
> 후자가 필요해지면 삭제 커밋을 `git revert` 한다. 배경 분석: [reference/desktop-overlay-unity6.md](reference/desktop-overlay-unity6.md)

## 4. `DebuggingComponent`

`Assets/Script/DebuggingComponent.cs` — 오버레이 앱 디버깅용 OSD.

| 블록 | 조건 | 내용 |
|---|---|---|
| `Awake` | `UNITY_STANDALONE_WIN && !UNITY_EDITOR` | `WindowNativeManager.GetWindowName()`을 TMP 텍스트에 출력 |
| `Update` / `OnGUI` | `DEBUG` | 스무딩된 FPS·ms를 `OnGUI`로 표시, `Tab` 키로 `debugObjects` 토글 |

FPS는 `unscaledDeltaTime`을 10% 계수로 스무딩한다. `OnGUI` 사용이라 릴리스 빌드에서는 `DEBUG` 조건으로 배제된다.

## 5. 전체 조합

```
MainProcess (BeforeSplashScreen)
   └ ResolutionManager.Initialize()
        └ WindowNativeManager.SetWindowFrame(주 모니터 작업 영역)
             ├ WS_CAPTION/THICKFRAME/MIN/MAX/SYSMENU 제거 + WS_POPUP     → 테두리 없는 창
             ├ SetWindowPos(HWND_TOPMOST, ...)                           → 항상 위
             └ DwmExtendFrameIntoClientArea(margins.cxLeftWidth = -1)    → 창 배경 제거
매 프레임 렌더
   └ 카메라 clearFlags=SolidColor + 배경색 (0,0,0,0) + preserveFramebufferAlpha
        → 아무것도 안 그린 픽셀의 알파가 0 으로 남음 → 데스크톱이 비침 (커스텀 패스 없음)
필요 시
   └ WindowNativeManager.SetTransparentClick(true)                        → 클릭 통과
```
