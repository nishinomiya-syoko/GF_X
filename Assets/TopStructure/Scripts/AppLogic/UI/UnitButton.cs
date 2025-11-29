using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmpireClash
{
    // 单位按钮
    public class UnitButton : MonoBehaviour
    {
        [Header("UI元素")]
        public Image iconImage;
        public Text nameText;
        public Button button;

        private UnitData unitData;

        public event Action<UnitData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetUnitData(UnitData data)
        {
            unitData = data;
            
            if (nameText != null)
                nameText.text = data.unitName;
            
            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(unitData);
        }
    }
}