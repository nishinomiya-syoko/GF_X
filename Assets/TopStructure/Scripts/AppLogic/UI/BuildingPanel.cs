using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EmpireClash
{
    // 建筑面板
    public class BuildingPanel : UIPanel
    {
        [Header("建筑UI")]
        public Transform buildingListContainer;
        public GameObject buildingButtonPrefab;
        public Text buildingInfoText;
        public Button buildButton;
        public Button upgradeButton;
        public Button collectButton;

        private BuildingData selectedBuilding;
        private List<BuildingButton> buildingButtons = new List<BuildingButton>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshBuildingList();
        }

        void Start()
        {
            if (buildButton != null)
                buildButton.onClick.AddListener(OnBuildClicked);
            
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            
            if (collectButton != null)
                collectButton.onClick.AddListener(OnCollectClicked);
        }

        private void RefreshBuildingList()
        {
            // 清除现有按钮
            foreach (var button in buildingButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            buildingButtons.Clear();

            // 创建建筑按钮
            if (GameManager.Instance?.BuildingManager?.buildingDatabase != null)
            {
                foreach (var buildingData in GameManager.Instance.BuildingManager.buildingDatabase.buildings)
                {
                    CreateBuildingButton(buildingData);
                }
            }
        }

        private void CreateBuildingButton(BuildingData buildingData)
        {
            if (buildingButtonPrefab != null && buildingListContainer != null)
            {
                GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingListContainer);
                BuildingButton button = buttonObj.GetComponent<BuildingButton>();
                
                if (button != null)
                {
                    button.SetBuildingData(buildingData);
                    button.OnSelected += OnBuildingSelected;
                    buildingButtons.Add(button);
                }
            }
        }

        private void OnBuildingSelected(BuildingData buildingData)
        {
            selectedBuilding = buildingData;
            UpdateBuildingInfo();
        }

        private void UpdateBuildingInfo()
        {
            if (selectedBuilding == null || buildingInfoText == null)
                return;

            string info = $"名称: {selectedBuilding.buildingName}\n";
            info += $"描述: {selectedBuilding.description}\n";
            info += $"类型: {selectedBuilding.buildingType}\n";
            info += $"等级: 1/{selectedBuilding.maxLevel}\n";
            
            // 显示建造成本
            if (selectedBuilding.buildCosts != null && selectedBuilding.buildCosts.Length > 0)
            {
                info += "建造成本:\n";
                foreach (var cost in selectedBuilding.buildCosts)
                {
                    info += $"  {cost.resourceType}: {cost.amount}\n";
                }
            }

            buildingInfoText.text = info;
        }

        private void OnBuildClicked()
        {
            if (selectedBuilding != null)
            {
                GameManager.Instance?.BuildingManager?.StartBuildingPlacement(selectedBuilding);
                Hide();
            }
        }

        private void OnUpgradeClicked()
        {
            // 显示升级界面
            GameManager.Instance?.UIManager?.ShowUpgradePanel();
        }

        private void OnCollectClicked()
        {
            // 收集资源逻辑
            var resourceBuildings = GameManager.Instance?.BuildingManager?.GetBuildingsOfType(BuildingType.Resource);
            if (resourceBuildings != null)
            {
                foreach (var building in resourceBuildings)
                {
                    if (building is IResourceProducer producer)
                    {
                        int collected = producer.CollectProduction();
                        if (collected > 0)
                        {
                            GameManager.Instance?.ResourceManager?.AddResource(producer.ProductionType, collected);
                        }
                    }
                }
            }
        }
    }
}