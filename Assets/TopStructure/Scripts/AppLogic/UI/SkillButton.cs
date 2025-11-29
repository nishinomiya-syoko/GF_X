using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 技能按钮
    public class SkillButton : MonoBehaviour
    {
        [Header("UI元素")]
        public Image iconImage;
        public Text nameText;
        public Image cooldownImage;
        public Text cooldownText;
        public Button button;

        private SkillInstance skillInstance;
        private SkillData skillData;

        public event Action<SkillData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetSkillInstance(SkillInstance instance)
        {
            skillInstance = instance;
            skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(instance.skillId);

            if (skillData != null)
            {
                if (nameText != null)
                    nameText.text = skillData.skillName;

                if (iconImage != null && skillData.icon != null)
                    iconImage.sprite = skillData.icon;
            }

            UpdateCooldown();
        }

        private void UpdateCooldown()
        {
            if (skillInstance != null)
            {
                float progress = GameManager.Instance?.SkillManager?.GetSkillCooldownProgress(skillInstance.skillId) ?? 1f;
                if (cooldownImage != null)
                    cooldownImage.fillAmount = 1f - progress;

                float remaining = GameManager.Instance?.SkillManager?.GetSkillCooldownRemaining(skillInstance.skillId) ?? 0f;
                if (cooldownText != null)
                    cooldownText.text = remaining > 0 ? $"{remaining:F0}" : "";
            }
        }

        private void OnClicked()
        {
            if (skillData != null)
            {
                OnSelected?.Invoke(skillData);
            }
        }

        void Update()
        {
            UpdateCooldown();
        }
    }
}