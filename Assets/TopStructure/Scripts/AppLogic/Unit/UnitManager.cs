using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace EmpireClash
{
    // 单位管理器
    public class UnitManager : MonoBehaviour
    {
        [Header("单位数据库")]
        public UnitDatabase unitDatabase;

        [Header("军队设置")]
        public int maxArmySize = 200;
        public Transform spawnPoint;

        private List<Unit> activeUnits = new List<Unit>();
        public List<Unit> ActiveUnits
        {
            get { return activeUnits; }
        }
        private List<UnitData> trainingQueue = new List<UnitData>();
        private int currentArmySize = 0;
        public int CurrentArmySize
        {
            get { return currentArmySize; }
            set { currentArmySize = value; }
        }
        private Coroutine trainingCoroutine;

        // 事件
        public event Action<Unit> OnUnitTrained;
        public event Action<Unit> OnUnitDeployed;
        public event Action<Unit> OnUnitDied;

        void Start()
        {
            if (unitDatabase == null)
            {
                unitDatabase = Resources.Load<UnitDatabase>("UnitDatabase");
            }

            if (spawnPoint == null)
                spawnPoint = transform;
        }


        #region 单位训练

        public void TrainUnit(string unitId, int count = 1)
        {
            UnitData unitData = unitDatabase.GetUnitData(unitId);
            if (unitData != null)
            {
                for (int i = 0; i < count; i++)
                {
                    TrainUnit(unitData);
                }
            }
        }

        public void TrainUnit(UnitData unitData)
        {
            // 检查资源
            if (!GameManager.Instance?.ResourceManager?.CanAfford(unitData.trainingCosts) == true)
            {
                Debug.LogWarning("Not enough resources to train unit!");
                return;
            }

            // 检查军队容量
            if (currentArmySize + unitData.housingSpace > maxArmySize)
            {
                Debug.LogWarning("Army capacity exceeded!");
                return;
            }

            // 花费资源
            GameManager.Instance?.ResourceManager?.SpendResources(unitData.trainingCosts);

            // 加入训练队列
            trainingQueue.Add(unitData);

            // 开始训练
            if (trainingCoroutine == null)
            {
                trainingCoroutine = StartCoroutine(TrainingProcess());
            }
        }

        private IEnumerator TrainingProcess()
        {
            while (trainingQueue.Count > 0)
            {
                UnitData unitData = trainingQueue[0];
                float trainingTime = unitData.trainingTime;
                float elapsedTime = 0f;

                // 训练计时
                while (elapsedTime < trainingTime)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // 训练完成
                trainingQueue.RemoveAt(0);
                OnUnitTrained?.Invoke(null); // 可以传递训练完成的单位数据
            }

            trainingCoroutine = null;
        }

        #endregion

        #region 单位部署

        public Unit DeployUnit(string unitId, Vector3 position)
        {
            UnitData unitData = unitDatabase.GetUnitData(unitId);
            if (unitData != null)
            {
                return DeployUnit(unitData, position);
            }
            return null;
        }

        public Unit DeployUnit(UnitData unitData, Vector3 position)
        {
            if (unitData.prefab == null)
                return null;

            // 检查军队容量
            if (currentArmySize + unitData.housingSpace > maxArmySize)
            {
                Debug.LogWarning("Army capacity exceeded!");
                return null;
            }

            // 实例化单位
            GameObject unitObj = Instantiate(unitData.prefab, position, Quaternion.identity);
            Unit unit = unitObj.GetComponent<Unit>();

            if (unit == null)
            {
                unit = unitObj.AddComponent<Unit>();
            }

            // 设置单位数据
            unit.data = unitData;
            unit.currentLevel = 1;
            unit.currentState = UnitState.Idle;

            // 注册到单位列表
            activeUnits.Add(unit);
            currentArmySize += unitData.housingSpace;

            // 注册事件
            unit.OnUnitDied += OnUnitDiedHandler;

            OnUnitDeployed?.Invoke(unit);
            GameManager.Instance?.EventSystem?.OnUnitDeployed?.Invoke(unit);

            return unit;
        }

        #endregion

        #region 事件处理

        private void OnUnitDiedHandler(Unit unit)
        {
            activeUnits.Remove(unit);
            currentArmySize -= unit.data.housingSpace;
            OnUnitDied?.Invoke(unit);
        }

        #endregion

        #region 辅助方法

        public List<Unit> GetUnitsOfType(UnitType type)
        {
            return activeUnits.FindAll(u => u.data.unitType == type);
        }

        public int GetUnitCount(UnitType type)
        {
            return activeUnits.FindAll(u => u.data.unitType == type).Count;
        }

        public void ClearAllUnits()
        {
            foreach (var unit in activeUnits)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
            activeUnits.Clear();
            currentArmySize = 0;
        }

        #endregion
    }
}