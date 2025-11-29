using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 光环效果组件
    public class AuraEffect : MonoBehaviour
    {
        private string auraSkillId;
        private float radius;
        private SkillData auraSkill;
        private float lastApplyTime;
        private float applyInterval = 1f; // 每秒应用一次

        public void Initialize(string skillId, float radius)
        {
            this.auraSkillId = skillId;
            this.radius = radius;
            this.auraSkill = GameManager.Instance?.SkillManager?.skillDatabase?.GetSkillData(skillId);
        }

        public void UpdateAura()
        {
            if (auraSkill == null)
                return;

            if (Time.time - lastApplyTime >= applyInterval)
            {
                ApplyAuraToTargets();
                lastApplyTime = Time.time;
            }
        }

        private void ApplyAuraToTargets()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, radius);
            foreach (var target in targets)
            {
                var unit = target.GetComponent<Unit>();
                if (unit != null)
                {
                    // 应用光环效果
                    if (auraSkill.effects != null)
                    {
                        foreach (var effect in auraSkill.effects)
                        {
                            if (effect.effectType == SkillEffectType.Buff && effect.buffs != null)
                            {
                                unit.ApplyBuff(effect.buffs);
                            }
                        }
                    }
                }
            }
        }
    }
}