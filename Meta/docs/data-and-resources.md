# 데이터 계층 — 리소스 · 테이블 · 세이브 · 모델

세 개의 매니저가 각각 다른 축을 담당한다.

| 매니저 | 소스 | 저장 형태 | 키 |
|---|---|---|---|
| `ResourcesManager` | `Assets/Resources/**` | `Dictionary<Type, Dictionary<string, Object>>` | 파일명(확장자 제외) |
| `TableManager` | `Resources/Table/*.csv` | `Dictionary<Type, Dictionary<int, TableDataBase>>` | `index` 컬럼 |
| `IOManager` | 파일시스템 JSON | `Dictionary<ESaveType, SaveBase>` | `ESaveType` |

## 1. `ResourcesManager`

`Assets/Script/Manager/SingletonManager/ResourcesManager.cs`

**모든 런타임 에셋 로드의 단일 창구**를 목표로 하는 캐시 레이어. `Resources.Load` / `Resources.LoadAsync`를 감싸고 결과를 타입별로 캐싱한다.

```csharp
Sprite s      = await ResourcesManager.Instance.LoadAsync<Sprite>("Sprite/GoldSample.png");
TextAsset csv = ResourcesManager.Instance.Load<TextAsset>("Table/testTable.csv");
```

- **경로에 확장자를 붙여서 호출하는 것이 이 API의 관례다.** 내부에서 확장자를 제거한 뒤 `Resources.Load`에 넘긴다.
- 캐시 키는 **파일명만** 사용한다(`Path.GetFileNameWithoutExtension`). 폴더가 달라도 이름이 같으면 충돌한다.
- `Release()`는 딕셔너리만 비운다. 실제 언로드는 `SceneBase.ExitScene`의 `Resources.UnloadUnusedAssets()`에 의존.
- 캐시 개별 제거/만료 API 없음 — 앱 수명 동안 계속 쌓이는 구조.
- 우회 호출이 아직 남아 있다: `SpriteController.LoadSprite`, `LoadingScene`의 UILoading 로드, `Loader<T>`.

> `Assets/Resources/` 폴더의 내용물은 참조 여부와 무관하게 **전부 빌드에 포함되고 앱 시작 시 인덱스가 메모리에 올라간다.** 현재 씬 파일까지 이 폴더 안에 있다. → [advise/005](../advise/005-project-hygiene.md)

## 2. `TableManager` — CSV → 객체 매핑

`Assets/Script/Manager/SingletonManager/TableManager.cs`

### 사용법

```csharp
// 1. 테이블 행 타입 정의 — 필드 이름이 CSV 헤더와 정확히 일치해야 한다
public class TestTableData : TableDataBase   // TableDataBase에 public int index 존재
{
    public string   name;
    public int      level;
    public float    attackRange;
    public int[]    skillIDs;      // 배열 지원
    public string[] skillTags;
}

// 2. 로드 (보통 SceneBase.OnLoadResourceAsync 안에서)
await TableManager.Instance.LoadTableDataAsync<TestTableData>("Table/testTable.csv");

// 3. 조회 — static, 실패 시 false
if (TableManager.TryGetData<TestTableData>(1, out TestTableData row)) { /* ... */ }
```

### CSV 규약 (`Resources/Table/testTable.csv`)

```csv
index,name,level,attackRange,skillIDs,skillTags
1,"Warrior",10,1.5,"101,102,105","Melee,Physical,Stun"
```

- 1행 = 헤더, 2행부터 데이터. `index` 컬럼이 딕셔너리 키.
- 파싱 정규식: `"[^"]*"|[^,]+` — 큰따옴표로 감싸면 내부 콤마를 보존한다.
- **배열 필드는 `"a,b,c"` 형태로 감싸서 쓴다.** 따옴표를 벗긴 뒤 `,`로 다시 split한다.
- 헤더에 대응하는 필드가 없으면 그 컬럼은 조용히 무시된다(에러 없음).
- 컬럼 개수가 헤더와 다른 행은 경고 후 스킵. `index` 중복 시 경고 후 덮어쓴다.
- 값 변환은 `Convert.ChangeType` — public 인스턴스 필드만 대상(프로퍼티 불가).

> 빈 셀(`1,,3`)은 정규식이 매칭하지 못해 **컬럼 수가 어긋나 행 전체가 스킵된다.** 실수·소수점은 현재 컬처 설정을 탄다. → [advise/003](../advise/003-data-layer.md)

## 3. `IOManager` — JSON 세이브

`Assets/Script/Manager/SingletonManager/IOManager.cs`

`ESaveType` 하나당 파일 하나. `JsonUtility` 직렬화. 동기/비동기 API가 쌍으로 존재한다.

```csharp
IOManager.Instance.TryLoadData(ESaveType.Test, out SaveBase data);   // 파일 없으면 기본값 생성 후 true
var d = (TestSaveData)data;
d.SetTestString("hi!");
IOManager.Instance.TrySaveData(ESaveType.Test);                      // 메모리 캐시를 파일로

await IOManager.Instance.SaveDataAsync(ESaveType.Test, delaySeconds);
SaveBase loaded = await IOManager.Instance.LoadDataAsync(ESaveType.Test);
```

- 로드된 세이브는 `saveDict`에 캐시된다. `TrySaveData`는 **캐시에 있는 것만** 저장한다(없으면 false).
- 저장 경로: `Application.dataPath/SaveData/<ESaveType>` — **확장자 없음**.
  에디터에서는 `Assets/SaveData/Test`에 실제로 쓰인다(리포지토리에 커밋되어 있음).
- 예외는 전부 `Debug.LogError`로 삼키고 false/null 반환.

### 세이브 타입 추가 절차 (**3곳 수정 필요**)

1. `ESaveType`에 항목 추가 — `IOManager.cs:12`
2. `SaveBase` 상속 클래스 작성 + `[Serializable]` + `SetDefaultData()` 구현 — `Define/ModelDefine.cs`
3. `SaveDataFactory.CreateSaveBase` switch에 추가 — `ModelDefine.cs:25`
4. `IOManager.GetSaveType` switch에 `typeof(...)` 추가 — `IOManager.cs:88`

> 3·4는 사실상 같은 매핑을 두 번 쓰는 것이다. 하나를 빠뜨리면 런타임 예외. → [advise/003](../advise/003-data-layer.md)

## 4. Model / Save 분리 설계 (진행 중)

`Assets/Script/Define/ModelDefine.cs` — **아직 구현 없이 설계 메모 상태.** 주석에 남은 구상:

```
저장 데이터와 런타임 데이터를 분리 → SaveBase ↔ ModelBase
ModelManager를 통해 Model을 생성/할당하고, 값 변경 후 Notify
ModelManager는 Content나 Scene에 물려서 관리 (전역 매니저 아님)
IOManager 저장 → SaveData.Notify → ModelData.Listen
```

현재 `ModelBase`는 멤버가 없는 빈 추상 클래스다. `SaveBase`(직렬화 대상)만 실동작한다.
이 축을 실제로 만들 때 결정해야 할 것들은 [advise/003](../advise/003-data-layer.md) 참조.

## 5. `GamePathDefine` ScriptableObject

`Assets/Script/ScriptableObject/GamePathDefine.cs` + `Resources/ScriptableObject/GamePathDefine.asset`

`GameProcessObjectPath: Prefabs/GameProcess.prefab` 값이 들어 있지만 **어떤 코드도 이 에셋을 로드하지 않는다.**
`GameProcessManager.cs:16-17`에 `//TODO : ScriptableObject 로드 처리하기`와 하드코딩된 `const string GameProcessPath`가 그대로 남아 있다. 경로 상수를 에셋으로 빼려던 작업의 중간 상태.
