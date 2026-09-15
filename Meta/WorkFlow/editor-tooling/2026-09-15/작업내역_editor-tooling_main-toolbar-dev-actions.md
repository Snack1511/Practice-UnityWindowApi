# 에디터 툴바 : 검증 동작과 치트 패널 스위치 (2026-09-15)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.

## 목표

`unity-toolbar-extender-ui-toolkit` 을 도입하고, 이 프로젝트의 반복 동작을 에디터 메인 툴바에 올린다.

## 도입한 패키지

| 항목 | 값 |
|---|---|
| 이름 | `com.paps.unity-toolbar-extender-ui-toolkit` |
| 출처 | `https://github.com/Sammmte/unity-toolbar-extender-ui-toolkit.git?path=/Assets/Package#v3.0.2` |
| lock 해시 | `caddaa54e54c3376908e1962182dc45021409452` (= `v3.0.2`) |
| 최소 Unity | `6000.3` — 이 프로젝트는 `6000.5.5f1` |
| 끌려온 의존성 | `com.unity.serialization@6.5.0` (builtin) |
| 성격 | **에디터 전용.** `files` 에 `Editor` 만 들어 있어 빌드에 포함되지 않는다 |

`git` URL 패키지는 리비전을 안 붙이면 처음 푼 SHA 에 조용히 물린다 — UniTask 가 그렇게 2.5.10 에 묶였던 전례가
있어 **태그를 URL 에 박았다.**

## 범위 결정 — 치트를 옮기지 않았다

요청은 "치트 기능을 이걸 기반으로 마이그레이션" 이었으나, 확인해 보니 **옮길 수 있는 치트가 없다.**

| 치트 | 현재 상태 | 이유 |
|---|---|---|
| `Set TransparentClick` | `#if !UNITY_EDITOR` | 에디터에서 켜면 **에디터 창**이 클릭 통과가 되어 조작 불능 |
| 「화면 설정」 모니터 이동 | `#if !UNITY_EDITOR` | 에디터에서 옮기면 **에디터 창**이 움직인다 |
| `Quit` | 전 구간 | 에디터에서 `Application.Quit` 은 아무 일도 안 한다 |
| 런타임 콘솔 | 전 구간 | 에디터에는 이미 Console 창이 있다 |

치트는 전부 **빌드된 오버레이 창**을 대상으로 한다. 패키지는 에디터 전용이라 대상이 어긋난다.
그래서 **툴바에는 "에디터가 하는 일"만 올리고 런타임 치트 패널은 그대로 뒀다** (사용자 결정).

## 툴바 요소 5개

`Assets/Editor/DevToolbar.cs` 한 파일. 전부 `MainToolbarDockPosition.Right`.

| 요소 | 동작 |
|---|---|
| `Cheat Panel` (토글) | 에디터 플레이에서 치트 패널을 만들지 여부. **에디터 기본 꺼짐 / 개발 빌드 항상 켜짐** |
| `Scene ▾` | Build Settings 에 등록된 씬으로 전환. 현재 씬에 체크, 파일 없는 죽은 등록은 비활성으로 표시 |
| `▶ Dev Build` | Windows64 Development Build + 자동 실행 (`BuildOptions.Development \| AutoRunPlayer`) |
| 폴더 아이콘 | 빌드 산출 폴더 열기 |
| `Player.log` | 플레이어 로그를 기본 앱으로 열기 |

**임시 빌드 스크립트를 만들었다 지우는 절차가 이 버튼으로 대체된다.**

### 경로를 어디서 가져오나

| 값 | 출처 | 왜 |
|---|---|---|
| 빌드 산출 exe | `EditorUserBuildSettings.Get/SetBuildLocation` | Unity Build Settings 창과 **같은 저장소**라 한쪽에서 정하면 양쪽에 보인다. `Library` 에 있어 커밋되지 않는다 |
| 플레이어 로그 | `Path.Combine(Application.persistentDataPath, "Player.log")` | 에디터의 `persistentDataPath` 가 플레이어와 같은 LocalLow 경로다. 머신 종속 경로를 코드에 박지 않는다 |

### 치트 패널 스위치가 `PlayerPrefs` 인 이유

런타임 스크립트에서 `UnityEditor` 를 참조하면 asmdef 가 없어 **에디터는 통과하고 빌드에서만 터진다.**
그래서 `EditorPrefs` 를 쓸 수 없다. `PlayerPrefs` 는 런타임 API 라 양쪽에서 같은 키를 읽고 쓸 수 있다.

```csharp
// CheatPanel.cs — 런타임
public const string EnabledPrefKey = "CheatPanel.EnabledInEditor";

public static bool IsEnabled
{
#if UNITY_EDITOR
    get { return PlayerPrefs.GetInt(EnabledPrefKey, 0) != 0; }   // 기본 꺼짐
#else
    get { return true; }                                          // 개발 빌드는 항상 켜짐
#endif
}
```

판정은 `CmdCreateCheatPanel.Execute` 한 곳에서 한다 — 생성 경로가 거기 하나다.
**플레이 중에 토글을 바꾸면 다음 플레이부터 반영된다.** 패널이 씬 시작 명령으로 만들어지기 때문이다.

## 함정 — 새 툴바 요소는 처음에 안 보인다

Unity 6.3 의 신규 메인 툴바 API 버그다. 패키지 README 가 명시한다.

> Everytime you create a new toolbar element you will probably not see it on the toolbar.
> Although Unity says it should be enabled and displayed by default this does not happen.
> So you need to go to the 3 dots and enable it once.

**툴바 오른쪽 끝 「⋮」 메뉴에서 요소를 한 번 켜야 한다.** 코드 문제가 아니다.
이 설정은 사용자별 에디터 상태라 **다른 머신에서 클론하면 다시 켜야 한다.**

## 검증

| 단계 | 결과 |
|---|---|
| 패키지 해석 | `com.paps...@v3.0.2` 확인, lock 해시 `caddaa5` |
| 배치 모드 컴파일 | `error CS` 0건 |
| 툴바 표시 | 「⋮」로 5개 활성 후 전부 표시 (스크린샷 확인) |
| `Scene ▾` | `StartScene`·`LoadingScene`·`LobbyScene`·`GameScene`·`MenuScene`·`TestScene` 나열, 클릭으로 `StartScene` 전환 (창 제목 변경 확인) |
| 치트 패널 기본 꺼짐 | 플레이 → `[CmdCreateCheatPanel] 치트 패널이 꺼져 있어 만들지 않습니다` |
| 치트 패널 켬 | 토글 → `[DevToolbar] 에디터 치트 패널 = True` → 플레이 → `[CheatPanel] 생성 완료. 토글 0 개` |
| 토글 0 개 | 정상. Win32 치트는 `#if !UNITY_EDITOR` 라 에디터에서는 `Quit` 만 남는다 |

> 검증은 합성 입력(`SetCursorPos` + `mouse_event`)으로 실제 클릭을 넣어 확인했다.

**미검증** — `▶ Dev Build` · 폴더 열기 · `Player.log` 열기 버튼은 코드 경로만 확인했고 클릭으로 돌려보지 않았다.
셋 다 외부 프로세스를 띄우거나 긴 빌드를 시작한다.

## 범위 밖에서 발견한 것

- `Scene ▾` 에 `GameScene` 이 정상으로 나온다. advise/001 의 "죽은 등록"은 `.unity` 파일이 없는 게 아니라
  **대응하는 `ESceneType` 항목이 없는 것**이다 (`[SceneRegistryValidator]` 경고). 드롭다운의 「파일 없음」 표시로는 안 잡힌다.
