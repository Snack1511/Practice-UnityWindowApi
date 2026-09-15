# 005 — 프로젝트 위생 (P2)

코드가 아니라 **리포지토리와 에셋** 쪽. 혼자 작업할 때는 티가 안 나다가, 두 번째 사람이 붙거나 기기를 옮기는 순간 전부 한꺼번에 터진다.

---

## 5-1. `.gitattributes`가 없다 — 대량 변경 / 병합 지옥

**근거**
- 리포지토리 루트에 `.gitattributes` 없음
- 현재 작업 트리 상태: **수정 714 / 삭제 12 / 미추적 53**
- 그중 대부분이 `Assets/Plugins/Demigiant/**`, `SPUM/**`, `TextMesh Pro/**` 등 **손대지 않은 서드파티 에셋**의 `.meta`·`.dll`·`.unity` 파일

**영향**
- 실제 작업 내용(`Assets/Script/**` 3개 파일)이 700개 노이즈에 파묻혀 **리뷰도 되돌리기도 불가능**하다.
- `.unity`/`.prefab`/`.asset`은 텍스트지만 **줄 단위 병합이 불가능한 구조**다. `.gitattributes` 없이 병합하면 씬이 조용히 깨진다.
- `.dll`, `.png` 등을 Git이 텍스트로 오인하면 개행 변환으로 **파일이 손상**된다.

**권장** — 루트에 `.gitattributes` 추가. Unity 표준 구성:

```gitattributes
* text=auto

# Unity 직렬화 에셋 — 자동 병합 금지
*.unity      -text merge=unityyamlmerge diff
*.prefab     -text merge=unityyamlmerge diff
*.asset      -text merge=unityyamlmerge diff
*.mat        -text merge=unityyamlmerge diff
*.controller -text merge=unityyamlmerge diff
*.meta       -text merge=unityyamlmerge diff

# 바이너리 — 개행 변환 금지
*.dll  binary
*.so   binary
*.png  binary
*.jpg  binary
*.psd  binary
*.fbx  binary
*.wav  binary
*.mp3  binary
*.ttf  binary
*.otf  binary
*.cubism3.json -text
```

추가 조치:
1. **지금의 714개 변경을 먼저 정리한다.** 서드파티 churn은 `git checkout -- <경로>`로 되돌리거나, 되돌릴 수 없으면 **"플러그인 재직렬화" 단일 커밋으로 분리**해서 작업 커밋과 섞이지 않게 한다.
2. `.gitattributes` 적용 후 `git add --renormalize .` 1회 실행.
3. 대용량 바이너리가 늘면 Git LFS 검토 (`*.psd`, `*.fbx`, Live2D 모델 등).

> 참고: 이 리포지토리에는 `.gitignore`가 이미 잘 잡혀 있다(Library/Temp/obj/UserSettings/*.csproj/*.sln). 문제는 **ignore가 아니라 attributes 쪽**이다.

---

## 5-2. 세이브 파일이 리포지토리에 커밋되어 있다

**근거** — `Assets/SaveData/Test` (내용: `{"test":"hi!"}`), `Assets/SaveData/Test.meta`

원인은 [003-1](003-data-layer.md#3-1-세이브-경로가-applicationdatapath--빌드에서-쓰기-실패)의 `Application.dataPath` 사용.

**영향**
- 실행할 때마다 파일이 바뀌어 **매번 diff에 뜬다.**
- 유저 상태가 소스 관리에 들어가는 것 자체가 잘못된 경계다.
- 에셋 데이터베이스에 편입되어 임포터가 매번 처리한다.

**권장**
1. 저장 경로를 `Application.persistentDataPath`로 변경 ([003-1](003-data-layer.md)).
2. `git rm --cached -r Assets/SaveData`로 추적 해제.
3. `.gitignore`에 `/[Aa]ssets/SaveData/` 추가.

---

## 5-3. `Resources/` 전면 의존

**근거** — `Assets/Resources/` 하위에 **씬·프리팹·스프라이트·셰이더·머티리얼·CSV가 전부** 들어 있다. 코드의 모든 로드 경로(`ResourcesManager`, `TableManager`, `Loader<T>`, `LoadingScene`, `SpriteController`)가 `Resources.Load` 계열이다.

**영향**

| 문제 | 설명 |
|---|---|
| 전량 빌드 포함 | `Resources/` 안의 파일은 **참조 여부와 무관하게 전부 빌드에 들어간다.** 지금 `Sprite/asdfadsf.png` 같은 실험 파일도 포함된다 |
| 시작 시간 | Resources 인덱스가 앱 시작 시 메모리에 로드된다. 파일 수에 비례해 **첫 프레임이 늦어진다** |
| 문자열 경로 | 컴파일 검증 불가. 오타는 런타임 null |
| 패치 불가 | 빌드에 박히므로 에셋만 교체하는 업데이트가 불가능 |
| 씬이 Resources 안에 | `.unity`는 Resources로 로드되지 않는다(Build Settings 등록이 필요). **이 폴더에 있을 이유가 없다** |

**권장 (단계적)**
1. **즉시 · 저비용** — `Assets/Resources/Scenes/` → `Assets/Scenes/`로 이동. Build Settings 경로만 갱신하면 되고, 불필요한 빌드 포함이 줄어든다.
2. **중기** — Addressables 전환 검토. 다만 **`ResourcesManager`라는 단일 창구가 이미 있는 것이 큰 이점**이다. 내부 구현만 Addressables로 갈아끼우면 호출부는 그대로다. 그러려면 먼저 [003-7](003-data-layer.md#3-7-resourcesmanager-캐시-키-충돌-및-경로-처리)의 우회 호출들(`SpriteController`, `LoadingScene`, `Loader<T>`)을 창구로 모아야 한다.
3. **경로 상수화** — `"Prefabs/UILoading"`, `"Table/testTable.csv"` 같은 리터럴이 코드에 흩어져 있다. `GamePathDefine` ScriptableObject가 **바로 이 목적으로 만들어졌는데 아무도 안 쓴다** ([5-4](#5-4-죽은-에셋과-잔여물)).

> Addressables는 **지금 도입할 필요는 없다.** 에셋이 수백 개가 되거나 패치 배포가 필요해지는 시점이 기준이다. 그때까지는 1번과 3번만으로 충분하다.

---

## 5-4. 죽은 에셋과 잔여물

| 대상 | 상태 | 권장 |
|---|---|---|
| `Assets/_Recovery/0.unity` | Unity 크래시 복구 산출물 | 내용 확인 후 삭제 |
| `Assets/Resources/ScriptableObject/GamePathDefine.asset` | 값은 있으나 **로드하는 코드가 없음**. `GameProcessManager.cs:16-17`에 `//TODO : ScriptableObject 로드 처리하기` + 하드코딩 상수 | TODO를 완료하거나(경로 상수를 여기로 모음) 에셋+클래스를 삭제. **둘 중 하나로 끝낸다** |
| `Assets/Resources/Sprite/asdfadsf.png` | 임시 이름 | 삭제 또는 개명 |
| `Assets/Resources/Scenes/GameScene.unity` | Build Settings 등록 O, `ESceneType` X | [002-7](002-scene-system.md#2-7-사용되지-않는-경로-정리) |
| `GameFlow/Loader.cs:50-87` | 주석 처리된 `LoadManager` 전체 | 삭제. git 히스토리에 남아 있다 |
| ~~`Shader/BlitPass.cs`~~ | ✅ 해결 (2026-08-12) — 파일이 삭제되어 `Avx` using 과 tmp RT 주석이 함께 사라졌다. [001-5](001-build-blockers.md) | — |
| `Assets/TutorialInfo/`, `Assets/Readme.asset` | Unity 템플릿 잔여물 | 삭제 |
| `MenuiScene.unity` → `MenuScene.unity` | 오타 파일명 개명이 커밋 안 된 상태(삭제 12건에 포함) | 개명 커밋을 마무리 |

---

## 5-5. 미사용 패키지 / 플러그인

`Packages/manifest.json` 및 `Assets/Plugins`에 있으나 `Assets/Script`에서 참조가 확인되지 않는 것들:

| 대상 | 상태 |
|---|---|
| `com.unity.visualscripting` | 미사용. 빌드 크기·임포트 시간에 기여 |
| `com.unity.ai.navigation` | 미사용 |
| `com.unity.timeline` | 미사용 |
| `com.unity.multiplayer.center` | 미사용 (템플릿 기본) |
| DOTween / DOTweenPro | 코드 참조 없음 (씬/프리팹에서 쓸 가능성 있음) |
| SPUM, Febucci Text Animator, Live2D Cubism | 코드 참조 없음 |
| `InputSystem_Actions.inputactions` | 코드에서는 구식 `Input.GetKeyDown` 사용 중 (`TestScene.cs:110`, `DebuggingComponent.cs:43`) |

**권장**
- **당장 지울 필요는 없다.** 연습 프로젝트에서 나중에 쓸 에셋을 미리 넣어두는 것은 정상이다.
- 다만 **빌드 시간이나 임포트가 느려진다고 느끼는 시점**에 이 표를 보고 제거를 판단한다.
- Input System은 방향을 정해야 한다: 패키지를 쓸 거면 `Input.GetKeyDown`을 걷어내고, 안 쓸 거면 Player Settings의 Active Input Handling을 정리한다. **지금처럼 둘 다 있는 상태가 가장 나쁘다.**

---

## 5-6. 서브모듈 취급

**근거** — `.gitmodules`: `Assets/ThirdParty/UnityFramework-Extension` → `github.com/Snack1511/UnityFramework-Extension.git`

**주의사항** (문제는 아니지만 문서화 필요)
- 클론 시 `git clone --recursive` 또는 `git submodule update --init`이 필요하다. 빼먹으면 **`Framework.Extension.*` 네임스페이스가 통째로 없어 컴파일 실패**한다. 원인을 모르면 한참 헤맨다.
- 서브모듈 커밋 포인터가 갱신되지 않으면 다른 기기에서 옛 버전을 받는다.
- 확장 메서드를 수정하려면 별도 리포지토리에 커밋 → 포인터 갱신, 2단계다.

**권장**
- `README.md`(리포지토리 루트)에 클론 절차를 한 줄 남긴다. 현재 루트에 README가 없다.
- 서브모듈 안에 `.asmdef`를 넣어두면 [004-1](004-architecture-scale.md#4-1-asmdef-도입) 분할 시 자연스럽게 독립 어셈블리가 된다.

---

## 5-7. 매직 넘버 · 하드코딩

| 값 | 위치 | 문제 |
|---|---|---|
| `- 49` | `Assets/ThirdParty/Unity-WindowNativeCustom/Script/WindowNativeManager.cs` 의 `Initialize` | 작업 표시줄 높이 가정. **DPI 배율·작업 표시줄 위치(좌/우/상)·자동 숨김에서 전부 어긋난다.** `SystemParametersInfo(SPI_GETWORKAREA)`로 실제 작업 영역을 질의하는 것이 정답 |
| `"Prefabs/GameProcess.prefab"` | `StaticManager/GameProcessManager.cs:17` | `GamePathDefine.asset`에 같은 값이 있으나 미사용 |
| `"Prefabs/UILoading"` | `GameScene/LoadingScene.cs:43` | 상수화 대상 |
| `"Table/testTable.csv"` | `GameScene/TestScene.cs:43` | 테스트 코드라 허용 가능 |
| `"SceneManager"` | `GameFlow/MainProcess.cs:34` | updater 키 문자열. 오타 시 조용히 미등록 → `nameof()` 사용 권장 |
| `gravityScale = 2` | `Controller/MoveController.cs:33` | 인스펙터 노출 또는 상수화 |
| ~~`_TransparencyFactor = 0.01`~~ | ~~`Resources/Shader/MakeTransparent.shader`~~ | ✅ 해당 없음 (2026-08-12) — 셰이더 삭제됨 |
