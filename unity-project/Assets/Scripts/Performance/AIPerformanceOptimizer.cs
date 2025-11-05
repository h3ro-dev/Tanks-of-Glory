using UnityEngine;
using System.Collections.Generic;

namespace TankCommander.Performance
{
    /// <summary>
    /// Optimizes AI update frequency based on distance from player
    /// Reduces CPU load by updating distant AI less frequently
    /// </summary>
    public class AIPerformanceOptimizer : MonoBehaviour
    {
        [Header("Update Frequency Settings")]
        [SerializeField] private float closeRangeDistance = 30f;
        [SerializeField] private float mediumRangeDistance = 60f;
        [SerializeField] private float farRangeDistance = 100f;

        [SerializeField] private float closeRangeUpdateRate = 0.0f; // Every frame
        [SerializeField] private float mediumRangeUpdateRate = 0.1f; // 10 times per second
        [SerializeField] private float farRangeUpdateRate = 0.5f; // 2 times per second
        [SerializeField] private float veryFarRangeUpdateRate = 1.0f; // Once per second

        [Header("Performance Metrics")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private int activeAICount = 0;
        [SerializeField] private int optimizedAICount = 0;

        private static AIPerformanceOptimizer instance;
        public static AIPerformanceOptimizer Instance => instance;

        private Dictionary<TankAI, AIUpdateSchedule> aiSchedules = new Dictionary<TankAI, AIUpdateSchedule>();
        private Transform playerTransform;

        private class AIUpdateSchedule
        {
            public float nextUpdateTime;
            public float updateInterval;
            public float distanceToPlayer;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Register all AI tanks
            RegisterAllAI();
        }

        private void RegisterAllAI()
        {
            TankAI[] allAI = FindObjectsOfType<TankAI>();
            foreach (TankAI ai in allAI)
            {
                RegisterAI(ai);
            }

            activeAICount = allAI.Length;
        }

        public void RegisterAI(TankAI ai)
        {
            if (!aiSchedules.ContainsKey(ai))
            {
                aiSchedules[ai] = new AIUpdateSchedule
                {
                    nextUpdateTime = Time.time,
                    updateInterval = 0f,
                    distanceToPlayer = 0f
                };
            }
        }

        public void UnregisterAI(TankAI ai)
        {
            if (aiSchedules.ContainsKey(ai))
            {
                aiSchedules.Remove(ai);
            }
        }

        public bool ShouldUpdateAI(TankAI ai)
        {
            if (playerTransform == null) return true;
            if (!aiSchedules.ContainsKey(ai)) return true;

            AIUpdateSchedule schedule = aiSchedules[ai];

            // Update distance
            schedule.distanceToPlayer = Vector3.Distance(ai.transform.position, playerTransform.position);

            // Determine update interval based on distance
            if (schedule.distanceToPlayer < closeRangeDistance)
            {
                schedule.updateInterval = closeRangeUpdateRate;
            }
            else if (schedule.distanceToPlayer < mediumRangeDistance)
            {
                schedule.updateInterval = mediumRangeUpdateRate;
            }
            else if (schedule.distanceToPlayer < farRangeDistance)
            {
                schedule.updateInterval = farRangeUpdateRate;
            }
            else
            {
                schedule.updateInterval = veryFarRangeUpdateRate;
            }

            // Check if it's time to update
            if (Time.time >= schedule.nextUpdateTime)
            {
                schedule.nextUpdateTime = Time.time + schedule.updateInterval;
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (showDebugInfo)
            {
                optimizedAICount = 0;
                foreach (var schedule in aiSchedules.Values)
                {
                    if (schedule.updateInterval > 0f)
                    {
                        optimizedAICount++;
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (showDebugInfo)
            {
                GUI.Label(new Rect(10, 100, 300, 20), $"Active AI: {activeAICount}");
                GUI.Label(new Rect(10, 120, 300, 20), $"Optimized AI: {optimizedAICount}");
                GUI.Label(new Rect(10, 140, 300, 20), $"CPU Savings: {(optimizedAICount * 100f / Mathf.Max(activeAICount, 1)):F1}%");
            }
        }
    }
}
