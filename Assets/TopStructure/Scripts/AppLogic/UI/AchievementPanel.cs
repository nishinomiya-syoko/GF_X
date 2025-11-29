using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 成就面板
    public class AchievementPanel : UIPanel
    {
        [Header("成就UI")]
        public Transform achievementListContainer;
        public GameObject achievementItemPrefab;
        public Text achievementInfoText;

        private List<AchievementItemUI> achievementItems = new List<AchievementItemUI>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshAchievementList();
        }

        private void RefreshAchievementList()
        {
            // 清除现有成就项
            foreach (var item in achievementItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            achievementItems.Clear();

            // 获取成就数据
            var achievementDatabase = GameManager.Instance?.GetComponent<QuestManager>()?.achievementDatabase;
            if (achievementDatabase != null)
            {
                foreach (var achievement in achievementDatabase.achievements)
                {
                    CreateAchievementItem(achievement);
                }
            }
        }

        private void CreateAchievementItem(AchievementData achievementData)
        {
            if (achievementItemPrefab != null && achievementListContainer != null)
            {
                GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContainer);
                AchievementItemUI achievementItem = itemObj.GetComponent<AchievementItemUI>();

                if (achievementItem != null)
                {
                    achievementItem.SetAchievementData(achievementData);
                    achievementItem.OnSelected += OnAchievementSelected;
                    achievementItems.Add(achievementItem);
                }
            }
        }

        private void OnAchievementSelected(AchievementData achievementData)
        {
            if (achievementInfoText != null)
            {
                string info = $"成就名称: {achievementData.achievementName}\n";
                info += $"描述: {achievementData.description}\n";
                info += $"成就点数: {achievementData.points}\n";
                info += $"解锁条件: {achievementData.unlockType} x{achievementData.requiredAmount}\n";

                if (!string.IsNullOrEmpty(achievementData.unlockTitle))
                    info += $"解锁称号: {achievementData.unlockTitle}\n";

                achievementInfoText.text = info;
            }
        }
    }
}