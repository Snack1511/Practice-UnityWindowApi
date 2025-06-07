using System;
using System.Collections.Generic;
using System.Threading;
using CollectionExtension;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<GameObject> spawnObjects;

    [SerializeField] private int spawnCount = 1;
    [SerializeField] private float spawnDelayTime = 1.0f;
    
    Dictionary<int, List<GameObject>> spawnedPools = new Dictionary<int, List<GameObject>>();
    //private CancellationTokenSource autoSpawnTaskCTS = null;
    
    private void Awake()
    {
        // TODO : 임시코드
        //autoSpawnTaskCTS = new CancellationTokenSource();
        //SpawnObjectByIndex(0, 0);
    }

    // private void Start()
    // {
    //     SpawnTask(autoSpawnTaskCTS.Token).Forget();
    // }

    private void OnDestroy()
    {
        // autoSpawnTaskCTS.Cancel();
        // autoSpawnTaskCTS.Dispose();
        // autoSpawnTaskCTS = null;
    }

    async public UniTask StartSpawnAsync(CancellationToken autoSpawnTaskToken)
    {
        while (0 != spawnCount)
        {
            if (autoSpawnTaskToken.IsCancellationRequested)
            {
                Debug.Log("Auto spawn task cancelled");
                break;
            }
            
            Debug.Log("Spawn");
            SpawnObjectByRandom();
            spawnCount--;
            
            Debug.Log("StartWait");
            await UniTask.WaitForSeconds(spawnDelayTime, cancellationToken: autoSpawnTaskToken);
            Debug.Log("EndWait");
        }
    }

    private GameObject CloneObject(GameObject obj)
    {
        var clonedObject = Instantiate(obj, transform);
        clonedObject.SetActive(false);
        return clonedObject;
    }

    public void SpawnObjectByIndex(int spawnPointIndex = 0, int spawnObjectIndex = 0)
    {
        Transform targetPosition = null; 
        if (spawnPoints.IsInRange(spawnPointIndex))
        {
            targetPosition = spawnPoints[spawnPointIndex];
        }
        else
        {
            targetPosition = spawnPoints.Random(out int randomIndex);
        }

        GameObject targetObject = null;
        if (spawnObjects.IsInRange(spawnObjectIndex))
        {
            targetObject = spawnObjects[spawnObjectIndex];
        }
        else
        {
            targetObject = spawnObjects.Random(out int randomIndex);
            spawnObjectIndex= randomIndex;
        }

        Spawn(spawnObjectIndex, targetObject, targetPosition);
    }
    
    public void SpawnObjectByRandom()
    {
        Transform targetPosition = null; 
        targetPosition = spawnPoints.Random(out int randomPosIndex);

        GameObject targetObject = null;
        targetObject = spawnObjects.Random(out int randomObjIndex);

        Spawn(randomObjIndex, targetObject, targetPosition);
    }

    private void Spawn(int spawnIndex, GameObject targetObject, Transform targetPoint)
    {
        if (null == targetObject || null == targetPoint)
            return;

        // 풀 선택
        List<GameObject> targetList = null;
        if (!spawnedPools.ContainsKey(spawnIndex))
        {
            spawnedPools.Add(spawnIndex, new List<GameObject>());
        }
        
        targetList = spawnedPools[spawnIndex];
        
        //비활성화된 오브젝트 선택
        GameObject deactiveObject = targetList.IsNullOrEmpty() ? null : targetList.Find(x=>x.activeSelf == false);
        //없으면 생성 후 할당
        if (null == deactiveObject)
        {
            var clonedObject = Instantiate(targetObject, transform);
            clonedObject.SetActive(false);
            targetList.Add(clonedObject);
            deactiveObject = clonedObject;
        }

        //있으면 활성화
        if (null != deactiveObject)
        {
            deactiveObject.transform.position = targetPoint.position;
            deactiveObject.SetActive(true);
        }
    }
}
