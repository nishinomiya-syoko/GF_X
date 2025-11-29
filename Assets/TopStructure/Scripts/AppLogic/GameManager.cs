using UnityEngine;
using System;
using System.Collections;

namespace EmpireClash
{
    // 游戏管理器 - 核心控制器
    public class GameManager : MonoSingleton<GameManager>
    {

        #region 核心系统
        public ResourceManager ResourceManager { get; private set; }
        public MapManager MapManager { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        public UnitManager UnitManager { get; private set; }
        public TechManager TechManager { get; private set; }
        public LevelManager LevelManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public PerformanceManager PerformanceManager { get; private set; }

        public SkillManager SkillManager { get; private set; }
        public EquipmentManager EquipmentManager { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public static GridManager GridManager { get; private set; }
        public static EntityManager EntityManager { get; private set; }

        #endregion
        // [Header("事件系统")]
        public GameEventSystem EventSystem { get; private set; }

        // [Header("游戏状态")]
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("设置")]
        public bool enableAutoSave = true;
        public float autoSaveInterval = 60f; // 秒

        private Coroutine autoSaveCoroutine;

        // 游戏状态枚举
        public enum GameState
        {
            MainMenu,
            BaseView,
            AttackMode,
            DefenseMode,
            Pause,
            Loading
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }

        void Start()
        {
            LoadGame();
            StartAutoSave();
        }

        void Update()
        {
            HandleInput();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        void OnApplicationQuit()
        {
            SaveGame();
        }

        #region 初始化

        private void InitializeGame()
        {
            // 创建事件系统
            EventSystem = new GameEventSystem();

            // 获取或创建核心系统
            InitializeCoreSystems();

            // 设置系统引用
            SetupSystemReferences();

            // 注册事件
            RegisterEvents();

            Debug.Log("Game Manager initialized successfully");
        }

        private void InitializeCoreSystems()
        {
            // 资源管理器
            ResourceManager = FindObjectOfType<ResourceManager>();
            if (ResourceManager == null)
            {
                GameObject resourceGO = new GameObject("ResourceManager");
                ResourceManager = resourceGO.AddComponent<ResourceManager>();
            }

            // 地图管理器
            MapManager = FindObjectOfType<MapManager>();
            if (MapManager == null)
            {
                GameObject mapGO = new GameObject("MapManager");
                MapManager = mapGO.AddComponent<MapManager>();
            }

            // 建筑管理器
            BuildingManager = FindObjectOfType<BuildingManager>();
            if (BuildingManager == null)
            {
                GameObject buildingGO = new GameObject("BuildingManager");
                BuildingManager = buildingGO.AddComponent<BuildingManager>();
            }

            // 单位管理器
            UnitManager = FindObjectOfType<UnitManager>();
            if (UnitManager == null)
            {
                GameObject unitGO = new GameObject("UnitManager");
                UnitManager = unitGO.AddComponent<UnitManager>();
            }

            // 科技管理器
            TechManager = FindObjectOfType<TechManager>();
            if (TechManager == null)
            {
                GameObject techGO = new GameObject("TechManager");
                TechManager = techGO.AddComponent<TechManager>();
            }

            // 关卡管理器
            LevelManager = FindObjectOfType<LevelManager>();
            if (LevelManager == null)
            {
                GameObject levelGO = new GameObject("LevelManager");
                LevelManager = levelGO.AddComponent<LevelManager>();
            }

            // 音频管理器
            AudioManager = FindObjectOfType<AudioManager>();
            if (AudioManager == null)
            {
                GameObject audioGO = new GameObject("AudioManager");
                AudioManager = audioGO.AddComponent<AudioManager>();
            }

            // UI管理器
            UIManager = FindObjectOfType<UIManager>();
            if (UIManager == null)
            {
                GameObject uiGO = new GameObject("UIManager");
                UIManager = uiGO.AddComponent<UIManager>();
            }

            // 性能管理器
            PerformanceManager = FindObjectOfType<PerformanceManager>();
            if (PerformanceManager == null)
            {
                GameObject perfGO = new GameObject("PerformanceManager");
                PerformanceManager = perfGO.AddComponent<PerformanceManager>();
            }
            GridManager = GridManager.Instance;
            EntityManager = EntityManager.Instance;
        }

        private void SetupSystemReferences()
        {
            // 确保所有系统都能访问游戏管理器
            var systems = new MonoBehaviour[]
            {
                ResourceManager, MapManager, BuildingManager, UnitManager,
                TechManager, LevelManager, AudioManager, UIManager, PerformanceManager
            };

            foreach (var system in systems)
            {
                if (system != null)
                {
                    system.transform.SetParent(transform);
                }
            }
        }

        private void RegisterEvents()
        {
            // 注册建筑事件
            if (BuildingManager != null)
            {
                BuildingManager.OnBuildingPlaced += (building) => EventSystem.OnBuildingPlaced?.Invoke(building);
                BuildingManager.OnBuildingUpgraded += (building) => EventSystem.OnBuildingUpgraded?.Invoke(building);
                BuildingManager.OnBuildingDestroyed += (building) => EventSystem.OnBuildingDestroyed?.Invoke(building);
            }

            // 注册单位事件
            if (UnitManager != null)
            {
                UnitManager.OnUnitDeployed += (unit) => EventSystem.OnUnitDeployed?.Invoke(unit);
                UnitManager.OnUnitDied += (unit) => EventSystem.OnUnitDied?.Invoke(unit);
            }

            // 注册科技事件
            if (TechManager != null)
            {
                TechManager.OnTechResearched += (techId, level) => EventSystem.OnTechResearched?.Invoke(techId, level);
            }

            // 注册关卡事件
            if (LevelManager != null)
            {
                LevelManager.OnLevelStarted += (level) => EventSystem.OnLevelStarted?.Invoke(level.levelId);
                LevelManager.OnLevelCompleted += (level, stars) => EventSystem.OnLevelCompleted?.Invoke(level.levelId);
                LevelManager.OnLevelFailed += (level) => EventSystem.OnLevelFailed?.Invoke(level.levelId);
            }
        }

        #endregion

        #region 游戏状态管理

        public void ChangeGameState(GameState newState)
        {
            CurrentState = newState;
            
            // 根据状态执行相应操作
            switch (newState)
            {
                case GameState.BaseView:
                    EnterBaseView();
                    break;
                case GameState.AttackMode:
                    EnterAttackMode();
                    break;
                case GameState.DefenseMode:
                    EnterDefenseMode();
                    break;
                case GameState.Pause:
                    PauseGame();
                    break;
            }
        }

        private void EnterBaseView()
        {
            // 切换到基地视图
            Time.timeScale = 1f;
            
            if (AudioManager != null)
            {
                AudioManager.PlayMusic("BaseTheme");
            }
        }

        private void EnterAttackMode()
        {
            // 进入攻击模式
            if (AudioManager != null)
            {
                AudioManager.PlayMusic("BattleTheme");
            }
        }

        private void EnterDefenseMode()
        {
            // 进入防守模式
            if (AudioManager != null)
            {
                AudioManager.PlayMusic("BattleTheme");
            }
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            ChangeGameState(GameState.BaseView);
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // ESC键暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (CurrentState == GameState.Pause)
                {
                    ResumeGame();
                }
                else
                {
                    ChangeGameState(GameState.Pause);
                }
            }

            // 快捷键
            if (Input.GetKeyDown(KeyCode.B))
            {
                UIManager?.ShowBuildingPanel();
            }
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                UIManager?.ShowUnitPanel();
            }
            
            if (Input.GetKeyDown(KeyCode.T))
            {
                UIManager?.ShowTechTree();
            }
        }

        #endregion

        #region 保存和加载

        public void SaveGame()
        {
            try
            {
                // 保存各系统数据
                ResourceManager?.SaveData();
                TechManager?.SaveTechData();
                LevelManager?.SavePlayerProgress();

                // 保存游戏设置
                PlayerPrefs.SetInt("GameState", (int)CurrentState);
                PlayerPrefs.SetInt("PlayerLevel", LevelManager?.playerLevel ?? 1);
                PlayerPrefs.Save();

                EventSystem.OnGameSaved?.Invoke();
                UIManager?.ShowNotification("游戏已保存");
                
                Debug.Log("Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                // 加载各系统数据
                ResourceManager?.LoadData();
                TechManager?.LoadTechData();
                LevelManager?.LoadPlayerProgress();

                // 加载游戏状态
                if (PlayerPrefs.HasKey("GameState"))
                {
                    CurrentState = (GameState)PlayerPrefs.GetInt("GameState");
                }

                EventSystem.OnGameLoaded?.Invoke();
                Debug.Log("Game loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                // 初始化新游戏
                InitializeNewGame();
            }
        }

        private void InitializeNewGame()
        {
            // 重置所有系统
            ResourceManager?.ResetResources();
            TechManager?.ResetTechData();
            
            if (LevelManager != null)
            {
                LevelManager.playerLevel = 1;
                LevelManager.playerExperience = 0;
                LevelManager.completedLevels.Clear();
                LevelManager.levelStars.Clear();
            }

            CurrentState = GameState.MainMenu;
            
            Debug.Log("New game initialized");
        }

        #endregion

        #region 自动保存

        private void StartAutoSave()
        {
            if (enableAutoSave)
            {
                if (autoSaveCoroutine != null)
                    StopCoroutine(autoSaveCoroutine);
                
                autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }
        }

        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                SaveGame();
            }
        }

        #endregion

        #region 调试功能

        [ContextMenu("Debug - Add Resources")]
        public void DebugAddResources()
        {
            ResourceManager?.AddTestResources();
        }

        [ContextMenu("Debug - Reset Game")]
        public void DebugResetGame()
        {
            InitializeNewGame();
        }

        [ContextMenu("Debug - Save Game")]
        public void DebugSaveGame()
        {
            SaveGame();
        }

        [ContextMenu("Debug - Load Game")]
        public void DebugLoadGame()
        {
            LoadGame();
        }

        #endregion
    }

}