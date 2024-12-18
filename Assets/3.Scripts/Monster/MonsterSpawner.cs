using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
    public GameObject monsterPrefab; 
    public int spawnCount;           
}

public class MonsterSpawner : MonoBehaviour
{
    private List<MonsterSpawnData> monsterList = new List<MonsterSpawnData>();
    private List<Transform> activeSpawnPoints = new List<Transform>();
    private float spawnInterval = 0.5f;
    private bool isSpawning = false;
    private int currentSpawnIndex = 0;
    private Dictionary<GameObject, int> remainingSpawnCount = new Dictionary<GameObject, int>();
    private List<GameObject> weightedMonsterPool = new List<GameObject>();

    public event System.Action OnSpawnComplete;
    public event System.Action<GameObject> OnMonsterSpawned;
    public event System.Action<GameObject> OnMonsterDestroyed;

    private bool spawnComplete = false;

    private void InitializeSpawnCounts()
    {
        remainingSpawnCount.Clear();
        foreach (var spawnData in monsterList)
        {
            if (spawnData.monsterPrefab != null)
            {
                if (remainingSpawnCount.ContainsKey(spawnData.monsterPrefab))
                {
                    remainingSpawnCount[spawnData.monsterPrefab] += spawnData.spawnCount;
                }
                else
                {
                    remainingSpawnCount[spawnData.monsterPrefab] = spawnData.spawnCount;
                }
            }
        }
    }

    private void CreateWeightedMonsterPool()
    {
        weightedMonsterPool.Clear();
        foreach (var spawnData in monsterList)
        {
            if (spawnData.monsterPrefab != null && spawnData.spawnCount > 0)
            {
                for (int i = 0; i < spawnData.spawnCount; i++)
                {
                    weightedMonsterPool.Add(spawnData.monsterPrefab);
                }
            }
        }
    }

    private void SpawnMonster()
    {
        if (activeSpawnPoints == null || activeSpawnPoints.Count == 0)
        {
            return;
        }

        currentSpawnIndex = (currentSpawnIndex + 1) % activeSpawnPoints.Count;
        Transform selectedSpawnPoint = activeSpawnPoints[currentSpawnIndex];

        if (weightedMonsterPool.Count == 0)
        {
            StopSpawning();
            return;
        }

        GameObject selectedMonsterPrefab = weightedMonsterPool[Random.Range(0, weightedMonsterPool.Count)];

        if (remainingSpawnCount.ContainsKey(selectedMonsterPrefab))
        {
            remainingSpawnCount[selectedMonsterPrefab]--;
            if (remainingSpawnCount[selectedMonsterPrefab] <= 0)
            {
                weightedMonsterPool.RemoveAll(m => m == selectedMonsterPrefab);
            }

            Monster monster = ObjectManager.Instance.Spawn<Monster>(selectedMonsterPrefab, selectedSpawnPoint.position);
            

            if (monster != null)
            {
                monster.Initialize(selectedSpawnPoint);
                //monster.spawnPos = selectedSpawnPoint;
                monster.IsSpawnDirect = true;
                monster.OnDead -= HandleMonsterDestroyed;
                monster.OnDead += HandleMonsterDestroyed;

                void HandleMonsterDestroyed()
                {
                    OnMonsterDestroyed?.Invoke(monster.gameObject);
                    monster.OnDead -= HandleMonsterDestroyed;
                    ObjectManager.Instance.Despawn(monster);
                }

                OnMonsterSpawned?.Invoke(monster.gameObject);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        spawnComplete = false;
        while (isSpawning)
        {
            SpawnMonster();
            yield return new WaitForSeconds(spawnInterval);
        }

        spawnComplete = true;
        OnSpawnComplete?.Invoke();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            currentSpawnIndex = -1;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void SetMonsterData(
        List<MonsterSpawnData> newMonsterList,
        float newSpawnInterval,
        List<Transform> activeSpawnPoints)
    {
        monsterList = newMonsterList;
        spawnInterval = newSpawnInterval;
        this.activeSpawnPoints = activeSpawnPoints;
        InitializeSpawnCounts();
        CreateWeightedMonsterPool();
    }

    public bool IsSpawningComplete()
    {
        return spawnComplete;
    }

}
