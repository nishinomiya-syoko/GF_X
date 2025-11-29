using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 装备面板
    public class EquipmentPanel : UIPanel
    {
        [Header("装备UI")]
        public Transform equipmentSlotsContainer;
        public Transform inventoryContainer;
        public GameObject equipmentSlotPrefab;
        public GameObject inventoryItemPrefab;
        public Text equipmentInfoText;
        public Button equipButton;
        public Button unequipButton;
        public Button enhanceButton;

        private PlayerEquipment selectedEquipment;
        private List<EquipmentSlotUI> equipmentSlots = new List<EquipmentSlotUI>();
        private List<InventoryItemUI> inventoryItems = new List<InventoryItemUI>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshEquipmentPanel();
        }

        void Start()
        {
            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquipClicked);

            if (unequipButton != null)
                unequipButton.onClick.AddListener(OnUnequipClicked);

            if (enhanceButton != null)
                enhanceButton.onClick.AddListener(OnEnhanceClicked);

            CreateEquipmentSlots();
        }

        private void CreateEquipmentSlots()
        {
            // 创建装备槽位
            EquipmentType[] slotTypes = { EquipmentType.Weapon, EquipmentType.Armor,
                                        EquipmentType.Helmet, EquipmentType.Boots, EquipmentType.Accessory };

            foreach (var slotType in slotTypes)
            {
                if (equipmentSlotPrefab != null && equipmentSlotsContainer != null)
                {
                    GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                    EquipmentSlotUI slot = slotObj.GetComponent<EquipmentSlotUI>();

                    if (slot != null)
                    {
                        slot.SetSlotType(slotType);
                        slot.OnSelected += OnEquipmentSlotSelected;
                        equipmentSlots.Add(slot);
                    }
                }
            }
        }

        private void RefreshEquipmentPanel()
        {
            RefreshEquipmentSlots();
            RefreshInventory();
        }

        private void RefreshEquipmentSlots()
        {
            var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
            if (equipmentManager == null)
                return;

            var equippedItems = equipmentManager.GetEquippedItems();

            foreach (var slot in equipmentSlots)
            {
                var equippedItem = equippedItems.Find(e =>
                {
                    var equipmentData = GameManager.Instance?.GetComponent<EquipmentManager>()?.equipmentDatabase?.GetEquipmentData(e.equipmentId);
                    return equipmentData != null && equipmentData.equipmentType == slot.slotType;
                });

                slot.SetEquipment(equippedItem);
            }
        }

        private void RefreshInventory()
        {
            // 清除现有物品
            foreach (var item in inventoryItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            inventoryItems.Clear();

            // 创建背包物品
            var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
            if (equipmentManager == null)
                return;

            var inventory = equipmentManager.GetInventory();
            foreach (var equipment in inventory)
            {
                if (inventoryItemPrefab != null && inventoryContainer != null)
                {
                    GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryContainer);
                    InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();

                    if (itemUI != null)
                    {
                        itemUI.SetEquipment(equipment);
                        itemUI.OnSelected += OnInventoryItemSelected;
                        inventoryItems.Add(itemUI);
                    }
                }
            }
        }

        private void OnEquipmentSlotSelected(EquipmentSlotUI slot)
        {
            if (slot.equippedItem != null)
            {
                selectedEquipment = slot.equippedItem;
                UpdateEquipmentInfo();
            }
        }

        private void OnInventoryItemSelected(InventoryItemUI item)
        {
            selectedEquipment = item.equipment;
            UpdateEquipmentInfo();
        }

        private void UpdateEquipmentInfo()
        {
            if (selectedEquipment == null || equipmentInfoText == null)
                return;

            var equipmentData = GameManager.Instance?.GetComponent<EquipmentManager>()?.equipmentDatabase?.GetEquipmentData(selectedEquipment.equipmentId);
            if (equipmentData == null)
                return;

            string info = $"装备名称: {equipmentData.equipmentName}\n";
            info += $"装备类型: {equipmentData.equipmentType}\n";
            info += $"装备品质: {equipmentData.quality}\n";
            info += $"装备等级: {selectedEquipment.level}\n";
            info += $"强化等级: +{selectedEquipment.enhancementLevel}\n";
            info += $"需要等级: {equipmentData.requiredLevel}\n";

            // 显示属性
            info += "\n基础属性:\n";
            if (equipmentData.attack > 0) info += $"攻击力: +{equipmentData.attack}\n";
            if (equipmentData.defense > 0) info += $"防御力: +{equipmentData.defense}\n";
            if (equipmentData.health > 0) info += $"生命值: +{equipmentData.health}\n";
            if (equipmentData.attackSpeed > 0) info += $"攻击速度: +{equipmentData.attackSpeed:P0}\n";
            if (equipmentData.moveSpeed > 0) info += $"移动速度: +{equipmentData.moveSpeed:P0}\n";

            // 显示附加技能
            if (selectedEquipment.attachedSkills != null && selectedEquipment.attachedSkills.Length > 0)
            {
                info += "\n附加技能:\n";
                foreach (var skillId in selectedEquipment.attachedSkills)
                {
                    var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
                    if (skillData != null)
                    {
                        info += $"- {skillData.skillName}\n";
                    }
                }
            }

            equipmentInfoText.text = info;

            // 更新按钮状态
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (selectedEquipment == null)
                return;

            var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
            if (equipmentManager == null)
                return;

            bool isEquipped = equipmentManager.IsEquipped(selectedEquipment.equipmentId);

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(!isEquipped);
                equipButton.interactable = !isEquipped;
            }

            if (unequipButton != null)
            {
                unequipButton.gameObject.SetActive(isEquipped);
                unequipButton.interactable = isEquipped;
            }

            if (enhanceButton != null)
            {
                enhanceButton.interactable = true;
            }
        }

        private void OnEquipClicked()
        {
            if (selectedEquipment != null)
            {
                var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
                if (equipmentManager != null)
                {
                    if (equipmentManager.EquipItem(selectedEquipment.equipmentId))
                    {
                        RefreshEquipmentPanel();
                        UpdateEquipmentInfo();
                    }
                }
            }
        }

        private void OnUnequipClicked()
        {
            if (selectedEquipment != null)
            {
                var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
                if (equipmentManager != null)
                {
                    if (equipmentManager.UnequipItem(selectedEquipment.equipmentId))
                    {
                        RefreshEquipmentPanel();
                        UpdateEquipmentInfo();
                    }
                }
            }
        }

        private void OnEnhanceClicked()
        {
            if (selectedEquipment != null)
            {
                var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
                if (equipmentManager != null)
                {
                    if (equipmentManager.EnhanceEquipment(selectedEquipment.equipmentId))
                    {
                        UpdateEquipmentInfo();
                    }
                }
            }
        }
    }
}