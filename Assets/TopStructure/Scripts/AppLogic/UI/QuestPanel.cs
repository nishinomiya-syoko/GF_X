using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 任务面板
    public class QuestPanel : UIPanel
    {
        [Header("任务UI")]
        public Transform questListContainer;
        public GameObject questItemPrefab;
        public Text questDetailsText;
        public Button acceptButton;
        public Button claimButton;
        public Toggle showDailyToggle;
        public Toggle showMainToggle;
        public Toggle showCompletedToggle;

        private QuestData selectedQuest;
        private List<QuestItemUI> questItems = new List<QuestItemUI>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshQuestList();
        }

        void Start()
        {
            if (acceptButton != null)
                acceptButton.onClick.AddListener(OnAcceptClicked);
            
            if (claimButton != null)
                claimButton.onClick.AddListener(OnClaimClicked);
            
            if (showDailyToggle != null)
                showDailyToggle.onValueChanged.AddListener(OnFilterChanged);
            
            if (showMainToggle != null)
                showMainToggle.onValueChanged.AddListener(OnFilterChanged);
            
            if (showCompletedToggle != null)
                showCompletedToggle.onValueChanged.AddListener(OnFilterChanged);
        }

        private void RefreshQuestList()
        {
            // 清除现有任务项
            foreach (var item in questItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            questItems.Clear();

            // 获取任务管理器
            var questManager = GameManager.Instance?.QuestManager;
            if (questManager == null)
                return;

            // 获取可用任务
            var availableQuests = questManager.GetAvailableQuests();
            // var availableQuests = questManager.GetActiveQuests();
            foreach (var quest in availableQuests)
            {
                if (ShouldShowQuest(quest))
                {
                    CreateQuestItem(quest);
                }
            }
        }

        private bool ShouldShowQuest(QuestData quest)
        {
            if (showDailyToggle != null && !showDailyToggle.isOn && quest.questType == QuestType.Daily)
                return false;
            
            if (showMainToggle != null && !showMainToggle.isOn && quest.questType == QuestType.MainStory)
                return false;
            
            if (showCompletedToggle != null && !showCompletedToggle.isOn && 
                GameManager.Instance?.GetComponent<QuestManager>()?.IsQuestCompleted(quest.id) == true)
                return false;

            return true;
        }

        private void CreateQuestItem(QuestData questData)
        {
            if (questItemPrefab != null && questListContainer != null)
            {
                GameObject itemObj = Instantiate(questItemPrefab, questListContainer);
                QuestItemUI questItem = itemObj.GetComponent<QuestItemUI>();
                
                if (questItem != null)
                {
                    questItem.SetQuestData(questData);
                    questItem.OnSelected += OnQuestSelected;
                    questItems.Add(questItem);
                }
            }
        }

        private void OnQuestSelected(QuestData questData)
        {
            selectedQuest = questData;
            UpdateQuestDetails();
        }

        private void UpdateQuestDetails()
        {
            if (selectedQuest == null || questDetailsText == null)
                return;

            string details = $"任务名称: {selectedQuest.questName}\n";
            details += $"任务类型: {selectedQuest.questType}\n";
            details += $"描述: {selectedQuest.description}\n";
            details += $"优先级: {selectedQuest.priority}\n";

            // 显示任务目标
            if (selectedQuest.objectives != null && selectedQuest.objectives.Length > 0)
            {
                details += "\n任务目标:\n";
                foreach (var obj in selectedQuest.objectives)
                {
                    details += $"- {obj.GetProgressText()}\n";
                }
            }

            // 显示奖励
            if (selectedQuest.rewards != null)
            {
                details += "\n任务奖励:\n";
                if (selectedQuest.rewards.resources != null)
                {
                    foreach (var reward in selectedQuest.rewards.resources)
                    {
                        details += $"- {reward.resourceType}: {reward.amount}\n";
                    }
                }
                if (selectedQuest.rewards.experience > 0)
                {
                    details += $"- 经验: {selectedQuest.rewards.experience}\n";
                }
            }

            questDetailsText.text = details;

            // 更新按钮状态
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (selectedQuest == null)
                return;

            var questManager = GameManager.Instance?.GetComponent<QuestManager>();
            if (questManager == null)
                return;

            var playerQuest = questManager.GetQuestData(selectedQuest.id);

            // 接受按钮
            if (acceptButton != null)
            {
                bool canAccept = playerQuest == null || playerQuest.state == QuestState.Available;
                acceptButton.gameObject.SetActive(canAccept);
                acceptButton.interactable = canAccept;
            }

            // 领取奖励按钮
            if (claimButton != null)
            {
                bool canClaim = playerQuest != null && playerQuest.state == QuestState.Completed;
                claimButton.gameObject.SetActive(canClaim);
                claimButton.interactable = canClaim;
            }
        }

        private void OnAcceptClicked()
        {
            if (selectedQuest != null)
            {
                var questManager = GameManager.Instance?.GetComponent<QuestManager>();
                if (questManager != null)
                {
                    if (questManager.AcceptQuest(selectedQuest.id))
                    {
                        RefreshQuestList();
                        UpdateQuestDetails();
                    }
                }
            }
        }

        private void OnClaimClicked()
        {
            if (selectedQuest != null)
            {
                var questManager = GameManager.Instance?.GetComponent<QuestManager>();
                if (questManager != null)
                {
                    if (questManager.ClaimQuestReward(selectedQuest.id))
                    {
                        RefreshQuestList();
                        UpdateQuestDetails();
                    }
                }
            }
        }

        private void OnFilterChanged(bool value)
        {
            RefreshQuestList();
        }
    }
}