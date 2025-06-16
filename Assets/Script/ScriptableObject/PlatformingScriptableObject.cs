using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using ChracterDefine;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformingScriptableObject", menuName = "ScriptableObjects/PlatformingScriptableObject")]
public class PlatformingScriptableObject : ScriptableObject
{
    [Header("Obstacle")]
    [SerializeField] private SerializedDictionary<PlatformingObstacleType, GameObject> obstaclesColliderObjects; 
    [SerializeField] private SerializedDictionary<PlatformingObstacleType, List<GameObject>> obstacleUnits; 
}

