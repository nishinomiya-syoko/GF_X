using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmpireClash
{
    // 建筑按钮
    public class BuildingButton : MonoBehaviour
    {
        [Header("UI元素")]
        public Image iconImage;
        public Text nameText;
        public Button button;

        private BuildingData buildingData;

        public event Action<BuildingData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetBuildingData(BuildingData data)
        {
            buildingData = data;
            
            if (nameText != null)
                nameText.text = data.buildingName;
            
            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(buildingData);
        }
    }
}