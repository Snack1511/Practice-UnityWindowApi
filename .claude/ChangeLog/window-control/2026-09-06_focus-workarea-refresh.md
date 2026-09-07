# 포커스 시 작업 영역 재계산 (2026-09-06)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../docs/CHANGELOG.md)는 시간순 인덱스다.

작업 영역을 부팅 때 1회만 읽으면, 실행 중에 작업 표시줄을 옮기거나 자동 숨김을 켰을 때 창이 안 따라간다.
`MainProcess.OnApplicationChangedFocus` — 원래 훅만 걸려 있고 본문이 비어 있던 자리 — 에서 다시 읽는다.

```csharp
if (!isFocus) return;
if (!WindowNativeManager.TryGetMonitorForWindow(out var monitor)) return;
if (WindowNativeManager.IsWindowFittedTo(monitor)) return;
WindowNativeManager.SetWindowToMonitor(monitor);
```

**주 모니터가 아니라 창이 올라가 있는 모니터를 기준으로 잡는다.**
주 모니터로 맞추면 치트 패널 「화면 설정」으로 옮겨둔 창이 포커스를 얻는 순간 도로 끌려온다.
`MonitorFromWindow(MONITOR_DEFAULTTONEAREST)` 로 찾고, `GetWindowRect` 로 이미 맞는지 확인해 불필요한 `SetWindowPos` 를 건너뛴다.

### 넣지 않은 것 — 창 서브클래싱

즉시 감지의 정석은 `WM_SETTINGCHANGE`(`SPI_SETWORKAREA`) · `WM_DISPLAYCHANGE` · `WM_DPICHANGED` 를 받는 것이다.
Unity 가 WndProc 을 노출하지 않아 `SetWindowLongPtr(GWLP_WNDPROC)` 서브클래싱이 필요한데,
델리게이트 수명을 틀리면 크래시고 프로시저 복원을 빠뜨리면 도메인 리로드에서 죽은 포인터가 남는다.
**포커스 방식으로 부족하다는 근거가 생기기 전에는 넣지 않는다.** 방법과 주의점은 `WindowNativeManager` 안에 주석으로 남겼다.

### 검증

| 단계 | 결과 |
|---|---|
| Windows Development Build | `Succeeded` |
| 최초 | `0,0 2560x1392` (DISPLAY1 작업 영역) |
| 외부에서 DISPLAY2 로 `1000x800` 이동 | `2560,0 1000x800` |
| 포커스 상실 | `2560,0 1000x800` — 손대지 않는다 |
| **포커스 복귀** | `2560,0 1920x1032` — DISPLAY2 작업 영역으로 스냅, 주 모니터로 안 끌려옴 |

---
