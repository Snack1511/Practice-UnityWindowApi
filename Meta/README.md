# Meta/ — 프로젝트 공통 문서

이 프로젝트에서 쓰는 **작업 규약과 코드베이스 문서**의 정본. 커밋되고, 도구 중립이다 —
Claude Code 뿐 아니라 codex·gemini·cursor 도 여기를 읽는다.

자동 로드되는 것은 루트의 `CLAUDE.md` (공통) 와 `CLAUDE.local.md` (개인) 둘뿐이다.
이 폴더 안은 자동으로 읽히지 않고 **거기 링크를 따라 필요할 때만** 열린다.

| 폴더 | 성격 | 언제 보나 |
|---|---|---|
| [process/](process/README.md) | 어느 프로젝트에서든 통하는 **절차** (영문 `en/` · 국문 `ko/` 이중) | 작업 시작 전 |
| [roles/](roles/README.md) | 이 프로젝트에서만 참인 **값** | 코드를 쓰기 전 · 고친 뒤 |
| [docs/](docs/README.md) | 코드베이스 문서 — **지금 어떻게 동작하는가** | 코드를 새로 훑기 전 |
| [advise/](advise/README.md) | 개선 제언 — **어떻게 바꿔야 하는가** (10개 규칙) | 수정 작업 전 |
| `WorkFlow/` | 작업 명세 — **이 작업을 언제 왜 어떻게 했는가** | 과거 작업 근거를 찾을 때 |

`docs/` 와 `advise/` 는 중복 서술하지 않는다. 제언이 반영되어 "현재 동작"이 되면
`docs/` 로 옮기고 `advise/` 에는 해결 표기만 남긴다.

## `Meta/` 와 `.claude/`

| | `Meta/` | `.claude/` |
|---|---|---|
| 성격 | 프로젝트 공통 · 도구 중립 | 개인 계층 |
| git | 추적 | `.gitignore` 로 제외 |
| 내용 | 정본 전체 | 델타만. 구조는 `Meta/` 를 미러 |
| 충돌 시 | | **`.claude/` 가 이긴다** |

`CLAUDE.md` : `CLAUDE.local.md` 관계를 폴더 한 층 위로 복제한 것이다.
`.claude/` 는 gitignore 라 **git 백업이 없다.** [HandOff.md](../.claude/HandOff.md) 도 거기 있다.

## WorkFlow/

기능(영역) 단위로 폴더를 만들고 그 아래 성격별로 나눈다. 구조와 템플릿은
[process/03-docs.md](process/03-docs.md) §2 · [process/04-templates.md](process/04-templates.md).

```
WorkFlow/<영역>/
├─ 명세서/              정본: 구현 명세서, 이관 명세서
├─ YYYY-MM-DD/          날짜별 작업 내역 (세션 단위)
└─ Change_log/          QA 티켓 묶음 처리 기록
```

현재 영역 6개 — `docs-system` · `window-control` · `cheat-panel` · `ui-system` ·
`transparency-rendering` · `p0-resolution`. 시간순 인덱스는 [docs/CHANGELOG.md](docs/CHANGELOG.md).
