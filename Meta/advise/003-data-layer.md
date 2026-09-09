# 003 — 데이터 계층: 세이브 · 테이블 · 리소스 (P1)

세이브는 **한 번 잘못 나가면 유저 데이터가 날아가는** 영역이다. 다른 항목과 우선순위를 같이 두지 않는다.

---

## 3-1. 세이브 경로가 `Application.dataPath` — 빌드에서 쓰기 실패

**근거** — `Assets/Script/Manager/SingletonManager/IOManager.cs:151-157`

```csharp
private string GetFilePath(ESaveType saveType)
{
    string path = Application.dataPath;            // 에디터: <프로젝트>/Assets
    path = Path.Combine(path, "SaveData");         // 빌드:   <게임>_Data
    path = Path.Combine(path, saveType.ToString());
    return path;
}
```

**영향**
- 에디터에서는 `Assets/SaveData/Test`에 파일이 생기고 **에셋 데이터베이스에 세이브 파일이 편입된다.** 실제로 `Assets/SaveData/Test`(`{"test":"hi!"}`)가 리포지토리에 커밋되어 있다.
- 빌드에서 `*_Data` 폴더는 플랫폼에 따라 읽기 전용이며, Program Files 하위 설치 시 **UAC로 쓰기가 거부**된다. macOS 앱 번들, 모바일에서는 확실히 실패한다.
- 확장자가 없어 파일 연결·필터링·백업 스크립트 작성이 불편하다.

**권장**
```csharp
private static string GetFilePath(ESaveType saveType) =>
    Path.Combine(Application.persistentDataPath, "SaveData", $"{saveType}.json");
```
`Assets/SaveData/`는 리포지토리에서 제거하고 `.gitignore`에 추가한다.
에디터에서 세이브 파일을 열어보고 싶다면 `Application.persistentDataPath`를 로그로 찍거나 `[MenuItem]`으로 폴더를 여는 편이 낫다 — 에셋 폴더에 두는 것보다 안전하다.

---

## 3-2. 저장이 원자적이지 않다 — 크래시 시 세이브 파손

**근거** — `IOManager.cs:48`, `:114`

```csharp
File.WriteAllText(filePath, jsonString);            // 기존 파일을 직접 덮어씀
await File.WriteAllTextAsync(filePath, jsonString);
```

**영향**
쓰기 도중 프로세스가 죽거나 전원이 끊기면 **원본과 새 데이터가 둘 다 사라진 잘린 파일**이 남는다. 이후 `JsonUtility.FromJson`이 예외를 던지고, 현재 코드는 그 예외를 `catch`해 로그만 남기므로 **유저는 세이브가 통째로 초기화된 것으로 경험**한다.

**권장** — 임시 파일 → 교체 패턴. 3줄이면 된다.
```csharp
string tmp = filePath + ".tmp";
File.WriteAllText(tmp, jsonString);
File.Move(tmp, filePath, overwrite: true);   // .NET Standard 2.1 / Unity 6 지원
```
한 단계 더 안전하게 가려면 `File.Replace(tmp, filePath, filePath + ".bak")`로 직전 세이브를 백업으로 남긴다. 로드 실패 시 `.bak` 폴백까지 붙이면 실질적인 데이터 손실이 사라진다.

---

## 3-3. `if (...);` 오타로 조건이 무효화됨

**근거** — `IOManager.cs:74-78`

```csharp
//파일 없으면 신규 데이터 생성
if(TryAddNewSaveData(saveType));       // ← 세미콜론. if 본문이 빈 문장이 된다
    saveBase = GetSaveData(saveType);  // ← 들여쓰기만 종속, 실제로는 항상 실행
return true;
```

**영향**
`TryAddNewSaveData`가 실패해도 `saveBase = null`인 채 **`true`를 반환한다.** 호출부(`TestScene.cs:127-130`)는 반환값을 믿고 캐스팅하므로 NRE.

**권장**
```csharp
if (!TryAddNewSaveData(saveType))
    return false;
saveBase = GetSaveData(saveType);
return true;
```
같은 계열 방어로, Rider/VS에서 **"Possible mistaken empty statement" 경고를 에러로 승격**시켜 두면 재발하지 않는다.

---

## 3-4. 세이브 타입 매핑이 두 곳에 중복된다

**근거** — `Define/ModelDefine.cs:25-35` 와 `IOManager.cs:88-95`

```csharp
// (1) 생성 매핑
ESaveType.Test => new TestSaveData(),

// (2) 역직렬화 타입 매핑 — 같은 지식의 두 번째 사본
ESaveType.Test => typeof(TestSaveData)
```

`GetSaveType`의 switch에는 **default 절이 없다.** C#은 이를 경고(CS8509)로만 처리하고, 매핑되지 않은 값이 오면 런타임에 `InvalidOperationException`이 난다 — 그것도 `catch`에 삼켜져 "파일 저장 중 오류 발생"이라는 **엉뚱한 메시지**로 나온다(로드 실패인데 저장 메시지, `IOManager.cs:82`·`:146`).

**권장** — 매핑을 한 곳으로.
```csharp
private static readonly Dictionary<ESaveType, Type> SaveTypeMap = new()
{
    [ESaveType.Test] = typeof(TestSaveData),
};

public static SaveBase CreateSaveBase(ESaveType t)
{
    if (!SaveTypeMap.TryGetValue(t, out var type))
        throw new ArgumentOutOfRangeException(nameof(t), $"지원하지 않는 SaveType: {t}");
    var data = (SaveBase)Activator.CreateInstance(type);
    data.SetDefaultData();
    return data;
}
```
`ESaveType`을 늘릴 때 **손댈 곳이 enum + 클래스 + 맵 한 줄**로 줄어든다.
에러 메시지도 컨텍스트에 맞게 분리한다(`저장 실패` / `로드 실패` / `경로: {filePath}` 포함).

---

## 3-5. 세이브 스키마 버전이 없다

**근거** — `SaveBase`에 버전 필드 없음 (`ModelDefine.cs:41-44`)

**영향**
필드를 추가/제거/개명하는 순간 기존 세이브 파일과 호환이 깨진다. `JsonUtility`는 없는 필드를 조용히 기본값으로 두므로 **에러 없이 값만 틀어지는** 형태로 나타난다 — 가장 찾기 어려운 종류의 버그다.

**권장** — 지금 필드 하나 넣는 비용이 나중에 마이그레이션 코드를 짜는 비용보다 압도적으로 싸다.
```csharp
public abstract class SaveBase
{
    public int schemaVersion = 1;
    public abstract void SetDefaultData();
    public virtual void Migrate(int fromVersion) { }   // 로드 직후 호출
}
```
로드 시 `schemaVersion`이 현재보다 낮으면 `Migrate` 호출, 높으면(구버전 클라이언트가 신버전 세이브를 읽는 경우) **로드를 거부**한다 — 덮어써서 날리는 것보다 낫다.

---

## 3-6. CSV 파서의 한계

**근거** — `Manager/SingletonManager/TableManager.cs:39`

```csharp
const string pattern = @"""[^""]*""|[^,]+";
```

| 케이스 | 현재 동작 | 심각도 |
|---|---|---|
| 빈 셀 `1,,3` | `[^,]+`가 1자 이상을 요구 → **매칭 안 됨.** 컬럼 수 불일치로 **행 전체 스킵**(경고만) | 높음 |
| 셀 안의 escaped quote `""` | 미지원 | 낮음 |
| 개행 포함 셀 | 미지원 (줄 단위 split) | 낮음 |
| `Convert.ChangeType` 컬처 | `CurrentCulture` 사용 → 소수점 표기가 `,`인 로케일에서 `1.5` 파싱 실패 | 중간 |
| 헤더에 없는 필드 | 조용히 무시 | 중간 (오타 탐지 불가) |

**영향**
빈 셀은 실무 CSV에서 흔하다. **데이터가 조용히 사라지고 경고 로그 한 줄만 남는다.**
컬처 문제는 한국어/영어 환경에서는 드러나지 않다가 다른 로케일 PC에서만 재현되는 형태로 나타난다.

**권장**
1. **컬처 고정 (즉시, 1줄)** — `Convert.ChangeType(values[j], field.FieldType, CultureInfo.InvariantCulture)`. `TableManager.cs:134`, `:144` 두 곳.
2. **빈 셀 지원** — 정규식을 `("[^"]*"|[^,]*)(,|$)` 계열로 바꾸거나, 정규식을 버리고 상태 기반 파서로 교체한다. **또는 CSV를 포기하고 JSON/ScriptableObject로 간다** — 테이블이 늘어날수록 이 선택이 유리하다.
3. **헤더 검증** — 매칭되지 않은 헤더/필드를 로드 시 1회 경고. 오타를 즉시 발견할 수 있다.
4. **성능** — 현재 셀마다 `Array.Find`로 필드를 선형 탐색한다(`:112`). 행 수 × 컬럼 수 × 필드 수. 테이블이 커지면 헤더 파싱 시점에 `FieldInfo[]`를 헤더 순서대로 한 번만 배열에 매핑해두는 것으로 해결된다.

---

## 3-7. `ResourcesManager` 캐시 키 충돌 및 경로 처리

**근거** — `ResourcesManager.cs:33-35`, `:66-68`

```csharp
string ext       = Path.GetExtension(filePath);
string loadPath  = filePath.Replace(ext, "");                 // (a)
string assetName = Path.GetFileNameWithoutExtension(filePath); // (b)
```

**(a) `Replace`는 문자열 전체를 치환한다.**
경로 어디에든 확장자와 같은 문자열이 있으면 함께 지워진다.
예: `"Table/csv/data.csv"` + ext `".csv"` → `"Table/csv/data"`가 아니라 **`"Table/csvdata"`**. → `Path.ChangeExtension(filePath, null)` 또는 `filePath[..^ext.Length]` 사용.

**(b) 캐시 키가 파일명뿐이다.**
`"UI/Icon/gold.png"`와 `"Sprite/gold.png"`는 **같은 키 `gold`로 취급**되어, 나중에 요청한 쪽이 먼저 캐시된 다른 에셋을 받는다. 타입이 같으면 캐스팅도 성공해서 **에러 없이 잘못된 에셋이 표시된다.**
→ 키를 `loadPath`(폴더 포함)로 바꾼다. 한 줄 수정이고, 지금 에셋이 적을 때가 가장 안전하다.

**추가**
- 캐시 무효화/개별 해제 API가 없다. `Release()`는 딕셔너리만 비우고 실제 메모리는 `Resources.UnloadUnusedAssets()`에 의존한다. 씬 단위 해제를 하려면 **씬별 캐시 스코프**가 필요하다.
- `SpriteController.LoadSprite`(`Controller/SpriteController.cs:18`)가 `ResourcesManager`를 우회해 `Resources.Load`를 직접 호출한다. 파일에 남은 TODO 주석대로 정리 대상.
- `Loader<T>`(`GameFlow/Loader.cs:27-29`)는 `Path.Combine`을 써서 Windows에서 `Prefabs\GameProcess` 형태의 경로를 만든다. `Resources.Load`는 `/` 기준 경로를 전제하므로 **동작 여부를 실제로 확인하고, 안전하게 `Replace('\\','/')`로 정규화**할 것을 권한다.

---

## 3-8. Model 계층 설계 시 미리 정해둘 것

`Define/ModelDefine.cs`의 주석에 남은 구상(저장 데이터 ↔ 런타임 데이터 분리, ModelManager, Notify/Listen)은 방향이 타당하다. 다만 착수 전에 아래 네 가지를 먼저 결정해야 나중에 갈아엎지 않는다.

1. **소유권** — `ModelManager`를 전역 싱글턴으로 둘 것인가, 주석대로 Content/Scene에 물릴 것인가.
   전역이면 편하지만 **씬 전환 시 어떤 모델이 살아남는지가 흐려진다.** 주석의 방향(Content/Scene 소유)이 낫다고 본다.
2. **변경 통지 방식** — `EventBusLocater`(전역 타입 라우팅)를 재사용할지, 모델별 `event`/`IObservable`을 쓸지.
   전역 버스는 "누가 이 값을 듣는가"를 추적 불가능하게 만든다. **모델이 자기 이벤트를 갖는 편**이 디버깅 가능하다.
3. **Save ↔ Model 변환 위치** — `SaveBase.ToModel()` / `ModelBase.ToSave()`를 어디에 둘지. IOManager가 Model을 알면 계층이 역전된다. **별도 매퍼**를 권장.
4. **저장 트리거** — 값이 바뀔 때마다 저장할지, 명시적 세이브 포인트에서만 저장할지. 전자는 I/O 폭주, 후자는 데이터 유실 위험. 보통 **"더티 플래그 + 주기적/이벤트 기반 플러시"**로 절충한다. `SaveDataAsync(delayDuration)`에 이미 지연 인자가 있으니 그 방향에 맞다.
