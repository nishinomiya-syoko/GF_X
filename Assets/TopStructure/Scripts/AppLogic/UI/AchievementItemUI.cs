using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 成就项UI
    public class AchievementItemUI : MonoBehaviour
    {
        [Header("UI元素")]
        public Text nameText;
        public Text pointsText;
        public Image iconImage;
        public Button button;

        private AchievementData achievementData;

        public event Action<AchievementData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetAchievementData(AchievementData data)
        {
            achievementData = data;

            if (nameText != null)
                nameText.text = data.achievementName;

            if (pointsText != null)
                pointsText.text = $"{data.points}点";

            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(achievementData);
        }
    }
}