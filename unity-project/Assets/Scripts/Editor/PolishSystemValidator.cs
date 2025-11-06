using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using TankCommander;

namespace TankCommander.Editor
{
    /// <summary>
    /// Validates Polish Systems setup in the scene
    /// Provides comprehensive checking for game feel and polish components
    /// Context menu: "🎨 VALIDATE POLISH SYSTEMS"
    /// </summary>
    public class PolishSystemValidator : MonoBehaviour
    {
        private StringBuilder report = new StringBuilder();
        private int errorCount = 0;
        private int warningCount = 0;
        private int successCount = 0;

        [ContextMenu("🎨 VALIDATE POLISH SYSTEMS")]
        public void ValidatePolishSystems()
        {
            Debug.Log("🔍 Starting Polish Systems Validation...\n");

            report.Clear();
            errorCount = 0;
            warningCount = 0;
            successCount = 0;

            report.AppendLine("==============================================");
            report.AppendLine("    POLISH SYSTEMS VALIDATION REPORT");
            report.AppendLine("==============================================\n");

            ValidateCameraShakeSystem();
            ValidateHitMarkerSystem();
            ValidateDamageIndicatorSystem();
            ValidateTutorialSystem();
            ValidateGameFeelEnhancer();
            ValidateAudioFeedbackSystem();
            ValidatePolishIntegrationManager();
            ValidatePlayerEventWiring();
            ValidateUISetup();

            GenerateReport();
        }

        #region Validation Methods

        private void ValidateCameraShakeSystem()
        {
            report.AppendLine("📹 Camera Shake System");
            report.AppendLine("─────────────────────────────");

            CameraShakeManager shakeManager = FindObjectOfType<CameraShakeManager>();

            if (shakeManager == null)
            {
                Warning("CameraShakeManager not found in scene");
                report.AppendLine("  • Add CameraShakeManager to Main Camera");
            }
            else
            {
                Success("CameraShakeManager found");

                // Check if attached to camera
                Camera cam = shakeManager.GetComponent<Camera>();
                if (cam == null)
                {
                    Warning("CameraShakeManager not attached to a Camera");
                }
                else
                {
                    Success($"Attached to camera: {cam.gameObject.name}");
                }

                // Check singleton instance
                if (CameraShakeManager.Instance == null)
                {
                    Error("CameraShakeManager.Instance is null (Awake not called yet)");
                }
                else
                {
                    Success("Singleton instance initialized");
                }
            }

            report.AppendLine();
        }

        private void ValidateHitMarkerSystem()
        {
            report.AppendLine("🎯 Hit Marker System");
            report.AppendLine("─────────────────────────────");

            HitMarker hitMarker = FindObjectOfType<HitMarker>();

            if (hitMarker == null)
            {
                Warning("HitMarker not found in scene");
                report.AppendLine("  • Create GameObject with HitMarker component");
            }
            else
            {
                Success("HitMarker found");

                // Check if it has a Canvas parent
                Canvas canvas = hitMarker.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Warning("HitMarker not under a Canvas");
                    report.AppendLine("  • HitMarker should be child of Canvas for UI rendering");
                }
                else
                {
                    Success($"Under Canvas: {canvas.gameObject.name}");

                    // Check canvas render mode
                    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    {
                        Warning($"Canvas render mode is {canvas.renderMode} (recommend ScreenSpaceOverlay)");
                    }
                }

                // Check singleton
                if (HitMarker.Instance == null)
                {
                    Error("HitMarker.Instance is null");
                }
                else
                {
                    Success("Singleton instance initialized");
                }
            }

            report.AppendLine();
        }

        private void ValidateDamageIndicatorSystem()
        {
            report.AppendLine("📍 Damage Indicator System");
            report.AppendLine("─────────────────────────────");

            DamageIndicator damageIndicator = FindObjectOfType<DamageIndicator>();

            if (damageIndicator == null)
            {
                Warning("DamageIndicator not found in scene");
                report.AppendLine("  • Create GameObject with DamageIndicator component");
            }
            else
            {
                Success("DamageIndicator found");

                // Check for player
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null)
                {
                    Error("No GameObject with 'Player' tag found");
                    report.AppendLine("  • DamageIndicator requires player to show directional indicators");
                }
                else
                {
                    Success("Player found for direction calculation");
                }

                // Check for canvas
                Canvas canvas = damageIndicator.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Warning("DamageIndicator not under a Canvas");
                }
                else
                {
                    Success($"Under Canvas: {canvas.gameObject.name}");
                }
            }

            report.AppendLine();
        }

        private void ValidateTutorialSystem()
        {
            report.AppendLine("📚 Tutorial System");
            report.AppendLine("─────────────────────────────");

            TutorialManager tutorial = FindObjectOfType<TutorialManager>();

            if (tutorial == null)
            {
                Warning("TutorialManager not found in scene");
                report.AppendLine("  • Add TutorialManager if you want tutorial guidance");
            }
            else
            {
                Success("TutorialManager found");

                // Check singleton
                if (TutorialManager.Instance == null)
                {
                    Error("TutorialManager.Instance is null");
                }
                else
                {
                    Success("Singleton instance initialized");
                }

                // Check for Canvas/UI
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Warning("No Canvas found for tutorial UI");
                }
                else
                {
                    Success("Canvas available for tutorial display");
                }

                // Check tutorial completion status
                int tutorialComplete = PlayerPrefs.GetInt("TutorialComplete", 0);
                if (tutorialComplete == 1)
                {
                    report.AppendLine("  ℹ️ Tutorial already completed (PlayerPrefs)");
                }
                else
                {
                    report.AppendLine("  ℹ️ Tutorial will run on first play");
                }
            }

            report.AppendLine();
        }

        private void ValidateGameFeelEnhancer()
        {
            report.AppendLine("✨ Game Feel Enhancer");
            report.AppendLine("─────────────────────────────");

            GameFeelEnhancer enhancer = FindObjectOfType<GameFeelEnhancer>();

            if (enhancer == null)
            {
                Warning("GameFeelEnhancer not found in scene");
                report.AppendLine("  • Add GameFeelEnhancer for dynamic UI feedback");
            }
            else
            {
                Success("GameFeelEnhancer found");

                // Check singleton
                if (GameFeelEnhancer.Instance == null)
                {
                    Error("GameFeelEnhancer.Instance is null");
                }
                else
                {
                    Success("Singleton instance initialized");
                }

                // Check for Canvas
                Canvas canvas = enhancer.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Warning("GameFeelEnhancer not under a Canvas");
                    report.AppendLine("  • Needs Canvas for crosshair, reload indicator, vignette");
                }
                else
                {
                    Success($"Under Canvas: {canvas.gameObject.name}");
                }
            }

            report.AppendLine();
        }

        private void ValidateAudioFeedbackSystem()
        {
            report.AppendLine("🔊 Audio Feedback System");
            report.AppendLine("─────────────────────────────");

            AudioFeedbackSystem audio = FindObjectOfType<AudioFeedbackSystem>();

            if (audio == null)
            {
                Warning("AudioFeedbackSystem not found in scene");
                report.AppendLine("  • Add AudioFeedbackSystem for audio feedback");
            }
            else
            {
                Success("AudioFeedbackSystem found");

                // Check singleton
                if (AudioFeedbackSystem.Instance == null)
                {
                    Error("AudioFeedbackSystem.Instance is null");
                }
                else
                {
                    Success("Singleton instance initialized");
                }

                // Check for AudioListener
                AudioListener listener = FindObjectOfType<AudioListener>();
                if (listener == null)
                {
                    Error("No AudioListener found in scene");
                    report.AppendLine("  • Add AudioListener to Main Camera");
                }
                else
                {
                    Success($"AudioListener found on: {listener.gameObject.name}");
                }

                // Note about audio clips (optional)
                report.AppendLine("  ℹ️ Audio clips are optional (visual fallback available)");
            }

            report.AppendLine();
        }

        private void ValidatePolishIntegrationManager()
        {
            report.AppendLine("🔗 Polish Integration Manager");
            report.AppendLine("─────────────────────────────");

            PolishIntegrationManager manager = FindObjectOfType<PolishIntegrationManager>();

            if (manager == null)
            {
                Warning("PolishIntegrationManager not found in scene");
                report.AppendLine("  • Add PolishIntegrationManager for automatic setup");
                report.AppendLine("  • Right-click component → '🎨 SETUP ALL POLISH SYSTEMS'");
            }
            else
            {
                Success("PolishIntegrationManager found");

                // Check which systems are enabled
                var serializedManager = new SerializedObject(manager);
                bool enableCameraShake = serializedManager.FindProperty("enableCameraShake")?.boolValue ?? true;
                bool enableDamageIndicators = serializedManager.FindProperty("enableDamageIndicators")?.boolValue ?? true;
                bool enableHitMarkers = serializedManager.FindProperty("enableHitMarkers")?.boolValue ?? true;
                bool enableTutorial = serializedManager.FindProperty("enableTutorial")?.boolValue ?? true;
                bool enableGameFeel = serializedManager.FindProperty("enableGameFeel")?.boolValue ?? true;
                bool enableAudioFeedback = serializedManager.FindProperty("enableAudioFeedback")?.boolValue ?? true;

                report.AppendLine("  Enabled Systems:");
                report.AppendLine($"    • Camera Shake: {(enableCameraShake ? "✓" : "✗")}");
                report.AppendLine($"    • Damage Indicators: {(enableDamageIndicators ? "✓" : "✗")}");
                report.AppendLine($"    • Hit Markers: {(enableHitMarkers ? "✓" : "✗")}");
                report.AppendLine($"    • Tutorial: {(enableTutorial ? "✓" : "✗")}");
                report.AppendLine($"    • Game Feel: {(enableGameFeel ? "✓" : "✗")}");
                report.AppendLine($"    • Audio Feedback: {(enableAudioFeedback ? "✓" : "✗")}");
            }

            report.AppendLine();
        }

        private void ValidatePlayerEventWiring()
        {
            report.AppendLine("🔌 Player Event Wiring");
            report.AppendLine("─────────────────────────────");

            GameObject player = GameObject.FindWithTag("Player");

            if (player == null)
            {
                Error("No GameObject with 'Player' tag found");
                report.AppendLine("  • Polish systems need player for event integration");
            }
            else
            {
                Success($"Player found: {player.name}");

                // Check for Health component
                Health health = player.GetComponent<Health>();
                if (health == null)
                {
                    Warning("Player missing Health component");
                    report.AppendLine("  • Health events needed for damage feedback");
                }
                else
                {
                    Success("Player has Health component");

                    // Check if events have listeners
                    int damagedListeners = health.OnDamaged?.GetPersistentEventCount() ?? 0;
                    int deathListeners = health.OnDeath?.GetPersistentEventCount() ?? 0;
                    int healthChangedListeners = health.OnHealthChanged?.GetPersistentEventCount() ?? 0;

                    if (damagedListeners > 0 || deathListeners > 0 || healthChangedListeners > 0)
                    {
                        Success($"Health events wired ({damagedListeners + deathListeners + healthChangedListeners} listeners)");
                    }
                    else
                    {
                        Warning("No listeners on Health events (may be wired at runtime)");
                        report.AppendLine("  ℹ️ PolishIntegrationManager wires events at runtime");
                    }
                }

                // Check for WeaponSystem
                WeaponSystem weapon = player.GetComponent<WeaponSystem>();
                if (weapon == null)
                {
                    Warning("Player missing WeaponSystem component");
                    report.AppendLine("  • Weapon events needed for fire/reload feedback");
                }
                else
                {
                    Success("Player has WeaponSystem component");

                    // Check if events have listeners
                    int firedListeners = weapon.OnWeaponFired?.GetPersistentEventCount() ?? 0;
                    int reloadingListeners = weapon.OnReloading?.GetPersistentEventCount() ?? 0;

                    if (firedListeners > 0 || reloadingListeners > 0)
                    {
                        Success($"Weapon events wired ({firedListeners + reloadingListeners} listeners)");
                    }
                    else
                    {
                        Warning("No listeners on WeaponSystem events (may be wired at runtime)");
                    }
                }
            }

            report.AppendLine();
        }

        private void ValidateUISetup()
        {
            report.AppendLine("🎨 UI Setup");
            report.AppendLine("─────────────────────────────");

            Canvas[] canvases = FindObjectsOfType<Canvas>();

            if (canvases.Length == 0)
            {
                Error("No Canvas found in scene");
                report.AppendLine("  • Polish systems need Canvas for UI rendering");
                report.AppendLine("  • Create Canvas (Screen Space - Overlay)");
            }
            else
            {
                Success($"Found {canvases.Length} Canvas(es)");

                foreach (Canvas canvas in canvases)
                {
                    report.AppendLine($"  • {canvas.gameObject.name}:");
                    report.AppendLine($"    - Render Mode: {canvas.renderMode}");
                    report.AppendLine($"    - Sort Order: {canvas.sortingOrder}");

                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        Success($"    ✓ {canvas.gameObject.name} using recommended ScreenSpaceOverlay");
                    }
                }

                // Check for EventSystem (needed for UI interaction)
                UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem == null)
                {
                    Warning("No EventSystem found");
                    report.AppendLine("  • EventSystem needed for UI interaction (buttons, etc.)");
                }
                else
                {
                    Success("EventSystem found");
                }
            }

            report.AppendLine();
        }

        #endregion

        #region Reporting

        private void Error(string message)
        {
            errorCount++;
            report.AppendLine($"  ❌ ERROR: {message}");
        }

        private void Warning(string message)
        {
            warningCount++;
            report.AppendLine($"  ⚠️ WARNING: {message}");
        }

        private void Success(string message)
        {
            successCount++;
            report.AppendLine($"  ✅ {message}");
        }

        private void GenerateReport()
        {
            report.AppendLine("==============================================");
            report.AppendLine("                  SUMMARY");
            report.AppendLine("==============================================");
            report.AppendLine($"✅ Successes: {successCount}");
            report.AppendLine($"⚠️  Warnings: {warningCount}");
            report.AppendLine($"❌ Errors: {errorCount}");
            report.AppendLine();

            if (errorCount == 0 && warningCount == 0)
            {
                report.AppendLine("🎉 PERFECT! All polish systems properly configured!");
            }
            else if (errorCount == 0)
            {
                report.AppendLine("✓ Good! Polish systems are functional with minor warnings.");
            }
            else
            {
                report.AppendLine("⚠️ Issues found. Fix errors for optimal polish system functionality.");
            }

            report.AppendLine();
            report.AppendLine("==============================================");
            report.AppendLine("              QUICK SETUP GUIDE");
            report.AppendLine("==============================================");
            report.AppendLine("1. Create GameObject → Add PolishIntegrationManager");
            report.AppendLine("2. Right-click component → '🎨 SETUP ALL POLISH SYSTEMS'");
            report.AppendLine("3. Re-run validation to verify setup");
            report.AppendLine("4. Enter Play Mode to test polish systems");
            report.AppendLine();
            report.AppendLine("For detailed guide, see: GAME_FEEL_GUIDE.md");
            report.AppendLine("==============================================");

            // Output to console
            Debug.Log(report.ToString());

            // Show in editor window
            EditorUtility.DisplayDialog(
                "Polish Systems Validation Complete",
                $"Validation complete!\n\n" +
                $"✅ Successes: {successCount}\n" +
                $"⚠️ Warnings: {warningCount}\n" +
                $"❌ Errors: {errorCount}\n\n" +
                "See Console for full report.",
                "OK"
            );
        }

        #endregion

        #region Editor Menu

        [MenuItem("Tank Commander/Validate Polish Systems 🎨")]
        public static void ValidateFromMenu()
        {
            // Create temporary validator
            GameObject validatorObj = new GameObject("_PolishSystemValidator");
            PolishSystemValidator validator = validatorObj.AddComponent<PolishSystemValidator>();
            validator.ValidatePolishSystems();
            DestroyImmediate(validatorObj);
        }

        #endregion
    }
}
