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

### `GetWindowName()` — `:123`
활성 창 제목 조회. `DebuggingComponent`가 화면에 띄우는 용도로 사용.

## 2. `ResolutionManager` — 적용 지점

`Assets/Script/Manager/StaticManager/ResolutionManager.cs`

```csharp
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    int rWidth  = Display.main.systemWidth;
    int rHeight = Display.main.systemHeight - 49;   // 작업 표시줄 높이 상수(하드코딩)
    WindowNativeManager.SetWindowFrame(0, 0, rWidth, rHeight);
#endif
```

`MainProcess.OnCallBeforeSplashScreen`에서 호출된다.
`- 49`는 Windows 작업 표시줄 높이를 가정한 매직 넘버 — DPI 배율/작업 표시줄 위치·자동 숨김에 따라 어긋난다.

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
        └ WindowNativeManager.SetWindowFrame(0, 0, 화면너비, 화면높이-49)
             ├ WS_CAPTION/THICKFRAME/MIN/MAX/SYSMENU 제거 + WS_POPUP     → 테두리 없는 창
             ├ SetWindowPos(HWND_TOPMOST, ...)                           → 항상 위
             └ DwmExtendFrameIntoClientArea(margins.cxLeftWidth = -1)    → 창 배경 제거
매 프레임 렌더
   └ 카메라 clearFlags=SolidColor + 배경색 (0,0,0,0) + preserveFramebufferAlpha
        → 아무것도 안 그린 픽셀의 알파가 0 으로 남음 → 데스크톱이 비침 (커스텀 패스 없음)
필요 시
   └ WindowNativeManager.SetTransparentClick(true)                        → 클릭 통과
```
