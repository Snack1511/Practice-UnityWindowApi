# 변경 이력

`docs/` = 지금 어떻게 동작하는가, `advise/` = 어떻게 바꿔야 하는가.
이 문서는 **언제 무엇이 왜 바뀌었는가**를 시간순으로 남긴다.

**상세는 [`ChangeLog/<영역>/`](../ChangeLog/)가 정본이다.** 이 문서는 **시간순 인덱스**로 링크와 한 줄 요약만 둔다.
같은 내용을 두 곳에 쓰지 않는다 → [process/en/04-templates.md](../process/en/04-templates.md) 부록 원칙 1.

---

## 2026-09-08

| 영역 | 기록 | 한 줄 |
|---|---|---|
| docs-system | [문서 체계 정비](../ChangeLog/docs-system/2026-09-08_process-roles-and-structure.md) | `process/` 도입(en/ko), `roles/` 분리, 두 축 로드맵과 닫힘 기준, 트레일러 소급 제거, `.claude/` 재편 |

## 2026-09-06

| 영역 | 기록 | 한 줄 |
|---|---|---|
| window-control | [포커스 시 작업 영역 재계산](../ChangeLog/window-control/2026-09-06_focus-workarea-refresh.md) | 부팅 때 1회만 읽던 작업 영역을 포커스 시점에 다시 읽는다. 창이 올라가 있는 모니터 기준 |
| window-control | [창 하단 1px 빈칸](../ChangeLog/window-control/2026-09-06_taskbar-1px-gap.md) | `systemHeight - 49` 상수 가정 제거. 작업 표시줄이 실측 48px 이라 1px 이 비었다 |
| cheat-panel | [종료 버튼 · 화면 설정 세부 패널](../ChangeLog/cheat-panel/2026-09-06_quit-button-and-display-panel.md) | 오버레이 창은 닫을 방법이 없었다. 세부 치트를 별도 패널로 여는 형태 도입 |
| ui-system | [전역 UIRoot 도입, 씬 UI 이전](../ChangeLog/ui-system/2026-09-06_global-uiroot-migration.md) | 씬마다 있던 EventSystem 이 서로를 꺼서 UI 입력이 죽던 문제. advise/002-8 |

## 2026-08-12

| 영역 | 기록 | 한 줄 |
|---|---|---|
| transparency-rendering | [BlitPass 스택 삭제](../ChangeLog/transparency-rendering/2026-08-12_blitpass-removal.md) | 투명은 렌더 피처 없이 프로젝트 설정만으로 성립한다. advise/001 1-5 |

## 2026-08-10

| 영역 | 기록 | 한 줄 |
|---|---|---|
| p0-resolution | [P0 해소와 투명 오버레이 실동작 확보](../ChangeLog/p0-resolution/2026-08-10_build-and-overlay.md) | 첫 Windows 빌드 통과. 빌드 차단·씬 등록·투명·클릭 통과·치트 패널·커맨드 큐 |

---

## 영역 목록

`ChangeLog/` 아래 현재 영역 6개 — `docs-system` · `window-control` · `cheat-panel` ·
`ui-system` · `transparency-rendering` · `p0-resolution`.

새 영역을 만들 때는 폴더명을 **영어 슬러그**로 짓고, 파일명은 `YYYY-MM-DD_<주제>.md` 로 맞춘다.
이름은 작업이 끝난 뒤 제안하고 사용자 승인을 받는다 → [process/en/00-process.md](../process/en/00-process.md) §8
