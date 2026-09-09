# 치트 패널 : 종료 버튼 · 화면 설정 세부 패널 (2026-09-06)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.

### 치트 패널 추가

| 항목 | 내용 |
|---|---|
| `AddButton(label, onClick)` | 동작 버튼. `toggles` 에 들어가지 않아 **숫자키 번호를 밀지 않는다** |
| `Quit` 버튼 | `Application.Quit()`. 오버레이 창은 테두리가 없어 닫을 방법이 없었다 |
| 「화면 설정」 세부 패널 | 별도 패널(`display-root`)로 연다. 드래그·접기는 기존 `SetupPanelChrome` 재사용 |
| 화면 설정 내용 | 연결된 모니터를 버튼으로 나열. 누르면 그 모니터의 **작업 영역**으로 창 이동 |

세부 패널은 `IsPointerOverPanel()` 에도 포함시켰다 — 빠뜨리면 클릭 통과 중에 패널 위에서 클릭이 안 먹는다.
에디터에서 창을 옮기면 에디터 창이 움직이므로 「화면 설정」은 `#if !UNITY_EDITOR` 로 빌드에서만 노출한다.

### 검증

| 단계 | 결과 |
|---|---|
| 에디터 배치 모드 컴파일 | `error CS` 0건 |
| Windows Development Build | `Succeeded` |
| 창 크기 (`GetWindowRect`) | `0,0 2560x1392` — 작업 영역과 정확히 일치, 1px 빈칸 없음 |
| Quit 버튼 | 클릭 후 `[CheatPanel] 종료 요청` + 프로세스 **스스로 종료** |
| 화면 설정 패널 | `[CheatPanel] 화면 설정 : 모니터 2 개 등록`, 버튼으로 열림 |
| 모니터 전환 | DISPLAY2 버튼 클릭 → 창이 `2560,0 1920x1032` 로 이동 (작업 영역 일치) |

> 검증은 합성 입력(`SetCursorPos` + `mouse_event`)으로 실제 클릭을 넣어 확인했다. 로그 부재가 아니라 **동작 발생**으로 판정했다.

---

> 창 하단 1px 수정은 같은 세션 작업이지만 영역이 달라 분리했다 → [window-control](../../window-control/2026-09-06/작업내역_window-control_taskbar-1px-gap.md)
