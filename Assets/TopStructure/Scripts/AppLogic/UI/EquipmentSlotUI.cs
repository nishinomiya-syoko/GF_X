using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 装备槽位UI
    public class EquipmentSlotUI : MonoBehaviour
    {
        [Header("UI元素")]
        public Image slotIcon;
        public Image equipmentIcon;
        public Text slotNameText;
        public Button button;

        public EquipmentType slotType { get; private set; }
        public PlayerEquipment equippedItem { get; private set; }

        public event Action<EquipmentSlotUI> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetSlotType(EquipmentType type)
        {
            slotType = type;
            if (slotNameText != null)
                slotNameText.text = type.ToString();
        }

        public void SetEquipment(PlayerEquipment equipment)
        {
            equippedItem = equipment;

            if (equipmentIcon != null)
            {
                if (equipment != null)
                {
                    var equipmentData = GameManager.Instance?.GetComponent<EquipmentManager>()?.equipmentDatabase?.GetEquipmentData(equipment.equipmentId);
                    if (equipmentData != null && equipmentData.icon != null)
                    {
                        equipmentIcon.sprite = equipmentData.icon;
                        equipmentIcon.gameObject.SetActive(true);
                    }
                    else
                    {
                        equipmentIcon.gameObject.SetActive(false);
                    }
                }
                else
                {
                    equipmentIcon.gameObject.SetActive(false);
                }
            }
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(this);
        }
    }
}