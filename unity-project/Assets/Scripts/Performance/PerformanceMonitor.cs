using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace TankCommander.Performance
{
    /// <summary>
    /// Real-time performance monitoring and profiling tool
    /// Tracks FPS, memory usage, object counts, and bottlenecks
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showPerformanceOverlay = true;
        [SerializeField] private bool showDetailedStats = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private int fontSize = 12;

        [Header("Performance Thresholds")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private float warningFrameRate = 45f;
        [SerializeField] private float criticalFrameRate = 30f;

        [Header("Monitoring")]
        [SerializeField] private float updateInterval = 0.5f;

        // Performance metrics
        private float fps;
        private float frameTime;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;
        private long memoryUsed;
        private int drawCalls;
        private int triangles;
        private int vertices;

        // Object counts
        private int tankCount;
        private int projectileCount;
        private int aiCount;
        private int particleCount;

        // Timing
        private float deltaTime = 0f;
        private float updateTimer = 0f;

        // Style
        private GUIStyle labelStyle;
        private bool stylesInitialized = false;

        private void Update()
        {
            // Calculate FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;
            frameTime = deltaTime * 1000f;

            // Track min/max FPS
            if (fps < minFPS) minFPS = fps;
            if (fps > maxFPS) maxFPS = fps;

            // Update stats periodically
            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateStats();
                updateTimer = 0f;
            }

            // Toggle display
            if (Input.GetKeyDown(toggleKey))
            {
                showPerformanceOverlay = !showPerformanceOverlay;
            }
        }

        private void UpdateStats()
        {
            // Memory
            memoryUsed = System.GC.GetTotalMemory(false) / 1048576; // Convert to MB

            // Rendering stats (approximation)
            drawCalls = 0; // Would need UnityEngine.Rendering for exact count
            triangles = 0;
            vertices = 0;

            // Object counts
            tankCount = FindObjectsOfType<TankController>().Length;
            projectileCount = FindObjectsOfType<Projectile>().Length;
            aiCount = FindObjectsOfType<TankAI>().Length;

            // Particle systems
            ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
            particleCount = 0;
            foreach (ParticleSystem ps in particles)
            {
                if (ps.isPlaying)
                    particleCount += ps.particleCount;
            }
        }

        private void OnGUI()
        {
            if (!showPerformanceOverlay) return;

            // Initialize styles
            if (!stylesInitialized)
            {
                InitializeStyles();
            }

            // Create performance report
            StringBuilder report = new StringBuilder();

            // FPS section
            Color fpsColor = GetFPSColor();
            report.AppendLine($"<color={ColorToHex(fpsColor)}>FPS: {fps:F1}</color>");
            report.AppendLine($"Frame Time: {frameTime:F2}ms");
            report.AppendLine($"Min FPS: {minFPS:F1} | Max FPS: {maxFPS:F1}");
            report.AppendLine();

            // Memory
            report.AppendLine($"Memory: {memoryUsed}MB");
            report.AppendLine();

            // Object counts
            report.AppendLine("=== OBJECTS ===");
            report.AppendLine($"Tanks: {tankCount}");
            report.AppendLine($"AI Tanks: {aiCount}");
            report.AppendLine($"Projectiles: {projectileCount}");
            report.AppendLine($"Particles: {particleCount}");
            report.AppendLine();

            if (showDetailedStats)
            {
                // Detailed stats
                report.AppendLine("=== SYSTEM ===");
                report.AppendLine($"Time Scale: {Time.timeScale}");
                report.AppendLine($"Unity Version: {Application.unityVersion}");
                report.AppendLine($"Platform: {Application.platform}");
                report.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
                report.AppendLine();

                // Performance tips
                report.AppendLine("=== TIPS ===");
                if (fps < warningFrameRate)
                {
                    report.AppendLine("<color=yellow>⚠ Low FPS detected!</color>");
                    if (projectileCount > 50)
                        report.AppendLine("  • Too many projectiles");
                    if (particleCount > 1000)
                        report.AppendLine("  • Too many particles");
                    if (aiCount > 10)
                        report.AppendLine("  • Consider AI optimization");
                }
                else
                {
                    report.AppendLine("<color=lime>✓ Performance OK</color>");
                }
            }

            report.AppendLine();
            report.AppendLine($"[{toggleKey}] Toggle Display");

            // Display
            GUI.Label(new Rect(10, 10, 300, 600), report.ToString(), labelStyle);
        }

        private void InitializeStyles()
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = Color.white;
            labelStyle.richText = true;
            labelStyle.padding = new RectOffset(5, 5, 5, 5);

            // Create background texture
            Texture2D bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
            bgTexture.Apply();
            labelStyle.normal.background = bgTexture;

            stylesInitialized = true;
        }

        private Color GetFPSColor()
        {
            if (fps >= targetFrameRate)
                return Color.green;
            else if (fps >= warningFrameRate)
                return Color.yellow;
            else if (fps >= criticalFrameRate)
                return new Color(1f, 0.5f, 0f); // Orange
            else
                return Color.red;
        }

        private string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        [ContextMenu("Reset FPS Stats")]
        public void ResetFPSStats()
        {
            minFPS = float.MaxValue;
            maxFPS = 0f;
        }

        [ContextMenu("Log Performance Report")]
        public void LogPerformanceReport()
        {
            Debug.Log("========================================");
            Debug.Log("PERFORMANCE REPORT");
            Debug.Log("========================================");
            Debug.Log($"FPS: {fps:F1} (Min: {minFPS:F1}, Max: {maxFPS:F1})");
            Debug.Log($"Frame Time: {frameTime:F2}ms");
            Debug.Log($"Memory: {memoryUsed}MB");
            Debug.Log($"Tanks: {tankCount}, AI: {aiCount}, Projectiles: {projectileCount}");
            Debug.Log($"Active Particles: {particleCount}");
            Debug.Log("========================================");
        }

        public float GetCurrentFPS() => fps;
        public float GetFrameTime() => frameTime;
        public long GetMemoryUsage() => memoryUsed;
    }
}
