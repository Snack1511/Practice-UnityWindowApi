# 006 — 설계 참조: 검증된 구조에서 무엇을 가져올 것인가 (P2)

`docs/reference/`의 외부 자료 두 건을 **설계 관점에서** 정리한 제언.
자료 자체의 내용은 reference 문서에 있고, **여기는 "그래서 우리 코드를 어떤 형태로 바꿀 것인가"만 다룬다.**

- [UniWindowController (kirurobo, MIT, 725★)](../docs/reference/uniwindowcontroller.md) — 같은 문제를 8년째 풀고 있는 성숙한 구현
- [Sunny Valley Studio — Desktop Overlay in Unity 6](../docs/reference/desktop-overlay-unity6.md) — 최소 구현의 반대 극단

**전제** — 이 프로젝트의 목적은 Win32 창 제어 학습이다. 라이브러리를 넣으면 배울 대상이 사라진다.
따라서 **코드를 가져오는 것이 아니라 구조를 가져온다.** 아래 항목은 전부 "직접 구현하되 형태만 참조"를 뜻한다.

---

## 6-1. 창 제어를 "명령"이 아니라 "상태"로 모델링한다

### 현재

`Assets/ThirdParty/Unity-WindowNativeCustom/Script/WindowNativeManager.cs:87` — `SetWindowFrame()` 하나가 **다섯 가지 일**을 한다.

```csharp
public static string SetWindowFrame(int x, int y, int w, int h)
{
    hWnd = GetActiveWindow();
    // (1) 스타일 제거  (2) POPUP 부여  (3) 위치·크기  (4) TOPMOST  (5) DWM 확장
    ...
    return GetWindowText(...);   // (6) 창 제목까지 반환
}
```

**문제**
- **부분 적용 불가** — "항상 위만 끄고 싶다"가 안 된다. 전부 다시 호출해야 한다.
- **되돌리기 불가** — 적용 후 원래 스타일을 모른다.
- **상태 조회 불가** — 지금 topmost인지 코드가 모른다. 유일한 상태 변수 `IsTransparentClick`은 실제 창 상태와 어긋난다 ([001-4](001-build-blockers.md#1-4-settransparentclickfalse가-동작하지-않는다)).
- **반환값이 창 제목** — 함수 이름과 무관한 부수 효과.

### 참조 형태

UniWindowController는 **모든 창 속성을 개별 프로퍼티**로 노출한다.

```csharp
isTransparent · isTopmost · isBottommost · isClickThrough · isZoomed
windowPosition · windowSize · clientSize
```

각 축이 독립적으로 켜지고 꺼지며, **setter가 곧 Win32 호출**이고 getter가 현재 상태다.

### 제안

`WindowNativeManager`를 **두 겹**으로 나눈다.

```
WindowNativeManager   (변경)  — P/Invoke 선언과 상수만. 로직 없음. 얇게 유지
        ↑
WindowController      (신규)  — 상태 프로퍼티 + 적용 로직. 순수 C#
        ↑
ResolutionManager     (변경)  — 부팅 시 원하는 값을 세팅하는 호출자
```

```csharp
public static class WindowController
{
    private static IntPtr hWnd;
    private static uint originalStyle;      // 되돌리기용 원본 보관

    public static void Initialize()
    {
        hWnd = WindowNativeManager.GetActiveWindow();
        originalStyle = WindowNativeManager.GetWindowLong(hWnd, GWL.STYLE);
    }

    public static bool IsBorderless { get => ...; set => ApplyBorderless(value); }
    public static bool IsTopmost    { get => ...; set => ApplyTopmost(value); }
    public static bool IsClickThrough { get => ...; set => ApplyClickThrough(value); }
    public static Vector2Int Position { get; set; }
    public static Vector2Int Size     { get; set; }

    public static event Action<WindowStateChange> OnStateChanged;   // 6-3 참조
}
```

**얻는 것**
- 축별 개별 제어 → `SetTransparentClick`의 해제 불가 문제가 구조적으로 사라진다
- `originalStyle` 보관 → 되돌리기 가능
- `WindowController`가 순수 C#이라 **P/Invoke를 모킹하면 테스트 가능**
- `WindowNativeManager`의 `#if ... || DEBUG` 가드가 선언부에만 남아 [001-2](001-build-blockers.md)류 사고가 줄어든다

**비용** — 파일 1개 추가, 기존 호출부 1곳(`ResolutionManager`) 수정. 지금 호출부가 하나뿐이라 **이보다 쌀 수 없는 시점**이다.

---

## 6-2. 직교하는 축은 열거형으로 분리한다

UniWindowController가 **투명 방식**과 **히트 테스트 방식**을 별개 열거형으로 둔 것은 의미가 있다.

```csharp
enum TransparentType { None, Alpha, ColorKey }   // 어떻게 투명하게 만드는가
enum HitTestType     { None, Opacity, Raycast }  // 클릭 통과를 어떻게 판정하는가
```

두 축은 독립이다 — `Alpha` + `Raycast`도, `ColorKey` + `Opacity`도 성립한다. 하나의 플래그로 묶으면 조합이 표현되지 않는다.

**현 프로젝트 적용**
현재는 투명 방식이 "URP Blit 셰이더" 하나로 코드에 박혀 있고, 클릭 통과는 bool 하나다.
[001-5](001-build-blockers.md#1-5-blitpass가-같은-텍스처를-소스이자-대상으로-사용한다)에서 **blit 스택 삭제 vs 유지**를 판단해야 하는데, **열거형으로 분리해두면 둘 다 유지하며 전환 비교가 가능**하다.

```csharp
public enum TransparentMode
{
    None,
    CameraAlpha,   // 카메라 알파 0 + DWM만. blit 없음 (참고 자료 방식)
    BlitShader,    // 현재 방식. MakeTransparent로 전체 알파 강제
}
```

> ✅ **결론 남 (2026-08-12) — 만들지 않는다.** 판별 실험 결과 `CameraAlpha` 만으로 투명이 성립했고,
> [001-5](001-build-blockers.md) 에서 blit 스택을 삭제했다. 남길 방식이 하나뿐이라 열거형이 필요 없다.
> `BlitShader` 가 다시 필요해지면 삭제 커밋을 `git revert` 한 뒤에 이 설계를 꺼낸다.

---

## 6-3. 상태 변화는 폴링이 아니라 이벤트로

UniWindowController: `OnStateChanged(WindowStateEventType)` · `OnMonitorChanged()` · `OnDropFiles(string[])`

**현 프로젝트에 이미 있는 것** — `EventBusLocater`(전역 타입 라우팅) 하나뿐이고, [004-5](004-architecture-scale.md#4-5-eventbuslocater의-구조적-위험)에서 지적했듯 사용처가 1곳이며 구조적 위험이 있다.

**제안** — 창 상태처럼 **발신자가 명확한 이벤트는 전역 버스에 태우지 않는다.** `WindowController`가 자기 이벤트를 갖는다.

```csharp
public static event Action<WindowStateChange> OnStateChanged;
```

이유: 전역 버스는 "누가 이 이벤트를 듣는가"를 코드에서 추적 불가능하게 만든다. 발신자가 하나로 고정된 이벤트에 그 비용을 낼 이유가 없다.
**전역 버스는 발신자가 여럿이고 수신자를 모를 때만 값어치가 있다.**

---

## 6-4. 설정 의존이 큰 기능에는 자동 검증 장치를 붙인다

**근거** — 투명 창은 코드보다 **프로젝트 설정 의존이 압도적으로 크다.**

독립된 두 출처가 동일하게 요구하는 확정 요건:

| 요건 | 두 출처 일치 |
|---|---|
| Direct3D11 사용 (D3D12는 투명 불가) | ✅ |
| `Use DXGI flip model swapchain for D3D11` 비활성 | ✅ |
| HDR 비활성 | ✅ |
| 카메라 배경 Solid Color + 알파 0 | ✅ (SVS 명시) |
| URP Alpha Processing 활성 | ✅ (UniWinC 명시) |

UniWindowController는 이 문제를 **인스펙터의 "녹색 버튼 하나로 Player Settings 일괄 수정"**으로 해결한다.

**현 프로젝트 적용** — 자동 수정까지는 과하다. **검증만으로 충분하다.**
[001-3](001-build-blockers.md#1-3-lobbyscene이-build-settings에-없다--첫-화면-이후-진행-불가)에서 제안한 `[InitializeOnLoad]` 씬 검증기와 **같은 자리에 창 설정 검증을 얹는다.**

```csharp
// Assets/Editor/OverlaySettingsValidator.cs
[InitializeOnLoad]
static class OverlaySettingsValidator
{
    static OverlaySettingsValidator()
    {
        if (PlayerSettings.useFlipModelSwapchain)
            Debug.LogWarning("[Overlay] DXGI Flip Model이 켜져 있으면 투명이 동작하지 않습니다.");

        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
        if (apis.Length == 0 || apis[0] != GraphicsDeviceType.Direct3D11)
            Debug.LogWarning("[Overlay] Graphics API 1순위가 Direct3D11이 아닙니다.");
        // URP Asset의 HDR / Alpha Processing도 같은 방식으로 확인
    }
}
```

**설정이 조용히 바뀌어 투명이 깨지는 사고를 컴파일 시점에 잡는다.** 런타임 비용 0, 코드 30줄.
지금 프로젝트 설정이 전부 올바르게 맞춰져 있어서 **오히려 지금 만들어야 한다** — 깨진 뒤에는 무엇이 정상이었는지 모른다.

---

## 6-5. 히트 테스트는 만들되, 싼 쪽부터 만든다

**핵심 UX** — "캐릭터가 있는 픽셀은 클릭이 먹고, 빈 곳은 뒤 창으로 통과". 이게 없으면 오버레이는 **화면 전체를 막는 창**이거나 **아무것도 클릭할 수 없는 창** 둘 중 하나다.

현재 `SetTransparentClick(bool)`은 수동 토글이고 해제도 안 된다 ([001-4](001-build-blockers.md)). 즉 **이 기능이 사실상 없다.**

UniWindowController의 두 방식:

| 방식 | 판정 | 비용 | 선결 조건 |
|---|---|---|---|
| `Raycast` | 콜라이더 레이캐스트 | 가벼움 | 오브젝트에 콜라이더 필요 |
| `Opacity` | 커서 위치 픽셀 알파 vs 임계값 | 무거움 (GPU→CPU 리드백) | 없음. 보이는 그대로 일치 |

**권장 순서**
1. **`Raycast`부터.** `Camera.ScreenPointToRay` + `Physics2D.OverlapPoint` 수준이면 끝난다. 현 프로젝트는 2D(`Rigidbody2D`, `SpriteRenderer`)라 특히 싸다.
2. 판정 결과로 매 프레임 `IsClickThrough`를 갱신한다 — [6-1](#6-1-창-제어를-명령이-아니라-상태로-모델링한다)의 프로퍼티가 있어야 성립한다. **선행 조건이다.**
3. 갱신은 `GameProcessManager.AddUpdate("WindowHitTest", ...)`로 등록한다 ([architecture.md](../docs/architecture.md) 규약).
4. `Opacity`는 **스프라이트 경계와 콜라이더가 실제로 어긋나서 문제가 될 때** 만든다. 리드백은 프레임 동기화·성능 문제를 동반하므로 필요가 증명되기 전에는 손대지 않는다.

> **매 프레임 `SetWindowLong`을 호출하지 말 것.** 상태가 바뀔 때만 호출한다. [6-1](#6-1-창-제어를-명령이-아니라-상태로-모델링한다)의 프로퍼티 setter에서 이전 값과 비교해 걸러내면 자연히 해결된다.

---

## 6-6. 지금 만들지 말아야 할 것

UniWindowController에 있다고 해서 전부 필요한 건 아니다. **연습 프로젝트에 없어도 되는 것**:

| 기능 | 판단 |
|---|---|
| macOS 지원 / 플랫폼 추상화 레이어 | 대상 플랫폼이 Windows 하나다. 구현체가 하나뿐인 인터페이스는 만들지 않는다 |
| 다중 창 | UniWindowController조차 미지원이다 |
| 파일 드래그 앤 드롭 / 파일 다이얼로그 | 요구가 없다 |
| 배경화면 모드(`isBottommost`) | 재미있지만 지금 쓸 데가 없다 |
| 네이티브 DLL 분리 | C# P/Invoke로 충분하다. **네이티브를 쓰는 순간 빌드 파이프라인이 생긴다** |
| `ColorKey` 투명 방식 | `Alpha` 계열이 동작하는 한 불필요 |

**필요해지면 그때 만든다.** 지금 만들면 검증할 방법도 없이 코드만 는다.

---

## 6-7. 프로젝트 전반에 적용할 구조 참조

창 제어 외에, UniWindowController에서 가져올 만한 일반 구조.

| 참조 | 현 프로젝트 적용 |
|---|---|
| **Runtime / Editor asmdef 분리** (`Kirurobo.UniWindowController` / `.Editor`) | [004-1](004-architecture-scale.md#4-1-asmdef-도입)의 첫 단계와 정확히 같다. `Project.Editor` 하나만 만들어도 [001-1](001-build-blockers.md)류 사고가 막힌다 |
| **`LowLevel/` 하위 폴더로 P/Invoke 격리** (`UniWinCore.cs`, `FilePanel.cs`) | [6-1](#6-1-창-제어를-명령이-아니라-상태로-모델링한다)의 2계층 분리와 같은 발상. 네이티브 경계를 폴더로 표시하면 "여기는 플랫폼 종속"이 한눈에 보인다 |
| **`static current` 단일 인스턴스** | 현 프로젝트의 `Singleton<T>`와 역할이 겹친다. 다만 **UniWinC는 "씬에 배치된 프리팹이 자기를 등록"**하는 방식이라, 인스펙터에서 값을 조정할 수 있다. `MonoSingleton<T>`이 씬의 기존 인스턴스를 찾지 않는 문제([004-6](004-architecture-scale.md#4-6-잔여-코드-품질-항목))와 대비된다 |
| **커스텀 인스펙터로 설정을 한곳에** | 창 설정처럼 **여러 파일에 흩어진 값**은 인스펙터 하나로 모아 보여주는 것이 사고를 줄인다. [6-4](#6-4-설정-의존이-큰-기능에는-자동-검증-장치를-붙인다)의 검증기를 만들 때 같이 고려 |
| **`Samples~` 폴더 규약** | 샘플 씬을 배포물에서 분리하는 UPM 규약. 현 프로젝트의 `TestScene`이 프로덕션 씬 목록에 섞여 있는 것([002-7](002-scene-system.md#2-7-사용되지-않는-경로-정리))과 대비된다 |

---

## 우선순위 요약

**하나만 한다면 [6-1]** — 상태 기반 재설계. 나머지 대부분이 여기에 의존한다([6-2], [6-3], [6-5]).
**두 개 한다면 [6-1] + [6-4]** — 재설계 + 설정 검증기. 창 관련 사고의 대부분을 덮는다.
**[6-5]는 클릭 통과를 실제로 쓸 때** 착수한다. 그 전에는 [6-1]만 있으면 준비가 끝난 상태다.
**[6-6]은 하지 않기로 하는 결정 자체가 산출물**이다. 나중에 "이건 왜 없지"라는 질문이 나올 때의 답이 된다.
