using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 波次管理器
    public class WaveManager : MonoBehaviour
    {
        public WaveData currentWave;
        public int currentWaveIndex = 0;
        public List<Unit> spawnedUnits = new List<Unit>();
        private Coroutine waveCoroutine;

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        public void StartWave(WaveData wave, int waveIndex)
        {
            currentWave = wave;
            currentWaveIndex = waveIndex;

            if (waveCoroutine != null)
                StopCoroutine(waveCoroutine);

            waveCoroutine = StartCoroutine(WaveProcess());
        }

        private IEnumerator WaveProcess()
        {
            // 等待波次开始延迟
            yield return new WaitForSeconds(currentWave.startDelay);

            OnWaveStarted?.Invoke(currentWaveIndex);

            // 生成单位
            foreach (var spawnData in currentWave.unitsToSpawn)
            {
                for (int i = 0; i < spawnData.count; i++)
                {
                    yield return new WaitForSeconds(spawnData.spawnDelay);
                    SpawnUnit(spawnData);
                }
            }

            // 等待所有单位被消灭
            yield return new WaitUntil(() => spawnedUnits.Count == 0);

            OnWaveCompleted?.Invoke(currentWaveIndex);
            spawnedUnits.Clear();
        }

        private void SpawnUnit(UnitSpawnData spawnData)
        {
            Unit unit = GameManager.Instance?.UnitManager?.DeployUnit(spawnData.unitId, spawnData.spawnPosition);
            if (unit != null)
            {
                spawnedUnits.Add(unit);
                unit.OnUnitDied += (unit) => spawnedUnits.Remove(unit);
            }
        }

        public bool IsWaveComplete()
        {
            return spawnedUnits.Count == 0;
        }

        public void StopAllWaves()
        {
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
                waveCoroutine = null;
            }

            // 清理生成的单位
            foreach (var unit in spawnedUnits)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
            spawnedUnits.Clear();
        }
    }
}