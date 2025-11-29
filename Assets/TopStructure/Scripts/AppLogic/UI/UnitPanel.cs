using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmpireClash
{
    // 单位面板
    public class UnitPanel : UIPanel
    {
        [Header("单位UI")]
        public Transform unitListContainer;
        public GameObject unitButtonPrefab;
        public Text unitInfoText;
        public Button trainButton;
        public InputField trainCountInput;

        private UnitData selectedUnit;

        protected override void OnShow()
        {
            base.OnShow();
            RefreshUnitList();
        }

        void Start()
        {
            if (trainButton != null)
                trainButton.onClick.AddListener(OnTrainClicked);
            
            if (trainCountInput != null)
                trainCountInput.text = "1";
        }

        private void RefreshUnitList()
        {
            // 清除现有按钮
            foreach (Transform child in unitListContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建单位按钮
            if (GameManager.Instance?.UnitManager?.unitDatabase != null)
            {
                foreach (var unitData in GameManager.Instance.UnitManager.unitDatabase.units)
                {
                    CreateUnitButton(unitData);
                }
            }
        }

        private void CreateUnitButton(UnitData unitData)
        {
            if (unitButtonPrefab != null && unitListContainer != null)
            {
                GameObject buttonObj = Instantiate(unitButtonPrefab, unitListContainer);
                UnitButton button = buttonObj.GetComponent<UnitButton>();
                
                if (button != null)
                {
                    button.SetUnitData(unitData);
                    button.OnSelected += OnUnitSelected;
                }
            }
        }

        private void OnUnitSelected(UnitData unitData)
        {
            selectedUnit = unitData;
            UpdateUnitInfo();
        }

        private void UpdateUnitInfo()
        {
            if (selectedUnit == null || unitInfoText == null)
                return;

            string info = $"名称: {selectedUnit.unitName}\n";
            info += $"类型: {selectedUnit.unitType}\n";
            info += $"生命值: {selectedUnit.hitPoints}\n";
            info += $"攻击力: {selectedUnit.damage}\n";
            info += $"训练时间: {selectedUnit.trainingTime}秒\n";
            info += $"占用空间: {selectedUnit.housingSpace}\n";
            
            // 显示训练成本
            if (selectedUnit.trainingCosts != null && selectedUnit.trainingCosts.Length > 0)
            {
                info += "训练成本:\n";
                foreach (var cost in selectedUnit.trainingCosts)
                {
                    info += $"  {cost.resourceType}: {cost.amount}\n";
                }
            }

            unitInfoText.text = info;
        }

        private void OnTrainClicked()
        {
            if (selectedUnit != null)
            {
                int count = 1;
                if (trainCountInput != null && int.TryParse(trainCountInput.text, out int parsedCount))
                {
                    count = Mathf.Max(1, parsedCount);
                }

                GameManager.Instance?.UnitManager?.TrainUnit(selectedUnit.id, count);
            }
        }
    }
}