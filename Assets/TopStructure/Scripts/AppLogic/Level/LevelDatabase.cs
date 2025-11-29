using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 关卡数据库
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "EmpireClash/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> levels = new List<LevelData>();

        public LevelData GetLevelData(int levelId)
        {
            return levels.Find(l => l.levelId == levelId);
        }

        public List<LevelData> GetLevelsByType(LevelType type)
        {
            return levels.FindAll(l => l.levelType == type);
        }
    }
}