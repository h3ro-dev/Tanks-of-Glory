using UnityEngine;
using System.Collections.Generic;

namespace TankCommander.Performance
{
    /// <summary>
    /// Simple Level of Detail (LOD) system for tanks and objects
    /// Reduces visual complexity based on distance from camera
    /// </summary>
    public class SimpleLODSystem : MonoBehaviour
    {
        [System.Serializable]
        public class LODLevel
        {
            public string name;
            public float distance;
            public bool showMeshDetails = true;
            public bool enableShadows = true;
            public bool enableParticles = true;
            public int textureQuality = 0; // 0 = full, 1 = half, 2 = quarter
        }

        [Header("LOD Configuration")]
        [SerializeField] private LODLevel[] lodLevels = new LODLevel[]
        {
            new LODLevel { name = "High", distance = 30f, showMeshDetails = true, enableShadows = true, enableParticles = true, textureQuality = 0 },
            new LODLevel { name = "Medium", distance = 60f, showMeshDetails = true, enableShadows = false, enableParticles = true, textureQuality = 1 },
            new LODLevel { name = "Low", distance = 100f, showMeshDetails = false, enableShadows = false, enableParticles = false, textureQuality = 2 },
        };

        [Header("Target")]
        [SerializeField] private Transform targetObject; // Object to apply LOD to
        [SerializeField] private bool autoDetectDetails = true;

        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.5f;

        private Camera mainCamera;
        private float lastUpdateTime = 0f;
        private int currentLODLevel = 0;

        // Cached components
        private MeshRenderer[] meshRenderers;
        private ParticleSystem[] particleSystems;
        private Light[] lights;

        private void Start()
        {
            mainCamera = Camera.main;

            if (targetObject == null)
                targetObject = transform;

            if (autoDetectDetails)
            {
                CacheComponents();
            }

            // Initial LOD application
            UpdateLOD();
        }

        private void CacheComponents()
        {
            meshRenderers = targetObject.GetComponentsInChildren<MeshRenderer>();
            particleSystems = targetObject.GetComponentsInChildren<ParticleSystem>();
            lights = targetObject.GetComponentsInChildren<Light>();
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime < updateInterval)
                return;

            lastUpdateTime = Time.time;
            UpdateLOD();
        }

        private void UpdateLOD()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            float distance = Vector3.Distance(targetObject.position, mainCamera.transform.position);

            // Determine appropriate LOD level
            int newLODLevel = lodLevels.Length - 1; // Start with lowest LOD

            for (int i = 0; i < lodLevels.Length; i++)
            {
                if (distance < lodLevels[i].distance)
                {
                    newLODLevel = i;
                    break;
                }
            }

            // Apply LOD if changed
            if (newLODLevel != currentLODLevel)
            {
                ApplyLODLevel(newLODLevel);
                currentLODLevel = newLODLevel;
            }
        }

        private void ApplyLODLevel(int level)
        {
            if (level < 0 || level >= lodLevels.Length)
                return;

            LODLevel lod = lodLevels[level];

            // Apply mesh settings
            if (meshRenderers != null)
            {
                foreach (MeshRenderer renderer in meshRenderers)
                {
                    if (renderer == null) continue;

                    // Shadows
                    if (lod.enableShadows)
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    else
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    // Texture quality (simplified)
                    if (!lod.showMeshDetails)
                    {
                        // Could disable secondary details here
                        // For now, just toggle the renderer on lower LODs if needed
                    }
                }
            }

            // Apply particle settings
            if (particleSystems != null)
            {
                foreach (ParticleSystem ps in particleSystems)
                {
                    if (ps == null) continue;

                    if (lod.enableParticles)
                    {
                        if (!ps.isPlaying && ps.gameObject.activeInHierarchy)
                            ps.Play();
                    }
                    else
                    {
                        if (ps.isPlaying)
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            // Apply light settings
            if (lights != null)
            {
                foreach (Light light in lights)
                {
                    if (light == null) continue;

                    // Disable lights at lower LODs
                    light.enabled = (level <= 1); // Only high and medium LOD
                }
            }
        }

        public void ForceLODLevel(int level)
        {
            if (level >= 0 && level < lodLevels.Length)
            {
                ApplyLODLevel(level);
                currentLODLevel = level;
            }
        }

        public int GetCurrentLODLevel() => currentLODLevel;

        public string GetCurrentLODName()
        {
            if (currentLODLevel >= 0 && currentLODLevel < lodLevels.Length)
                return lodLevels[currentLODLevel].name;
            return "Unknown";
        }

        private void OnDrawGizmosSelected()
        {
            // Draw LOD distance spheres in editor
            Gizmos.color = Color.green;
            if (lodLevels.Length > 0)
                Gizmos.DrawWireSphere(transform.position, lodLevels[0].distance);

            Gizmos.color = Color.yellow;
            if (lodLevels.Length > 1)
                Gizmos.DrawWireSphere(transform.position, lodLevels[1].distance);

            Gizmos.color = Color.red;
            if (lodLevels.Length > 2)
                Gizmos.DrawWireSphere(transform.position, lodLevels[2].distance);
        }
    }

    /// <summary>
    /// Helper component to manage LOD for all tanks in scene
    /// </summary>
    public class TankLODManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoSetupLODs = true;
        [SerializeField] private float updateInterval = 1f;

        private List<SimpleLODSystem> lodSystems = new List<SimpleLODSystem>();

        private void Start()
        {
            if (autoSetupLODs)
            {
                SetupAllTankLODs();
            }
        }

        [ContextMenu("Setup All Tank LODs")]
        public void SetupAllTankLODs()
        {
            // Find all tanks
            TankController[] tanks = FindObjectsOfType<TankController>();

            foreach (TankController tank in tanks)
            {
                SimpleLODSystem lod = tank.GetComponent<SimpleLODSystem>();
                if (lod == null)
                {
                    lod = tank.gameObject.AddComponent<SimpleLODSystem>();
                }

                lodSystems.Add(lod);
            }

            Debug.Log($"Setup LOD for {lodSystems.Count} tanks");
        }
    }
}
