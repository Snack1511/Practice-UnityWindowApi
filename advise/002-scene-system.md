# 002 — 씬 시스템 (P1)

씬 전환은 이 프레임워크의 중심축이고, 앞으로 붙을 모든 콘텐츠가 여기를 지나간다.
**지금 씬이 5개일 때 고치는 비용이, 15개일 때의 1/3이다.**

---

## 2-1. `LoadScene`이 완료 콜백을 버린다 — 이전 씬이 언로드되지 않음

**근거** — `Assets/Script/GameFlow/GameScene/SceneController.cs:114-127`

```csharp
public void LoadScene(ESceneType SceneType, ISceneInfo sceneInfo = null, Action SceneLoadComplete = null)
{                                                                    // ← 파라미터를 받지만
    if (scenes.TryGetValue(SceneType, out SceneBase targetSceneInstance))
    {
        var asyncOperation = SceneManager.LoadSceneAsync(SceneType.ToString(), LoadSceneMode.Additive);
        if (null != asyncOperation)
            asyncOperation.completed += (operation) => { targetSceneInstance.EnterScene(sceneInfo); };
    }                                                                // ← 본문에서 한 번도 쓰지 않는다
}
```

호출부 (`SceneController.cs:70-78`) — 이전 씬 언로드가 이 콜백에 들어 있다:

```csharp
LoadScene(nextScene, sceneInfo, () =>
{
    if (scenes.TryGetValue(prevSceneType, out SceneBase prevSceneInstance))
        UnloadScene(prevSceneType);      // ← 영원히 실행되지 않음
    SceneLoadComplete?.Invoke();         // ← 외부 완료 통보도 유실
});
```

**영향**
`isVisitLoadingScene: false` 경로로 전환하면 **이전 씬이 Additive로 계속 남는다.** 전환을 반복할수록 씬이 누적되고, `ExitScene`/`ReleaseResource`가 호출되지 않아 리소스도 해제되지 않는다. 호출자가 넘긴 완료 콜백도 사라지므로 "전환 끝났다"는 신호를 아무도 못 받는다.

**권장**
```csharp
asyncOperation.completed += _ =>
{
    targetSceneInstance.EnterScene(sceneInfo);
    SceneLoadComplete?.Invoke();
};
```
`asyncOperation`이 null인 경로(씬 미등록 등)에서도 콜백이 유실되므로, **실패 시 로그를 남기고 콜백을 호출할지 여부를 명시적으로 정한다.** 조용한 실패가 이 클래스의 반복 패턴이다.

---

## 2-2. 상태 전이가 로드 완료보다 먼저 일어난다

**근거** — `SceneController.cs:82` / `:105-112` / `:144-148`

```csharp
public void ChangeScene(...)
{
    ...                       // LoadSceneAsync 시작 (비동기)
    UpdateCurrentSceneType(); // ← 로드가 끝나기 전에 currentScene = nextScene 확정
}

public void UpdateScene()
{
    if (scenes.ContainsKey(currentScene))
        scenes[currentScene].UpdateScene();   // ← EnterScene 전인 씬의 Update가 돌 수 있음
}
```

**영향**
`EnterScene`이 아직 호출되지 않은(= `_sceneInfo`가 null이고 `enterScene == false`인) 씬의 `UpdateScene`이 수 프레임 먼저 실행된다. 지금은 각 씬의 `UpdateScene`이 비어 있어 증상이 없지만, **씬 상태에 접근하는 코드를 넣는 순간 NRE가 된다.**

`SceneBase`에는 이미 `IsActiveScene()`이 있는데 아무도 확인하지 않는다.

**권장 (최소 변경)**
```csharp
public void UpdateScene()
{
    if (scenes.TryGetValue(currentScene, out var scene) && scene.IsActiveScene())
        scene.UpdateScene();
}
```

**권장 (구조적)**
전환 상태를 명시한다. `Pattern/State.cs`의 `IState`가 이 용도로 이미 정의만 되어 있다.
```
Idle → Loading → Entering → Running → Exiting → Idle
```
전환 중에는 Update를 돌리지 않고, `ChangeScene` 재진입도 차단한다(현재는 **로딩 중 다시 `ChangeScene`을 부르면 두 전환이 겹친다** — `nextScene`이 덮어써지고 이전 콜백은 살아 있다).

---

## 2-3. `ChangeSceneSingle`이 로드 완료 전에 `EnterScene`을 호출한다

**근거** — `SceneController.cs:93-95`

```csharp
SceneManager.LoadScene(currentScene.ToString(), LoadSceneMode.Single);
targetSceneInstance.EnterScene(sceneInfo);    // 같은 프레임에서 즉시 호출
```

**영향**
`LoadScene`(동기 버전)조차 **실제 씬 반영은 현재 프레임 종료 시점**이다. 따라서 `EnterScene` 안에서 새 씬의 GameObject를 찾으면 못 찾고, 아직 살아 있는 이전 씬의 오브젝트를 잡을 수도 있다.
현재 이 경로를 타는 것은 부팅 시 `StartScene` 하나뿐이라 증상이 없지만, **StartScene이 씬 오브젝트를 참조하기 시작하는 순간 터진다.**

**권장**
`LoadSceneAsync` + `completed` 콜백으로 통일하거나, `SceneManager.sceneLoaded` 이벤트에서 `EnterScene`을 호출한다. 어차피 (a)(b)(c) 세 경로가 각각 다른 타이밍 규약을 갖고 있는 것이 문제의 본질이다 → **"씬은 로드 완료 후에만 EnterScene된다"는 단일 불변식**으로 맞춘다.

---

## 2-4. 씬 등록이 4곳으로 흩어져 있다

씬 하나 추가에 필요한 수정 지점:

| # | 위치 | 종류 | 누락 시 |
|---|---|---|---|
| 1 | `ESceneType` (`SceneBase.cs:8`) | 코드 | 컴파일 에러 (즉시 발견) |
| 2 | `SceneBase` 파생 클래스 | 코드 | 컴파일 에러 (즉시 발견) |
| 3 | `SceneManager.RegistScenes()` (`SceneManager.cs:34`) | 코드 | **런타임 조용한 실패** |
| 4 | Build Settings 등록 | 에디터 설정 | **런타임 조용한 실패** |
| (+) | `.unity` 파일명 == enum 이름 | 파일 | **런타임 조용한 실패** |

3·4·5는 전부 컴파일러가 검증하지 못한다. **실제로 지금 LobbyScene/MenuScene이 4번에서 누락되어 있다** ([001-3](001-build-blockers.md#1-3-lobbyscene이-build-settings에-없다--첫-화면-이후-진행-불가)).

**권장 (단계적)**

1. **즉시** — [001-3](001-build-blockers.md)의 `[InitializeOnLoad]` 검증 스크립트로 4·5번을 자동 감지. 비용 20줄.
2. **다음** — 3번 제거. `RegistScenes()`의 수동 나열 대신 리플렉션으로 `SceneBase` 파생 타입을 자동 등록한다.

```csharp
private void RegistScenes()
{
    foreach (var type in typeof(SceneBase).Assembly.GetTypes())
    {
        if (type.IsAbstract || !typeof(SceneBase).IsAssignableFrom(type)) continue;
        if (!Enum.TryParse(type.Name, out ESceneType sceneType)) continue;   // 클래스명 == enum명 규약
        AddScene(sceneType, (SceneBase)Activator.CreateInstance(type, sceneType));
    }
}
```
단, **"클래스명 == enum명 == 파일명"이라는 암묵 규약을 명시 규약으로 승격**시키는 셈이므로 팀 합의가 필요하다. 규약을 싫어한다면 3번 수동 등록을 유지하되 1번 검증 스크립트를 `scenes` 딕셔너리까지 확인하도록 확장하는 편이 낫다 — **어느 쪽이든 "조용한 실패"만은 없애야 한다.**

---

## 2-5. `LoadingScene`의 자원 관리

**근거** — `Assets/Script/GameFlow/GameScene/LoadingScene.cs:21, 30-35, 48`

```csharp
private static UILoading uiLoading;              // (a) static

~LoadingScene()                                  // (b) 파이널라이저
{
    loadTokenSource.Cancel();
    loadTokenSource.Dispose();
    loadTokenSource = null;
}

uiLoading = GameObject.Instantiate(origin);      // (c) 매 로딩마다 Instantiate, Destroy 없음
```

**영향**

- **(a) `static` 필드** — `LoadingScene` 인스턴스는 어차피 하나뿐이므로 static일 이유가 없다. 도메인 리로드를 끄는 순간(Enter Play Mode Options) 플레이 세션 간에 파괴된 오브젝트 참조가 살아남아 `MissingReferenceException`이 된다.
- **(b) 파이널라이저** — 관리 객체(CTS) 정리에 C# 소멸자를 쓰면 GC 스레드에서 비결정적으로 실행된다. 게다가 `SceneManager`가 이 인스턴스를 앱 종료까지 들고 있으므로 **사실상 호출되지 않는다.** `IDisposable` 또는 `ExitScene`에서 명시적으로 정리해야 한다.
- **(b') CTS 재사용 불가** — 생성자에서 1회 생성한다. 한 번 Cancel되면 이후 모든 로딩이 즉시 취소된다. **로딩씬은 반복 사용되는 씬**이므로 치명적이다. `EnterScene`에서 새로 만들고 `ExitScene`에서 Dispose하는 것이 맞다.
- **(c) UILoading 인스턴스** — `Destroy` 호출이 없고 로딩씬 언로드에 의존한다. 부모 지정(`SetParent(roots[0])`)이 실패하는 경우(루트 오브젝트 없음)엔 씬에 속하지 않아 **언로드로도 사라지지 않는다.**

**권장**
```csharp
public override void EnterScene(ISceneInfo sceneInfo)
{
    base.EnterScene(sceneInfo);
    loadTokenSource?.Dispose();
    loadTokenSource = new CancellationTokenSource();
    ...
}

public override void ExitScene()
{
    loadTokenSource?.Cancel();
    loadTokenSource?.Dispose();
    loadTokenSource = null;
    if (uiLoading != null) { GameObject.Destroy(uiLoading.gameObject); uiLoading = null; }
    base.ExitScene();
}
```
`static` 제거, 파이널라이저 삭제.

---

## 2-6. `EnterScene` 인자 캐스팅이 검증 없이 이뤄진다

**근거** — `LoadingScene.cs:70-72`

```csharp
LoadingSceneInfo info = sceneInfo as LoadingSceneInfo;
LoadingProcess(info.loadingTargetScene, ...);     // info가 null이면 NRE
```

`ISceneInfo`는 빈 마커 인터페이스라 **타입 불일치를 컴파일러가 못 잡는다.** `ChangeScene(ESceneType.LoadingScene, 아무_ISceneInfo)`를 호출하면 즉시 NRE.

**권장**
제네릭으로 씬과 인포 타입을 묶으면 컴파일 타임에 강제된다.
```csharp
public abstract class SceneBase<TInfo> : SceneBase where TInfo : class, ISceneInfo
{
    protected TInfo Info { get; private set; }
    public sealed override void EnterScene(ISceneInfo context)
    {
        Info = context as TInfo;
        if (Info == null && context != null)
            Debug.LogError($"{GetType().Name}: {typeof(TInfo).Name} 필요, 받은 값 {context.GetType().Name}");
        base.EnterScene(context);
        OnEnter();
    }
    protected virtual void OnEnter() { }
}
```
과하다고 판단되면 최소한 **캐스팅 실패 시 명시적 에러 로그 + early return**은 넣는다.

---

## 2-7. 사용되지 않는 경로 정리

| 대상 | 상태 | 판단 |
|---|---|---|
| `StartScene.OnLoadResourceAsync` (`StartScene.cs:17`) | 10초 가짜 진행률. `ChangeSceneSingle` 경로라 **호출되지 않음** | 로딩 UI 검증용이면 `TestScene`으로 옮기고 StartScene에서는 제거 |
| `ChangeSceneWithOutLoadSceneObject` (`SceneController.cs:150`) | 호출처 없음 | 쓸 계획이 없으면 삭제. "나중에 쓸지도"는 남길 이유가 안 된다 |
| `SceneController.Release()` (`:26`) | 정의만 있고 `SceneManager.Release()`가 호출하지 않음 (`SceneManager.cs:24` 본문 비어 있음) | 종료 경로 연결 또는 삭제 — 둘 중 하나로 결정 |
| `MenuScene` | "로비 안에서 메뉴를 띄우자"는 재검토 주석 (`MenuScene.cs:14-17`) | 방향이 정해졌으면 씬을 지우고 UI로 전환. 미결이면 결정 시점을 문서에 남긴다 |
| `Assets/Resources/Scenes/GameScene.unity` | Build Settings 등록 O, `ESceneType` X | 사용 여부 확정 후 등록 해제 또는 enum 추가 |

---

## 2-8. `EventSystem`이 씬마다 있어 전환 후 UI 입력이 죽는다

**근거** — 씬별 `EventSystem` 보유 현황

```
LoadingScene   있음
LobbyScene     있음
MenuScene      있음
StartScene     없음
```

실행 로그 (`Player.log`):
```
There can be only one active Event System.
  ↳ UIElementsRuntimeUtility.RegisterEventSystem
  ↳ UIToolkitInteroperabilityBridge.OnEnable
  ↳ EventSystem.OnEnable
```

**증상**

```
LoadingScene 로드    → EventSystem 활성 (첫 번째)
LobbyScene  로드     → 두 번째 → 경고 발생, 로비 것이 비활성화됨
LoadingScene 언로드  → 활성 EventSystem 파기
                      로비 것은 비활성 상태 그대로 (Unity 가 자동 복구하지 않는다)
결과                 → 활성 EventSystem 0개 → 로비의 모든 UI 클릭이 무반응
```

**영향**
로비 진입 후 **화면은 정상적으로 그려지는데 클릭만 안 먹는다.** 렌더링은 `EventSystem` 없이 동작하고 입력만 죽기 때문에, 증상이 "UI가 고장났다"가 아니라 "특정 기능이 안 된다"로 보인다. 원인 추적이 오래 걸리는 유형이다.

실제로 치트 패널의 토글 두 개가 무반응이라 기능 결함으로 오인됐고, 로그의 경고 한 줄을 찾고서야 원인이 드러났다. `UILobbyPanel`·`GoldHUD` 등 로비의 다른 UI도 같은 상태였다.

**즉시 조치** — `LoadingScene`의 `EventSystem` 제거 (`이 커밋`).
로딩 화면은 클릭을 받지 않으므로 없어도 무방하고, 로비 것이 유일해져 경고도 사라진다.

**구조적 권장** — 씬마다 두지 말고 **부팅 시 하나만 만들어 `DontDestroyOnLoad`** 로 유지한다.
`GameProcess`(`GameFlow/GameProcess.cs:12`)가 이미 같은 방식을 쓰고 있어 패턴이 일관된다.

```csharp
// 부팅 시 1회
if (EventSystem.current == null)
{
    GameObject go = new GameObject("EventSystem");
    go.AddComponent<EventSystem>();
    go.AddComponent<InputSystemUIInputModule>();
    Object.DontDestroyOnLoad(go);
}
```

씬에서 전부 제거하면 **씬이 늘어도 재발하지 않는다.** 즉시 조치는 로딩씬 하나만 막은 것이라, `MenuScene`이 로비와 함께 뜨는 순간 같은 문제가 다시 난다.

**검증 방법** — 실행 로그에 `There can be only one active Event System` 이 나오면 중복이다. 없어야 정상이다.
