using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 血条组件
    public class HealthBar : MonoBehaviour
    {
        [Header("UI引用")]
        public UnityEngine.UI.Slider healthSlider;
        public UnityEngine.UI.Image fillImage;
        public Color fullHealthColor = Color.green;
        public Color lowHealthColor = Color.red;

        private int maxHealth;
        private int currentHealth;

        public void SetMaxHealth(int health)
        {
            maxHealth = health;
            if (healthSlider != null)
            {
                healthSlider.maxValue = health;
                healthSlider.value = health;
            }
            UpdateHealthColor();
        }

        public void SetHealth(int health)
        {
            currentHealth = health;
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }
            UpdateHealthColor();
        }

        private void UpdateHealthColor()
        {
            if (fillImage != null)
            {
                float healthPercentage = (float)currentHealth / maxHealth;
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
            }
        }
    }
}