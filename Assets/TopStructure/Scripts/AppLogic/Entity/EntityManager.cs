using System.Collections.Generic;
using UnityEngine;
using EmpireClash;

public class MapManager : MonoBehaviour
{
    public Vector3 GetNearestGridPosition(Vector3 worldPosition)
    {
        var node = GridManager.Instance.GetNodeFromWorldPos(worldPosition);
        return GridManager.Instance.GetWorldPosition(node.x, node.y);
    }
    public bool CanPlaceBuilding(BuildingData buildingData, Vector3 position)
    {
        var gridSystem = GridManager.Instance;
        var pos = gridSystem.GetNodeFromWorldPos(position);
        return gridSystem.CheckRegionPlaceable(pos.x, pos.y, buildingData.size.x, buildingData.size.y);
    }
    public void PlaceBuilding(BuildingData building, Vector3 position)
    {
        var gridSystem = GridManager.Instance;
        var pos = gridSystem.GetNodeFromWorldPos(position);

        gridSystem.SetRegionPlaced(pos.x, pos.y, building.size.x, building.size.y, true);
        // building.transform.position = position;
    }
}
public class EntityManager : MonoSingleton<EntityManager>
{
}
public class AudioManager : MonoBehaviour
{
    public void PlayMusic(string musicName){}
}
//     private float _deltaTime;
//     private float _longDeltaTime;
//     private float _accumulatedTime;
//     private float _accumulatedLongTime;
//     private readonly List<EntityBase> _entities = new List<EntityBase>();

//     private void Awake()
//     {
//         _deltaTime = Constant.DEFAULT_DELTA_TIME;
//         _longDeltaTime = Constant.LONG_DELTA_TIME;
//     }

//     public void Register(EntityBase entity)
//     {
//         if (entity == null) return;
//         if (!_entities.Contains(entity))
//         {
//             _entities.Add(entity);
//             // entity.OnInit();      // 注册时初始化一次
//         }
//     }

//     public void Unregister(EntityBase entity)
//     {
//         if (entity == null) return;
//         _entities.Remove(entity);
//     }

//     private void Update()
//     {
//         float dt = Time.deltaTime;
//         _accumulatedLongTime += dt;
//         _accumulatedTime += dt;

//         // 用 for 避免遍历中修改列表的问题
//         for (int i = 0; i < _entities.Count; i++)
//         {
//             var e = _entities[i];
//             if (e != null && e.isActiveAndEnabled)
//                 e.OnUpdate(dt);
//         }
//         if (_accumulatedTime >= _deltaTime)
//         {
//             _accumulatedTime = 0;
//             for (int i = 0; i < _entities.Count; i++)
//             {
//                 var e = _entities[i];
//                 if (e != null && e.isActiveAndEnabled)
//                     e.OnIntervalUpdate(_deltaTime);
//             }
//         }
//         if (_accumulatedLongTime >= _longDeltaTime)
//         {
//             _accumulatedLongTime = 0;
//             for (int i = 0; i < _entities.Count; i++)
//             {
//                 var e = _entities[i];
//                 if (e != null && e.isActiveAndEnabled)
//                     e.OnIntervalLateUpdate(_longDeltaTime);
//             }
//         }
//     }
// }