# 코드 작성 규약

> **이 프로젝트 고유 규약이다.** 어느 프로젝트에서든 통하는 절차는 [`process/`](../process/README.md)에 있다.
> 절차와 값이 갈릴 때는 여기가 이 프로젝트의 정본이다.

- **매 프레임 로직**은 새 `MonoBehaviour.Update`를 만들지 말고 `GameProcessManager.AddUpdate("키", 메서드)`로 등록한다. Unity `Update` 진입점은 `GameProcess` 하나뿐이다.
- **에셋 로드**는 `ResourcesManager.Instance.Load/LoadAsync<T>(경로)`를 쓴다. `Resources.Load` 직접 호출은 정리 대상이다.
- **확장 메서드**를 새로 만들기 전에 서브모듈을 확인한다: `Framework.Extension.Collection` / `.Component` / `.GameObject` (`IsNullOrEmpty`, `AddOrGetComponent`, `TryAddComponent` 등).
- **비동기**는 `UniTask`. `Coroutine`/`Task` 혼용하지 않는다.
- 새 매니저를 추가하면 `MainProcess`의 `Initialize`와 `Release` **양쪽**에 넣는다 (해제는 역순).
