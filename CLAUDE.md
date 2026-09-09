# CLAUDE.md

Unity 6 (`6000.5.5f1`) + URP 데스크톱 오버레이 게임 프레임워크 연습 프로젝트.
프로젝트 코드는 **`Assets/Script/` 하위 43개 파일이 전부**다. 나머지는 서드파티 에셋/플러그인.

## 프로세스

프로젝트 공통 문서는 [`Meta/`](Meta/README.md) 에 있다 (커밋 · 도구 중립).
개인 계층은 `.claude/` 이고 `.gitignore` 로 제외된다 — **충돌 시 `.claude/` 가 이긴다.**

작업 순서와 범용 규약은 [`process/`](Meta/process/README.md)에 **영문(`en/`)·국문(`ko/`) 이중 언어**로 있다.
**에이전트는 `en/`을 읽는다** — 아래 링크가 그쪽을 가리킨다. 사용자가 규칙을 검토·논의할 때는 `ko/`의 같은 파일을 본다.
**작업 시작 전에 [process/00-process.md](Meta/process/00-process.md)를 읽는다.**

| 문서 | 언제 보나 |
|---|---|
| [process/00-process.md](Meta/process/00-process.md) | 세션 프로세스. 접수 → 범위 → 조사 → 구현 → 검증 → 기록 → 인수인계 |
| [process/02-git.md](Meta/process/02-git.md) | 브랜치·커밋·푸시·stash |
| [process/03-docs.md](Meta/process/03-docs.md) | 문서를 어디에 어떤 형식으로 쓰나 |
| [process/07-rule-pipeline.md](Meta/process/07-rule-pipeline.md) | 새 규약이 생겼을 때 이 문서들을 갱신하는 절차 |


## 이 프로젝트 규약

`process/`가 어느 프로젝트에서든 통하는 **절차**라면, [`roles/`](Meta/roles/README.md)는 이 프로젝트에서만 참인 **값**이다.

| 문서 | 언제 보나 |
|---|---|
| [roles/coding.md](Meta/roles/coding.md) | 코드를 쓰기 전. 업데이트 등록·에셋 로드·확장 메서드·비동기·매니저 추가 |
| [roles/model.md](Meta/roles/model.md) | 작업을 받았을 때. 어느 모델로 할지 |
| [roles/verification.md](Meta/roles/verification.md) | 코드를 고친 뒤. 컴파일 검사·빌드·육안 확인 명령 |

**아래 「반드시 알아야 할 함정」은 일부러 여기 남겼다.** 안 읽으면 조용히 실패하는 종류라 자동으로 읽혀야 한다.

## 문서

작업 전에 해당 문서를 먼저 읽는다. 코드를 새로 훑는 것보다 빠르다.

| 문서 | 언제 보나 |
|---|---|
| [docs/README.md](Meta/docs/README.md) | 폴더 맵, 부트 시퀀스, "어디를 봐야 하나" 표 |
| [docs/roadmap.md](Meta/docs/roadmap.md) | **무엇을 어떤 순서로 어느 계층에 만드나.** 단계 마일스톤과 닫힘 기준 |
| [docs/architecture.md](Meta/docs/architecture.md) | 초기화 순서, 매니저 3계층, 업데이트 펌프, 공용 패턴 |
| [docs/scene-and-content.md](Meta/docs/scene-and-content.md) | 씬 추가·전환, `SceneBase` 라이프사이클, 콘텐츠 시스템 |
| [docs/data-and-resources.md](Meta/docs/data-and-resources.md) | 리소스 로드, CSV 테이블, 세이브 |
| [docs/window-native.md](Meta/docs/window-native.md) | Win32 창 제어, DWM, URP 투명 렌더링 |
| [docs/reference/](Meta/docs/reference/) | 외부 자료 분석 (적용 여부와 무관하게 보관) |
| [advise/README.md](Meta/advise/README.md) | 알려진 문제와 개선 제언. **수정 작업 전 필수 확인** |

문서를 갱신할 때 원칙: **`docs/` = 지금 어떻게 동작하는가, `advise/` = 어떻게 바꿔야 하는가.** 중복 서술하지 않는다.

## 반드시 알아야 할 함정

- **씬 추가는 코드 3곳 + 에디터 설정 2곳**이다. Build Settings 등록과 `.unity` 파일명(= enum 이름) 누락은 **컴파일 에러가 아니라 런타임 조용한 실패**로 나타난다.
- **런타임 스크립트에서 `UnityEditor` 네임스페이스 금지.** asmdef가 없어 에디터에서는 통과하고 빌드에서만 터진다.
- **플랫폼 조건부 블록(`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR`)은 에디터가 검증하지 않는다.** 손댔으면 실제 Windows 빌드로 확인한다.
- 세이브 경로가 `Application.dataPath` 기준이라 에디터에서 `Assets/SaveData/`에 파일이 생긴다.
- **수정 작업 전 [advise/README.md](Meta/advise/README.md)를 확인한다.** 001 의 P0 와 002-8 은 해소됐고, 남은 항목은 [HandOff.md](.claude/HandOff.md)의 열린 항목 표에 있다.
