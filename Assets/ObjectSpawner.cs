using System;
using System.Collections.Generic;
using CollectionExtension;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<GameObject> spawnObjects;

    [SerializeField] private int spawnCount = 1;
    
    Dictionary<int, List<GameObject>> spawnedPools = new Dictionary<int, List<GameObject>>();
    
    private void Awake()
    {
        // TODO : 임시코드
        SpawnObject(0, 0);
    }

    private GameObject CloneObject(GameObject obj)
    {
        var clonedObject = Instantiate(obj, transform);
        clonedObject.SetActive(false);
        return clonedObject;
    }

    public void SpawnObject(int spawnPointIndex = 0, int spawnObjectIndex = 0)
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

    private void Spawn(int spawnIndex, GameObject targetObject, Transform targetPoint)
    {
        if (null == targetObject || null == targetPoint)
            return;
        
        List<GameObject> targetList = null;
        if (!spawnedPools.ContainsKey(spawnIndex))
        {
            spawnedPools.Add(spawnIndex, new List<GameObject>());
        }
        
        targetList = spawnedPools[spawnIndex];
        
        GameObject deactiveObject = targetList.Find(x=>x.activeSelf == false);
        if (null == deactiveObject)
        {
            var clonedObject = Instantiate(targetObject, transform);
            clonedObject.SetActive(false);
            deactiveObject = clonedObject;
            targetList.Add(deactiveObject);
        }

        if (null != deactiveObject)
        {
            deactiveObject.transform.position = targetPoint.position;
            deactiveObject.SetActive(true);
        }
    }
}
