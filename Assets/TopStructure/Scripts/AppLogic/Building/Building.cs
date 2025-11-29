using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace EmpireClash
{
    // 建筑基类
    public partial class Building : EntityLogic
    {
        [Header("建筑信息")]
        public BuildingData data;
        public int currentLevel = 1;
        public BuildingState currentState = BuildingState.Idle;

        [Header("组件引用")]
        public Renderer buildingRenderer;
        public HealthBar healthBar;
        public ParticleSystem buildEffect;
        public ParticleSystem upgradeEffect;

        // 属性
        public int CurrentHitPoints { get; private set; }
        public int MaxHitPoints { get; private set; }
        public bool IsBuilding => currentState == BuildingState.Building;
        public bool IsUpgrading => currentState == BuildingState.Upgrading;
        public bool IsWorking => currentState == BuildingState.Working;
        public bool IsDestroyed => currentState == BuildingState.Destroyed;

        // 事件
        public event Action<Building> OnBuildingPlaced;
        public event Action<Building> OnBuildingUpgraded;
        public event Action<Building> OnBuildingDestroyed;
        public event Action<Building, int> OnBuildingDamaged;

        private Coroutine buildCoroutine;
        private Coroutine upgradeCoroutine;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            InitializeBuilding();
            OnStart();
        }
        

        public void OnStart()
        {
            UpdateVisuals();
            AStart();
        }

        protected override void OnIntervalUpdate(float deltaTime)
        {
            UpdateBuildingState();
            AUpdate();
        }

        #region 初始化

        private void InitializeBuilding()
        {
            if (data != null)
            {
                MaxHitPoints = GetMaxHitPoints();
                CurrentHitPoints = MaxHitPoints;

                if (healthBar != null)
                {
                    healthBar.SetMaxHealth(MaxHitPoints);
                    healthBar.SetHealth(CurrentHitPoints);
                }
            }
        }

        private void UpdateVisuals()
        {
            if (buildingRenderer != null && data != null)
            {
                // 根据等级更新材质或颜色
                Material[] materials = buildingRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    // 可以在这里根据等级调整材质属性
                    Color baseColor = materials[i].color;
                    float levelBrightness = 0.8f + (currentLevel * 0.02f);
                    materials[i].color = baseColor * levelBrightness;
                }
                buildingRenderer.materials = materials;
            }
        }

        #endregion

        #region 建造和升级

        public void StartBuilding()
        {
            if (currentState != BuildingState.Idle)
                return;

            currentState = BuildingState.Building;

            // 播放建造特效
            if (buildEffect != null)
                buildEffect.Play();

            // 开始建造计时
            if (buildCoroutine != null)
                StopCoroutine(buildCoroutine);

            buildCoroutine = StartCoroutine(BuildingProcess());
        }

        private IEnumerator BuildingProcess()
        {
            float buildTime = data.buildTime;
            float elapsedTime = 0f;

            // 建造动画
            while (elapsedTime < buildTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / buildTime;

                // 建造进度动画
                if (buildingRenderer != null)
                {
                    Vector3 scale = Vector3.one * Mathf.Lerp(0.1f, 1f, progress);
                    buildingRenderer.transform.localScale = scale;
                }

                yield return null;
            }

            // 建造完成
            buildingRenderer.transform.localScale = Vector3.one;
            currentState = BuildingState.Working;

            // 停止建造特效
            if (buildEffect != null)
                buildEffect.Stop();

            OnBuildingPlaced?.Invoke(this);
            GameManager.Instance?.EventSystem?.OnBuildingPlaced?.Invoke(this);
        }

        public void StartUpgrade()
        {
            if (currentState != BuildingState.Working || currentLevel >= data.maxLevel)
                return;

            currentState = BuildingState.Upgrading;

            // 播放升级特效
            if (upgradeEffect != null)
                upgradeEffect.Play();

            // 开始升级计时
            if (upgradeCoroutine != null)
                StopCoroutine(upgradeCoroutine);

            upgradeCoroutine = StartCoroutine(UpgradeProcess());
        }

        private IEnumerator UpgradeProcess()
        {
            var levelData = data.GetLevelData(currentLevel + 1);
            if (levelData == null)
                yield break;

            float upgradeTime = levelData.upgradeTime;
            float elapsedTime = 0f;

            // 升级动画
            while (elapsedTime < upgradeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / upgradeTime;

                // 升级进度动画
                if (buildingRenderer != null)
                {
                    Color color = buildingRenderer.material.color;
                    color.a = Mathf.Lerp(1f, 0.5f, Mathf.PingPong(progress * 2, 1));
                    buildingRenderer.material.color = color;
                }

                yield return null;
            }

            // 升级完成
            currentLevel++;
            MaxHitPoints = GetMaxHitPoints();
            CurrentHitPoints = MaxHitPoints;

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(MaxHitPoints);
                healthBar.SetHealth(CurrentHitPoints);
            }

            currentState = BuildingState.Working;
            UpdateVisuals();

            // 停止升级特效
            if (upgradeEffect != null)
                upgradeEffect.Stop();

            OnBuildingUpgraded?.Invoke(this);
            GameManager.Instance?.EventSystem?.OnBuildingUpgraded?.Invoke(this);
        }

        #endregion

        #region 战斗相关

        public void TakeDamage(int damage)
        {
            if (currentState == BuildingState.Destroyed || !data.canBeAttacked)
                return;

            int actualDamage = Mathf.Max(1, damage - data.hitPoints);
            CurrentHitPoints -= actualDamage;

            if (healthBar != null)
            {
                healthBar.SetHealth(CurrentHitPoints);
            }

            OnBuildingDamaged?.Invoke(this, actualDamage);

            if (CurrentHitPoints <= 0)
            {
                DestroyBuilding();
            }
        }

        private void DestroyBuilding()
        {
            currentState = BuildingState.Destroyed;

            // 播放摧毁特效
            if (buildEffect != null)
                buildEffect.Play();

            // 隐藏建筑
            if (buildingRenderer != null)
                buildingRenderer.gameObject.SetActive(false);

            OnBuildingDestroyed?.Invoke(this);
            GameManager.Instance?.EventSystem?.OnBuildingDestroyed?.Invoke(this);

            // 延迟销毁对象
            Destroy(gameObject, 2f);
        }

        public void Repair(int repairAmount)
        {
            if (currentState == BuildingState.Destroyed)
                return;

            CurrentHitPoints = Mathf.Min(CurrentHitPoints + repairAmount, MaxHitPoints);

            if (healthBar != null)
            {
                healthBar.SetHealth(CurrentHitPoints);
            }
        }

        #endregion

        #region 辅助方法

        private int GetMaxHitPoints()
        {
            var levelData = data.GetLevelData(currentLevel);
            return levelData != null ? levelData.hitPoints : data.hitPoints;
        }

        private void UpdateBuildingState()
        {
            // 根据建筑类型更新状态
            switch (data.buildingType)
            {
                case BuildingType.Resource:
                    UpdateResourceBuilding();
                    break;
                case BuildingType.Defense:
                    UpdateDefenseBuilding();
                    break;
                case BuildingType.Military:
                    UpdateMilitaryBuilding();
                    break;
            }
        }

        private void UpdateResourceBuilding()
        {
            if (currentState == BuildingState.Working && data.isResourceProducer)
            {
                // 资源建筑自动工作
            }
        }

        private void UpdateDefenseBuilding()
        {
            if (currentState == BuildingState.Working)
            {
                // 防御建筑检查攻击范围
            }
        }

        private void UpdateMilitaryBuilding()
        {
            if (currentState == BuildingState.Working)
            {
                // 军事建筑更新训练状态
            }
        }

        public bool CanUpgrade()
        {
            return currentState == BuildingState.Working &&
                   currentLevel < data.maxLevel &&
                   GameManager.Instance?.ResourceManager?.CanAfford(data.GetLevelData(currentLevel + 1)?.upgradeCosts) == true;
        }

        public ResourceCost[] GetUpgradeCosts() 
        {
            var levelData = data.GetLevelData(currentLevel + 1);
            return levelData?.upgradeCosts;
        }

        #endregion
    }
}