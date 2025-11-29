using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 扩展的建筑类
    public partial class Building
    {
        [Header("技能系统")]
        public List<SkillInstance> buildingSkills = new List<SkillInstance>();
        public AuraEffect auraEffect;

        void AStart()
        {
            InitializeBuildingSkills();
            InitializeAuraEffect();
        }

        void AUpdate()
        {
            UpdateBuildingSkills();
            UpdateAuraEffect();
        }

        #region 建筑技能

        private void InitializeBuildingSkills()
        {
            if (data != null && data.buildingSkills != null)
            {
                foreach (var skillId in data.buildingSkills)
                {
                    if (!string.IsNullOrEmpty(skillId))
                    {
                        buildingSkills.Add(new SkillInstance(skillId));
                    }
                }
            }
        }

        private void UpdateBuildingSkills()
        {
            foreach (var skill in buildingSkills)
            {
                skill.Update(Time.deltaTime);

                // 自动使用被动技能
                if (skill.CanUse())
                {
                    UseBuildingSkill(skill);
                }
            }
        }

        private void UseBuildingSkill(SkillInstance skillInstance)
        {
            // var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillInstance.skillId);
            // if (skillData == null)
            //     return;

            // // 执行建筑技能效果
            // ExecuteBuildingSkillEffect(skillData);
            // skillInstance.Use();
        }

        private void ExecuteBuildingSkillEffect(SkillData skillData)
        {
            foreach (var effect in skillData.effects)
            {
                switch (effect.effectType)
                {
                    case SkillEffectType.Buff:
                        // 为范围内的友军提供BUFF
                        ApplyBuffToAllies(effect.buffs);
                        break;

                    case SkillEffectType.Damage:
                        // 攻击范围内的敌人
                        AttackEnemiesInRange(effect.baseValue);
                        break;

                    case SkillEffectType.Shield:
                        // 为建筑提供护盾
                        CreateBuildingShield(effect.baseValue);
                        break;
                }
            }
        }

        private void ApplyBuffToAllies(BuffData[] buffs)
        {
            // 找到范围内的友军单位并应用BUFF
            Collider[] allies = FindAlliesInRange(data.auraRadius);
            foreach (var ally in allies)
            {
                var unit = ally.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.ApplyBuff(buffs);
                }
            }
        }

        private void AttackEnemiesInRange(float damage)
        {
            // 攻击范围内的敌人
            Collider[] enemies = FindEnemiesInRange(data.range);
            foreach (var enemy in enemies)
            {
                var unit = enemy.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.TakeDamage((int)damage);
                }
            }
        }
        private void CreateBuildingShield(float shieldValue)
        {
            // 为建筑提供护盾
            // ...
        }

        #endregion

        #region 光环效果

        private void InitializeAuraEffect()
        {
            if (data != null && data.hasAuraEffect && !string.IsNullOrEmpty(data.auraSkillId))
            {
                auraEffect = gameObject.AddComponent<AuraEffect>();
                auraEffect.Initialize(data.auraSkillId, data.auraRadius);
            }
        }

        private void UpdateAuraEffect()
        {
            if (auraEffect != null)
            {
                auraEffect.UpdateAura();
            }
        }

        #endregion
        public Collider[] FindAlliesInRange(float range)
        {
            // 找到范围内的友军单位
            return Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Allies"));
        }
        public Collider[] FindEnemiesInRange(float range)
        {
            // 找到范围内的敌人单位
            return Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Enemies"));
        }
    }
}