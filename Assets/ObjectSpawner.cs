using System;
using System.Collections.Generic;
using CollectionExtension;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<GameObject> spawnObjects;

    [SerializeField] private int spawnCount = 1;

    private void Awake()
    {
        SpawnObject(0, 0);
    }

    public void SpawnObject(int spawnPointIndex = 0, int spawnObjectIndex = 0)
    {
        Transform targetPosition = null; 
        if (spawnPoints.IsInRange(spawnPointIndex))
        {
            targetPosition = spawnPoints[spawnPointIndex];
        }

        GameObject targetObject = null;
        if (spawnObjects.IsInRange(spawnObjectIndex))
        {
            targetObject = spawnObjects[spawnObjectIndex];
        }

        Spawn(targetPosition, targetObject);
    }

    private void Spawn(Transform targetPoint, GameObject targetObject)
    {
        if (null != targetPoint && null != targetObject)
        {
            var clonedObject = Instantiate(targetObject, transform);
            clonedObject.SetActive(false);
            clonedObject.transform.position = targetPoint.position;
            clonedObject.SetActive(true);
        }
    }
}
