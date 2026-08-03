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

## 3. URP 투명 렌더링 파이프라인

창을 뚫었어도 유니티가 그리는 화면은 불투명하다. 알파를 강제로 낮춰 데스크톱이 비치게 하는 것이 이 파스의 역할.

```
PC_Renderer.asset (m_RendererFeatures에 등록됨)
└─ BlitFeature : ScriptableRendererFeature      Script/Shader/BlitFeature.cs
   │  settings.blitMaterial : Material          ← Resources/Material/Custom_MakeTransparent.mat
   │  settings.passEvent    : RenderPassEvent   ← 기본 AfterRendering
   └─ BlitPass : ScriptableRenderPass           Script/Shader/BlitPass.cs
      └─ RecordRenderGraph()                    RenderGraph API (Unity 6)
         ├ resourceData.activeColorTexture 를 src로 확보
         ├ builder.UseTexture(src, ReadWrite) / AllowPassCulling(false)
         └ Blitter.BlitTexture(cmd, src, src, material, 0)
```

셰이더 `Assets/Resources/Shader/MakeTransparent.shader` — `Custom/MakeTransparent_URP`

```hlsl
half4 frag(Varyings i) : SV_Target
{
    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    color.a = _TransparencyFactor;   // 알파를 상수로 덮어씀 (기본 0.01)
    return color;
}
```

`ZWrite Off`, `Blend SrcAlpha OneMinusSrcAlpha`, `Cull Off`, `Queue = Transparent`.

> `BlitTexture(cmd, src, src, ...)` — **소스와 목적지가 같은 텍스처다.** 동일 RT를 동시에 읽고 쓰는 것은 GPU/드라이버에 따라 결과가 달라진다. 코드에 임시 RT(`passData.tmp`)를 만들려던 흔적이 주석으로 남아 있다. → [advise/001](../advise/001-build-blockers.md)

> **대안 접근** — 카메라 배경 알파 0 + HDR off + DXGI Flip Model off + D3D11 조합만으로 blit 패스 없이 픽셀 단위 투명을 얻는 방법이 있다. 현 프로젝트는 그 조건을 **이미 전부 충족하고 있어** blit 스택 자체가 불필요할 수 있다. 설정 대조표와 검증 순서: [reference/desktop-overlay-unity6.md](reference/desktop-overlay-unity6.md)

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
매 프레임 렌더 후
   └ BlitFeature → BlitPass → MakeTransparent_URP (color.a = _TransparencyFactor)
        → 렌더 결과 알파 강제 하향 → 데스크톱이 비침
필요 시
   └ WindowNativeManager.SetTransparentClick(true)                        → 클릭 통과
```
