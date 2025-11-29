using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmpireClash
{
    // 科技节点UI
    public class TechNodeUI : MonoBehaviour
    {
        [Header("UI元素")]
        public Image iconImage;
        public Text levelText;
        public Button button;
        public Image progressImage;

        private TechData techData;

        public event Action<TechData> OnSelected;

        void Start()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        public void SetTechData(TechData data)
        {
            techData = data;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (techData == null)
                return;

            int currentLevel = GameManager.Instance?.TechManager?.GetTechLevel(techData.id) ?? 0;
            
            if (levelText != null)
                levelText.text = currentLevel.ToString();
            
            if (iconImage != null && techData.icon != null)
                iconImage.sprite = techData.icon;

            // 更新颜色表示状态
            if (button != null)
            {
                ColorBlock colors = button.colors;
                
                if (currentLevel >= techData.maxLevel)
                {
                    colors.normalColor = Color.green; // 已满级
                }
                else if (GameManager.Instance?.TechManager?.CanResearchTech(techData.id) == true)
                {
                    colors.normalColor = Color.white; // 可研究
                }
                else
                {
                    colors.normalColor = Color.gray; // 不可研究
                }
                
                button.colors = colors;
            }

            // 更新研究进度
            if (progressImage != null)
            {
                float progress = GameManager.Instance?.TechManager?.GetResearchProgress(techData.id) ?? 0f;
                progressImage.fillAmount = progress;
                progressImage.gameObject.SetActive(progress > 0f);
            }
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(techData);
        }

        void Update()
        {
            UpdateVisual();
        }
    }
}