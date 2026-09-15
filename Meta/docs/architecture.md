# 아키텍처 — 부트스트랩 · 매니저 · 공용 패턴

## 1. 진입점: `MainProcess`

`Assets/Script/GameFlow/MainProcess.cs` — **씬에 붙은 오브젝트가 아니라 `[RuntimeInitializeOnLoadMethod]` 정적 훅**이 프로그램 진입점.
"부트 씬"이나 "GameManager 프리팹을 첫 씬에 배치" 방식을 쓰지 않는다. 어느 씬에서 Play를 눌러도 초기화가 보장되는 구조.

| 훅 | 시점 | 하는 일 |
|---|---|---|
| `OnCallBeforeSplashScreen` | `BeforeSplashScreen` | 앱 이벤트 구독 → 매니저 3계층 초기화 → 업데이트 등록 (`MainProcess.cs:9`) |
| `OnCallAfterSplashScreen` | `AfterSceneLoad` | `SceneManager.OnCallFirstLoadedSceneAfter()` → StartScene 진입 (`MainProcess.cs:38`) |
| `OnApplicationQuit` | `Application.quitting` | 초기화 **역순** Release + 이벤트 구독 해제 (`MainProcess.cs:45`) |

초기화 순서는 의도적으로 고정되어 있다: **Static → Singleton → MonoSingleton**, 해제는 정확히 역순.
새 매니저를 추가하면 `Initialize`/`Release` 두 곳 모두 손대야 대칭이 유지된다.

```
Initialize:  GameProcess → Resolution → Resources → Table → Scene → IO → UI → Content
Release:     Content → UI → IO → Scene → Table → Resources → Resolution → GameProcess
```

`OnApplicationChangedFocus(bool)`는 훅만 걸려 있고 본문 비어 있음 — 포커스 기반 처리(오버레이 앱이라 향후 필요) 확장 지점.

## 2. 매니저 3계층

폴더가 곧 인스턴스 전략이다.

### StaticManager — `static class`
상태가 거의 없거나 Unity 오브젝트가 필요 없는 것.

- **`GameProcessManager`** — 업데이트 펌프의 주인. `Dictionary<string, Action> updaters`를 들고 매 프레임 전부 Invoke.
  `Initialize()`가 `Loader<GameProcess>`로 `Prefabs/GameProcess.prefab`을 **비동기 로드 후 Instantiate**한다(`Forget()` — 완료 대기 없음).
  즉 **GameProcess 인스턴스가 생기기 전 몇 프레임 동안 Update가 돌지 않는다.**
- **`WindowNativeManager`** — Win32 P/Invoke 모음 + 창 배치 정책(`Initialize` · `OnApplicationFocusChanged` · `Release`).
  스탠드얼론 Windows 빌드에서만 실제로 동작한다. **서브모듈이다** (`Assets/ThirdParty/Unity-WindowNativeCustom`). → [window-native.md](window-native.md)

### SingletonManager — `Singleton<T>` (순수 C#)
`Pattern/Singleton.cs`. `Instance` getter에서 `new T()` 지연 생성. MonoBehaviour 아님 → Unity 라이프사이클과 무관.

- `SceneManager`, `TableManager`, `IOManager`, `UIManager`
- `ResourcesManager`는 이 폴더에 있으나 **`MonoSingleton<T>` 상속** (예외 — 정리 대상)

**`UIManager`** — `Prefabs/UIRoot.prefab`을 **동기 로드**해 Instantiate하고 `DontDestroyOnLoad`로 유지한다.
`EventSystem`이 이 프리팹 안에 들어 있어서, 첫 씬보다 늦게 생기면 그 사이에 뜬 UI가 입력을 못 받는다 — 그래서 `LoadAsync`가 아니라 `Load`다.
UI 레이어 구조는 [scene-and-content.md 5항](scene-and-content.md).

### MonoSingleManager — `MonoSingleton<T>`
`Instance` 접근 시 `new GameObject(typeof(T).Name)` + `DontDestroyOnLoad` + `AddComponent<T>()`.
씬 계층에 실제 오브젝트가 필요한 매니저용.

- `ContentManager` — 콘텐츠 오브젝트들의 부모 역할(`go.transform.SetParent(transform)`)을 하므로 Transform이 필요해서 이 계층.

> 세 계층 모두 **초기화는 `MainProcess`가 명시적으로 호출**하고, `Instance` 접근만으로 초기화되지는 않는다(생성만 됨). 순서 의존이 있으면 `MainProcess`에서 해결한다.

## 3. 업데이트 펌프

Unity의 `Update()` 진입점은 **`GameProcess` 하나뿐**이다.

```
GameProcess.Update()                       // Prefabs/GameProcess.prefab, DontDestroyOnLoad
  └ GameProcessManager.Update()
      └ foreach updaters → Action.Invoke()
          └ SceneManager.Update()          // MainProcess에서 "SceneManager" 키로 등록
              └ SceneController.UpdateScene()
                  └ scenes[currentScene].UpdateScene()
```

**새 전역 갱신 로직은 `MonoBehaviour.Update`를 새로 만들지 말고 `GameProcessManager.AddUpdate("키", 메서드)`로 등록한다.**
현재 `AddUpdate`만 있고 `RemoveUpdate`는 없다 — 등록은 부팅 시 1회를 전제로 한 설계.

## 4. 공용 패턴 (`Assets/Script/Pattern/`)

### `Singleton<T>` / `MonoSingleton<T>` — `Singleton.cs`
- 락 없음(메인 스레드 전용 전제), 인스턴스 파기/리셋 API 없음.
- `MonoSingleton`은 씬에 이미 배치된 동일 컴포넌트를 탐색하지 않는다 — **항상 새 GameObject를 만든다.** 인스펙터 배치와 병행하면 중복이 생긴다.

### `IState` — `State.cs`
```csharp
bool Enter(); void Exit(); void Update(); void EnterAsync(); void ExitAsync();
```
**인터페이스만 정의되어 있고 구현체가 아직 없다.** 씬/콘텐츠 상태 머신을 위해 미리 잡아둔 자리.

### `EventBusLocater` — `EventBusLocater.cs`
`Type → 델리게이트 체인` 형태의 전역 이벤트 버스. 페이로드는 `INotifiedValue` 마커 인터페이스를 구현한 클래스.

```csharp
// 구독 (보통 Awake)
EventBusLocater.RegistService<OutboundRegister>(OnRegister);
// 해제 (반드시 OnDestroy)
EventBusLocater.UnRegistLocate<OutboundRegister>(OnRegister);
// 발행 — 런타임 타입으로 라우팅됨
EventBusLocater.Notify(new OutboundRegister { targetTransform = t, BoundCheckAction = cb });
```

- 라우팅 키는 `notifiedValue.GetType()` — **런타임 타입 기준**이므로 파생 타입으로 Notify하면 부모 타입 구독자에게 가지 않는다.
- `static` 딕셔너리가 앱 수명 내내 유지되고 정리 API가 없다. 구독 해제 누락 = 누수.
- 현재 유일한 실사용처: `Controller/OutboundNotifier.cs` (화면 밖 이탈 감지 등록/해제).
- 소스 주석에도 남아 있듯("너무 전역적인대..") 도입 여부 자체가 재검토 대상. → [advise/004](../advise/004-architecture-scale.md)

## 5. `Loader<T>` — `GameFlow/Loader.cs`

`Resources.LoadAsync` 1회용 래퍼. 경로에서 확장자를 떼고 로드 → `completeCallback(path, asset)` → 자기 CTS를 Cancel/Dispose.

```csharp
new Loader<GameProcess> { loadPath = "Prefabs/GameProcess.prefab", completeCallback = SetProcessObject }
    .AsyncLoad().Forget();
```

- `Clear()`가 CTS를 null로 만들기 때문에 **인스턴스 재사용 불가(1회용)**.
- 같은 파일에 주석 처리된 `LoadManager`가 남아 있다 — 로드 태스크 중앙 관리 시도의 흔적. 현재는 `ResourcesManager`가 캐시 역할을 대신한다.
- `Path.Combine` 사용 때문에 Windows에서 경로 구분자가 `\`가 된다. → [advise/003](../advise/003-data-layer.md)

## 6. Controller 컴포넌트 (`Assets/Script/Controller/`)

씬/프리팹에 직접 붙이는 독립 유틸. **전부 전역 네임스페이스**이며 매니저 계층과 직접 결합되어 있지 않다.

| 컴포넌트 | 역할 |
|---|---|
| `MoveController` | `useRigid` 플래그로 Rigidbody2D `AddForce` ↔ `Transform.Translate` 전환 |
| `SpriteController` | `AddOrGetComponent<SpriteRenderer>` 후 `Resources.Load<Sprite>`로 교체 (`ResourcesManager` 미경유 — TODO 주석 있음) |
| `FitToCamera` | 오소그래픽 뷰포트 너비에 맞춰 `localScale.x` 조정 |
| `PlaceToCamera` | 뷰포트 좌표 + 방향 벡터로 화면 가장자리에 스냅 배치 |
| `OutboundNotifier` | `EventBusLocater`로 등록된 Transform들의 화면 이탈을 `LateUpdate`에서 검사 |

## 7. 확장 메서드 (서브모듈)

`Assets/ThirdParty/UnityFramework-Extension/Script/` — 코드 작성 전 여기부터 확인할 것.

```csharp
using Framework.Extension.Collection;   // IsNullOrEmpty(ICollection), IsInRange, Random(out index)
using Framework.Extension.Component;    // this Component  → AddOrGetComponent<T>, TryAddComponent<T>
using Framework.Extension.GameObject;   // this GameObject → AddOrGetComponent<T>, TryAddComponent<T>
```

서브모듈이므로 수정은 별도 저장소에 커밋 후 포인터 갱신이 필요하다.
