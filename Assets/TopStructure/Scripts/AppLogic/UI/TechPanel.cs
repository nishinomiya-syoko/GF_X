using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EmpireClash
{
    // 科技树面板
    public class TechPanel : UIPanel
    {
        [Header("科技树UI")]
        public Transform techTreeContainer;
        public GameObject techNodePrefab;
        public Text techInfoText;
        public Button researchButton;

        private TechData selectedTech;
        private List<TechNodeUI> techNodes = new List<TechNodeUI>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshTechTree();
        }

        void Start()
        {
            if (researchButton != null)
                researchButton.onClick.AddListener(OnResearchClicked);
        }

        private void RefreshTechTree()
        {
            // 清除现有节点
            foreach (var node in techNodes)
            {
                if (node != null)
                    Destroy(node.gameObject);
            }
            techNodes.Clear();

            // 创建科技节点
            if (GameManager.Instance?.TechManager?.techTree != null)
            {
                foreach (var techData in GameManager.Instance.TechManager.techTree.availableTechs)
                {
                    CreateTechNode(techData);
                }
            }
        }

        private void CreateTechNode(TechData techData)
        {
            if (techNodePrefab != null && techTreeContainer != null)
            {
                GameObject nodeObj = Instantiate(techNodePrefab, techTreeContainer);
                TechNodeUI node = nodeObj.GetComponent<TechNodeUI>();
                
                if (node != null)
                {
                    node.SetTechData(techData);
                    node.OnSelected += OnTechSelected;
                    techNodes.Add(node);
                }
            }
        }

        private void OnTechSelected(TechData techData)
        {
            selectedTech = techData;
            UpdateTechInfo();
        }

        private void UpdateTechInfo()
        {
            if (selectedTech == null || techInfoText == null)
                return;

            int currentLevel = GameManager.Instance?.TechManager?.GetTechLevel(selectedTech.id) ?? 0;
            
            string info = $"名称: {selectedTech.techName}\n";
            info += $"描述: {selectedTech.description}\n";
            info += $"类型: {selectedTech.techType}\n";
            info += $"等级: {currentLevel}/{selectedTech.maxLevel}\n";
            
            // 显示研究成本
            if (selectedTech.researchCosts != null && selectedTech.researchCosts.Length > 0)
            {
                info += "研究成本:\n";
                foreach (var cost in selectedTech.researchCosts)
                {
                    info += $"  {cost.resourceType}: {cost.amount}\n";
                }
            }

            techInfoText.text = info;

            // 更新研究按钮
            if (researchButton != null)
            {
                bool canResearch = GameManager.Instance?.TechManager?.CanResearchTech(selectedTech.id) == true;
                researchButton.interactable = canResearch;
                researchButton.GetComponentInChildren<Text>().text = canResearch ? "研究" : "无法研究";
            }
        }

        private void OnResearchClicked()
        {
            if (selectedTech != null)
            {
                GameManager.Instance?.TechManager?.StartResearch(selectedTech.id);
                UpdateTechInfo();
            }
        }
    }
}