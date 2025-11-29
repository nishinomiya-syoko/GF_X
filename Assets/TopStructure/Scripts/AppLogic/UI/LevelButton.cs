using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmpireClash
{
    // 关卡按钮
    public class LevelButton : MonoBehaviour
    {
        [Header("UI元素")]
        public Image thumbnailImage;
        public Text nameText;
        public Text starsText;
        public Button button;
        public GameObject lockIcon;

        private LevelData levelData;

        public event Action<LevelData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetLevelData(LevelData data)
        {
            levelData = data;
            
            if (nameText != null)
                nameText.text = data.levelName;
            
            if (thumbnailImage != null && data.thumbnail != null)
                thumbnailImage.sprite = data.thumbnail;

            // 更新星级显示
            if (starsText != null)
            {
                int stars = GameManager.Instance?.LevelManager?.GetLevelStars(data.levelId) ?? 0;
                starsText.text = $"⭐ {stars}";
            }

            // 更新锁定状态
            bool isLocked = !data.IsUnlocked();
            if (lockIcon != null)
                lockIcon.SetActive(isLocked);
            
            if (button != null)
                button.interactable = !isLocked;
        }

        private void OnClicked()
        {
            if (levelData.IsUnlocked())
            {
                OnSelected?.Invoke(levelData);
            }
        }
    }
}