# 창 제어 : WindowNative 를 서브모듈로 분리 (2026-09-15)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.

## 목표

Win32 창 제어 코드를 별도 저장소로 빼서 다른 데스크톱 오버레이 프로젝트에서 재사용한다.

## 새 저장소

| 항목 | 값 |
|---|---|
| 저장소 | `https://github.com/Snack1511/Unity-WindowNativeCustom` (**private**) |
| 경로 | `Assets/ThirdParty/Unity-WindowNativeCustom` |
| 최초 커밋 | `d0c19bf` |
| 네임스페이스 | `Script.Manager.StaticManager` → **`WindowNative`** |

경로와 형태는 **이 프로젝트에 이미 있던 전례**(`Assets/ThirdParty/UnityFramework-Extension`)를 그대로 따랐다 —
`Script/` 아래 `.cs` + `.meta`, `.gitignore`, 자체 네임스페이스 루트, asmdef 없음.

> **부모는 public 인데 서브모듈은 private 이다.** 본인 계정에서는 정상이지만
> **제3자가 클론하면 `submodule update` 가 인증 에러로 죽는다.** 사용자 결정.

## 옮긴 것

| 파일 | 줄 | 왜 옮겼나 |
|---|---|---|
| `WindowNativeManager.cs` | 396 | 프로젝트 의존성 **0**. `System` · `InteropServices` · `UnityEngine.Debug` 뿐이라 그대로 떨어진다 |
| `ResolutionManager.cs` | 53 | 창을 작업 영역에 맞추는 정책. 사용자가 함께 넣기로 결정 |

`.meta` 는 **원본 GUID 그대로** 가져왔다. 참조가 끊기지 않는다.

### 남긴 것

`DebuggingComponent.cs` · `CheatPanel.cs` 는 호출부라 본체에 남는다.
`GameProcessManager.cs` 는 같은 폴더였을 뿐 관계없다.

## 호출부 변경

| 파일 | 변경 |
|---|---|
| `DebuggingComponent.cs` | `using WindowNative;` 추가 |
| `CheatPanel.cs` | `using WindowNative;` 추가 |
| `MainProcess.cs` | `Manager.StaticManager.ResolutionManager.*` → `WindowNative.ResolutionManager.*` (3곳) |

`ResolutionManager` 는 생명주기 진입점이 없어 `MainProcess` 가 직접 부른다.
**본체가 서브모듈 타입을 직접 호출하는 결합**이 생겼다 — 범위 결정 시 인지하고 택한 것이다.

## 이력을 옮기지 않은 이유

`git filter-repo` 가 이 머신에 없다. `filter-branch --index-filter` 로 뽑을 수는 있지만
해당 5개 커밋이 전부 다른 작업과 섞여 있어(`P0 빌드 차단 이슈 해소` 등) 단독으로 떼면 메시지가 내용과 안 맞는다.
**새 저장소는 커밋 1개로 시작하고, 원본 커밋 목록을 README 에 적었다.**

## 검증

| 단계 | 결과 |
|---|---|
| 에디터 배치 모드 컴파일 | `error CS` 0건 |
| Windows Development Build | `Succeeded`, `error CS` 0건 — **`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽이 새 네임스페이스로 컴파일됨** |
| 실행 예외 | 0건 |
| 모니터 열거 | `[CheatPanel] 화면 설정 : 모니터 1 개 등록` (`WindowNativeManager.GetMonitors`) |
| 창 스타일 | `WS_POPUP` 적용, `WS_CAPTION` · `WS_THICKFRAME` 제거, `WS_EX_TOPMOST` (`SetWindowFrame`) |
| 창 크기 | 작업 영역과 **정확히 일치** (`ResolutionManager.Initialize`) |
| 클릭 통과 | 합성 키 `O` → `1` 후 `WS_EX_LAYERED` · `WS_EX_TRANSPARENT` (`SetTransparentClick`) |
| 투명 배경 | 스크린샷 육안 확인 |

**이 변경은 실제 빌드가 유일한 검증 수단이었다.** `ResolutionManager` 는 본문 전체가
`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안에 있어 에디터 컴파일로는 아무것도 증명되지 않는다.

## 문서 갱신

| 문서 | 변경 |
|---|---|
| `CLAUDE.md` | 파일 수 43 → 41 + 서브모듈 2개 표. 클론 직후 `submodule update --init` 안내. 함정 항목 추가 |
| `docs/window-native.md` | 파일 경로 2곳 |
| `advise/004` · `005` · `006` | 파일 경로 각 1곳 |
| `docs/reference/desktop-overlay-unity6.md` | 파일 경로 1곳 |

**`advise/001` 은 일부러 안 고쳤다.** 해소된 P0 의 기록이라 당시 상태를 그대로 두는 편이 맞다.
