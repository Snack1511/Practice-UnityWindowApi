# 창 하단 1px 빈칸 (2026-09-06)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../docs/CHANGELOG.md)는 시간순 인덱스다.

### 창 하단 1px 빈칸

`ResolutionManager` 가 `Display.main.systemHeight - 49` 로 작업 표시줄 높이를 상수 가정했다.
실측 작업 표시줄이 **48px** 이라 창이 1px 작게 잡혔고, 그 줄로 배경이 그대로 보였다.

`EnumDisplayMonitors` + `GetMonitorInfo` 로 **작업 영역(`rcWork`)을 OS 에서 직접 읽는다.** 상수 가정을 없앴다.

| 환경 | 전체 | 작업 영역 | 이전 창 높이 | 지금 |
|---|---|---|---|---|
| DISPLAY1 (주) | 2560x1440 | 2560x1392 | 1391 | 1392 |

## 검증

원래 치트 패널 작업과 한 항목으로 기록돼 있었다. 검증 표 전문은 [치트 패널 기록](../cheat-panel/2026-09-06_quit-button-and-display-panel.md)에 있다.

창 크기 `GetWindowRect` 결과가 `0,0 2560x1392` — 작업 영역과 정확히 일치, 1px 빈칸 없음(이전 1391).
