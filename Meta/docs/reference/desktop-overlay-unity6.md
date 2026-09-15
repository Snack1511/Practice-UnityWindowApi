# [참고] Unity 6.1 데스크톱 오버레이 구성 — Sunny Valley Studio

> **출처**: Sunny Valley Studio — **"Desktop Overlay Game in Unity 6 (like Rusty's Retirement) [UnityTip]"**
> <https://www.patreon.com/sunnyvalleystudio/posts/desktop-overlay-143876901> (2025-11-18 게시)
> **수집일**: 2026-07-28 / **상태**: 외부 참고 자료. **적용 안 함.** 향후 투명 창 관련 제언 시 근거로 사용.
>
> **수집 방법과 신뢰도** — 페이지를 3회 조회해 교차 확인함. 페이월 없음, 본문 약 550~600단어.
> 원문 목차: `🎯 What This Effect Does` → `🛠️ The Script` → `🎨 Camera Setup` → `⚙️ Required Player Settings` → `✨ Final Result` → `❤️ If You Enjoy My Unity Tips…`
> 아래 설정값들은 3회 조회에서 모두 일치했다. 단 **본문 원문(raw HTML)을 직접 읽은 것은 아니며**, 페이지 변환 결과를 통해 얻은 내용이다.
>
> **주의**: `🛠️ The Script` 절이 있으나 **페이지에 코드 블록이 없다.** 핵심 스크립트 `TransparentGame.cs`는 첨부파일이며, **2026-07-28 사용자가 직접 내려받아 제공하여 전문 확보 완료.** 아래 §2 코드는 그 원문이다.
>
> 코드 저작권은 Sunny Valley Studio에 있다. 이 문서는 내부 참고용 인용이며, 배포·재배포 시에는 별도 확인이 필요하다.

---

## 1. 요지

Unity 6.1에서 **창 테두리·타이틀바를 제거하고 픽셀 단위 투명을 적용해 데스크톱에 게임을 녹이는** 구성.
레퍼런스 사례로 *Rusty's Retirement* 를 든다. 용도: 데스크톱 펫, 방치형, UI 위젯, 투명 비주얼라이저.

원문이 명시하는 핵심 경고:

> "Unity 6.1 changed how transparency works, and most older tutorials no longer function."

즉 **Unity 6.x 이전 시대의 투명 창 튜토리얼(`SetLayeredWindowAttributes` 컬러키 방식 등)은 더 이상 동작하지 않는다.** 현재 리포지토리가 참고했을 법한 자료들도 여기 해당할 가능성이 높다.

## 2. 요구 설정 (원문 기준)

### 카메라

| 항목 | 값 | 비고 |
|---|---|---|
| Background Type | Solid Color | |
| Background **Alpha** | **0** | |
| Post Processing | Off | |
| HDR Rendering | Off | |

> 원문: *"If alpha is not zero or if HDR is active, transparency will not work."*
> → **알파 0과 HDR 비활성이 동시 조건.** 둘 중 하나만 어겨도 투명이 죽는다.

### Player Settings (Unity 6.1)

| 위치 | 항목 | 값 |
|---|---|---|
| Resolution & Presentation | Fullscreen Mode | **Windowed** |
| Resolution & Presentation | Use DXGI Flip Model Swapchain | **비활성** |
| Other Settings | Auto Graphics API for Windows | **비활성** |
| Other Settings | Graphics APIs | **Direct3D11 우선** |

DXGI Flip Model과 D3D12/Vulkan 경로에서는 백버퍼 알파가 DWM에 전달되지 않는다는 것이 이 설정들의 이유로 보인다.

### 스크립트 — `TransparentGame.cs` (원문 전문)

**첫 씬의 아무 GameObject에나 부착**한다. 전체 54줄이 전부다.

```csharp
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class TransparentGame : MonoBehaviour
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        const int GWL_STYLE   = -16;
        const int WS_BORDER   = 0x00800000;
        const int WS_DLGFRAME = 0x00400000;
        const int WS_CAPTION  = WS_BORDER | WS_DLGFRAME;

        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong
            (IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea
            (IntPtr hWnd, ref MARGINS pMargins);

        void Start()
        {
            IntPtr hwnd = GetActiveWindow();

            int style = GetWindowLong(hwnd, GWL_STYLE);
            style &= ~WS_CAPTION;
            SetWindowLong(hwnd, GWL_STYLE, style);

            var margins = new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = 0,
                cyTopHeight = 0,
                cyBottomHeight = 0
            };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
#endif
}
```

### 원문 스크립트가 **하지 않는** 것

전문을 확보하고 나니 이쪽이 더 중요하다. 스크립트는 놀랄 만큼 최소한이다.

| 항목 | 원문 | 비고 |
|---|---|---|
| `WS_POPUP` 부여 | **하지 않음** | 스타일은 `WS_CAPTION`(= `WS_BORDER \| WS_DLGFRAME`) 제거뿐 |
| `WS_THICKFRAME` / `MINIMIZEBOX` / `MAXIMIZEBOX` / `SYSMENU` 제거 | **하지 않음** | 리사이즈 테두리·시스템 메뉴가 남는다 |
| `SetWindowPos` (위치·크기·Z오더) | **하지 않음** | 창 크기는 Player Settings 해상도 그대로 |
| `HWND_TOPMOST` (항상 위) | **하지 않음** | 별도 구현 영역 |
| `WS_EX_TRANSPARENT` (클릭 통과) | **하지 않음** | 별도 구현 영역 |
| `SWP_FRAMECHANGED` | **하지 않음** | `SetWindowLong` 후 프레임 재계산을 DWM 호출에 의존 |
| 오류 처리 / `hwnd` 유효성 검사 | **하지 않음** | |

즉 **"창 껍데기를 최소한으로 건드리고 나머지는 DWM + 렌더 설정에 맡기는" 접근**이다.
`MARGINS { -1, 0, 0, 0 }`에서 `cxLeftWidth = -1`은 DWM의 *sheet of glass* 관용구로, 프레임을 클라이언트 영역 전체로 확장하라는 의미다.

> 참고 — P/Invoke 시그니처가 `int` 기반(`GetWindowLong`/`SetWindowLong`)이다. 64비트에서 정석은 `GetWindowLongPtr`/`SetWindowLongPtr`이지만, `GWL_STYLE` 값은 32비트에 들어가므로 실동작에는 문제가 없다.

---

## 3. 현재 프로젝트와의 대조

현재 리포지토리(`Unity 6000.2.10f1`)의 실제 설정값을 확인한 결과:

| 항목 | 원문 요구 | 현재 프로젝트 | 위치 |
|---|---|---|---|
| Fullscreen Mode | Windowed | ✅ `fullscreenMode: 3` (Windowed) | `ProjectSettings.asset:109` |
| DXGI Flip Model Swapchain | 비활성 | ✅ `useFlipModelSwapchain: 0` | `ProjectSettings.asset:96` |
| Auto Graphics API (Windows) | 비활성 | ✅ `m_Automatic: 0` | `ProjectSettings.asset:533` |
| Graphics API 우선순위 | D3D11 우선 | ✅ `[Direct3D11, OpenGLCore]` | `ProjectSettings.asset:532` |
| Camera Background | Solid Color | ✅ `m_ClearFlags: 2` | `Main Camera.prefab:46` |
| Camera Background Alpha | 0 | ✅ `{r:0, g:0, b:0, a:0}` | `Main Camera.prefab:47` |
| Camera Post Processing | Off | ✅ `m_RenderPostProcessing: 0` | `Main Camera.prefab:119` |
| Camera HDR | Off | ✅ `m_HDR: 0` | `Main Camera.prefab:81` |
| — | (원문 언급 없음) | ⚠️ `m_AllowHDROutput: 1` | `Main Camera.prefab:126` |
| — | (원문 언급 없음) | ⚠️ URP Asset `m_SupportsHDR: 1` | `PC_RPAsset.asset:26` |

**즉 원문이 요구하는 8개 항목을 현재 프로젝트가 이미 모두 충족한다.**
남은 두 개(`m_AllowHDROutput`, 파이프라인 레벨 `m_SupportsHDR`)는 원문이 다루지 않았으나 "HDR이 켜져 있으면 투명이 깨진다"는 서술의 연장선에서 **투명이 안 될 때 먼저 의심할 후보**다.

부수적으로 이미 맞춰져 있는 오버레이 필수 설정:
`runInBackground: 1`, `visibleInBackground: 1`, `resizableWindow: 0`, `allowFullscreenSwitch: 0`.

### 창 조작 코드 대조

원문 `TransparentGame.cs` vs 현 프로젝트 `WindowNativeManager.SetWindowFrame` (`Assets/ThirdParty/Unity-WindowNativeCustom/Script/WindowNativeManager.cs:87`)

| 동작 | 원문 | 현 프로젝트 |
|---|---|---|
| `GetActiveWindow()`로 hWnd 획득 | ✅ | ✅ |
| `WS_CAPTION` 제거 | ✅ | ✅ (`WS.CAPTION` = `0x00C00000`, 동일 값) |
| `WS_THICKFRAME`/`MINIMIZEBOX`/`MAXIMIZEBOX`/`SYSMENU` 제거 | ❌ | ✅ **추가로 수행** |
| `WS_POPUP` 부여 | ❌ | ✅ **추가로 수행** |
| `SetWindowPos` 위치·크기 지정 | ❌ | ✅ **추가로 수행** |
| `HWND_TOPMOST` | ❌ | ✅ **추가로 수행** |
| `SWP_FRAMECHANGED` | ❌ | ✅ **추가로 수행** |
| `DwmExtendFrameIntoClientArea(hwnd, MARGINS{-1,0,0,0})` | ✅ | ✅ **완전히 동일** |
| 호출 시점 | `MonoBehaviour.Start()` (첫 씬) | `[RuntimeInitializeOnLoadMethod(BeforeSplashScreen)]` → `ResolutionManager.Initialize()` |
| 컴파일 가드 | `#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` | 선언부 `... \|\| DEBUG`, 호출부 `#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` |

**결론 — 현 프로젝트의 창 조작 코드는 원문 스크립트의 완전한 상위집합이다.**
투명이 나오지 않는다면 **창 조작 쪽 원인이 아니다.** 렌더 설정 또는 blit 스택을 봐야 한다.

두 구현의 유일한 실질적 차이는 `WS_POPUP` 부여 여부다. `WS_POPUP`은 비클라이언트 영역을 통째로 없애므로, `DwmExtendFrameIntoClientArea`가 확장할 프레임 자체가 사라진다. **DWM 확장과 `WS_POPUP`이 상호작용할 가능성**이 있으니, 투명이 안 나올 때 검증 항목으로 남겨둔다 (확인된 사실 아님 — 가설).

---

## 4. 이 자료가 시사하는 것

### 4-1. `BlitFeature` / `BlitPass` / `MakeTransparent.shader` 스택이 불필요할 수 있다

현재 프로젝트는 렌더 결과의 알파를 **셰이더로 강제 덮어쓰는** 방식이다.

```
BlitFeature → BlitPass → Custom/MakeTransparent_URP: color.a = _TransparencyFactor
```

원문의 방식은 **카메라가 알파 0으로 클리어한 결과를 그대로 DWM에 넘기는 것**이라 blit 패스가 없다.
현재 프로젝트가 이미 알파 0 + HDR off + D3D11 + flip model off를 만족하므로, **blit 스택을 제거해도 투명이 나올 가능성이 있다.**

의미:
- [advise/001-5](../../advise/001-build-blockers.md#1-5-blitpass가-같은-텍스처를-소스이자-대상으로-사용한다)에서 지적한 `BlitTexture(cmd, src, src, ...)` 문제를 **고치는 대신 스택 자체를 지우는** 선택지가 생긴다.
- 다만 두 방식은 결과가 다르다. 셰이더 방식은 **그려진 픽셀까지 반투명(알파 0.01)**으로 만들고, 원문 방식은 **그려진 픽셀은 불투명, 배경만 투명**하다. 어느 쪽이 원하는 그림인지 먼저 정해야 한다.
  - "게임 오브젝트는 또렷하게, 배경만 데스크톱이 비침" → 원문 방식. blit 제거.
  - "화면 전체가 유령처럼 반투명" → 현재 방식 유지, 단 src/dst 분리 필요.

### 4-2. 검증 순서 제안

투명이 안 나올 때 뒤지는 순서를 고정해두면 시간이 크게 절약된다.

1. 카메라 알파 0 / Clear Flags = Solid Color
2. 카메라 HDR off → URP Asset `m_SupportsHDR` off → `m_AllowHDROutput` off
3. Post Processing off (카메라 + Renderer)
4. Player Settings: Windowed / Flip Model off / D3D11 강제
5. `DwmExtendFrameIntoClientArea(hWnd, MARGINS{ cxLeftWidth = -1 })` 호출 여부와 `hWnd` 유효성
6. **`WS_POPUP`을 빼고** 원문처럼 `WS_CAPTION`만 제거해 본다 (현 프로젝트만의 차이점)
7. 그 다음에야 셰이더/blit 패스를 의심한다

**최소 재현 절차** — 원인이 창인지 렌더인지 한 번에 가른다.
빈 씬에 `TransparentGame.cs`만 붙이고, `BlitFeature`를 Renderer에서 해제한 뒤 Windows 빌드를 돌린다.
- 투명이 나온다 → 원인은 blit 스택 또는 `SetWindowFrame`의 추가 동작. **§4-1의 "스택 삭제" 선택지가 유효**
- 안 나온다 → 원인은 프로젝트 설정 또는 엔진 버전(6.2) 동작 변경. 위 1~4를 다시 본다

### 4-3. 버전 민감성

원문이 "6.1에서 동작 방식이 바뀌었다"고 명시한다. 현 프로젝트는 **6000.2.10f1(6.2)** 로 그보다 더 최신이다.
→ **6.2에서 또 바뀌었을 가능성을 배제할 수 없다.** 투명 관련 이슈를 만나면 "코드 버그"보다 **"엔진 버전 동작 변경"을 먼저 의심**하고, Unity 릴리스 노트의 swapchain/DWM/HDR 항목을 확인하는 편이 빠르다.

### 4-4. 확인되지 않은 영역

원문이 다루지 않아 별도 조사가 필요한 것들:

- **클릭 통과** (`WS_EX_TRANSPARENT`) — 현 프로젝트에 `SetTransparentClick`으로 존재하나 해제 불가 상태. [advise/001-4](../../advise/001-build-blockers.md#1-4-settransparentclickfalse가-동작하지-않는다)
- **항상 위** (`HWND_TOPMOST`) — 현 프로젝트는 `SetWindowFrame`에서 적용 중
- **작업 표시줄 아이콘 숨김** (`WS_EX_TOOLWINDOW`)
- **멀티 모니터 / DPI 스케일링** — 현 프로젝트의 `systemHeight - 49` 매직 넘버가 여기서 깨진다
- **입력 포커스 처리** — 오버레이가 포커스를 뺏지 않게 하는 방법 (`WS_EX_NOACTIVATE`)

---

## 5. 수집 이력

| 날짜 | 내용 |
|---|---|
| 2026-07-28 | 페이지 3회 조회로 설정값·목차·제목 확인. 스크립트는 첨부파일이라 미확보 상태로 기록 |
| 2026-07-28 | 사용자가 첨부파일을 직접 내려받아 제공 → **`TransparentGame.cs` 전문 확보.** §2 재구성 코드를 원문으로 교체 |

**교체 시 드러난 오류** — 이전 재구성은 `SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE)`로 추정했으나, **원문은 `WS_POPUP`을 쓰지 않고 기존 스타일에서 `WS_CAPTION`만 제거**한다. 재구성이 실제보다 과하게 창을 조작하는 것으로 잘못 그렸다. 원문의 핵심은 최소 개입이다.

### 여전히 미확보

- 원문 본문의 `🎨 Camera Setup` / `⚙️ Required Player Settings` 절 **원문 그대로의 문장** — 설정 항목과 값은 3회 조회로 교차 확인했으나 축자 인용은 아니다.
- `✨ Final Result` 절의 스크린샷/영상 — 어떤 시각적 결과를 "성공"으로 보는지 판단 기준.
