using UnityEngine;
using System.Collections.Generic;

namespace TankCommander.Performance
{
    /// <summary>
    /// Enhanced object pooling system with automatic expansion and cleanup
    /// Improves performance by reusing objects instead of instantiating/destroying
    /// </summary>
    public class EnhancedObjectPooler : MonoBehaviour
    {
        public static EnhancedObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 100;
            public bool autoExpand = true;
            public bool allowShrink = false;
            public float shrinkInterval = 60f;
        }

        [Header("Pool Configuration")]
        [SerializeField] private List<PoolConfig> pools = new List<PoolConfig>();

        [Header("Performance")]
        [SerializeField] private bool useParenting = true;
        [SerializeField] private bool useActiveTracking = true;
        [SerializeField] private int framesBetweenCleanup = 300;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool logPoolOperations = false;

        private Dictionary<string, Pool> poolDictionary = new Dictionary<string, Pool>();
        private int frameCount = 0;

        private class Pool
        {
            public PoolConfig config;
            public Queue<GameObject> availableObjects = new Queue<GameObject>();
            public HashSet<GameObject> activeObjects = new HashSet<GameObject>();
            public Transform parent;
            public int totalCreated = 0;
            public int peakActive = 0;
            public float lastShrinkTime = 0f;

            public int AvailableCount => availableObjects.Count;
            public int ActiveCount => activeObjects.Count;
            public int TotalCount => AvailableCount + ActiveCount;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            foreach (PoolConfig config in pools)
            {
                CreatePool(config);
            }
        }

        private void CreatePool(PoolConfig config)
        {
            if (poolDictionary.ContainsKey(config.poolName))
            {
                Debug.LogWarning($"Pool '{config.poolName}' already exists!");
                return;
            }

            Pool pool = new Pool
            {
                config = config
            };

            // Create parent transform
            if (useParenting)
            {
                GameObject parentObj = new GameObject($"Pool_{config.poolName}");
                parentObj.transform.SetParent(transform);
                pool.parent = parentObj.transform;
            }

            // Pre-instantiate objects
            for (int i = 0; i < config.initialSize; i++)
            {
                GameObject obj = CreatePoolObject(pool);
                pool.availableObjects.Enqueue(obj);
            }

            poolDictionary.Add(config.poolName, pool);

            if (logPoolOperations)
                Debug.Log($"Created pool '{config.poolName}' with {config.initialSize} objects");
        }

        private GameObject CreatePoolObject(Pool pool)
        {
            GameObject obj = Instantiate(pool.config.prefab);
            obj.SetActive(false);

            if (useParenting && pool.parent != null)
            {
                obj.transform.SetParent(pool.parent);
            }

            pool.totalCreated++;
            return obj;
        }

        public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(poolName))
            {
                Debug.LogError($"Pool '{poolName}' doesn't exist!");
                return null;
            }

            Pool pool = poolDictionary[poolName];

            // Get object from pool
            GameObject obj = null;

            if (pool.availableObjects.Count > 0)
            {
                obj = pool.availableObjects.Dequeue();
            }
            else if (pool.config.autoExpand && pool.TotalCount < pool.config.maxSize)
            {
                obj = CreatePoolObject(pool);
                if (logPoolOperations)
                    Debug.Log($"Pool '{poolName}' expanded to {pool.totalCreated} objects");
            }
            else
            {
                Debug.LogWarning($"Pool '{poolName}' is full ({pool.config.maxSize} objects)!");
                return null;
            }

            // Setup object
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            if (useActiveTracking)
            {
                pool.activeObjects.Add(obj);
                if (pool.activeObjects.Count > pool.peakActive)
                    pool.peakActive = pool.activeObjects.Count;
            }

            // Call OnSpawn interface
            IPooledObject pooledObj = obj.GetComponent<IPooledObject>();
            pooledObj?.OnObjectSpawn();

            return obj;
        }

        public void Despawn(string poolName, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(poolName))
            {
                Debug.LogError($"Pool '{poolName}' doesn't exist!");
                return;
            }

            Pool pool = poolDictionary[poolName];

            obj.SetActive(false);

            if (useParenting && pool.parent != null)
            {
                obj.transform.SetParent(pool.parent);
            }

            if (useActiveTracking)
            {
                pool.activeObjects.Remove(obj);
            }

            pool.availableObjects.Enqueue(obj);
        }

        public void DespawnAll(string poolName)
        {
            if (!poolDictionary.ContainsKey(poolName))
                return;

            Pool pool = poolDictionary[poolName];

            if (useActiveTracking)
            {
                List<GameObject> toDeactivate = new List<GameObject>(pool.activeObjects);
                foreach (GameObject obj in toDeactivate)
                {
                    Despawn(poolName, obj);
                }
            }
        }

        private void Update()
        {
            frameCount++;

            // Periodic cleanup
            if (frameCount >= framesBetweenCleanup)
            {
                frameCount = 0;
                PerformCleanup();
            }
        }

        private void PerformCleanup()
        {
            foreach (var kvp in poolDictionary)
            {
                Pool pool = kvp.Value;

                // Shrink oversized pools
                if (pool.config.allowShrink &&
                    Time.time - pool.lastShrinkTime > pool.config.shrinkInterval)
                {
                    ShrinkPool(kvp.Key, pool);
                    pool.lastShrinkTime = Time.time;
                }
            }
        }

        private void ShrinkPool(string poolName, Pool pool)
        {
            // Keep at least initial size + 25% buffer
            int targetSize = Mathf.CeilToInt(pool.config.initialSize * 1.25f);
            int excessCount = pool.AvailableCount - targetSize;

            if (excessCount > 0)
            {
                for (int i = 0; i < excessCount && pool.availableObjects.Count > 0; i++)
                {
                    GameObject obj = pool.availableObjects.Dequeue();
                    Destroy(obj);
                }

                if (logPoolOperations)
                    Debug.Log($"Shrunk pool '{poolName}' by {excessCount} objects");
            }
        }

        public void PrewarmPool(string poolName, int count)
        {
            if (!poolDictionary.ContainsKey(poolName))
                return;

            Pool pool = poolDictionary[poolName];

            for (int i = 0; i < count; i++)
            {
                if (pool.TotalCount >= pool.config.maxSize)
                    break;

                GameObject obj = CreatePoolObject(pool);
                pool.availableObjects.Enqueue(obj);
            }
        }

        public PoolStats GetPoolStats(string poolName)
        {
            if (!poolDictionary.ContainsKey(poolName))
                return null;

            Pool pool = poolDictionary[poolName];

            return new PoolStats
            {
                poolName = poolName,
                available = pool.AvailableCount,
                active = pool.ActiveCount,
                total = pool.TotalCount,
                peak = pool.peakActive,
                maxSize = pool.config.maxSize
            };
        }

        public class PoolStats
        {
            public string poolName;
            public int available;
            public int active;
            public int total;
            public int peak;
            public int maxSize;
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            int y = 200;
            foreach (var kvp in poolDictionary)
            {
                PoolStats stats = GetPoolStats(kvp.Key);
                string info = $"{stats.poolName}: {stats.active}/{stats.total} (Peak: {stats.peak}, Max: {stats.maxSize})";
                GUI.Label(new Rect(10, y, 400, 20), info);
                y += 20;
            }
        }

        [ContextMenu("Log All Pool Stats")]
        public void LogAllPoolStats()
        {
            Debug.Log("========================================");
            Debug.Log("OBJECT POOL STATISTICS");
            Debug.Log("========================================");

            foreach (var kvp in poolDictionary)
            {
                PoolStats stats = GetPoolStats(kvp.Key);
                Debug.Log($"{stats.poolName}:");
                Debug.Log($"  Active: {stats.active}/{stats.total}");
                Debug.Log($"  Available: {stats.available}");
                Debug.Log($"  Peak Usage: {stats.peak}");
                Debug.Log($"  Max Size: {stats.maxSize}");
                Debug.Log($"  Utilization: {(stats.peak * 100f / stats.maxSize):F1}%");
            }

            Debug.Log("========================================");
        }
    }
}
