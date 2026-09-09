# 004 — 구조와 확장성 (P2)

지금 당장 깨지는 것은 없다. **코드가 3배가 됐을 때 비용이 10배가 되는** 항목들이다.
전부 다 할 필요는 없다. 우선순위는 **4-1 > 4-2 > 4-4 > 나머지**.

---

## 4-1. asmdef 도입

**현재** — `Assets` 하위에 asmdef가 **하나도 없다.** 프로젝트 코드 전체가 `Assembly-CSharp` 단일 어셈블리로 컴파일된다.

**영향**

| 문제 | 지금 | 커진 후 |
|---|---|---|
| 컴파일 시간 | 39파일이라 체감 없음 | 파일 하나 고칠 때마다 전체 재컴파일 |
| 계층 강제 | 불가능 — `Pattern`이 `GameContent`를 참조해도 컴파일된다 | 순환 참조가 누적되어 분리 불가 |
| `UnityEditor` 오염 | **런타임 코드에서 참조해도 에디터에서 통과** | [001-1](001-build-blockers.md)이 정확히 이 사례 |
| 테스트 | 테스트 어셈블리를 붙일 자리가 없음 | 도입 시점에 전면 재배치 필요 |

**권장 분할** — 지금 폴더 구조가 이미 계층을 반영하고 있어서 그대로 옮기면 된다.

```
Script/Pattern/          → Project.Core        (의존: 없음, UniTask만)
Script/Define/           → Project.Define      (의존: Core)
Script/Manager/          → Project.Manager     (의존: Core, Define)
Script/GameFlow/         → Project.GameFlow    (의존: Core, Define, Manager)
Script/GameContent/      → Project.Content     (의존: Core, Define, Manager)
Script/Controller/       → Project.Controller  (의존: Core)
Assets/Editor/           → Project.Editor      (의존: 전부 + UnityEditor, includePlatforms: Editor)
```

주의 — 현재 코드는 이 방향과 **반대 의존**을 이미 갖고 있다:
- `TableManager`(Manager) → `using Script.GameFlow.GameScene` (`TableManager.cs:8`) — **사용하지 않는 using**. 삭제하면 해결.
- `SceneManager`(Manager) → `Script.GameFlow.GameScene` — 실제 의존. `SceneController`/`SceneBase`가 GameFlow에 있기 때문.
  → **씬 정의(`ESceneType`, `SceneBase`, `ISceneInfo`)를 `Define`으로 내리고, 구현 씬만 GameFlow에 남기면** 방향이 정리된다.

asmdef는 **한 번에 다 쪼갤 필요 없다.** `Project.Editor` 하나만 먼저 만들어도 [001-1](001-build-blockers.md) 유형의 사고는 막힌다. 거기서 시작한다.

---

## 4-2. 네임스페이스가 4갈래로 갈라져 있다

| 네임스페이스 | 해당 파일 |
|---|---|
| `Script.*` | 대부분의 매니저·씬·콘텐츠 |
| `pattern` (소문자) | `Singleton.cs`, `State.cs`, `EventBusLocater.cs` |
| `Manager` | `GameFlow/Loader.cs` — **폴더는 GameFlow인데 네임스페이스는 Manager** |
| (전역) | `Controller/*` 5개, `UIRoot`, `GamePathDefine` |

**영향**
- `using pattern;` — C# 관례상 네임스페이스는 PascalCase. 소문자는 변수/타입과 헷갈린다.
- **전역 네임스페이스 타입은 이름 충돌 지뢰다.** `MoveController`, `UIRoot` 같은 흔한 이름이 전역에 있으면 나중에 에셋 스토어 패키지를 넣는 순간 충돌한다. 이 프로젝트는 이미 DOTween·SPUM·Febucci·Live2D를 쓰고 있어 위험이 실재한다.
- `Script`라는 루트 이름 자체가 폴더명에서 온 것으로, 의미가 없다.

**권장**
1. 루트를 하나로: `Practice.WindowApi` 또는 짧게 `Game`. (Rider의 Adjust Namespaces로 일괄 처리 가능)
2. `pattern` → `Game.Pattern`, `Manager`(Loader.cs) → `Game.Manager` 또는 실제 위치에 맞게.
3. 전역 타입 전부 네임스페이스 안으로.
4. Edit > Project Settings > Editor > **Root Namespace**를 설정해 새 스크립트가 자동으로 붙게 한다 (현재 비어 있음).

**한 번에 처리해야 하는 작업이다.** 지금 39파일일 때가 가장 싸다.

---

## 4-3. `ResourcesManager`가 폴더 규약을 위반한다

**근거** — `Manager/SingletonManager/ResourcesManager.cs:10`

```csharp
public class ResourcesManager : MonoSingleton<ResourcesManager>   // ← SingletonManager 폴더인데 MonoSingleton
```

폴더가 곧 인스턴스 전략인 구조에서 이 하나만 예외다. 게다가 `MonoSingleton`은 `Instance` 접근 시 GameObject를 만드는데, **`ResourcesManager`는 Transform도 코루틴도 쓰지 않는다** — MonoBehaviour일 이유가 없다.

**권장** — `Singleton<ResourcesManager>`로 변경. 파생 클래스가 없어 영향 범위가 좁다.
(`MonoSingleton`이 필요한 경우: Transform 계층이 필요하거나, `OnApplicationPause`/`OnDestroy` 같은 Unity 콜백이 필요할 때. `ContentManager`가 전자에 해당해 정당하다.)

---

## 4-4. 전역 정적 상태의 도메인 리로드 의존

**현재 정적 상태 목록**

| 위치 | 상태 |
|---|---|
| `Singleton<T>.instance` / `MonoSingleton<T>.instance` | `Pattern/Singleton.cs:17, 36` |
| `GameProcessManager.updaters` / `ProcessObject` | `StaticManager/GameProcessManager.cs:13, 15` |
| `EventBusLocater.locators` | `Pattern/EventBusLocater.cs:15` |
| `WindowNativeManager.hWnd` / `IsTransparentClick` | `StaticManager/WindowNativeManager.cs:85, 134` |
| `LoadingScene.uiLoading` | `GameScene/LoadingScene.cs:21` |

현재 프로젝트 설정: `m_EnterPlayModeOptionsEnabled: 1`, `m_EnterPlayModeOptions: 0` → **도메인 리로드가 켜져 있어** 플레이 진입마다 정적 상태가 초기화된다. 그래서 지금은 문제가 없다.

**영향**
프로젝트가 커져서 **플레이 진입 속도 때문에 Enter Play Mode Options에서 "Reload Domain"을 끄는 순간**(거의 모든 팀이 결국 끈다), 위 정적 상태가 세션 간에 살아남는다. 파괴된 GameObject 참조, 중복 등록된 델리게이트, 이전 세션의 씬 딕셔너리가 그대로 남아 **재현이 안 되는 버그**가 된다.

**권장** — 지금 대비해두는 비용이 매우 싸다.
```csharp
// Singleton<T>, MonoSingleton<T>, GameProcessManager, EventBusLocater 각각에
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    instance = null;      // 각자의 정적 필드 초기화
}
```
`SubsystemRegistration`은 도메인 리로드 여부와 무관하게 플레이 진입 시 항상 실행된다.

---

## 4-5. `EventBusLocater`의 구조적 위험

**근거** — `Pattern/EventBusLocater.cs`

```csharp
public static void Notify(INotifiedValue notifiedValue)
{
    Type targetType = notifiedValue.GetType();
    Debug.Log(targetType.FullName + " Notify");            // (c)
    if(locators.ContainsKey(targetType))
        locators[targetType].Invoke(notifiedValue);        // (a)
}
```

- **(a) NRE** — 마지막 구독자가 `-=`로 빠지면 델리게이트가 `null`이 되지만 **키는 남는다.** 그 상태에서 `Notify`하면 `ContainsKey`는 true, `Invoke`는 NRE. → `?.Invoke(...)` 한 글자로 해결.
- **(b) 런타임 타입 라우팅** — `notifiedValue.GetType()` 기준이라 **파생 타입으로 Notify하면 부모 타입 구독자가 못 받는다.** 이벤트 계층을 만들 수 없다는 뜻. 의도한 것이라면 문서화, 아니라면 설계 변경.
- **(c) 무조건 로그** — 모든 이벤트마다 `Debug.Log`. 이벤트가 초당 수십 개 되면 에디터 콘솔이 마비되고 릴리스에서도 문자열 연결 비용이 든다. → `[Conditional("UNITY_EDITOR")]` 헬퍼로 감싸거나 삭제.
- **(d) 정리 API 없음** — `locators`가 앱 수명 내내 유지된다. `Release()`/`Clear()`가 필요하고, 씬 전환 시 남은 구독자를 확인할 수단도 없다.
- **(e) 사용처 1곳** — 현재 `OutboundNotifier` 하나뿐이다. 소스 주석에도 "너무 전역적인대.."라고 남아 있다.

**권장**
`Notify`의 `?.Invoke`와 로그 제거는 즉시. 그 외에는 **"전역 이벤트 버스를 계속 쓸 것인가"를 먼저 결정한다.**
사용처가 1곳이면 **직접 참조로 바꾸고 버스를 지우는 것**이 가장 싼 선택지다. 전역 버스는 "누가 이 이벤트를 듣는가"를 코드에서 추적 불가능하게 만들며, 그 비용은 프로젝트가 커질수록 기하급수적으로 는다. 남긴다면 최소한 (a)(c)(d)는 처리한다.

---

## 4-6. 잔여 코드 품질 항목

| 항목 | 근거 | 권장 |
|---|---|---|
| `OutboundNotifier` 인덱스 기반 제거 | `Controller/OutboundNotifier.cs:46-50` — `RemoveAt(index)`를 루프로 돌린다. 첫 제거 후 뒤 인덱스가 밀려 **다른 요소가 지워진다** | `outBoundTargets.RemoveAll(x => unregistSet.Contains(x))` 또는 인덱스 내림차순 정렬 후 제거 |
| `ContentObject` null 컨트롤러 | `GameContent/ContentObject.cs:20, 23` — switch의 `_ => null` 직후 `contentController.InitalizeContent(...)` 호출 → NRE | null 체크 후 `Debug.LogError` + return |
| `InitalizeContent` 오타 | `GameContent/ContentBase.cs:13` | 파생 클래스가 전부 비어 있는 **지금** `InitializeContent`로 개명 |
| `GameProcess.gameProcess` 미사용 정적 필드 | `GameFlow/GameProcess.cs:10` | 삭제 |
| `IState` 구현체 없음 | `Pattern/State.cs` | 씬/콘텐츠 상태 머신에 실제로 쓰거나([002-2](002-scene-system.md)) 삭제. 인터페이스만 떠 있는 상태를 유지하지 않는다 |
| `ModelBase` 빈 추상 클래스 | `Define/ModelDefine.cs:15` | [003-8](003-data-layer.md#3-8-model-계층-설계-시-미리-정해둘-것)의 4개 결정 후 착수 |
| `MonoSingleton`이 씬의 기존 인스턴스를 찾지 않음 | `Pattern/Singleton.cs:22-34` | 인스펙터 배치와 병행할 계획이면 `FindFirstObjectByType<T>()` 폴백 추가. 아니면 "코드 생성 전용"임을 주석으로 명시 |
| **`GameProcessManager`에 `RemoveUpdate` 없음 — 필요해졌다** | `StaticManager/GameProcessManager.cs:38` | 아래 참조. **더 이상 "필요해질 때"가 아니다** |
| `updaters` 순회 중 수정 위험 | `GameProcessManager.cs:45` — Update 안에서 `AddUpdate`가 호출되면 `InvalidOperationException` | `RemoveUpdate` 도입 시 함께 스냅샷 순회로 변경 |

### `RemoveUpdate` — 판정 갱신 (2026-08-04)

기존 판정은 "동적 등록이 생기면 필요. 지금은 불필요"였다. **생겼다.**

`GameContent/UI/CheatPanel.cs`(`395b743`)가 `AddUpdate("CheatPanel", ...)`로 등록하는데,
파기 시 해제할 수단이 없다. 콜백 안에서 `this == null`을 검사해 우회했다:

```csharp
// GameProcessManager 에 RemoveUpdate 가 없어서, 파기된 뒤에도 호출될 수 있다.
private void OnUpdate()
{
    if (this == null || root == null)
        return;
    ...
}
```

**문제**
- 델리게이트가 `updaters`에 영구히 남아 **파기된 MonoBehaviour를 캡처한 클로저가 살아 있다.** 씬 전환마다 누적된다.
- 우회 코드가 등록하는 쪽마다 복제된다. 등록 지점이 늘어날수록 빠뜨리기 쉽고, 빠뜨리면 NRE다.
- `this == null`은 Unity의 fake-null 연산자 오버로드에 의존한다. 등록 주체가 MonoBehaviour가 아니면 이 방어가 통하지 않는다.

**권장**

```csharp
public static void RemoveUpdate(string key)
{
    updaters.Remove(key);
}
```

한 줄이다. 동시에 위 표의 `updaters` 순회 문제도 같이 처리한다 — `Update()`에서
`updaters.Values`를 직접 순회하면 콜백 안의 `AddUpdate`/`RemoveUpdate`가
`InvalidOperationException`을 낸다. 스냅샷 순회로 바꾼다.

도입하면 `CheatPanel.OnUpdate`의 null 가드를 지우고 `OnDestroy`에서
`RemoveUpdate(UpdateKey)`를 부르는 형태로 정리할 수 있다.

---

## 4-7. 테스트가 하나도 없다

`com.unity.test-framework 1.6.0`이 설치되어 있으나 테스트 어셈블리도, 테스트 파일도 없다.

**모든 것을 테스트할 필요는 없다.** 하지만 **순수 C#이고 로직이 있고 조용히 틀리는** 것들은 값어치가 확실하다.

| 대상 | 왜 |
|---|---|
| `TableManager` CSV 파싱 | 빈 셀·따옴표·배열·컬처 — [003-6](003-data-layer.md)의 케이스가 전부 테이블 하나로 검증 가능 |
| `IOManager` 저장/로드 왕복 | 임시 폴더 사용. [003-3](003-data-layer.md)의 `if(...);` 같은 버그를 즉시 잡는다 |
| `SceneController` 상태 전이 | Unity 씬 로드가 끼어 있어 난이도가 높다. **로드 호출을 인터페이스로 분리**하면 순수 테스트 가능 |

asmdef가 없어서 지금은 테스트 어셈블리를 붙이기도 어렵다 → **[4-1](#4-1-asmdef-도입)이 선행 조건.**
현재 `TestScene`이 수동 테스트(A키로 세이브/로드)를 대신하고 있는데, 이건 **매번 사람이 실행해야 하고 결과를 눈으로 확인해야 한다.** EditMode 테스트 3개면 그 역할을 자동화한다.
