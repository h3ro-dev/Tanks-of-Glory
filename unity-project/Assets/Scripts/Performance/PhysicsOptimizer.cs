using UnityEngine;
using System.Collections.Generic;

namespace TankCommander.Performance
{
    /// <summary>
    /// Optimizes physics calculations by managing collision detection and simulation
    /// Reduces physics overhead for distant or inactive objects
    /// </summary>
    public class PhysicsOptimizer : MonoBehaviour
    {
        [Header("Distance-Based Optimization")]
        [SerializeField] private bool enableDistanceOptimization = true;
        [SerializeField] private float fullPhysicsDistance = 50f;
        [SerializeField] private float reducedPhysicsDistance = 100f;
        [SerializeField] private float frozenPhysicsDistance = 150f;

        [Header("Sleep Optimization")]
        [SerializeField] private bool enableSleepOptimization = true;
        [SerializeField] private float sleepThreshold = 0.14f;
        [SerializeField] private float sleepTimeout = 0.5f;

        [Header("Collision Optimization")]
        [SerializeField] private bool optimizeCollisionDetection = true;
        [SerializeField] private float updateInterval = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private Transform playerTransform;
        private List<OptimizedRigidbody> trackedRigidbodies = new List<OptimizedRigidbody>();
        private float lastUpdateTime = 0f;

        private class OptimizedRigidbody
        {
            public Rigidbody rb;
            public CollisionDetectionMode originalDetectionMode;
            public RigidbodyInterpolation originalInterpolation;
            public float distanceToPlayer;
            public PhysicsState currentState;
        }

        private enum PhysicsState
        {
            Full,           // Full physics simulation
            Reduced,        // Reduced update rate
            Frozen          // No physics updates
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Configure global physics settings
            OptimizeGlobalPhysics();

            // Register all rigidbodies
            RegisterAllRigidbodies();
        }

        private void OptimizeGlobalPhysics()
        {
            // Set appropriate physics settings
            Physics.defaultSolverIterations = 6;        // Lower for performance (default 6)
            Physics.defaultSolverVelocityIterations = 1; // Lower for performance (default 1)
            Physics.sleepThreshold = sleepThreshold;
            Physics.bounceThreshold = 2f;                // Higher reduces collision processing

            // Set fixed timestep for consistent physics
            Time.fixedDeltaTime = 0.02f; // 50 Hz physics (default 0.02)

            if (showDebugInfo)
            {
                Debug.Log("Global Physics Settings Optimized:");
                Debug.Log($"  Fixed Timestep: {Time.fixedDeltaTime}s ({1f / Time.fixedDeltaTime} Hz)");
                Debug.Log($"  Sleep Threshold: {Physics.sleepThreshold}");
                Debug.Log($"  Solver Iterations: {Physics.defaultSolverIterations}");
            }
        }

        private void RegisterAllRigidbodies()
        {
            // Find all rigidbodies in scene (excluding player)
            Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();

            foreach (Rigidbody rb in allRigidbodies)
            {
                // Skip player rigidbody
                if (rb.CompareTag("Player"))
                    continue;

                // Skip kinematic rigidbodies
                if (rb.isKinematic)
                    continue;

                RegisterRigidbody(rb);
            }

            if (showDebugInfo)
            {
                Debug.Log($"Registered {trackedRigidbodies.Count} rigidbodies for optimization");
            }
        }

        public void RegisterRigidbody(Rigidbody rb)
        {
            if (rb == null) return;

            OptimizedRigidbody optRb = new OptimizedRigidbody
            {
                rb = rb,
                originalDetectionMode = rb.collisionDetectionMode,
                originalInterpolation = rb.interpolation,
                distanceToPlayer = 0f,
                currentState = PhysicsState.Full
            };

            trackedRigidbodies.Add(optRb);
        }

        public void UnregisterRigidbody(Rigidbody rb)
        {
            trackedRigidbodies.RemoveAll(o => o.rb == rb);
        }

        private void Update()
        {
            if (!enableDistanceOptimization) return;
            if (playerTransform == null) return;

            // Update at intervals to save CPU
            if (Time.time - lastUpdateTime < updateInterval)
                return;

            lastUpdateTime = Time.time;

            // Update all tracked rigidbodies
            foreach (OptimizedRigidbody optRb in trackedRigidbodies)
            {
                if (optRb.rb == null) continue;

                UpdateRigidbodyState(optRb);
            }
        }

        private void UpdateRigidbodyState(OptimizedRigidbody optRb)
        {
            // Calculate distance to player
            optRb.distanceToPlayer = Vector3.Distance(optRb.rb.position, playerTransform.position);

            // Determine appropriate physics state
            PhysicsState targetState;

            if (optRb.distanceToPlayer < fullPhysicsDistance)
            {
                targetState = PhysicsState.Full;
            }
            else if (optRb.distanceToPlayer < reducedPhysicsDistance)
            {
                targetState = PhysicsState.Reduced;
            }
            else if (optRb.distanceToPlayer < frozenPhysicsDistance)
            {
                targetState = PhysicsState.Frozen;
            }
            else
            {
                targetState = PhysicsState.Frozen;
            }

            // Apply state if changed
            if (targetState != optRb.currentState)
            {
                ApplyPhysicsState(optRb, targetState);
                optRb.currentState = targetState;
            }
        }

        private void ApplyPhysicsState(OptimizedRigidbody optRb, PhysicsState state)
        {
            switch (state)
            {
                case PhysicsState.Full:
                    // Full physics simulation
                    optRb.rb.collisionDetectionMode = optRb.originalDetectionMode;
                    optRb.rb.interpolation = optRb.originalInterpolation;
                    optRb.rb.isKinematic = false;
                    if (enableSleepOptimization)
                        optRb.rb.WakeUp();
                    break;

                case PhysicsState.Reduced:
                    // Reduced physics
                    optRb.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    optRb.rb.interpolation = RigidbodyInterpolation.None;
                    optRb.rb.isKinematic = false;
                    if (enableSleepOptimization && optRb.rb.velocity.sqrMagnitude < 0.1f)
                        optRb.rb.Sleep();
                    break;

                case PhysicsState.Frozen:
                    // Frozen - no physics
                    optRb.rb.isKinematic = true;
                    optRb.rb.Sleep();
                    break;
            }
        }

        [ContextMenu("Force Sleep All Distant Objects")]
        public void ForceSleepDistantObjects()
        {
            int sleptCount = 0;

            foreach (OptimizedRigidbody optRb in trackedRigidbodies)
            {
                if (optRb.rb != null && optRb.distanceToPlayer > reducedPhysicsDistance)
                {
                    optRb.rb.Sleep();
                    sleptCount++;
                }
            }

            Debug.Log($"Put {sleptCount} distant rigidbodies to sleep");
        }

        [ContextMenu("Wake All Objects")]
        public void WakeAllObjects()
        {
            foreach (OptimizedRigidbody optRb in trackedRigidbodies)
            {
                if (optRb.rb != null)
                {
                    optRb.rb.WakeUp();
                    optRb.rb.isKinematic = false;
                }
            }

            Debug.Log("Woke up all rigidbodies");
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            int fullCount = 0;
            int reducedCount = 0;
            int frozenCount = 0;

            foreach (OptimizedRigidbody optRb in trackedRigidbodies)
            {
                if (optRb.rb == null) continue;

                switch (optRb.currentState)
                {
                    case PhysicsState.Full:
                        fullCount++;
                        break;
                    case PhysicsState.Reduced:
                        reducedCount++;
                        break;
                    case PhysicsState.Frozen:
                        frozenCount++;
                        break;
                }
            }

            int y = 300;
            GUI.Label(new Rect(10, y, 300, 20), "=== PHYSICS OPTIMIZATION ===");
            GUI.Label(new Rect(10, y + 20, 300, 20), $"Full Physics: {fullCount}");
            GUI.Label(new Rect(10, y + 40, 300, 20), $"Reduced Physics: {reducedCount}");
            GUI.Label(new Rect(10, y + 60, 300, 20), $"Frozen: {frozenCount}");
            GUI.Label(new Rect(10, y + 80, 300, 20), $"Total: {trackedRigidbodies.Count}");
        }

        private void OnDestroy()
        {
            // Restore all rigidbodies to original state
            foreach (OptimizedRigidbody optRb in trackedRigidbodies)
            {
                if (optRb.rb != null)
                {
                    optRb.rb.collisionDetectionMode = optRb.originalDetectionMode;
                    optRb.rb.interpolation = optRb.originalInterpolation;
                    optRb.rb.isKinematic = false;
                    optRb.rb.WakeUp();
                }
            }
        }
    }
}
