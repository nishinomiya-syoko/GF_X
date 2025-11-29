// using UnityEngine;
// using Procedure;

// namespace EmpireClash
// {
//     // 演示场景设置
//     public class DemoSceneSetup : ProcedureBase
//     {
//         [Header("演示设置")]
//         public bool setupDemoBuildings = true;
//         public bool setupDemoUnits = true;
//         public bool setupDemoResources = true;
//         public override void OnEnter()
//         {
//             Debug.Log("Starting demo setup...");
//             SetupDemoContent();
//         }

//         private void SetupDemoContent()
//         {
//             // 确保游戏管理器存在
//             if (GameManager.Instance == null)
//             {
//                 Debug.LogError("GameManager not found! Please ensure GameManager is in the scene.");
//                 return;
//             }

//             // 设置演示资源
//             if (setupDemoResources)
//             {
//                 SetupDemoResources();
//             }

//             // 设置演示建筑
//             if (setupDemoBuildings)
//             {
//                 SetupDemoBuildings();
//             }

//             // 设置演示单位
//             if (setupDemoUnits)
//             {
//                 SetupDemoUnits();
//             }

//             Debug.Log("Demo content setup completed");
//         }

//         private void SetupDemoResources()
//         {
//             if (GameManager.Instance.ResourceManager != null)
//             {
//                 // 添加演示资源
//                 GameManager.Instance.ResourceManager.AddResource(ResourceType.Gold, 5000);
//                 GameManager.Instance.ResourceManager.AddResource(ResourceType.Elixir, 5000);
//                 GameManager.Instance.ResourceManager.AddResource(ResourceType.DarkElixir, 1000);
//                 GameManager.Instance.ResourceManager.AddResource(ResourceType.Gems, 500);
//             }
//         }

//         private void SetupDemoBuildings()
//         {
//             if (GameManager.Instance.BuildingManager != null && GameManager.Instance.MapManager != null)
//             {
//                 // 创建演示建筑数据
//                 BuildingData goldMineData = CreateDemoBuildingData("GoldMine", "金矿", BuildingType.Resource, 2, 2);
//                 BuildingData elixirCollectorData = CreateDemoBuildingData("ElixirCollector", "圣水收集器", BuildingType.Resource, 2, 2);
//                 BuildingData cannonData = CreateDemoBuildingData("Cannon", "加农炮", BuildingType.Defense, 2, 2);
//                 BuildingData barracksData = CreateDemoBuildingData("Barracks", "兵营", BuildingType.Military, 3, 3);

//                 // 放置演示建筑
//                 PlaceDemoBuilding(goldMineData, new Vector3(0, 0, 0));
//                 PlaceDemoBuilding(elixirCollectorData, new Vector3(4, 0, 0));
//                 PlaceDemoBuilding(cannonData, new Vector3(0, 4, 0));
//                 PlaceDemoBuilding(barracksData, new Vector3(0, 0, 4));
//             }
//         }

//         private void SetupDemoUnits()
//         {
//             if (GameManager.Instance.UnitManager != null)
//             {
//                 // 创建演示单位数据
//                 UnitData warriorData = CreateDemoUnitData("Warrior", "战士", UnitType.Infantry);
//                 UnitData archerData = CreateDemoUnitData("Archer", "弓箭手", UnitType.Ranged);

//                 // 部署演示单位
//                 DeployDemoUnit(warriorData, new Vector3(10, 0, 0));
//                 DeployDemoUnit(archerData, new Vector3(11, 0, 0));
//             }
//         }

//         private BuildingData CreateDemoBuildingData(string id, string name, BuildingType type, int width, int height)
//         {
//             BuildingData data = ScriptableObject.CreateInstance<BuildingData>();
//             data.id = id;
//             data.buildingName = name;
//             data.buildingType = type;
//             data.size = new Vector2Int(width, height);
//             data.buildTime = 5; // 5秒建造时间
//             data.maxLevel = 5;
//             data.hitPoints = 100;
//             data.prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
//             // 设置建造成本
//             data.buildCosts = new ResourceCost[]
//             {
//                 new ResourceCost(ResourceType.Gold, 100)
//             };

//             return data;
//         }

//         private UnitData CreateDemoUnitData(string id, string name, UnitType type)
//         {
//             UnitData data = ScriptableObject.CreateInstance<UnitData>();
//             data.id = id;
//             data.unitName = name;
//             data.unitType = type;
//             data.hitPoints = 100;
//             data.damage = 25;
//             data.movementSpeed = 3f;
//             data.attackSpeed = 1f;
//             data.attackRange = 2f;
//             data.trainingTime = 10; // 10秒训练时间
//             data.prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            
//             // 设置训练成本
//             data.trainingCosts = new ResourceCost[]
//             {
//                 new ResourceCost(ResourceType.Elixir, 50)
//             };

//             return data;
//         }

//         private void PlaceDemoBuilding(BuildingData data, Vector3 position)
//         {
//             // 创建简单的建筑预制体
//             GameObject buildingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             buildingObj.name = data.buildingName;
//             buildingObj.transform.position = position;
//             buildingObj.transform.localScale = new Vector3(data.size.x, 2, data.size.y);

//             // 添加建筑组件
//             Building building = buildingObj.AddComponent<Building>();
//             building.data = data;
//             building.currentLevel = 1;

//             // 注册到建筑管理器
//             GameManager.Instance.BuildingManager.PlaceBuilding(data, position);
//         }

//         private void DeployDemoUnit(UnitData data, Vector3 position)
//         {
//             // 创建简单的单位预制体
//             GameObject unitObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//             unitObj.name = data.unitName;
//             unitObj.transform.position = position;

//             // 添加单位组件
//             Unit unit = unitObj.AddComponent<Unit>();
//             unit.data = data;
//             unit.currentLevel = 1;

//             // 添加AI控制器
//             UnitAIController aiController = unitObj.AddComponent<UnitAIController>();

//             // 添加导航代理
//             UnityEngine.AI.NavMeshAgent agent = unitObj.AddComponent<UnityEngine.AI.NavMeshAgent>();
//             agent.speed = data.movementSpeed;
//             agent.stoppingDistance = data.attackRange * 0.8f;

//             // 注册到单位管理器
//             GameManager.Instance.UnitManager.ActiveUnits.Add(unit);
//         }

//         [ContextMenu("Setup Demo Content")]
//         public void ManualSetup()
//         {
//             SetupDemoContent();
//         }

//         [ContextMenu("Clear Demo Content")]
//         public void ClearDemoContent()
//         {
//             // 清理演示建筑
//             if (GameManager.Instance.BuildingManager != null)
//             {
//                 foreach (var building in GameManager.Instance.BuildingManager.PlacedBuildings)
//                 {
//                     if (building != null)
//                         Destroy(building.gameObject);
//                 }
//                 GameManager.Instance.BuildingManager.PlacedBuildings.Clear();
//             }

//             // 清理演示单位
//             if (GameManager.Instance.UnitManager != null)
//             {
//                 foreach (var unit in GameManager.Instance.UnitManager.ActiveUnits)
//                 {
//                     if (unit != null)
//                         Destroy(unit.gameObject);
//                 }
//                 GameManager.Instance.UnitManager.ActiveUnits.Clear();
//                 GameManager.Instance.UnitManager.CurrentArmySize = 0;
//             }

//             Debug.Log("Demo content cleared");
//         }
//     }
// }