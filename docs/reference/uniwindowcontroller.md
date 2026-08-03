# [참고] UniWindowController (kirurobo) — 투명 창 라이브러리

> **저장소**: <https://github.com/kirurobo/UniWindowController> — "Makes your Unity window transparent and allows you to drop files"
> **라이선스**: **MIT** / **최신 릴리스**: `v0.9.8` (2025-11-17) / 725★ · 91 fork · 미아카이브 · 최근 푸시 2026-05-11
> **수집일**: 2026-07-28 / **상태**: 외부 참고 자료. **도입 안 함.** 향후 투명 창 제언 시 근거 및 구현 대조용.
>
> **수집 방법** — GitHub API(메타데이터·릴리스), `upm` 브랜치의 `UniWindowController.cs`·`package.json`, `main` 브랜치 README를 각각 조회. 아래 API 목록은 소스에서 추출한 것이나 **원본 파일을 직접 정독한 것이 아니라 조회 결과를 정리한 것**이다. 실제 도입 시에는 소스를 직접 확인할 것.
> 코드 저작권은 kirurobo에게 있다(MIT). 이 문서는 API 목록과 분석이며 소스 전문 인용은 아니다.

---

## 1. 무엇인가

Windows / macOS 스탠드얼론 빌드에서 **창 투명화 · 테두리 제거 · 항상 위 · 클릭 통과 · 창 이동/크기 · 파일 드롭**을 통합 제공하는 Unity 라이브러리.
네이티브 플러그인(`LibUniWinC.dll` / `LibUniWinC.bundle`)에 Win32·Cocoa 호출을 넣고, C# 쪽은 `UniWindowController` MonoBehaviour 하나로 노출한다.

**현 프로젝트가 손으로 만들고 있는 것의 완성판**이라고 보면 정확하다.

## 2. 설치

| 방법 | 값 |
|---|---|
| UPM Git URL | `https://github.com/kirurobo/UniWindowController.git#upm` |
| unitypackage | Release 페이지에서 다운로드 |
| OpenUPM | 없음 |

`package.json`:
```json
{
    "name": "com.kirurobo.uniwinc",
    "version": "0.9.8",
    "displayName": "UniWindowController",
    "description": "Unified window controller for Mac and Windows",
    "unity": "2022.3",
    "dependencies": {},
    "samples": [{ "displayName": "UniWinC samples", "path": "Samples~" }]
}
```

> **버전 요구가 문서마다 다르다.** README는 `Unity 2019.4.31f1 or later`, `package.json`은 `"unity": "2022.3"`. UPM은 `package.json`을 기준으로 경고하므로 **실질 하한은 2022.3**으로 보는 편이 안전하다. 현 프로젝트(6000.2.10f1)는 어느 쪽이든 충족.

### 사용 절차 (README 원문 기준)

1. `Runtime/Prefabs`의 `UniWindowController` 프리팹을 씬에 배치
2. Player Settings 조정 — **인스펙터의 녹색 버튼이 필요한 설정을 한 번에 바꿔준다**
3. `IsTransparent` 등 원하는 값 설정
4. 창 드래그 이동이 필요하면 `DragMoveCanvas` 프리팹 추가
5. PC / Mac 스탠드얼론 빌드

## 3. 구조

```
Runtime/
├─ Scripts/
│  ├─ UniWindowController.cs        ← 사용자가 만지는 유일한 API
│  ├─ UniWindowMoveHandle.cs        ← UI 요소에 붙여 드래그 이동
│  └─ LowLevel/
│     ├─ UniWinCore.cs              ← 네이티브 P/Invoke 래퍼
│     └─ FilePanel.cs               ← 파일 열기/저장 다이얼로그
├─ Plugins/Windows/x64/LibUniWinC.dll
├─ Plugins/Windows/x86/LibUniWinC.dll
├─ Plugins/MacOS/LibUniWinC.bundle
└─ Kirurobo.UniWindowController.asmdef
Editor/
├─ Scripts/UniWindowControllerEditor.cs       ← 커스텀 인스펙터 + 설정 자동 수정 버튼
├─ Scripts/UniWindowControllerBatch.cs
└─ Kirurobo.UniWindowController.Editor.asmdef
```

네이티브 소스도 공개되어 있다: `VisualStudio/`(Windows), `Xcode/`(macOS).
**asmdef가 Runtime/Editor로 분리되어 있다** — 참고할 만한 구성이다 ([advise/004-1](../../advise/004-architecture-scale.md#4-1-asmdef-도입)).

## 4. API 표면 (`Kirurobo.UniWindowController`)

### 열거형

```csharp
enum TransparentType { None = 0, Alpha = 1, ColorKey = 2 }
enum HitTestType     { None = 0, Opacity = 1, Raycast = 2 }

[Flags] enum MouseButton  { None = 0, Left = 1, Right = 2, Middle = 4 }
[Flags] enum ModifierKey  { None = 0, Alt = 1, Control = 2, Shift = 4, Command = 8 }
[Flags] enum WindowStateEventType
{
    None = 0, StyleChanged = 1, Resized = 2,
    TopMostEnabled = 25, TopMostDisabled = 17,
    BottomMostEnabled = 41, BottomMostDisabled = 33,
    WallpaperModeEnabled = 73, WallpaperModeDisabled = 65,
}
```

### 프로퍼티

| 분류 | 프로퍼티 |
|---|---|
| 투명 | `isTransparent` · `transparentType` · `alphaValue` · `keyColor` |
| Z오더 | `isTopmost` · `isBottommost` |
| 클릭 통과 | `isClickThrough` · `isHitTestEnabled` · `hitTestType` · `opacityThreshold` · `onObject` · `pickedColor` |
| 위치·크기 | `windowPosition` · `windowSize` · `clientSize` · `isZoomed` · `shouldFitMonitor` · `monitorToFit` · `isFreePositioningEnabled` |
| 기타 | `current`(static 인스턴스) · `allowDropFiles` · `currentCamera` · `autoSwitchCameraBackground` · `forceWindowed` · `cursorPosition` |

### 메서드 · 이벤트

```csharp
void SetCamera(Camera newCamera);
void SetTransparentType(TransparentType type);
void Focus();

static int     GetMonitorCount();
static Rect    GetMonitorRect(int index);
static Vector2 GetCursorPosition();
static void    SetCursorPosition(Vector2 position);
static MouseButton GetMouseButtons();
static ModifierKey GetModifierKeys();

event OnStateChangedDelegate  OnStateChanged;   // void (WindowStateEventType type)
event FilesDelegate           OnDropFiles;      // void (string[] files)
event OnMonitorChangedDelegate OnMonitorChanged;
```

## 5. 투명 구현 방식

### `TransparentType` 두 갈래 (Windows)

| 타입 | 방식 | 특징 |
|---|---|---|
| `Alpha` | 렌더 결과의 알파를 그대로 사용 | 기본값. **반투명 표현 가능** |
| `ColorKey` | 레이어드 윈도우 + 단색 키 컬러 투명화 | **반투명 불가**(0 아니면 1). 터치 조작 등에서 대안으로 쓰임 |

현 프로젝트가 하는 것(카메라 알파 + DWM)은 `Alpha` 쪽 계열이다.

### 클릭 통과의 히트 테스트 — **이 라이브러리의 핵심 가치**

단순히 `WS_EX_TRANSPARENT`를 켜고 끄는 게 아니라, **커서 아래에 실제로 그려진 것이 있는지 매 프레임 판정해서 클릭 통과를 자동 전환**한다.

| `HitTestType` | 판정 방법 | 트레이드오프 |
|---|---|---|
| `Opacity` | 커서 위치의 픽셀 알파를 `opacityThreshold`와 비교 | 보이는 그대로 일치. **비용 높음** |
| `Raycast` | 콜라이더로 레이캐스트 | 가볍다. **콜라이더 배치 필요** |

판정 결과는 `onObject`(오브젝트 위인가) / `pickedColor`(집어낸 색)로 노출된다.

> 현 프로젝트의 `WindowNativeManager.SetTransparentClick(bool)`은 **수동 토글이고 해제도 안 되는 상태**다 ([advise/001-4](../../advise/001-build-blockers.md#1-4-settransparentclickfalse가-동작하지-않는다)). "데스크톱 펫이 있는 곳만 클릭이 먹고 나머지는 뒤 창으로 통과"라는 오버레이의 핵심 UX는 **이 히트 테스트가 있어야 성립한다.** 직접 만들 때 가장 품이 드는 부분도 여기다.

## 6. 요구 설정 · 제약 (README 원문 기준)

| 항목 | 내용 |
|---|---|
| Direct3D12 | **배경 투명 불가** |
| Direct3D11 | `Use DXGI flip model swapchain for D3D11` **비활성화 시 투명 가능** |
| URP | **HDR 비활성** + **Alpha Processing 활성** 필요 |
| 에디터 | **에디터에서는 투명이 동작하지 않는다. 빌드해서 확인할 것** |
| 다중 창 | 미지원 |
| 터치 조작 | 지원 부족. `ColorKey`가 대안이 될 수 있으나 반투명 표현 불가 |
| 안정성 | 원문: "This has not been fully tested and there may be unstable behavior." |

> **독립 출처 교차 확인** — D3D11 + DXGI flip model 비활성 + HDR off 요구는 [Sunny Valley Studio 자료](desktop-overlay-unity6.md)와 **완전히 일치**한다. 서로 무관한 두 출처가 같은 결론이므로 **이 세 가지는 Unity 투명 창의 확정 요건으로 취급해도 된다.**

## 7. 현 프로젝트와의 대조

| 기능 | UniWindowController | 현 프로젝트 |
|---|---|---|
| 테두리·타이틀바 제거 | ✅ | ✅ `SetWindowFrame` |
| 항상 위 | ✅ `isTopmost` | ✅ `HWND_TOPMOST` |
| **항상 아래(배경화면 모드)** | ✅ `isBottommost` · `WallpaperMode` | ❌ |
| 배경 투명 | ✅ `Alpha` / `ColorKey` 선택 | ⚠️ URP Blit + 셰이더로 알파 강제 (방식이 다름) |
| **히트 테스트 클릭 통과** | ✅ `Opacity` / `Raycast` | ❌ 수동 토글이며 **해제 불가** |
| 창 위치·크기 API | ✅ `windowPosition` / `windowSize` | ⚠️ `SetWindowFrame` 인자로 1회 지정만 |
| **멀티 모니터** | ✅ `GetMonitorCount` / `GetMonitorRect` / `shouldFitMonitor` | ❌ `Display.main` + `systemHeight - 49` 하드코딩 |
| **파일 드래그 앤 드롭** | ✅ `OnDropFiles` | ❌ |
| **파일 다이얼로그** | ✅ `FilePanel` | ❌ |
| **창 드래그 이동 UI** | ✅ `DragMoveCanvas` / `UniWindowMoveHandle` | ❌ |
| 상태 변경 이벤트 | ✅ `OnStateChanged` | ❌ |
| **macOS** | ✅ | ❌ Windows 전용 |
| **설정 자동 수정 에디터 툴** | ✅ 인스펙터 녹색 버튼 | ❌ |

### 현 프로젝트 설정 즉시 점검 항목

UniWindowController가 URP 요구사항으로 명시한 두 가지를 실제 값으로 확인했다.

| 요구 | 현 프로젝트 | 파일 |
|---|---|---|
| HDR 비활성 | ⚠️ 카메라는 `m_HDR: 0`이나 **URP Asset은 `m_SupportsHDR: 1`** | `Assets/Settings/PC_RPAsset.asset:26` |
| Alpha Processing 활성 | ❌ **`m_AllowPostProcessAlphaOutput: 0`** (비활성) | `Assets/Settings/PC_RPAsset.asset:81` |

**Alpha Processing은 포스트 프로세싱 경로에서 알파를 보존할지의 스위치**다. 현 프로젝트는 카메라 포스트 프로세싱이 꺼져 있어(`Main Camera.prefab:119`) 지금 당장은 영향이 없지만, **포스트 프로세싱을 켜는 순간 알파가 파괴되어 투명이 깨진다.** 그때 원인을 찾기 매우 어려우므로 지금 켜두거나, 최소한 이 문서에 근거를 남겨둔다.

## 8. 도입 판단

**결론: 지금은 도입하지 않는 편이 맞다. 다만 특정 시점에는 갈아타는 것이 합리적이다.**

### 도입하지 않는 이유

이 리포지토리의 목적이 **Windows Native Window API 학습**이다(프로젝트명 `Practice-UnityWindowApi`). 라이브러리를 넣으면 연습 대상 자체가 사라진다. 학습 목적에서는 손으로 만드는 것이 정답이다.

### 도입을 검토할 시점

아래 중 **하나라도 실제 요구가 되면** 직접 구현 비용이 라이브러리 학습 비용을 넘어선다.

- **클릭 통과를 실사용해야 할 때** — 히트 테스트는 직접 만들면 픽셀 리드백/성능/경계 조건이 전부 문제가 된다. 난이도가 다른 항목과 급이 다르다
- macOS 지원
- 멀티 모니터 대응 (현재 `- 49` 매직 넘버가 이미 취약점)
- 파일 드롭 / 파일 다이얼로그
- 실제 배포 제품으로 전환

### 도입하지 않아도 지금 가져올 것

| 대상 | 적용처 |
|---|---|
| **API 네이밍** — `isTransparent` / `isTopmost` / `isClickThrough` / `windowPosition` 프로퍼티 형태 | `WindowNativeManager`가 현재 `SetWindowFrame` 하나에 모든 걸 넣고 있다. 프로퍼티 단위로 쪼개면 테스트와 재사용이 쉬워진다 |
| **`TransparentType` / `HitTestType` 열거형 분리** | "투명 방식"과 "히트 테스트 방식"을 별개 축으로 본 설계. 현 프로젝트가 나중에 ColorKey를 시도할 때 그대로 쓸 수 있다 |
| **`OnStateChanged` 이벤트** | 창 상태 변화를 폴링이 아니라 이벤트로 노출 |
| **에디터 설정 자동 수정 버튼** | 투명 창은 Player Settings 의존이 커서 사람이 매번 맞추면 반드시 틀린다. [advise/001-3](../../advise/001-build-blockers.md)의 `[InitializeOnLoad]` 검증기와 같은 발상 |
| **Runtime / Editor asmdef 분리** | [advise/004-1](../../advise/004-architecture-scale.md#4-1-asmdef-도입) |
| **README의 제약 목록** | D3D12 불가 / 에디터에서 투명 불가 / 다중 창 불가 — 삽질 예방 |

> 특히 **"에디터에서는 투명이 동작하지 않는다"**는 점을 기억할 것. 현 프로젝트도 동일하며, **에디터에서 안 된다고 코드를 의심하면 안 된다.** 검증은 반드시 빌드로 한다.

## 9. 미확인 항목

- `UniWinCore.cs`의 실제 P/Invoke 시그니처와 `LibUniWinC` 네이티브 구현 — 창 조작을 C#이 아니라 **네이티브 DLL에서 하는 이유**(메시지 훅? 서브클래싱?)를 확인하면 현 프로젝트 구조 결정에 참고가 된다.
- `Alpha` 모드가 DWM 확장을 쓰는지, 레이어드 윈도우를 쓰는지 — 현 프로젝트 방식과 같은 계열인지 확정 필요.
- `WindowStateEventType`의 비트 조합 의미 (8 = enabled 플래그로 보이나 미확인).
- 열린 이슈 19건의 내용 — 알려진 버그 파악용.
