# 변경 이력

`docs/` = 지금 어떻게 동작하는가, `advise/` = 어떻게 바꿔야 하는가.
이 문서는 **언제 무엇이 왜 바뀌었는가**를 시간순으로 남긴다.

---

## 2026-08-12 — BlitPass 스택 삭제 (advise/001 1-5)

전체 알파를 상수로 덮어쓰던 커스텀 렌더 패스 스택을 제거했다.
2026-08-10 에 **치트 패널로 껐을 때 투명이 유지되는 것을 실측**했고, 이번에 실제 삭제와 재검증까지 마쳤다.

**투명은 이제 렌더 피처 없이 프로젝트 설정만으로 성립한다** — 카메라 `clearFlags=SolidColor` + 배경색 `(0,0,0,0)` + `preserveFramebufferAlpha` + HDR off + D3D11.
동작 설명은 [window-native.md 3항](window-native.md).

### 삭제 범위와 순서

**순서가 중요했다.** 에셋 참조를 먼저 끊지 않고 스크립트를 지우면 `PC_Renderer.asset` 에 missing script 항목이 남는다 — `Obstacle.prefab` 이 지금 겪고 있는 상태와 같아진다.

| # | 대상 |
|---|---|
| 1 | `PC_Renderer.asset` 의 `Renderer Features` 에서 `BlitFeature` 제거 (에디터 인스펙터) |
| 2 | `Script/Shader/BlitFeature.cs` · `BlitPass.cs` |
| 3 | `Resources/Material/Custom_MakeTransparent.mat` · `Resources/Shader/MakeTransparent.shader` |
| 4 | `CheatPanel` 의 `Set BitBlitPass` 토글과, 호출자를 잃은 `FindFeature<T>` · `RestoreFeatures` · 관련 필드 3개 · `using` 3개 |

빈 폴더 메타 3개(`Resources/Material.meta`, `Resources/Shader.meta`, `Script/Shader.meta`)도 함께 제거했다.
삭제한 GUID 4개를 프로젝트 전체에서 대조해 잔여 참조 0건을 확인했다.

### 딸려온 변경 — URP 스키마 업그레이드

같은 커밋의 `PC_Renderer.asset` 에 blit 과 무관한 재직렬화가 섞여 있다. 에디터가 프로젝트를 열면서 한 것이다.

```
m_AssetVersion: 2 → 3
+ m_PrepassLayerMask
- m_BlueNoise256Textures (7개), m_Shader
```

### 치트 키 매핑 변경

토글 등록 순서가 곧 `Alpha1..Alpha9` 순서다. 빌드 토글이 하나로 줄어 **`1` = `Set TransparentClick`** 이 됐다(이전에는 `2`).
에디터에서는 등록되는 토글이 0개다 — `Set TransparentClick` 은 `#if !UNITY_EDITOR` 이기 때문이다.

### 검증

| 단계 | 결과 |
|---|---|
| 참조 잔재 GUID 대조 | 0건 |
| `git diff` 정적 검토 | 의도한 13개 파일만 |
| 에디터 배치 모드 컴파일 | `error CS` 0건, `Exiting batchmode successfully` |
| Windows Development Build | `결과=Succeeded 오류=0`, 168 MB |
| 실행 로그 | 셰이더/머티리얼 오류 0건, `Direct3D 11.0 [level 11.1]`, `[CheatPanel] 토글 1 개` |
| 투명 배경 육안 | ✅ 확인 |

> 삭제 커밋을 독립적으로 끊었다. "화면 전체 반투명" 이 필요해지면 `git revert` 로 복원한다 — 두 방식은 결과가 다르다.

---

## 2026-08-10 — P0 해소와 투명 오버레이 실동작 확보

이 프로젝트에서 **처음으로 Windows 플레이어 빌드가 통과**했고, **투명 배경과 클릭 통과가 실제로 동작**하는 것까지 확인했다.
검증 환경: Unity `6000.2.10f1`, URP 17.2.0, D3D11, NVIDIA RTX 4070 Ti SUPER.

### 1. 빌드 차단 해소 (advise/001)

| # | 문제 | 조치 |
|---|---|---|
| 1-1 | `TestScene.cs` 가 런타임에서 `UnityEditor.Overlays` 참조 | 미사용 `using` 삭제 |
| 1-2a | `ResolutionManager` 에 `using UnityEngine` 없음 | 추가 |
| 1-2b | `DebuggingComponent` 가 `WindowNativeManager` 네임스페이스 미참조 | 추가 |
| 1-6 | **SPUM 의 `SPUM_SpriteEditManager` 가 `Editor` 폴더 밖에서 `UnityEditor.U2D` 참조** | SPUM 패키지 제거로 해소 |

1-2 는 `#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽이라 **에디터에서는 영원히 검증되지 않는다.** 실제 Windows 빌드로만 확인 가능했다.

1-6 은 최초 감사가 놓친 항목이다. 감사 범위가 `Assets/Script/` 였고 서드파티 에셋을 보지 않아서, **최초 커밋부터 플레이어 빌드가 불가능한 상태였는데도 원인이 하나 덜 적혀 있었다.**

### 2. 씬 등록 정합성 (advise/001 1-3)

- `LobbyScene` · `MenuScene` 을 Build Settings 에 등록. 부팅이 로딩 씬에서 멈추던 문제 해소
- `Assets/Editor/SceneRegistryValidator.cs` 신규 — `ESceneType` ↔ Build Settings 양방향 대조를 에디터 재컴파일마다 자동 실행

씬 등록 누락은 컴파일 에러가 아니라 런타임 조용한 실패로 나타난다. 검사기가 이제 Error 로 띄운다.

### 3. 투명 오버레이 — 네 조건을 모두 맞춰야 성립한다

투명이 전혀 나오지 않던 상태였다. 하나라도 어긋나면 나머지가 무의미하다.

| 항목 | 이전 | 이후 | 왜 |
|---|---|---|---|
| `preserveFramebufferAlpha` | `0` | `1` | 꺼져 있으면 Unity 가 최종 프레임버퍼 알파를 1 로 밀어버린다. **다른 걸 다 맞춰도 투명이 원천 불가능** |
| 그래픽 API | D3D12 우선 | **D3D11 단독** | D3D12 는 배경 투명 자체가 불가능 |
| URP HDR | on | off | HDR 버퍼는 알파 처리가 다르다 |
| 카메라 `clearFlags` | Skybox / Nothing | **SolidColor** | 스카이박스는 불투명, Nothing 은 이전 프레임 잔상 |
| 카메라 배경색 | `(0.19, 0.30, 0.47, 0)` | **`(0, 0, 0, 0)`** | 아래 참조 |

#### 화면 전체가 파랗게 물들던 원인

알파를 0 으로 맞춰도 **RGB 가 Unity 기본 파란색으로 남아 있으면 화면 전체가 그 색으로 물든다.**
DWM 은 미리 곱해진 알파(premultiplied)로 합성하는데 Unity 는 스트레이트 알파로 쓰기 때문에, 알파 0 이어도 RGB `(49, 77, 121)` 이 그대로 더해진다.

**알파만 0 으로 맞추는 것으로는 부족하다. RGB 도 완전한 검정이어야 한다.**

이 증상은 처음에 `BlitPass` 문제로 오인했다. 실제로는 카메라 배경색이었다.

### 4. 클릭 통과 (advise/001 1-4)

`SetTransparentClick(false)` 가 동작하지 않던 문제를 고치고, **실제 클릭 통과까지 확인**했다.

```csharp
// 이전 : flag 와 무관하게 항상 OR → 해제 불가
exFlag |= WS_EX.TRANSPARENT;

// 이후
exFlag = flag ? (exFlag | WS_EX.LAYERED | WS_EX.TRANSPARENT)
              : (exFlag & ~(WS_EX.LAYERED | WS_EX.TRANSPARENT));

if (flag)
    SetLayeredWindowAttributes(hWnd, 0, 255, LWA.ALPHA);

SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
    SWP.NOMOVE | SWP.NOSIZE | SWP.NOOWNERZORDER | SWP.NOACTIVATE | SWP.FRAMECHANGED);
```

동작에 **세 가지가 모두 필요했다:**

1. **`WS_EX_LAYERED` 를 같이 세운다.** `WS_EX_TRANSPARENT` 단독으로는 히트 테스트가 통과되지 않는다. 레이어드 윈도우일 때만 마우스가 아래 창으로 넘어간다
2. **`SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA)`.** 레이어드 윈도우는 표시 방식을 지정해야 그려진다. **알파 255(불투명)** 여야 DWM 의 픽셀 단위 알파가 유지된다. 기존 주석에 남아 있던 알파 `0` 은 창 전체를 지운다
3. **`SetWindowPos` 로 커밋.** `SetWindowLong` 만으로는 값만 바뀌고 히트 테스트 동작이 갱신되지 않는다

`SetLayeredWindowAttributes` 의 `DllImport` 선언 자체가 없었다(주석 처리된 호출만 존재). 추가했다.

해제 시 `LAYERED` 도 함께 제거한다. 남겨두면 렌더링 경로가 달라진다.

### 5. 런타임 치트 패널 (신규)

`Assets/Script/GameContent/UI/CheatPanel.cs` + `Assets/Resources/UI/`

`#if DEVELOPMENT_BUILD || UNITY_EDITOR` 로 가둔 개발용 도구다. 릴리즈 빌드에는 들어가지 않는다.

- **UI Toolkit 런타임 조립** — 프로젝트에 UI Toolkit 런타임 자산이 없어서 `PanelSettings` 를 `CreateInstance` 로 만들고 테마·uxml·uss 를 Resources 에서 읽어 조립한다. 손으로 작성한 `.asset` 이 없다
- **씬 배치 불필요** — `LobbyScene` 이 커맨드로 런타임 생성한다. 씬에 배치하면 릴리즈 빌드에서 missing script 참조로 남는다
- **렌더러 데이터 리플렉션** — 인스펙터가 없으므로 `UniversalRenderPipelineAsset.m_RendererDataList` 를 리플렉션으로 찾는다. URP 내부 필드명 의존이라 버전이 올라가면 여기부터 확인한다
- **플로팅 패널** — 헤더 드래그 이동, 접기
- **하단 콘솔** — `Application.logMessageReceived` 구독. Log/Warning/Error 를 검정/노랑/빨강으로. 최대 200줄
- **클릭 통과 중 히트 테스트** — `WS_EX_TRANSPARENT` 는 창 전체에 걸리는 플래그라 영역별 예외가 불가능하다. 커서가 패널 위에 있는 동안만 플래그를 빼서 조작 가능하게 유지한다
- **숫자키 단축키** — `O` 로 표시, `1`~`9` 로 각 토글. UI 입력 경로와 치트 기능을 분리해 진단할 수 있다

### 6. 커맨드 큐 (신규)

`Assets/Script/GameFlow/Command/`

씬 시작 시 필요한 오브젝트의 생성을 명령으로 위임한다.

```
ICmdBase          UniTask Execute()
CmdBase           추상 기반
CmdPriorityQueue  이진 힙. 정렬 조건 주입
CmdExecutor       파사드 — 큐·비동기 펌프·완료원·예외 격리를 감춘다
```

`CmdExecutor.ExecuteCommand(UnityAction completeAction = null)` 는 **동기 호출이며 즉시 반환**하고, 완료 여부는 `UniTaskCompletionSource<bool>` 로 전달한다. 기다릴 쪽은 `await executor.ExecuteCommand().Task`.

`SceneBase.OnEnqueueStartCommands(CmdExecutor)` 훅을 추가하고, `LoadingScene` 이 **`SetActiveScene` 직후** 실행한다.

#### 실행 시점이 중요한 이유

```
LoadSceneAsync(target, Additive)
OnLoadResourceAsync()        ← 여기서 만든 오브젝트는 죽는다
SetActiveScene(target)       ← 여기부터 생성물이 대상 씬에 들어간다
ExecuteStartCommands()       ← 큐 실행 위치
OnLoadComplete()
loadComplete()               ← 로딩 씬·이전 씬 언로드
```

`new GameObject()` 는 **그 시점의 활성 씬**에 들어간다. `OnLoadResourceAsync` 단계에서는 아직 대상 씬이 활성이 아니라서, 만든 오브젝트가 로딩 씬과 함께 파기된다.

`PriorityQueue` 는 .NET 6 도입이라 이 프로젝트(.NET Standard 2.1)에 없다. 직접 구현했고, 동률일 때 삽입 순서를 지키도록 일련번호를 2차 키로 쓴다 — 힙은 안정 정렬이 아니라서 그냥 두면 실행 순서가 매번 뒤집힌다.

### 7. 에셋 패키지 제거

`Assets` **789M → 19M** (-97%)

| 패키지 | 규모 | 사유 |
|---|---|---|
| SPUM | 35M / 1075 파일 | 현재 미사용. 빌드 차단(1-6)도 함께 해소 |
| GUIPackCartoon | 737M / 2981 파일 | 현재 미사용 |

제거 전 확인 — 프로젝트 코드 참조 없음, `ProjectSettings`·`manifest.json` 흔적 없음, 씬 6개의 GUID 참조 0건.

`GoldHUD.prefab` 이 GUIPackCartoon 의 `Coins - Chest.png` 를 참조하고 있어 프로젝트 자체 스프라이트 `GoldSample.png` 로 교체했다. 임포트 설정이 동일해 `fileID` 는 그대로 두고 `guid` 만 바꿨다.

> **주의** — 이 참조는 GUID 1574개를 전수 대조해서야 발견됐다. 첫 검사에서 `xargs` 가 145개만 처리해 "참조 없음" 이라는 잘못된 결과가 나왔고, 그대로 지웠으면 프리팹이 깨졌다. **대량 GUID 대조는 처리 건수를 반드시 확인한다.**

### 8. 그 밖에 드러난 것

**`EventSystem` 중복** — 로딩 씬과 로비 씬이 각자 갖고 있어, 로딩 씬 언로드 후 활성 `EventSystem` 이 0개가 되는 구조였다. 로딩 씬 것을 제거해 임시 조치했다. 구조적 해법은 [advise/002-8](../advise/002-scene-system.md).

**`ResourcesManager` 의 확장자 처리** — `Path.GetExtension` 결과로 `Replace` 를 하므로 **확장자 없는 경로를 넘기면 `ArgumentException`** 이 난다. 호출 시 확장자를 붙여 우회했다.

**Resources 경로 충돌** — Resources 는 확장자를 뗀 경로로 색인한다. `CheatPanel.uxml` 과 `CheatPanel.uss` 가 둘 다 `"UI/CheatPanel"` 이 되어 스타일시트가 로드되지 않았다. `CheatPanelStyle.uss` 로 이름을 갈라 해결했다.

> 증상이 "배경만 안 보임" 이라 스타일 일부 문제로 보였지만, 실제로는 **USS 전체가 적용되지 않고 있었다.** 패널이 절대 위치가 아니라 기본 흐름으로 쌓여 있었던 것이 단서였다.

### 9. 검증 방법

```bash
# 에디터 플랫폼 컴파일 (플랫폼 조건부 블록은 검증되지 않는다)
Unity.exe -batchmode -quit -nographics -projectPath <경로> -logFile <로그>

# Windows 스탠드얼론 빌드 — 조건부 블록의 유일한 검증 수단
# -buildWindows64Player 에는 개발 빌드 플래그가 없어 BuildOptions.Development 를 직접 넘긴다
Unity.exe -batchmode -quit -nographics -projectPath <경로> -executeMethod <빌드메서드>
```

실행 로그는 `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Practice-WindowApi\Player.log`.

**로그 기반 진단이 이 세션에서 가장 효과적이었다.** 화면을 볼 수 없는 상태에서 "클릭이 안 된다"는 증상이 실제로는 정상 동작이었음을 로그가 증명했고(토글 호출이 전부 기록돼 있었다), `exStyle` 을 찍어 Win32 계층이 정상임을 확인한 뒤에야 `WS_EX_LAYERED` 로 범위를 좁힐 수 있었다.

---

## 남은 작업

- **`BlitPass` 스택 삭제** — advise/001 1-5. 실측으로 불필요 확인됨. 아래 별도 항목 참조
- `GameScene` 의 죽은 Build Settings 등록 정리
- `EventSystem` 구조적 해법 (advise/002-8)
- `GameProcessManager.RemoveUpdate` 부재 (advise/004)
