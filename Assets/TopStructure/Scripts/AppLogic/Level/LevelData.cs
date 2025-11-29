using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 关卡类型
    public enum LevelType
    {
        Attack,
        Defense
    }

    // 关卡状态
    public enum LevelState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Paused
    }

    // 波次数据
    [System.Serializable]
    public class WaveData
    {
        public float startDelay = 10f; // 波次开始延迟
        public List<UnitSpawnData> unitsToSpawn = new List<UnitSpawnData>();
        public int waveReward = 100;
    }

    // 单位生成数据
    [System.Serializable]
    public class UnitSpawnData
    {
        public string unitId;
        public int count = 1;
        public Vector3 spawnPosition;
        public float spawnDelay = 0.5f; // 单位之间的生成延迟
    }

    // 敌方基地数据
    [System.Serializable]
    public class EnemyBaseData
    {
        public string baseName;
        public List<BuildingData> buildings = new List<BuildingData>();
        public List<Vector3> buildingPositions = new List<Vector3>();
        public Vector3 baseCenter;
        public int baseReward = 500;
    }

    // 奖励数据
    [System.Serializable]
    public class RewardData
    {
        public int goldReward;
        public int elixirReward;
        public int darkElixirReward;
        public int gemReward;
        public int experienceReward;
        public string[] unlockContent; // 解锁的内容ID
    }

    // 关卡数据
    [CreateAssetMenu(fileName = "LevelData", menuName = "EmpireClash/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("基本信息")]
        public int levelId;
        public string levelName;
        public string description;
        public LevelType levelType;
        public Sprite thumbnail;
        public int requiredLevel = 1;
        public bool isUnlocked = false;

        [Header("关卡内容")]
        public WaveData[] waves; // 防守关卡的波次
        public EnemyBaseData enemyBase; // 进攻关卡的敌方基地

        [Header("奖励")]
        public RewardData rewards;

        [Header("场景设置")]
        public int sceneIndex = 0; // 使用的场景索引
        public Vector3 cameraPosition = new Vector3(0, 20, 0);
        public Vector3 playerSpawnPosition = Vector3.zero;

        [Header("评级标准")]
        public float threeStarTime = 180f; // 3星时间限制
        public float twoStarTime = 300f; // 2星时间限制
        public int maxUnitsLost = 10; // 最大损失单位数

        public bool IsUnlocked()
        {
            return isUnlocked || GameManager.Instance?.LevelManager?.playerLevel >= requiredLevel;
        }

        public int GetStarRating(float completionTime, int unitsLost)
        {
            if (completionTime <= threeStarTime && unitsLost <= maxUnitsLost)
                return 3;
            else if (completionTime <= twoStarTime && unitsLost <= maxUnitsLost * 2)
                return 2;
            else
                return 1;
        }
    }

    
}