using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace EmpireClash
{
    // 技能面板
    public class SkillPanel : UIPanel
    {
        [Header("技能UI")]
        public Transform skillListContainer;
        public GameObject skillButtonPrefab;
        public Text skillInfoText;
        public Button useSkillButton;
        public Image cooldownImage;
        public Text cooldownText;

        private SkillData selectedSkill;
        private List<SkillButton> skillButtons = new List<SkillButton>();

        protected override void OnShow()
        {
            base.OnShow();
            RefreshSkillList();
        }

        void Start()
        {
            if (useSkillButton != null)
                useSkillButton.onClick.AddListener(OnUseSkillClicked);
        }

        private void RefreshSkillList()
        {
            // 清除现有技能按钮
            foreach (var button in skillButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            skillButtons.Clear();

            // 获取当前单位的技能
            var selectedUnit = GetSelectedUnit();
            if (selectedUnit != null)
            {
                foreach (var skill in selectedUnit.GetAvailableSkills())
                {
                    CreateSkillButton(skill);
                }
            }
        }

        private void CreateSkillButton(SkillInstance skillInstance)
        {
            if (skillButtonPrefab != null && skillListContainer != null)
            {
                GameObject buttonObj = Instantiate(skillButtonPrefab, skillListContainer);
                SkillButton skillButton = buttonObj.GetComponent<SkillButton>();

                if (skillButton != null)
                {
                    skillButton.SetSkillInstance(skillInstance);
                    skillButton.OnSelected += OnSkillSelected;
                    skillButtons.Add(skillButton);
                }
            }
        }

        private void OnSkillSelected(SkillData skillData)
        {
            selectedSkill = skillData;
            UpdateSkillInfo();
        }

        private void UpdateSkillInfo()
        {
            if (selectedSkill == null || skillInfoText == null)
                return;

            string info = $"技能名称: {selectedSkill.skillName}\n";
            info += $"技能类型: {selectedSkill.skillType}\n";
            info += $"目标类型: {selectedSkill.targetType}\n";
            info += $"技能描述: {selectedSkill.description}\n";
            info += $"冷却时间: {selectedSkill.cooldown}秒\n";
            info += $"施法时间: {selectedSkill.castTime}秒\n";
            info += $"技能范围: {selectedSkill.range}\n";
            info += $"魔法消耗: {selectedSkill.manaCost}\n";

            skillInfoText.text = info;

            // 更新冷却显示
            UpdateCooldownDisplay();
        }

        private void UpdateCooldownDisplay()
        {
            if (selectedSkill == null)
                return;

            var selectedUnit = GetSelectedUnit();
            if (selectedUnit != null)
            {
                var skillInstance = selectedUnit.GetSkill(selectedSkill.id);
                if (skillInstance != null)
                {
                    float cooldownRemaining = GameManager.Instance?.SkillManager?.GetSkillCooldownRemaining(selectedSkill.id) ?? 0f;
                    float cooldownProgress = GameManager.Instance?.SkillManager?.GetSkillCooldownProgress(selectedSkill.id) ?? 1f;

                    if (cooldownImage != null)
                        cooldownImage.fillAmount = cooldownProgress;

                    if (cooldownText != null)
                        cooldownText.text = cooldownRemaining > 0 ? $"{cooldownRemaining:F1}" : "就绪";

                    if (useSkillButton != null)
                        useSkillButton.interactable = skillInstance.CanUse();
                }
            }
        }

        private void OnUseSkillClicked()
        {
            if (selectedSkill != null)
            {
                var selectedUnit = GetSelectedUnit();
                if (selectedUnit != null)
                {
                    selectedUnit.UseSkill(selectedSkill.id);
                    UpdateCooldownDisplay();
                }
            }
        }

        private Unit GetSelectedUnit()
        {
            // 这里可以根据游戏逻辑获取当前选中的单位
            // 简化版本：返回第一个英雄单位
            var unitManager = GameManager.Instance?.UnitManager;
            if (unitManager != null)
            {
                var heroUnits = unitManager.GetUnitsOfType(UnitType.Hero);
                return heroUnits.Count > 0 ? heroUnits[0] : null;
            }
            return null;
        }

        void Update()
        {
            if (selectedSkill != null)
            {
                UpdateCooldownDisplay();
            }
        }
    }
}