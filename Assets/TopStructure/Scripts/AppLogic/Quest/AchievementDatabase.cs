using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 成就数据库
    [CreateAssetMenu(fileName = "AchievementDatabase", menuName = "EmpireClash/Achievement Database")]
    public class AchievementDatabase : ScriptableObject
    {
        public List<AchievementData> achievements = new List<AchievementData>();

        public AchievementData GetAchievementData(string achievementId)
        {
            return achievements.Find(a => a.id == achievementId);
        }
    }
}