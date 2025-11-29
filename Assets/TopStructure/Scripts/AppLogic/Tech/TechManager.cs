using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 科技研究管理器
    public class TechManager : MonoBehaviour
    {
        [Header("科技树")]
        public TechTree techTree;

        [Header("研究设置")]
        public int maxResearchSlots = 1;
        public float baseResearchSpeed = 1f;

        private Dictionary<string, int> researchedTechs = new Dictionary<string, int>();
        private List<TechResearch> activeResearch = new List<TechResearch>();
        private Dictionary<TechEffectType, float> effectCache = new Dictionary<TechEffectType, float>();

        // 事件
        public event Action<string, int> OnTechResearched;
        public event Action<string> OnResearchStarted;
        public event Action<string> OnResearchCompleted;

        void Start()
        {
            if (techTree == null)
            {
                techTree = Resources.Load<TechTree>("TechTree");
            }

            if (techTree != null)
            {
                techTree.Initialize();
            }

            LoadTechData();
        }

        void Update()
        {
            UpdateActiveResearch();
        }

        #region 科技研究

        public bool CanResearchTech(string techId)
        {
            var node = techTree?.GetNode(techId);
            if (node == null)
                return false;

            // 检查是否已达最高等级
            if (node.isMaxLevel)
                return false;

            // 检查前置条件
            if (!node.canResearch)
                return false;

            // 检查资源
            int nextLevel = node.currentLevel + 1;
            var researchCosts = node.techData.researchCosts;
            
            return GameManager.Instance?.ResourceManager?.CanAfford(researchCosts) == true;
        }

        public bool StartResearch(string techId)
        {
            if (!CanResearchTech(techId))
                return false;

            var node = techTree.GetNode(techId);
            if (node == null)
                return false;

            // 检查研究槽位
            if (activeResearch.Count >= maxResearchSlots)
                return false;

            // 花费资源
            int nextLevel = node.currentLevel + 1;
            var researchCosts = node.techData.researchCosts;
            
            if (!GameManager.Instance?.ResourceManager?.SpendResources(researchCosts) == true)
                return false;

            // 开始研究
            TechResearch research = new TechResearch
            {
                techId = techId,
                targetLevel = nextLevel,
                researchTime = CalculateResearchTime(node.techData.researchTime),
                elapsedTime = 0f
            };

            activeResearch.Add(research);
            OnResearchStarted?.Invoke(techId);

            return true;
        }

        private float CalculateResearchTime(float baseTime)
        {
            float researchSpeed = baseResearchSpeed * (1 + GetTechEffect(TechEffectType.ResearchSpeed));
            return baseTime / researchSpeed;
        }

        private void UpdateActiveResearch()
        {
            for (int i = activeResearch.Count - 1; i >= 0; i--)
            {
                var research = activeResearch[i];
                research.elapsedTime += Time.deltaTime;

                if (research.elapsedTime >= research.researchTime)
                {
                    CompleteResearch(research);
                    activeResearch.RemoveAt(i);
                }
            }
        }

        private void CompleteResearch(TechResearch research)
        {
            // 更新科技等级
            researchedTechs[research.techId] = research.targetLevel;
            
            // 清除效果缓存
            ClearEffectCache();
            
            // 触发事件
            OnResearchCompleted?.Invoke(research.techId);
            OnTechResearched?.Invoke(research.techId, research.targetLevel);
            GameManager.Instance?.EventSystem?.OnTechResearched?.Invoke(research.techId, research.targetLevel);

            Debug.Log($"Tech {research.techId} researched to level {research.targetLevel}");
        }

        #endregion

        #region 科技效果

        public float GetTechEffect(TechEffectType effectType)
        {
            // 检查缓存
            if (effectCache.TryGetValue(effectType, out float cachedValue))
            {
                return cachedValue;
            }

            float totalEffect = 0f;

            // 计算所有科技的累计效果
            foreach (var pair in researchedTechs)
            {
                var node = techTree?.GetNode(pair.Key);
                if (node != null && node.isResearched)
                {
                    float effectValue = node.techData.GetEffectValue(effectType, pair.Value);
                    totalEffect += effectValue;
                }
            }

            // 缓存结果
            effectCache[effectType] = totalEffect;
            return totalEffect;
        }

        public int GetTechLevel(string techId)
        {
            researchedTechs.TryGetValue(techId, out int level);
            return level;
        }

        public bool IsTechResearched(string techId)
        {
            return GetTechLevel(techId) > 0;
        }

        public bool IsTechMaxLevel(string techId)
        {
            int currentLevel = GetTechLevel(techId);
            var node = techTree?.GetNode(techId);
            return node != null && currentLevel >= node.techData.maxLevel;
        }

        private void ClearEffectCache()
        {
            effectCache.Clear();
        }

        #endregion

        #region 查询方法

        public List<TechTreeNode> GetAvailableTechs()
        {
            List<TechTreeNode> available = new List<TechTreeNode>();
            
            if (techTree != null)
            {
                foreach (var node in techTree.techNodes.Values)
                {
                    if (CanResearchTech(node.techData.id))
                    {
                        available.Add(node);
                    }
                }
            }
            
            return available;
        }

        public List<TechTreeNode> GetResearchedTechs()
        {
            List<TechTreeNode> researched = new List<TechTreeNode>();
            
            if (techTree != null)
            {
                foreach (var pair in researchedTechs)
                {
                    var node = techTree.GetNode(pair.Key);
                    if (node != null && node.isResearched)
                    {
                        researched.Add(node);
                    }
                }
            }
            
            return researched;
        }

        public float GetResearchProgress(string techId)
        {
            var research = activeResearch.Find(r => r.techId == techId);
            if (research != null)
            {
                return research.elapsedTime / research.researchTime;
            }
            return 0f;
        }

        #endregion

        #region 数据持久化

        [System.Serializable]
        private class TechSaveData
        {
            public Dictionary<string, int> researchedTechs;
        }

        public void SaveTechData()
        {
            var saveData = new TechSaveData
            {
                researchedTechs = researchedTechs
            };
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("TechData", json);
            PlayerPrefs.Save();
        }

        public void LoadTechData()
        {
            if (PlayerPrefs.HasKey("TechData"))
            {
                string json = PlayerPrefs.GetString("TechData");
                var saveData = JsonUtility.FromJson<TechSaveData>(json);
                
                if (saveData != null && saveData.researchedTechs != null)
                {
                    researchedTechs = saveData.researchedTechs;
                }
            }
        }

        public void ResetTechData()
        {
            researchedTechs.Clear();
            activeResearch.Clear();
            ClearEffectCache();
        }

        #endregion

        #region 辅助类

        [System.Serializable]
        private class TechResearch
        {
            public string techId;
            public int targetLevel;
            public float researchTime;
            public float elapsedTime;
        }

        #endregion
    }
}