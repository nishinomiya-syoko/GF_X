using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 任务项UI
    public class QuestItemUI : MonoBehaviour
    {
        [Header("UI元素")]
        public Text nameText;
        public Text typeText;
        public Image iconImage;
        public Image progressImage;
        public Text progressText;
        public Button button;

        private QuestData questData;

        public event Action<QuestData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetQuestData(QuestData data)
        {
            questData = data;

            if (nameText != null)
                nameText.text = data.questName;

            if (typeText != null)
                typeText.text = data.questType.ToString();

            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (questData == null)
                return;

            // 获取玩家任务数据
            var questManager = GameManager.Instance?.GetComponent<QuestManager>();
            if (questManager != null)
            {
                var playerQuest = questManager.GetQuestData(questData.id);
                if (playerQuest != null)
                {
                    float progress = questData.GetProgress();
                    if (progressImage != null)
                        progressImage.fillAmount = progress;

                    if (progressText != null)
                        progressText.text = $"{progress:P0}";
                }
            }
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(questData);
        }
    }
}