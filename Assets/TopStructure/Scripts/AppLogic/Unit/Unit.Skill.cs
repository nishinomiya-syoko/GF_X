using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 技能实例
    [System.Serializable]
    public class SkillInstance
    {
        public string skillId;
        public int level;
        public float cooldownRemaining;
        public float chargeProgress;
        public bool isUnlocked;

        public SkillInstance(string skillId, int level = 1)
        {
            this.skillId = skillId;
            this.level = level;
            this.cooldownRemaining = 0f;
            this.chargeProgress = 0f;
            this.isUnlocked = true;
        }

        public void Update(float deltaTime)
        {
            if (cooldownRemaining > 0)
            {
                cooldownRemaining -= deltaTime;
            }

            // 技能充能
            var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
            if (skillData != null && skillData.skillType == SkillType.Ultimate)
            {
                chargeProgress += deltaTime * 0.1f; // 基础充能速度
                chargeProgress = Mathf.Min(chargeProgress, 1f);
            }
        }

        public bool CanUse()
        {
            if (!isUnlocked)
                return false;

            var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
            if (skillData == null)
                return false;

            // 终极技能需要充能
            if (skillData.skillType == SkillType.Ultimate && chargeProgress < 1f)
                return false;

            return cooldownRemaining <= 0f;
        }

        public void Use()
        {
            var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
            if (skillData != null)
            {
                cooldownRemaining = skillData.GetCooldown(level);

                // 重置终极技能充能
                if (skillData.skillType == SkillType.Ultimate)
                {
                    chargeProgress = 0f;
                }
            }
        }
    }

    // 扩展的单位类
    public partial class Unit
    {
        [Header("技能系统")]
        public List<SkillInstance> skills = new List<SkillInstance>();
        public Dictionary<EquipmentType, string> equippedItems = new Dictionary<EquipmentType, string>();
        public List<BuffInstance> activeBuffs = new List<BuffInstance>();

        private Dictionary<BuffType, float> buffModifiers = new Dictionary<BuffType, float>();

        // 技能事件
        public event Action<Unit, SkillData> OnSkillUsed;
        public event Action<Unit, BuffData> OnBuffApplied;
        public event Action<Unit, BuffData> OnBuffExpired;

        void TAwake()
        {
            InitializeBuffModifiers();
            InitializeSkills();
        }

        void TUpdate()
        {
            UpdateSkills();
            UpdateBuffs();
        }

        #region 技能系统

        private void InitializeSkills()
        {
            if (data != null && data.innateSkills != null)
            {
                foreach (var skillId in data.innateSkills)
                {
                    if (!string.IsNullOrEmpty(skillId))
                    {
                        skills.Add(new SkillInstance(skillId));
                    }
                }
            }

            // 添加终极技能
            if (data != null && !string.IsNullOrEmpty(data.ultimateSkill))
            {
                skills.Add(new SkillInstance(data.ultimateSkill));
            }
        }

        private void UpdateSkills()
        {
            foreach (var skill in skills)
            {
                skill.Update(Time.deltaTime);
            }
        }

        public bool CanUseSkill(string skillId)
        {
            var skill = skills.Find(s => s.skillId == skillId);
            return skill != null && skill.CanUse();
        }

        public void UseSkill(string skillId, Vector3 targetPosition = default)
        {
            if (!CanUseSkill(skillId))
                return;

            var skillData = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
            if (skillData == null)
                return;

            var skill = skills.Find(s => s.skillId == skillId);
            skill.Use();

            // 执行技能效果
            ExecuteSkill(skillData, targetPosition);

            OnSkillUsed?.Invoke(this, skillData);
            GameManager.Instance?.EventSystem?.OnSkillUsed?.Invoke(this, skillData);
        }

        private void ExecuteSkill(SkillData skillData, Vector3 targetPosition)
        {
            // 播放技能特效
            if (skillData.effectPrefab != null)
            {
                var effect = Instantiate(skillData.effectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // 执行技能效果
            // foreach (var effect in skillData.effects)
            foreach (var effect in skillData.effects)
            {
                ExecuteSkillEffect(effect, targetPosition,skillData.range);
            }

            // 消耗资源
            ConsumeSkillResources(skillData);
        }

        // private void ExecuteSkillEffect(SkillEffectData effect, Vector3 targetPosition)
        private void ExecuteSkillEffect(SkillEffectData effect, Vector3 targetPosition,float range)
        {
            switch (effect.effectType)
            {
                case SkillEffectType.Damage:
                    // 对目标造成伤害
                    ApplyDamageToTargets(effect.baseValue, targetPosition,range);
                    break;

                // case SkillEffectType.Heal:
                //     // 治疗自身或友军
                //     Heal(effect.baseValue);
                //     break;

                case SkillEffectType.Buff:
                    // 应用增益效果
                    ApplyBuff(effect.buffs);
                    break;

                // case SkillEffectType.Shield:
                //     // 创建护盾
                //     CreateShield(effect.baseValue);
                //     break;

                // case SkillEffectType.Stun:
                //     // 眩晕目标
                //     StunTargets(targetPosition, effect.duration);
                //     break;

                // case SkillEffectType.Slow:
                //     // 减速目标
                //     SlowTargets(targetPosition, effect.baseValue, effect.duration);
                //     break;
            }
        }

        private void ConsumeSkillResources(SkillData skillData)
        {
            // 这里可以实现魔法值、能量值等资源的消耗
        }

        private void ApplyDamageToTargets(float damage, Vector3 targetPosition,float range =0)
        {
            // 根据技能目标类型找到目标并造成伤害
            Collider[] targets = FindTargetsInRange(targetPosition, range);
            foreach (var target in targets)
            {
                var unit = target.GetComponent<Unit>();
                if (unit != null && unit != this)
                {
                    unit.TakeDamage((int)damage);
                }
            }
        }

        private Collider[] FindTargetsInRange(Vector3 center, float range)
        {
            return Physics.OverlapSphere(center, range);
        }

        #endregion

        #region BUFF系统

        private void InitializeBuffModifiers()
        {
            foreach (BuffType type in Enum.GetValues(typeof(BuffType)))
            {
                buffModifiers[type] = 0f;
            }
        }

        private void UpdateBuffs()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                var buff = activeBuffs[i];
                buff.duration -= Time.deltaTime;

                if (buff.duration <= 0)
                {
                    RemoveBuff(buff.buffData);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        public void ApplyBuff(BuffData[] buffs)
        {
            if (buffs == null)
                return;

            foreach (var buffData in buffs)
            {
                ApplyBuff(buffData);
            }
        }

        public void ApplyBuff(BuffData buffData)
        {
            // 检查是否已存在相同BUFF
            var existingBuff = activeBuffs.Find(b => b.buffData.buffName == buffData.buffName);
            if (existingBuff != null)
            {
                if (buffData.isStackable && existingBuff.stacks < buffData.maxStacks)
                {
                    existingBuff.stacks++;
                    existingBuff.duration = buffData.duration;
                }
                else
                {
                    existingBuff.duration = buffData.duration;
                }
            }
            else
            {
                var newBuff = new BuffInstance(buffData);
                activeBuffs.Add(newBuff);
            }

            // 应用BUFF效果
            ApplyBuffEffect(buffData);

            OnBuffApplied?.Invoke(this, buffData);
        }

        private void ApplyBuffEffect(BuffData buffData)
        {
            buffModifiers[buffData.buffType] += buffData.value;

            // 根据BUFF类型应用不同效果
            switch (buffData.buffType)
            {
                case BuffType.MovementSpeed:
                    UpdateMovementSpeed();
                    break;
                case BuffType.AttackSpeed:
                    UpdateAttackSpeed();
                    break;
            }
        }

        private void RemoveBuff(BuffData buffData)
        {
            buffModifiers[buffData.buffType] -= buffData.value * GetBuffStacks(buffData);

            switch (buffData.buffType)
            {
                case BuffType.MovementSpeed:
                    UpdateMovementSpeed();
                    break;
                case BuffType.AttackSpeed:
                    UpdateAttackSpeed();
                    break;
            }

            OnBuffExpired?.Invoke(this, buffData);
        }

        private int GetBuffStacks(BuffData buffData)
        {
            var buff = activeBuffs.Find(b => b.buffData.buffName == buffData.buffName);
            return buff != null ? buff.stacks : 1;
        }

        private void UpdateMovementSpeed()
        {
            if (data != null)
            {
                float speedMultiplier = 1f + buffModifiers[BuffType.MovementSpeed];
                
                // var navAgent = GetComponent<NavMeshAgent>();
                // if (navAgent != null)
                // {
                //     navAgent.speed = data.movementSpeed * speedMultiplier;
                // }
            }
        }

        private void UpdateAttackSpeed()
        {
            // 更新攻击速度
        }

        public float GetBuffModifier(BuffType buffType)
        {
            return buffModifiers.GetValueOrDefault(buffType, 0f);
        }

        #endregion

        #region 装备系统

        public void AddEquipmentStats(EquipmentData equipmentData)
        {
            // 增加装备提供的属性
            if (data != null)
            {
                data.hitPoints += equipmentData.health;
                CurrentHitPoints += equipmentData.health;
                data.damage += equipmentData.attack;
                // 其他属性...
            }
        }

        public void RemoveEquipmentStats(EquipmentData equipmentData)
        {
            // 移除装备提供的属性
            if (data != null)
            {
                data.hitPoints -= equipmentData.health;
                CurrentHitPoints = Mathf.Min(CurrentHitPoints, data.hitPoints);
                data.damage -= equipmentData.attack;
                // 其他属性...
            }
        }

        public void EquipItem(string equipmentId)
        {
            var equipmentManager = GameManager.Instance?.GetComponent<EquipmentManager>();
            if (equipmentManager != null)
            {
                equipmentManager.EquipItem(equipmentId);
            }
        }

        #endregion

        #region 技能查询

        public List<SkillInstance> GetAvailableSkills()
        {
            return skills.FindAll(s => s.isUnlocked);
        }

        public SkillInstance GetSkill(string skillId)
        {
            return skills.Find(s => s.skillId == skillId);
        }

        #endregion
    }

    // BUFF实例
    [System.Serializable]
    public class BuffInstance
    {
        public BuffData buffData;
        public float duration;
        public int stacks;

        public BuffInstance(BuffData buffData)
        {
            this.buffData = buffData;
            this.duration = buffData.duration;
            this.stacks = 1;
        }
    }
}