# 부록 — Unity 전용 규칙

Unity 프로젝트가 아니면 이 문서를 통째로 지운다.

## 1. MonoBehaviour 생명주기

**`Awake()`는 바인딩 전용.** 직렬화 필드 캐싱, 내부 참조 연결만 한다.

**구독은 `Start()`에서.** 델리게이트, C# 이벤트, `UnityEvent.AddListener`, 메시지 버스 리스너의 최초 등록은 전부 `Start()`.
오버라이드하면 `base.Start()`를 먼저 부른다.

**해제는 대칭으로 `OnDisable()` / `OnDestroy()`에서.**

**Why:** `Awake()` 시점에는 다른 오브젝트의 초기화가 끝났다는 보장이 없다. 구독 대상이 아직 없을 수 있다.

## 2. 필드 선언

**참조·값 타입 필드에 명시적 기본값(`= null`, `= 0`, `= false`)을 붙이지 않는다.** C# 기본값에 맡긴다.

**예외 — 컬렉션은 반드시 초기화한다.** `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, `Queue<T>` 등은 `= new()`.
안 하면 `NullReferenceException`이다.

이 규칙은 **필드 선언에만** 적용된다. 프로퍼티·지역 변수·파라미터 기본값은 해당 없음.

## 3. Unity 에디터와 git 조작의 충돌

- **에디터가 열려 있는 체크아웃에서 rebase/checkout 하면** 도중에 Unity가 파일을 재생성해 rebase가 중단될 수 있다.
  - 대응: Unity가 만든 churn 파일을 discard 한 뒤 `git rebase --continue`
  - 가능하면 rebase 전에 해당 체크아웃의 에디터를 닫는다
- 워크트리 프로젝트와 메인 체크아웃 프로젝트를 각각 Unity로 열 수 있다.
  **MCP 연결이 어느 에디터를 가리키는지 혼동 주의** — 작업 전 대상 프로젝트를 확인한다.

## 4. 에디터 버전

**`ProjectSettings/ProjectVersion.txt`의 버전과 다른 에디터로 프로젝트를 열면
에셋 데이터베이스와 `.meta` 재직렬화가 일어나고 되돌리기 어렵다.**

버전을 확인할 수 없으면 실행하지 말고 사용자에게 알린다. 임의로 다른 버전으로 열지 않는다.

## 5. 프리팹 YAML 직접 편집

에디터 없이 프리팹/씬 YAML을 편집(스크립트 GUID 교체, 노드 삭제 등)할 때:

- 루트 `m_Script` GUID 교체 시 **필드명을 보존**하면 직렬화 참조가 유지된다.
- **stripped RectTransform의 자식은 `m_Children`에 나타나지 않는다.**
  중첩 프리팹에서 노드를 삭제하면 오버라이드 자식이 고아로 남을 수 있으므로 **삭제 후 고아 노드 스캔이 필수다.**
- 스왑 후 잔존 필드를 정리하고, 내부 fileID 참조 무결성을 전수 검사한다.
- 검증: 에디터 로그 기반으로 컴파일 성공(`error CS` 0건)과 프리팹 임포트 성공을 확인한 뒤 플레이 모드 QA.

> 노드 삭제·프리팹 생성은 **YAML 수술보다 에디터 배치 모드 스크립트가 안전하다.**
> `-executeMethod`로 `EditorSceneManager.OpenScene` → `DestroyImmediate` → `SaveScene`을 돌리면
> 자식 참조·SceneRoots 정합을 Unity가 알아서 맞춘다. 임시 스크립트는 실행 후 `.cs`와 `.meta`를 함께 지운다.

## 6. 조건부 컴파일 블록

`#if UNITY_STANDALONE_WIN && !UNITY_EDITOR` 안쪽은 **에디터가 검증하지 않는다.**
배치 모드 컴파일 검사도 이 블록을 건드리지 않는다. **실제 대상 플랫폼 빌드가 유일한 검증 수단이다.**

건드렸으면 검증 3단계(실제 빌드)를 반드시 거친다. → [00-process.md](00-process.md) §6

## 7. 비동기 로드 UI 통합 패턴 (참고)

여러 팝업을 하나의 프레임 + 패널 프리팹들로 통합할 때:

- 오픈 정책: `SetInfo`에서 루트 CanvasGroup alpha=0 → 전 패널 병렬 로드(`UniTask.WhenAll`) 완료 → 알파 복원 후 오픈 연출
- 패널 인스턴스는 팝업 풀링에 맞춰 1회 생성 후 캐싱, Addressables 핸들은 `OnDestroy`에서 해제
- AB 테스트 변형(`_B`)이 있으면 프레임 레이아웃(닫기 버튼 위치 등)까지 다를 수 있어 **프레임 프리팹도 변형별로 필요할 수 있다**
