using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 科技效果类型
    public enum TechEffectType
    {
        // 军事科技
        UnitDamage,
        UnitHealth,
        UnitSpeed,
        UnitTrainingSpeed,
        UnitCapacity,

        // 建筑科技
        BuildingHealth,
        BuildingDamage,
        BuildingBuildSpeed,
        BuildingUpgradeSpeed,

        // 资源科技
        ResourceProduction,
        ResourceCapacity,
        ResourceCollectionSpeed,

        // 防御科技
        DefenseDamage,
        DefenseRange,
        DefenseAccuracy,

        // 研究科技
        ResearchSpeed,
        ResearchCostReduction
    }

    // 科技类型
    public enum TechType
    {
        Military,
        Economic,
        Defense,
        Building,
        Research
    }

    // 科技数据
    [CreateAssetMenu(fileName = "TechData", menuName = "EmpireClash/Tech Data")]
    public class TechData : ScriptableObject
    {
        [Header("基本信息")]
        public string id;
        public string techName;
        public string description;
        public TechType techType;
        public Sprite icon;
        public int maxLevel = 5;

        [Header("研究信息")]
        public int researchTime = 3600; // 秒
        public ResourceCost[] researchCosts;
        public string[] prerequisiteTechs; // 前置科技

        [Header("科技效果")]
        public TechEffect[] effects;

        [Header("UI位置")]
        public Vector2 treePosition; // 在科技树中的位置

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
        }

        public int GetResearchCost(int level)
        {
            if (researchCosts != null && researchCosts.Length > 0)
            {
                return researchCosts[0].amount * (level + 1);
            }
            return 0;
        }

        public float GetEffectValue(TechEffectType effectType, int level)
        {
            foreach (var effect in effects)
            {
                if (effect.effectType == effectType)
                {
                    return effect.baseValue + (effect.perLevelIncrease * (level - 1));
                }
            }
            return 0f;
        }
    }

    // 科技效果
    [System.Serializable]
    public class TechEffect
    {
        public TechEffectType effectType;
        public float baseValue;
        public float perLevelIncrease;
        public EffectOperation operation = EffectOperation.Add;
    }

    // 效果操作类型
    public enum EffectOperation
    {
        Add,
        Multiply,
        Override
    }


}