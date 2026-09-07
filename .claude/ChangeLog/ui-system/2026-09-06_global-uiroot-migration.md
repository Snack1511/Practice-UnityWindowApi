# 전역 UIRoot 도입, 씬 UI 이전 (2026-09-06, advise/002-8)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../docs/CHANGELOG.md)는 시간순 인덱스다.

`EventSystem` 을 씬마다 두던 구조를 없앴다. 씬 UI 전체를 전역 `UIRoot` 밑으로 옮겼다.

**증상이었던 것** — 로비 진입 후 화면은 그려지는데 클릭만 안 먹었다.
`LoadingScene` 과 `LobbyScene` 이 각각 `EventSystem` 을 갖고 있어서, Additive 로드 때 나중 것이 비활성화되고 → 로딩씬이 언로드되면서 활성 인스턴스가 0개가 됐다.
2026-08-10 에 로딩씬 것만 제거한 임시 조치였고, `MenuScene` 이 로비 위에 뜨면 재발할 상태였다.

### 구조

```
UIRoot (Prefabs/UIRoot.prefab, DontDestroyOnLoad, UIManager 소유)
├─ Window        Canvas sortingOrder 10   HUD·로비 패널 등 기본 화면
├─ Popup                          20      다이얼로그·확인창
├─ CanvasEffect                   30      UI 연출·파티클
├─ Overlay                        40      로딩·토스트·시스템 메시지
└─ EventSystem   EventSystem + InputSystemUIInputModule
```

| 대상                                    | 변경                                                                             |
| --------------------------------------- | -------------------------------------------------------------------------------- |
| `Manager/SingletonManager/UIManager.cs` | 신규. `UIRoot` 프리팹 동기 로드·Instantiate·`DontDestroyOnLoad`, `GetLayer` / `CreateUI` |
| `GameContent/UI/UIRoot.cs`              | 빈 스텁 → 레이어 4개 참조와 `EUILayer` enum                                          |
| `Resources/Prefabs/UIRoot.prefab`       | 신규                                                                             |
| `GameFlow/MainProcess.cs`               | `UIManager` 초기화(IO 다음)·해제(역순)                                            |
| `GameFlow/GameScene/LobbyScene.cs`      | `UILobbyPanel` 을 Window 레이어에 생성/파기                                        |
| `Resources/Scenes/LobbyScene.unity`     | `UIRoot`·`Canvas`·`UILobbyPanel`·`EventSystem` 제거 (330줄 삭제)                  |
| `Resources/Scenes/MenuScene.unity`      | 비어 있던 `Canvas`·`EventSystem` 제거 (182줄 삭제)                                 |
| `Editor/SceneRegistryValidator.cs`      | 씬에 `EventSystem` 이 남아 있으면 경고                                             |

### 판단이 갈렸던 두 지점

**프리팹 로드를 동기로 했다.** `EventSystem` 이 첫 씬보다 늦게 생기면 그 사이에 뜬 UI 가 입력을 못 받는다.
`GameProcessManager` 가 `GameProcess` 를 비동기로 로드해 몇 프레임 Update 가 안 도는 것과 같은 함정이라 같은 실수를 반복하지 않았다.

**레이어 캔버스를 `ScreenSpaceOverlay` 로 했다.** 기존 씬 캔버스는 `ScreenSpaceCamera` 로 씬 카메라를 참조하고 있었다.
`DontDestroyOnLoad` 오브젝트가 씬 카메라를 붙들면 씬 전환에서 참조가 끊긴다.
`CanvasScaler` 는 기존과 같은 `ConstantPixelSize` 라 옮겨온 UI 크기는 그대로다.

### 검증

| 단계                          | 결과                                                                                  |
| ----------------------------- | ------------------------------------------------------------------------------------- |
| `git diff` 정적 검토          | 씬 파일은 순삭제(추가 0줄), 의도한 파일만                                             |
| 에디터 배치 모드 컴파일       | `error CS` 0건                                                                        |
| Windows Development Build     | `Succeeded`                                                                           |
| 실행 로그                     | `[UIManager] 전역 UIRoot 생성` 이 `StartScene::EnterScene` 보다 먼저. 예외 0건        |
| `There can be only one ...`   | 0건                                                                                   |
| UI 클릭 전달                  | ✅ `InputSystemUIInputModule → PanelEventHandler.OnPointerUp → CheatPanel 토글` 로그로 확인 |
| 로비 패널 육안                | ❌ 미확인 — `ScreenSpaceCamera` → `ScreenSpaceOverlay` 변경이라 사람이 봐야 한다       |

---
