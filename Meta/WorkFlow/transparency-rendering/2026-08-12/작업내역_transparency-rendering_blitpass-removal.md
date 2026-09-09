# BlitPass 스택 삭제 (2026-08-12, advise/001 1-5)

> 작업 명세 문서. 상세는 이 문서가 정본이고, [docs/CHANGELOG.md](../../../docs/CHANGELOG.md)는 시간순 인덱스다.

전체 알파를 상수로 덮어쓰던 커스텀 렌더 패스 스택을 제거했다.
2026-08-10 에 **치트 패널로 껐을 때 투명이 유지되는 것을 실측**했고, 이번에 실제 삭제와 재검증까지 마쳤다.

**투명은 이제 렌더 피처 없이 프로젝트 설정만으로 성립한다** — 카메라 `clearFlags=SolidColor` + 배경색 `(0,0,0,0)` + `preserveFramebufferAlpha` + HDR off + D3D11.
동작 설명은 [window-native.md 3항](../../../docs/window-native.md).

### 삭제 범위와 순서

**순서가 중요했다.** 에셋 참조를 먼저 끊지 않고 스크립트를 지우면 `PC_Renderer.asset` 에 missing script 항목이 남는다 — `Obstacle.prefab` 이 지금 겪고 있는 상태와 같아진다.

| # | 대상 |
|---|---|
| 1 | `PC_Renderer.asset` 의 `Renderer Features` 에서 `BlitFeature` 제거 (에디터 인스펙터) |
| 2 | `Script/Shader/BlitFeature.cs` · `BlitPass.cs` |
| 3 | `Resources/Material/Custom_MakeTransparent.mat` · `Resources/Shader/MakeTransparent.shader` |
| 4 | `CheatPanel` 의 `Set BitBlitPass` 토글과, 호출자를 잃은 `FindFeature<T>` · `RestoreFeatures` · 관련 필드 3개 · `using` 3개 |

빈 폴더 메타 3개(`Resources/Material.meta`, `Resources/Shader.meta`, `Script/Shader.meta`)도 함께 제거했다.
삭제한 GUID 4개를 프로젝트 전체에서 대조해 잔여 참조 0건을 확인했다.

### 딸려온 변경 — URP 스키마 업그레이드

같은 커밋의 `PC_Renderer.asset` 에 blit 과 무관한 재직렬화가 섞여 있다. 에디터가 프로젝트를 열면서 한 것이다.

```
m_AssetVersion: 2 → 3
+ m_PrepassLayerMask
- m_BlueNoise256Textures (7개), m_Shader
```

### 치트 키 매핑 변경

토글 등록 순서가 곧 `Alpha1..Alpha9` 순서다. 빌드 토글이 하나로 줄어 **`1` = `Set TransparentClick`** 이 됐다(이전에는 `2`).
에디터에서는 등록되는 토글이 0개다 — `Set TransparentClick` 은 `#if !UNITY_EDITOR` 이기 때문이다.

### 검증

| 단계 | 결과 |
|---|---|
| 참조 잔재 GUID 대조 | 0건 |
| `git diff` 정적 검토 | 의도한 13개 파일만 |
| 에디터 배치 모드 컴파일 | `error CS` 0건, `Exiting batchmode successfully` |
| Windows Development Build | `결과=Succeeded 오류=0`, 168 MB |
| 실행 로그 | 셰이더/머티리얼 오류 0건, `Direct3D 11.0 [level 11.1]`, `[CheatPanel] 토글 1 개` |
| 투명 배경 육안 | ✅ 확인 |

> 삭제 커밋을 독립적으로 끊었다. "화면 전체 반투명" 이 필요해지면 `git revert` 로 복원한다 — 두 방식은 결과가 다르다.

---
