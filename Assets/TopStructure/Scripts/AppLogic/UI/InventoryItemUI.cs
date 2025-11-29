using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 背包物品UI
    public class InventoryItemUI : MonoBehaviour
    {
        [Header("UI元素")]
        public Image iconImage;
        public Text nameText;
        public Image qualityBackground;
        public Button button;

        public PlayerEquipment equipment { get; private set; }

        public event Action<InventoryItemUI> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetEquipment(PlayerEquipment equipment)
        {
            this.equipment = equipment;

            var equipmentData = GameManager.Instance?.GetComponent<EquipmentManager>()?.equipmentDatabase?.GetEquipmentData(equipment.equipmentId);
            if (equipmentData != null)
            {
                if (nameText != null)
                    nameText.text = equipmentData.equipmentName;

                if (iconImage != null && equipmentData.icon != null)
                    iconImage.sprite = equipmentData.icon;

                if (qualityBackground != null)
                    qualityBackground.color = equipmentData.GetQualityColor();
            }
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(this);
        }
    }
}