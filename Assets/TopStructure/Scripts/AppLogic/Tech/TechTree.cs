using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 科技树节点
    [System.Serializable]
    public class TechTreeNode
    {
        public TechData techData;
        public int currentLevel = 0;
        public bool isResearched => currentLevel > 0;
        public bool isMaxLevel => currentLevel >= techData.maxLevel;
        public bool canResearch => CheckPrerequisites();

        public bool CheckPrerequisites()
        {
            if (techData.prerequisiteTechs == null || techData.prerequisiteTechs.Length == 0)
                return true;

            var techManager = GameManager.Instance?.TechManager;
            if (techManager == null)
                return false;

            foreach (var prereqId in techData.prerequisiteTechs)
            {
                if (!techManager.IsTechResearched(prereqId))
                {
                    return false;
                }
            }
            return true;
        }
    }

    // 科技树
    [CreateAssetMenu(fileName = "TechTree", menuName = "EmpireClash/Tech Tree")]
    public class TechTree : ScriptableObject
    {
        public List<TechData> availableTechs = new List<TechData>();
        public Dictionary<string, TechTreeNode> techNodes = new Dictionary<string, TechTreeNode>();

        public void Initialize()
        {
            techNodes.Clear();
            foreach (var techData in availableTechs)
            {
                techNodes[techData.id] = new TechTreeNode { techData = techData };
            }
        }

        public TechTreeNode GetNode(string techId)
        {
            techNodes.TryGetValue(techId, out TechTreeNode node);
            return node;
        }

        public List<TechTreeNode> GetNodesByType(TechType type)
        {
            List<TechTreeNode> nodes = new List<TechTreeNode>();
            foreach (var node in techNodes.Values)
            {
                if (node.techData.techType == type)
                {
                    nodes.Add(node);
                }
            }
            return nodes;
        }
    }
}