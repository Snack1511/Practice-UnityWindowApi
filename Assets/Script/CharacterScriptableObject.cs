using AYellowpaper.SerializedCollections;
using ChracterDefine;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterScriptableObject", menuName = "Scriptable Objects/CharacterScriptableObject")]
public class CharacterScriptableObject : ScriptableObject
{
    [SerializeField] private SerializedDictionary<JobType, CharacterStat> DefaultStats; 
    [SerializeField] private SerializedDictionary<JobType, CharacterIncreaseStats> IcharacterIncreaseStats; 
}
