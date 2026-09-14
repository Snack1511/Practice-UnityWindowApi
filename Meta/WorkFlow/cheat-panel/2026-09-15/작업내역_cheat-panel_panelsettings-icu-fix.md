# 치트 패널 : 런타임 PanelSettings 를 에셋으로 교체 (2026-09-15)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.

## 목표

에디터가 `6000.5.5f1` 로 올라간 뒤 **Windows 빌드가 미검증 상태**였다.
`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽 Win32 P/Invoke 경로는 배치 모드 컴파일 검사가
건드리지 않으므로, 실제 빌드와 실행만이 검증 수단이다. 그 검증에서 나온 회귀를 함께 고쳤다.

## 발견 — UI Toolkit 이 매 프레임 죽는다

빌드 자체는 통과했고 투명·클릭 통과도 살아 있었지만, 실행 로그에 예외가 프레임마다 쌓였다.

```
InvalidOperationException: Collection was modified; enumeration operation may not execute.
  at UnityEngine.UIElements.ATGTextJobSystem.PrepareShapingBeforeLayout (...) ATGTextJobSystem.cs:156
```

바로 앞 줄이 원인을 지목한다.

```
ICU Data not available. The data should be automatically assigned to the PanelSettings in the editor
if the advanced text option is enable in the project settings. It will not be present on PanelSettings
created at runtime, so make sure the build contains at least one PanelSettings asset
```

`6000.5` 의 Advanced Text Generator 는 `PanelSettings` 의 ICU 데이터를 요구한다.
`CheatPanel` 은 `ScriptableObject.CreateInstance<PanelSettings>()` 로 인스턴스를 만들고 있었고,
런타임 생성 인스턴스에는 에디터가 그 참조를 붙여줄 수 없다.
`InitTextLib` 이 실패하면서 `PrepareShapingBeforeLayout` 의 `HashSet` 열거가 중간에 깨지고,
레이아웃 갱신이 매 프레임 예외로 죽는다. 진단 로그의 `size=NaNxNaN` 이 그 결과였다.

## 수정

| 전 | 후 |
|---|---|
| `ScriptableObject.CreateInstance<PanelSettings>()` + `themeStyleSheet` 주입 | `Resources` 에서 `UI/CheatPanelSettings.asset` 로드 |
| `UI/UnityDefaultRuntimeTheme.tss` 를 런타임 로드해 주입 | 에셋에 이미 들어 있어 **로드 자체를 제거** |

theme / uxml / uss 를 `ResourcesManager` 로 읽던 기존 패턴을 그대로 따른다.
에디터가 만든 `.asset` 에는 `m_ICUDataAsset` 이 붙는다 — 이게 해결의 전부다.

에셋은 임시 에디터 스크립트(`AssetDatabase.CreateAsset`)로 생성하고 스크립트는 `.cs` · `.meta` 를 함께 지웠다.

> 프로젝트 설정으로 Advanced Text Generator 를 끄는 우회도 있지만, `ProjectSettings/` 에 해당 항목이 없다
> (기본 동작). 에셋 1개가 더 작은 변경이다.

## 검증

| 단계 | 결과 |
|---|---|
| 에디터 배치 모드 컴파일 | `error CS` 0건 |
| Windows Development Build | `Succeeded` (수정 전 · 후 모두) |
| 실행 예외 | 수정 전 매 프레임 → 수정 후 **0건**, `ICU Data not available` 도 사라짐 |
| 치트 패널 초기화 | `[CheatPanel] 생성 완료. 토글 1 개, 씬 'LobbyScene', 토글키 O` |
| 창 스타일 (`GetWindowLong`) | `WS_POPUP` 적용, `WS_CAPTION` · `WS_THICKFRAME` 제거, `WS_EX_TOPMOST` |
| 창 크기 (`GetWindowRect`) | 작업 영역과 정확히 일치 |
| 클릭 통과 | 합성 키 입력 `O` → `1` 후 `WS_EX_LAYERED` · `WS_EX_TRANSPARENT` 둘 다 `True`. 로그 `[WindowNative] SetTransparentClick(True) ... 0x8 -> 0x80028` |
| 투명 배경 | 스크린샷 육안 확인. 창이 화면 전체를 덮는데 뒤 창이 그대로 보이고 치트 패널·FPS·콘솔만 그려진다 |

> 창 크기 검증은 DPI 좌표계를 맞춰서 읽어야 한다. 아래 「함정」 참고.

## 함정 — DPI 가상화 때문에 창 크기를 오판할 뻔했다

`GetWindowRect` 가 `0,0 1920x1032` 를 돌려줘 작업 영역(물리 2560x1440)과 안 맞는 줄 알았다.
실제로는 **읽는 쪽이 DPI 가상화된 프로세스**였다. Unity 플레이어는 per-monitor DPI aware 라
물리 좌표로 창을 잡는데, 조회한 PowerShell 은 133% 스케일로 축소된 좌표를 본다.
같은 프로세스에서 읽은 `rcWork` 도 `1920x1032` 라 **창과 작업 영역이 정확히 일치**했다.

물리 해상도는 `Get-CimInstance Win32_VideoController` 로 따로 확인해야 갈린다.

## 범위 밖에서 발견한 것

- **디스플레이 구성이 바뀌었다.** `CLAUDE.local.md` 의 표는 2대(DISPLAY1 2560x1440 주 + DISPLAY2 1920x1080)인데,
  지금은 1대다 — 치트 패널도 `모니터 1 개 등록` 을 찍는다. 표를 갱신했다.
- 빌드 시 `Assets/Settings/` 3개가 재직렬화된다. URP 가 `6000.5` 신규 필드를 채우는 것과
  셰이더 프리필터 플래그 갱신이다. 내용 손실은 없다.
