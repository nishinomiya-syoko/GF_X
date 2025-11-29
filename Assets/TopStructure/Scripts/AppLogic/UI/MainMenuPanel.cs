using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace EmpireClash
{
    // 主菜单面板
    public class MainMenuPanel : UIPanel
    {
        [Header("主菜单按钮")]
        public Button baseButton;
        public Button attackButton;
        public Button defenseButton;
        public Button techButton;
        public Button settingsButton;
        [Header("资源显示")]
        public Text goldText;
        public Text elixirText;
        public Text darkElixirText;
        public Text gemText;

        protected override void OnShow()
        {
            base.OnShow();
            
            // 按钮点击动画
            AnimateButtons();
        }

        private void AnimateButtons()
        {
            Button[] buttons = { baseButton, attackButton, defenseButton, techButton, settingsButton };
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    RectTransform btnRect = buttons[i].GetComponent<RectTransform>();
                    btnRect.localScale = Vector3.zero;
                    
                    btnRect.DOScale(Vector3.one, 0.3f)
                        .SetDelay(i * 0.1f)
                        .SetEase(Ease.OutBack);
                }
            }
        }

        protected override void OnInit()
        {
            // 注册资源变化事件
            if (GameManager.Instance?.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.OnResourceChanged += OnResourceChanged;
                // 初始化一次资源显示
                UpdateResourceDisplay();
            }
            // 绑定按钮事件
            if (baseButton != null)
                baseButton.onClick.AddListener(() => GameManager.Instance?.UIManager?.ShowBaseView());

            if (attackButton != null)
                attackButton.onClick.AddListener(() => GameManager.Instance?.UIManager?.ShowAttackLevels());

            if (defenseButton != null)
                defenseButton.onClick.AddListener(() => GameManager.Instance?.UIManager?.ShowDefenseLevels());

            if (techButton != null)
                techButton.onClick.AddListener(() => GameManager.Instance?.UIManager?.ShowTechTree());

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => GameManager.Instance?.UIManager?.ShowSettings());
        }
         #region 资源显示

        private void OnResourceChanged(ResourceType type, int amount)
        {
            UpdateResourceDisplay();
        }

        private void UpdateResourceDisplay()
        {
            if (GameManager.Instance?.ResourceManager == null)
                return;

            var resourceManager = GameManager.Instance.ResourceManager;

            if (goldText != null)
                goldText.text = resourceManager.GetResource(ResourceType.Gold).ToString();

            if (elixirText != null)
                elixirText.text = resourceManager.GetResource(ResourceType.Elixir).ToString();

            if (darkElixirText != null)
                darkElixirText.text = resourceManager.GetResource(ResourceType.DarkElixir).ToString();

            if (gemText != null)
                gemText.text = resourceManager.GetResource(ResourceType.Gems).ToString();
        }

        #endregion
    }
}