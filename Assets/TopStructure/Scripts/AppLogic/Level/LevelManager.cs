using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{

    // 关卡管理器
    public class LevelManager : MonoBehaviour
    {
        [Header("关卡数据库")]
        public LevelDatabase levelDatabase;

        [Header("玩家进度")]
        public int playerLevel = 1;
        public int playerExperience = 0;
        public List<int> completedLevels = new List<int>();
        public Dictionary<int, int> levelStars = new Dictionary<int, int>(); // levelId -> stars

        [Header("当前关卡")]
        public LevelData currentLevel;
        public LevelState currentLevelState = LevelState.NotStarted;
        private float levelStartTime;
        private int unitsLostInLevel = 0;

        private WaveManager waveManager;

        // 事件
        public event Action<LevelData> OnLevelStarted;
        public event Action<LevelData, int> OnLevelCompleted;
        public event Action<LevelData> OnLevelFailed;

        void Start()
        {
            if (levelDatabase == null)
            {
                levelDatabase = Resources.Load<LevelDatabase>("LevelDatabase");
            }

            waveManager = gameObject.AddComponent<WaveManager>();
            waveManager.OnWaveStarted += OnWaveStartedHandler;
            waveManager.OnWaveCompleted += OnWaveCompletedHandler;
            waveManager.OnAllWavesCompleted += OnAllWavesCompletedHandler;

            LoadPlayerProgress();
        }

        void Update()
        {
            UpdateLevelState();
        }

        #region 关卡开始和结束

        public void StartLevel(int levelId)
        {
            LevelData levelData = levelDatabase?.GetLevelData(levelId);
            if (levelData == null)
            {
                Debug.LogError($"Level {levelId} not found!");
                return;
            }

            if (!levelData.IsUnlocked())
            {
                Debug.LogWarning($"Level {levelId} is not unlocked!");
                return;
            }

            currentLevel = levelData;
            currentLevelState = LevelState.InProgress;
            levelStartTime = Time.time;
            unitsLostInLevel = 0;

            // 切换到对应场景
            if (GameManager.Instance?.MapManager != null)
            {
                GameManager.Instance.MapManager.LoadScene(levelData.sceneIndex);
            }

            // 设置相机位置
            if (GameManager.Instance?.MapManager?.cameraController != null)
            {
                GameManager.Instance.MapManager.cameraController.transform.position = levelData.cameraPosition;
            }

            // 注册单位死亡事件
            if (GameManager.Instance?.UnitManager != null)
            {
                GameManager.Instance.UnitManager.OnUnitDied += OnUnitLostHandler;
            }

            // 开始关卡
            if (levelData.levelType == LevelType.Defense)
            {
                StartDefenseLevel(levelData);
            }
            else
            {
                StartAttackLevel(levelData);
            }

            OnLevelStarted?.Invoke(levelData);
            GameManager.Instance?.EventSystem?.OnLevelStarted?.Invoke(levelId);
        }

        private void StartDefenseLevel(LevelData levelData)
        {
            if (levelData.waves != null && levelData.waves.Length > 0)
            {
                waveManager.StartWave(levelData.waves[0], 0);
            }
        }

        private void StartAttackLevel(LevelData levelData)
        {
            // 生成敌方基地
            if (levelData.enemyBase != null)
            {
                SpawnEnemyBase(levelData.enemyBase);
            }

            // 设置胜利条件检查
            StartCoroutine(CheckAttackVictoryCondition());
        }

        public void CompleteLevel()
        {
            if (currentLevelState != LevelState.InProgress)
                return;

            currentLevelState = LevelState.Completed;

            // 计算完成时间
            float completionTime = Time.time - levelStartTime;
            
            // 计算星级
            int stars = currentLevel.GetStarRating(completionTime, unitsLostInLevel);

            // 记录完成状态
            if (!completedLevels.Contains(currentLevel.levelId))
            {
                completedLevels.Add(currentLevel.levelId);
            }
            
            levelStars[currentLevel.levelId] = Math.Max(levelStars.GetValueOrDefault(currentLevel.levelId, 0), stars);

            // 发放奖励
            GrantLevelRewards(currentLevel.rewards);

            // 解锁新内容
            UnlockContent(currentLevel.rewards.unlockContent);

            // 增加经验
            AddExperience(currentLevel.rewards.experienceReward);

            // 清理事件
            CleanupLevelEvents();

            OnLevelCompleted?.Invoke(currentLevel, stars);
            GameManager.Instance?.EventSystem?.OnLevelCompleted?.Invoke(currentLevel.levelId);

            SavePlayerProgress();
        }

        public void FailLevel()
        {
            if (currentLevelState != LevelState.InProgress)
                return;

            currentLevelState = LevelState.Failed;

            // 清理事件
            CleanupLevelEvents();

            OnLevelFailed?.Invoke(currentLevel);
            GameManager.Instance?.EventSystem?.OnLevelFailed?.Invoke(currentLevel.levelId);
        }

        #endregion

        #region 关卡逻辑

        private void UpdateLevelState()
        {
            if (currentLevelState != LevelState.InProgress)
                return;

            // 根据关卡类型更新状态
            if (currentLevel.levelType == LevelType.Attack)
            {
                UpdateAttackLevel();
            }
        }

        private void UpdateAttackLevel()
        {
            // 检查玩家是否还有单位
            if (GameManager.Instance?.UnitManager?.GetUnitsOfType(UnitType.Infantry).Count == 0 &&
                GameManager.Instance?.UnitManager?.GetUnitsOfType(UnitType.Cavalry).Count == 0 &&
                GameManager.Instance?.UnitManager?.GetUnitsOfType(UnitType.Ranged).Count == 0 &&
                GameManager.Instance?.UnitManager?.GetUnitsOfType(UnitType.Siege).Count == 0)
            {
                FailLevel();
            }
        }

        private IEnumerator CheckAttackVictoryCondition()
        {
            while (currentLevelState == LevelState.InProgress)
            {
                yield return new WaitForSeconds(1f);

                // 检查是否所有敌方建筑都被摧毁
                if (AreAllEnemyBuildingsDestroyed())
                {
                    CompleteLevel();
                    yield break;
                }
            }
        }

        private bool AreAllEnemyBuildingsDestroyed()
        {
            // 这里需要实现检查敌方建筑状态的逻辑
            // 简化版本：假设所有敌方建筑都有Building组件
            Building[] enemyBuildings = FindObjectsOfType<Building>();
            
            foreach (var building in enemyBuildings)
            {
                if (building.data.buildingType != BuildingType.Wall && !building.IsDestroyed)
                {
                    return false;
                }
            }
            
            return true;
        }

        #endregion

        #region 波次处理

        private void OnWaveStartedHandler(int waveIndex)
        {
            Debug.Log($"Wave {waveIndex + 1} started!");
        }

        private void OnWaveCompletedHandler(int waveIndex)
        {
            Debug.Log($"Wave {waveIndex + 1} completed!");

            // 检查是否还有下一波
            if (currentLevel.waves != null && waveIndex + 1 < currentLevel.waves.Length)
            {
                // 开始下一波
                waveManager.StartWave(currentLevel.waves[waveIndex + 1], waveIndex + 1);
            }
        }

        private void OnAllWavesCompletedHandler()
        {
            // 所有波次完成，关卡胜利
            CompleteLevel();
        }

        #endregion

        #region 奖励和经验

        private void GrantLevelRewards(RewardData rewards)
        {
            if (rewards == null)
                return;

            var resourceManager = GameManager.Instance?.ResourceManager;
            if (resourceManager != null)
            {
                resourceManager.AddResource(ResourceType.Gold, rewards.goldReward);
                resourceManager.AddResource(ResourceType.Elixir, rewards.elixirReward);
                resourceManager.AddResource(ResourceType.DarkElixir, rewards.darkElixirReward);
                resourceManager.AddResource(ResourceType.Gems, rewards.gemReward);
            }
        }

        public void AddExperience(int experience)
        {
            playerExperience += experience;
            
            // 检查是否升级
            while (playerExperience >= GetExperienceForLevel(playerLevel + 1))
            {
                playerExperience -= GetExperienceForLevel(playerLevel + 1);
                playerLevel++;
                
                // 触发升级事件
                GameManager.Instance?.EventSystem?.OnPlayerLeveledUp?.Invoke(playerLevel);
            }
        }

        private int GetExperienceForLevel(int level)
        {
            // 经验曲线：每级所需经验递增
            return 100 * level * level;
        }

        private void UnlockContent(string[] unlockContent)
        {
            if (unlockContent == null)
                return;

            foreach (var contentId in unlockContent)
            {
                // 解锁对应的关卡或内容
                var level = levelDatabase?.GetLevelData(int.Parse(contentId));
                if (level != null)
                {
                    level.isUnlocked = true;
                }
            }
        }

        #endregion

        #region 事件处理

        private void OnUnitLostHandler(Unit unit)
        {
            unitsLostInLevel++;
        }

        #endregion

        #region 辅助方法

        private void SpawnEnemyBase(EnemyBaseData enemyBase)
        {
            if (enemyBase == null)
                return;

            for (int i = 0; i < enemyBase.buildings.Count && i < enemyBase.buildingPositions.Count; i++)
            {
                var buildingData = enemyBase.buildings[i];
                var position = enemyBase.buildingPositions[i];

                // 实例化敌方建筑
                if (buildingData.prefab != null)
                {
                    GameObject buildingObj = Instantiate(buildingData.prefab, position, Quaternion.identity);
                    Building building = buildingObj.GetComponent<Building>();
                    
                    if (building == null)
                    {
                        building = buildingObj.AddComponent<Building>();
                    }

                    building.data = buildingData;
                    building.currentLevel = 1;
                    building.currentState = BuildingState.Working;
                }
            }
        }

        private void CleanupLevelEvents()
        {
            if (GameManager.Instance?.UnitManager != null)
            {
                GameManager.Instance.UnitManager.OnUnitDied -= OnUnitLostHandler;
            }

            waveManager.StopAllWaves();
        }

        #endregion

        #region 数据持久化

        [System.Serializable]
        private class PlayerProgressData
        {
            public int playerLevel;
            public int playerExperience;
            public List<int> completedLevels;
            public Dictionary<int, int> levelStars;
        }

        public void SavePlayerProgress()
        {
            var data = new PlayerProgressData
            {
                playerLevel = playerLevel,
                playerExperience = playerExperience,
                completedLevels = completedLevels,
                levelStars = levelStars
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("PlayerProgress", json);
            PlayerPrefs.Save();
        }

        public void LoadPlayerProgress()
        {
            if (PlayerPrefs.HasKey("PlayerProgress"))
            {
                string json = PlayerPrefs.GetString("PlayerProgress");
                var data = JsonUtility.FromJson<PlayerProgressData>(json);
                
                if (data != null)
                {
                    playerLevel = data.playerLevel;
                    playerExperience = data.playerExperience;
                    completedLevels = data.completedLevels ?? new List<int>();
                    levelStars = data.levelStars ?? new Dictionary<int, int>();
                }
            }
        }

        #endregion

        #region 查询方法

        public int GetLevelStars(int levelId)
        {
            return levelStars.GetValueOrDefault(levelId, 0);
        }

        public bool IsLevelCompleted(int levelId)
        {
            return completedLevels.Contains(levelId);
        }

        public List<LevelData> GetAvailableLevels()
        {
            List<LevelData> available = new List<LevelData>();
            
            if (levelDatabase != null)
            {
                foreach (var level in levelDatabase.levels)
                {
                    if (level.IsUnlocked())
                    {
                        available.Add(level);
                    }
                }
            }
            
            return available;
        }

        public List<LevelData> GetCompletedLevels()
        {
            List<LevelData> completed = new List<LevelData>();
            
            if (levelDatabase != null)
            {
                foreach (var levelId in completedLevels)
                {
                    var level = levelDatabase.GetLevelData(levelId);
                    if (level != null)
                    {
                        completed.Add(level);
                    }
                }
            }
            
            return completed;
        }

        #endregion
    }
}