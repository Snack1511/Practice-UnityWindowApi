# process/ — 범용 작업 절차

어느 프로젝트에서든 통하는 절차다. **이 프로젝트에서만 참인 값은 여기 두지 않는다** —
그건 [../roles/](../roles/) 로, 이 머신에서만 참인 값은 `CLAUDE.local.md` 로 뺀다.

정본 보관소는 `~/Desktop/Claude/MD/claude-workflow/` 이고, 이 폴더는 그 사본이다.
어긋남을 잡는 대조 절차는 [07-rule-pipeline.md](07-rule-pipeline.md) 7단계에 있다 —
실제로 한 줄이 조용히 빠진 적이 있어서 만든 절차다.

## 읽는 순서

| # | 파일 | 무엇인가 |
|---|---|---|
| 00 | [00-process.md](00-process.md) | **세션 프로세스.** 접수 → 범위 → 조사 → 구현 → 검증 → 기록 → 인수인계. 여기부터 읽는다 |
| 01 | [01-rules.md](01-rules.md) | `CLAUDE.md` 본체로 복사하는 부분 |
| 02 | [02-git.md](02-git.md) | 브랜치 · 커밋 · 푸시 · stash |
| 03 | [03-docs.md](03-docs.md) | 문서를 어디에 어떤 형식으로 쓰나 |
| 04 | [04-templates.md](04-templates.md) | 작업 명세 문서 템플릿 6종 |
| 05 | [05-memory.md](05-memory.md) | 파일 기반 메모리 운영 |
| 06 | [06-model-and-prompting.md](06-model-and-prompting.md) | 모델 선택 규약 + 프롬프팅 원칙 |
| 07 | [07-rule-pipeline.md](07-rule-pipeline.md) | **이 문서들 자체를 만들고 갱신하는 절차** |
| — | [appendix-unity.md](appendix-unity.md) | Unity 전용. 다른 스택이면 통째로 지운다 |

00은 **코드 작업 하나**를 어떻게 하는가, 07은 **규약 자체**를 어떻게 쌓아 가는가다.

## 언어 — 한국어 단일

**전부 한국어로 쓴다.** 이전에는 에이전트용 `en/` 과 사용자 검토용 `ko/` 를 나란히 뒀으나
2026-09-10 에 폐기하고 `ko/` 를 이 자리로 올렸다.

매 세션 토큰 비용을 내는 것은 자동 로드되는 `CLAUDE.md` · `CLAUDE.local.md` 둘뿐이고
나머지는 읽을 때만 낸다. 문서 묶음 전체를 이중화해도 절감은 그 둘에서만 나고 유지 비용만 2배가 됐다.
게다가 이중이면 "어느 언어가 정본인가"가 새로 생겨 정본 1곳 원칙과 부딪힌다.

폐기 근거는 [07-rule-pipeline.md](07-rule-pipeline.md) 의 해당 절에 남겼다.

영문판은 `17fb220` (docs: 문서 체계를 Meta/ 로 이전하고 이중 언어판 폐기) 에서 지웠다.
**그 직전** 커밋의 트리에 남아 있으므로 이렇게 꺼낸다.

```bash
git show 17fb220^:.claude/process/en/00-process.md
git show 17fb220^ --stat -- .claude/process/en/    # 파일 목록
```

`HEAD` 기준으로는 못 꺼낸다 — 지운 커밋 이후로는 트리에 없고, `HEAD~N` 은 커밋이 쌓일수록 어긋난다.
SHA 도 리베이스하면 바뀌므로, 안 맞으면 커밋 메시지로 다시 찾는다:
`git log --oneline --diff-filter=D -- .claude/process/en/`
