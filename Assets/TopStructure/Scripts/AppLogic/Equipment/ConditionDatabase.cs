using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 技能数据库
    [CreateAssetMenu(fileName = "ConditionDatabase", menuName = "EmpireClash/Condition Database")]
    public class ConditionDatabase : ScriptableObject
    {
        public List<ConditionData> conditions = new List<ConditionData>();

        public ConditionData GetConditionData(string conditionId)
        {
            return conditions.Find(s => s.id == conditionId);
        }

    }
}