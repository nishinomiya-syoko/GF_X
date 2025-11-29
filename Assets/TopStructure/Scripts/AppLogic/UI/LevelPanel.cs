using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EmpireClash
{
    // 关卡选择面板
    public class LevelPanel : UIPanel
    {
        [Header("关卡UI")]
        public Transform levelListContainer;
        public GameObject levelButtonPrefab;
        public Text levelInfoText;

        private List<LevelButton> levelButtons = new List<LevelButton>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshLevelList();
        }

        private void RefreshLevelList()
        {
            // 清除现有按钮
            foreach (var button in levelButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            levelButtons.Clear();

            // 创建关卡按钮
            if (GameManager.Instance?.LevelManager?.levelDatabase != null)
            {
                foreach (var levelData in GameManager.Instance.LevelManager.levelDatabase.levels)
                {
                    CreateLevelButton(levelData);
                }
            }
        }

        private void CreateLevelButton(LevelData levelData)
        {
            if (levelButtonPrefab != null && levelListContainer != null)
            {
                GameObject buttonObj = Instantiate(levelButtonPrefab, levelListContainer);
                LevelButton button = buttonObj.GetComponent<LevelButton>();
                
                if (button != null)
                {
                    button.SetLevelData(levelData);
                    button.OnSelected += OnLevelSelected;
                    levelButtons.Add(button);
                }
            }
        }

        private void OnLevelSelected(LevelData levelData)
        {
            // 开始关卡
            GameManager.Instance?.LevelManager?.StartLevel(levelData.levelId);
            Hide();
        }
    }
}