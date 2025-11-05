using UnityEngine;
using System.Collections.Generic;

namespace TankCommander
{
    /// <summary>
    /// Object pooling system for efficient spawning of projectiles and effects
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 10;
        }

        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        /// <summary>
        /// Spawn an object from the pool
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag '{tag}' doesn't exist!");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
            pooledObj?.OnObjectSpawn();

            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        /// <summary>
        /// Add a new pool at runtime
        /// </summary>
        public void AddPool(string tag, GameObject prefab, int size)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag '{tag}' already exists!");
                return;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(tag, objectPool);
        }
    }

    /// <summary>
    /// Interface for pooled objects to implement
    /// </summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
    }
}
