using System;
using UnityEngine;

namespace ChracterDefine
{
    public enum JobType
    {
        Default,
        warrior,
        hunter,
    }

    [Serializable]
    public class CharacterStat
    {
        #region 기본 스펙
        //체력
        [SerializeField]private int Health = 0;
        //액션 수행 재화
        [SerializeField]private int Stamina = 0;
        #endregion
        
        //물리 데미지 및 가드 수치
        [SerializeField]private int Strength = 0;
        //캐릭터 액션 속도
        [SerializeField]private int Speed = 0;

        public void UpdateStat(int level, CharacterStat defaultStat, CharacterIncreaseStats increaseStats)
        {
            Health = defaultStat.Health + (int)increaseStats.GetIncreaseHealth(level);
            Stamina = defaultStat.Stamina + (int)increaseStats.GetIncreaseStamina(level);
            Strength = defaultStat.Strength + (int)increaseStats.GetIncreaseStrengh(level);
            Speed = defaultStat.Speed + (int)increaseStats.GetIncreaseSpeed(level);
        }
    }

    [Serializable]
    public class CharacterIncreaseStats
    {
        //체력
        [SerializeField] public float IncreaseHealth { get; } = 0;
        //액션 수행 재화
        [SerializeField]public float IncreaseStamina { get; } = 0;
        
        //물리 데미지 및 가드 수치
        [SerializeField]public float IncreaseStrength { get; } = 0;
        //캐릭터 액션 속도
        [SerializeField]public float IncreaseSpeed { get; } = 0;

        public float GetIncreaseHealth(int curLevel)
        {
            return curLevel * IncreaseHealth;
        }
        
        public float GetIncreaseStamina(int curLevel)
        {
            return curLevel * IncreaseStamina;
        }
        public float GetIncreaseStrengh(int curLevel)
        {
            return curLevel * IncreaseStrength;
        }
        public float GetIncreaseSpeed(int curLevel)
        {
            return curLevel * IncreaseSpeed;
        }
    }

    public class UserCharacterData
    {
        public int level = 0;
        public int experience = 0;
        public JobType curJob = JobType.Default;
        public CharacterStat defaultStat;
        public CharacterIncreaseStats increaseStats;

        public CharacterStat currentStat;

        //초기화, 레벨업 타이밍마다 호출
        public void CalculateCharacterStat()
        {
            currentStat ??= new CharacterStat();

            currentStat.UpdateStat(level, defaultStat, increaseStats);
        }
    }
}