# process/ — bilingual workflow docs

Same content, two languages, kept in sync file-for-file and section-for-section.

```
process/
├─ en/   Agent-facing. CLAUDE.md links here — read every session.
└─ ko/   User-facing. Read this to review or discuss the rules in Korean.
```

| # | File | Content |
|---|---|---|
| 00 | `00-process.md` | Session process. The sequence from intake to handoff |
| 01 | `01-rules.md` | The body copied into `CLAUDE.md` |
| 02 | `02-git.md` | Branch · commit · push · stash |
| 03 | `03-docs.md` | Where and in what format to write docs |
| 04 | `04-templates.md` | 6 work-spec document templates |
| 05 | `05-memory.md` | File-based memory operations |
| 06 | `06-model-and-prompting.md` | Model selection + prompting principles |
| 07 | `07-rule-pipeline.md` | How this doc set itself gets built and updated |
| — | `appendix-unity.md` | Unity-only. Delete wholesale for a different stack |

## Why two copies instead of one bilingual file

Interleaved bilingual text is slower for both sides to read and drifts silently — a partial edit in one
language is easy to miss. Two parallel files force `07-rule-pipeline.md`'s own maintenance rule: **update
both together, or the agent and the user end up reading different rules.**

## Which one is "the" version

`en/` is what `CLAUDE.md` links to and what gets read every session — **treat it as the working copy.**
`ko/` exists for the user to read comfortably; when discussing a rule change with the user, edit `ko/`
first, then port the same change to `en/` per [07-rule-pipeline.md](en/07-rule-pipeline.md) (English) /
[07-rule-pipeline.md](ko/07-rule-pipeline.md) (Korean).

---

# process/ — 이중 언어 워크플로 문서

같은 내용, 두 언어. 파일명·섹션 단위로 동기화 상태를 유지한다.

```
process/
├─ en/   에이전트 확인용. CLAUDE.md 가 여기를 링크 — 매 세션 읽힌다.
└─ ko/   사용자 확인용. 규칙을 한국어로 검토·논의할 때 읽는다.
```

표는 위 영문판과 동일하다.

## 왜 한 파일 이중 언어가 아니라 두 파일인가

교차 배치된 이중 언어 텍스트는 양쪽 다 읽기 느리고 조용히 어긋난다 — 한쪽만 반영된 수정이 눈에 잘 안 띈다.
두 파일을 나란히 두면 `07-rule-pipeline.md` 자신의 유지 규칙이 강제된다: **양쪽을 함께 갱신하지 않으면
에이전트와 사용자가 서로 다른 규칙을 읽게 된다.**

## 어느 쪽이 "정본"인가

`en/`은 `CLAUDE.md`가 링크하고 매 세션 읽히는 쪽이다 — **작업 원본으로 취급한다.**
`ko/`는 사용자가 편하게 읽기 위한 것이다. 사용자와 규칙 변경을 논의할 때는 `ko/`를 먼저 고치고,
[07-rule-pipeline.md](ko/07-rule-pipeline.md)의 절차대로 같은 변경을 `en/`에도 반영한다.
