# 001 — 빌드/실행 차단 이슈 (P0)

에디터에서는 드러나지 않고 **빌드하거나 실제 흐름을 태우는 순간 터지는** 항목만 모았다.
프레임워크 골격이 계속 커지기 전에 여기부터 막아야 한다.

---

## 1-1. 런타임 스크립트가 `UnityEditor` 네임스페이스를 참조한다 — 빌드 컴파일 실패

**근거** — `Assets/Script/GameFlow/GameScene/TestScene.cs:7`

```csharp
using UnityEditor.Overlays;
```

**영향**
`UnityEditor` 어셈블리는 플레이어 빌드에 포함되지 않는다. 사용하지 않는 `using`이라도 네임스페이스가 존재하지 않으므로 `CS0246`으로 **빌드 자체가 실패**한다. 에디터 플레이에서는 아무 증상이 없어서 빌드를 돌릴 때까지 발견되지 않는다.

**권장**
1. 해당 `using` 삭제 (사용처 없음).
2. 재발 방지 — 런타임 코드를 asmdef로 분리하면 `UnityEditor` 참조가 **에디터에서 즉시 컴파일 에러**로 잡힌다. → [004](004-architecture-scale.md#4-1-asmdef-도입)

---

## 1-2. Windows 빌드 전용 블록의 심볼 미해결 — 빌드 컴파일 실패

플랫폼 조건부 블록은 **에디터에서 컴파일되지 않으므로 IDE도 검증해주지 않는다.** 현재 두 곳이 깨져 있다.

### (a) `ResolutionManager` — `UnityEngine` using 없음

**근거** — `Assets/Script/Manager/StaticManager/ResolutionManager.cs:1-11`

```csharp
namespace Script.Manager.StaticManager      // ← using 지시문이 파일에 하나도 없음
{
    public static class ResolutionManager
    {
        public static void Initialize()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        int rWidth = Display.main.systemWidth;      // Display = UnityEngine.Display → 미해결
```

### (b) `DebuggingComponent` — `WindowNativeManager` 네임스페이스 미참조

**근거** — `Assets/Script/DebuggingComponent.cs:1-27`

```csharp
using System; using System.Collections.Generic; using TMPro; using UnityEngine;
namespace Script
{
    public class DebuggingComponent : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private void Awake()
        {
            string str = WindowNativeManager.GetWindowName();   // 실제 위치: Script.Manager.StaticManager
```

**영향**
스탠드얼론 Windows 빌드에서 두 파일 모두 `CS0103`/`CS0246`. **이 프로젝트의 주 타깃 플랫폼이 바로 그 플랫폼이다.**

**권장**
1. `ResolutionManager.cs` 최상단에 `using UnityEngine;` 추가.
2. `DebuggingComponent.cs`에 `using Script.Manager.StaticManager;` 추가.
3. 근본 대책 — **조건부 컴파일 블록을 CI/수동 빌드로 주기적으로 검증**하거나, 플랫폼 분기를 `#if`가 아니라 런타임 분기(`Application.platform`)로 바꿔 항상 컴파일되게 한다. P/Invoke 선언부는 `#if`가 필요하지만, **호출부는 런타임 분기로 빼는 편이 검증 가능성이 훨씬 높다.**

---

## 1-3. `LobbyScene`이 Build Settings에 없다 — 첫 화면 이후 진행 불가

**근거**

`ProjectSettings/EditorBuildSettings.asset` 등록 목록:
```
StartScene / LoadingScene / GameScene / TestScene
```

`ESceneType` 정의 (`GameScene/SceneBase.cs:8`):
```
None / StartScene / LoadingScene / LobbyScene / MenuScene / TestScene
```

부팅 직후 흐름 (`GameFlow/GameScene/StartScene.cs:47`):
```csharp
SceneManager.Instance.ChangeScene(ESceneType.LobbyScene, null, null, true);
```

**영향**
- `LobbyScene` · `MenuScene`은 **미등록** → `SceneManager.LoadSceneAsync("LobbyScene", Additive)`가 로드되지 않고 에러 로그만 남는다. 부팅 → 로딩씬까지는 뜨지만 **로비로 넘어가지 못한다.**
- `GameScene`은 등록되어 있으나 `ESceneType`에 대응 항목이 없다 → 코드에서 접근 불가한 죽은 등록.
- 이 불일치는 **컴파일 타임에 잡히지 않고, 런타임 로그로만 드러난다.**

**권장**
1. 즉시: Build Profiles(Build Settings)에 `LobbyScene`, `MenuScene` 추가. `GameScene`은 쓰지 않으면 목록에서 제거.
2. 구조적 방어: **`ESceneType` ↔ Build Settings 정합성 검사**를 에디터 스크립트로 자동화. → [002](002-scene-system.md#2-4-씬-등록이-4곳으로-흩어져-있다)

```csharp
// Assets/Editor/SceneRegistryValidator.cs (신규, Editor 폴더 필수)
[InitializeOnLoad]
static class SceneRegistryValidator
{
    static SceneRegistryValidator()
    {
        var registered = EditorBuildSettings.scenes
            .Select(s => Path.GetFileNameWithoutExtension(s.path)).ToHashSet();

        foreach (ESceneType t in Enum.GetValues(typeof(ESceneType)))
        {
            if (t == ESceneType.None || registered.Contains(t.ToString())) continue;
            Debug.LogError($"[SceneRegistry] {t} 가 Build Settings에 없습니다.");
        }
    }
}
```
에디터 재컴파일마다 자동 검사되고, 런타임 비용은 0이다.

---

## 1-4. `SetTransparentClick(false)`가 동작하지 않는다

**근거** — `Assets/Script/Manager/StaticManager/WindowNativeManager.cs:135-142`

```csharp
public static void SetTransparentClick(bool flag)
{
    IsTransparentClick = flag;
    uint exFlag = GetWindowLong(hWnd, GWL.EXSTYLE);
    exFlag |= (WS_EX.TRANSPARENT);     // ← flag 값과 무관하게 항상 OR
    SetWindowLong(hWnd, GWL.EXSTYLE, exFlag);
}
```

**영향**
`false`를 넘겨도 `WS_EX_TRANSPARENT`가 유지되어 **클릭 통과를 끌 수 없다.** 오버레이 앱에서 "평소엔 통과, 상호작용 시엔 입력 받기"는 핵심 기능인데 편도로만 동작한다.
게다가 `IsTransparentClick` 프로퍼티는 `false`를 기록하므로 **상태 변수와 실제 창 상태가 어긋난다.**

**권장**
```csharp
public static void SetTransparentClick(bool flag)
{
    if (hWnd == IntPtr.Zero) hWnd = GetActiveWindow();

    uint exFlag = GetWindowLong(hWnd, GWL.EXSTYLE);
    exFlag = flag ? (exFlag | WS_EX.TRANSPARENT) : (exFlag & ~WS_EX.TRANSPARENT);
    if (0 == SetWindowLong(hWnd, GWL.EXSTYLE, exFlag))
        Debug.LogWarning($"SetWindowLong 실패: {Marshal.GetLastWin32Error()}");

    IsTransparentClick = flag;   // 실제 적용 후에 기록
}
```
`hWnd`가 `SetWindowFrame` 호출 전에는 `IntPtr.Zero`라는 점도 함께 방어한다.

---

## 1-5. `BlitPass`가 같은 텍스처를 소스이자 대상으로 사용한다

**근거** — `Assets/Script/Shader/BlitPass.cs:79`

```csharp
Blitter.BlitTexture(CommandBuffer, data.src, data.src, data.material, 0);
```

**영향**
동일 RenderTexture를 동시에 읽고 쓰는 것은 그래픽 API 상 정의되지 않은 동작이다. GPU/드라이버/해상도에 따라 결과가 달라지고, 특정 환경에서만 화면이 깨지는 재현 난이도 최상급 버그가 된다.
같은 파일에 임시 RT(`passData.tmp`)를 만들려던 코드가 주석으로 남아 있어 **이미 인지된 문제**로 보인다.

**부수 문제**
- `BlitPass.cs:6` — `using static Unity.Burst.Intrinsics.X86.Avx;` 의미 없는 using. 삭제 권장.
- `BlitFeature.cs:20` — `settings.blitMaterial`이 null이어도 그대로 패스를 등록한다. `Create()`/`AddRenderPasses`에서 null 체크 후 skip 필요.

**권장**
주석 처리된 임시 RT 경로를 복원해 `src → tmp → src` 2회 blit로 바꾸는 것이 직접적인 수정이다.

다만 **그 전에 blit 패스가 필요한지부터 확인할 것.** 카메라 배경 알파 0 + HDR off + DXGI Flip Model off + D3D11 조합이면 blit 없이 픽셀 단위 투명이 나오며, **현 프로젝트는 그 조건을 이미 전부 충족하고 있다.** 설정 대조표와 검증 순서: [docs/reference/desktop-overlay-unity6.md](../docs/reference/desktop-overlay-unity6.md)

참고 자료의 스크립트 전문(`TransparentGame.cs`)을 확보해 대조한 결과, **현 프로젝트의 창 조작 코드는 그 스크립트의 완전한 상위집합**이다(동일한 `WS_CAPTION` 제거 + 동일한 `DwmExtendFrameIntoClientArea(MARGINS{-1,0,0,0})`, 여기에 topmost·위치·크기까지 추가). 프로젝트 설정 8개 항목도 전부 일치한다.
**즉 두 접근의 차이는 blit 스택 하나뿐이다.**

두 방식은 결과가 다르므로 원하는 그림을 먼저 정한다.
- **게임 오브젝트는 또렷, 배경만 데스크톱이 비침** → blit 스택 삭제 (가장 싼 해결)
- **화면 전체가 반투명** → 현재 방식 유지 + src/dst 분리 필수

**판별 방법** — 빈 씬 + `BlitFeature`를 Renderer에서 해제 + Windows 빌드.
투명이 나오면 blit 스택은 불필요하고, 안 나오면 원인은 설정/엔진 버전 쪽이다. 이 실험 한 번이면 위 선택지가 갈린다.

---

## 처리 체크리스트

- [ ] `TestScene.cs:7` `using UnityEditor.Overlays;` 삭제
- [ ] `ResolutionManager.cs` `using UnityEngine;` 추가
- [ ] `DebuggingComponent.cs` `using Script.Manager.StaticManager;` 추가
- [ ] Build Settings에 `LobbyScene` / `MenuScene` 등록, `GameScene` 정리
- [ ] `SetTransparentClick` 해제 경로 구현
- [ ] `BlitPass` src/dst 분리 또는 **패스 스택 삭제** ([reference 문서](../docs/reference/desktop-overlay-unity6.md) 확인 후 결정)
- [ ] **위 수정 후 실제 Windows 스탠드얼론 빌드를 한 번 돌려 확인** ← 이 항목이 검증의 전부다
