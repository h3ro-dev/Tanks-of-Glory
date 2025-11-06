using UnityEngine;

namespace TankCommander.Polish
{
    /// <summary>
    /// Automatically integrates all polish systems with the game
    /// One-click setup for all visual/audio feedback improvements
    /// </summary>
    public class PolishIntegrationManager : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool attachToPlayer = true;

        [Header("Systems")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private bool enableDamageIndicators = true;
        [SerializeField] private bool enableHitMarkers = true;
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private bool enableGameFeel = true;
        [SerializeField] private bool enableAudioFeedback = true;

        [Header("References (Auto-Found)")]
        private Camera mainCamera;
        private GameObject player;
        private Health playerHealth;
        private WeaponSystem playerWeapon;
        private TankController playerController;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupAllPolishSystems();
            }
        }

        [ContextMenu("🎨 SETUP ALL POLISH SYSTEMS")]
        public void SetupAllPolishSystems()
        {
            Debug.Log("========================================");
            Debug.Log("🎨 SETTING UP POLISH SYSTEMS");
            Debug.Log("========================================");

            // Find references
            FindReferences();

            // Setup each system
            if (enableCameraShake) SetupCameraShake();
            if (enableDamageIndicators) SetupDamageIndicators();
            if (enableHitMarkers) SetupHitMarkers();
            if (enableTutorial) SetupTutorial();
            if (enableGameFeel) SetupGameFeel();
            if (enableAudioFeedback) SetupAudioFeedback();

            // Wire up events
            WireUpEventHandlers();

            Debug.Log("========================================");
            Debug.Log("✅ POLISH SYSTEMS SETUP COMPLETE!");
            Debug.Log("========================================");
        }

        private void FindReferences()
        {
            // Find camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main Camera not found!");
            }

            // Find player
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                playerWeapon = player.GetComponent<WeaponSystem>();
                playerController = player.GetComponent<TankController>();

                Debug.Log($"✓ Found player: {player.name}");
            }
            else
            {
                Debug.LogWarning("Player not found! Some systems may not work.");
            }
        }

        private void SetupCameraShake()
        {
            if (mainCamera == null) return;

            CameraShakeManager shake = mainCamera.GetComponent<CameraShakeManager>();
            if (shake == null)
            {
                shake = mainCamera.gameObject.AddComponent<CameraShakeManager>();
                Debug.Log("✓ Added CameraShakeManager to Main Camera");
            }
        }

        private void SetupDamageIndicators()
        {
            DamageIndicator indicator = FindObjectOfType<DamageIndicator>();
            if (indicator == null)
            {
                GameObject indicatorObj = new GameObject("DamageIndicator");
                indicator = indicatorObj.AddComponent<DamageIndicator>();
                Debug.Log("✓ Created DamageIndicator");
            }
        }

        private void SetupHitMarkers()
        {
            HitMarker hitMarker = FindObjectOfType<HitMarker>();
            if (hitMarker == null)
            {
                GameObject hitMarkerObj = new GameObject("HitMarker");
                hitMarker = hitMarkerObj.AddComponent<HitMarker>();
                Debug.Log("✓ Created HitMarker");
            }
        }

        private void SetupTutorial()
        {
            TutorialManager tutorial = FindObjectOfType<TutorialManager>();
            if (tutorial == null)
            {
                GameObject tutorialObj = new GameObject("TutorialManager");
                tutorial = tutorialObj.AddComponent<TutorialManager>();
                Debug.Log("✓ Created TutorialManager");
            }
        }

        private void SetupGameFeel()
        {
            GameFeelEnhancer gameFeel = FindObjectOfType<GameFeelEnhancer>();
            if (gameFeel == null)
            {
                GameObject gameFeelObj = new GameObject("GameFeelEnhancer");
                gameFeel = gameFeelObj.AddComponent<GameFeelEnhancer>();
                Debug.Log("✓ Created GameFeelEnhancer");
            }
        }

        private void SetupAudioFeedback()
        {
            AudioFeedbackSystem audioFeedback = FindObjectOfType<AudioFeedbackSystem>();
            if (audioFeedback == null)
            {
                GameObject audioObj = new GameObject("AudioFeedbackSystem");
                audioFeedback = audioObj.AddComponent<AudioFeedbackSystem>();
                Debug.Log("✓ Created AudioFeedbackSystem");
            }
        }

        private void WireUpEventHandlers()
        {
            if (player == null) return;

            // Wire up player health events
            if (playerHealth != null)
            {
                playerHealth.OnDamaged.RemoveListener(HandlePlayerDamaged);
                playerHealth.OnDamaged.AddListener(HandlePlayerDamaged);

                playerHealth.OnDeath.RemoveListener(HandlePlayerDeath);
                playerHealth.OnDeath.AddListener(HandlePlayerDeath);

                playerHealth.OnHealthChanged.RemoveListener(HandlePlayerHealthChanged);
                playerHealth.OnHealthChanged.AddListener(HandlePlayerHealthChanged);

                Debug.Log("✓ Wired up player health events");
            }

            // Wire up weapon events
            if (playerWeapon != null)
            {
                playerWeapon.OnWeaponFired.RemoveListener(HandleWeaponFired);
                playerWeapon.OnWeaponFired.AddListener(HandleWeaponFired);

                playerWeapon.OnReloading.RemoveListener(HandleReloading);
                playerWeapon.OnReloading.AddListener(HandleReloading);

                Debug.Log("✓ Wired up weapon events");
            }
        }

        // Event Handlers
        private void HandlePlayerDamaged(float damage, GameObject attacker)
        {
            // Camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeDamage();
            }

            // Damage indicator
            if (DamageIndicator damageIndicator = FindObjectOfType<DamageIndicator>())
            {
                if (attacker != null)
                {
                    damageIndicator.ShowDamageIndicator(attacker.transform.position, damage);
                }
            }

            // Audio feedback
            if (AudioFeedbackSystem.Instance != null)
            {
                AudioFeedbackSystem.Instance.PlayTakeDamage(damage);
            }

            // Tutorial tracking
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlayerTookDamage();
            }
        }

        private void HandlePlayerDeath()
        {
            // Camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeExplosion();
            }

            // Audio feedback
            if (AudioFeedbackSystem.Instance != null)
            {
                AudioFeedbackSystem.Instance.PlayDefeat();
            }
        }

        private void HandlePlayerHealthChanged(float current, float max)
        {
            float healthPercent = current / max;

            // Update game feel enhancer
            if (GameFeelEnhancer.Instance != null)
            {
                GameFeelEnhancer.Instance.UpdateHealth(healthPercent);
            }

            // Low health warning
            if (AudioFeedbackSystem.Instance != null)
            {
                if (healthPercent < 0.3f)
                {
                    AudioFeedbackSystem.Instance.PlayLowHealthWarning();
                }
                else
                {
                    AudioFeedbackSystem.Instance.StopLowHealthWarning();
                }
            }
        }

        private void HandleWeaponFired(bool isPrimary)
        {
            // Camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeFire();
            }

            // Audio feedback
            if (AudioFeedbackSystem.Instance != null)
            {
                AudioFeedbackSystem.Instance.PlayWeaponFire();
            }

            // Game feel
            if (GameFeelEnhancer.Instance != null)
            {
                GameFeelEnhancer.Instance.OnFire();
            }

            // Tutorial tracking
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlayerFired();
            }
        }

        private void HandleReloading(bool isReloading, float reloadTime, bool isPrimary)
        {
            if (isReloading)
            {
                // Audio feedback
                if (AudioFeedbackSystem.Instance != null)
                {
                    AudioFeedbackSystem.Instance.PlayReload();
                }

                // Game feel
                if (GameFeelEnhancer.Instance != null)
                {
                    GameFeelEnhancer.Instance.OnReloadStart(reloadTime);
                }
            }
            else
            {
                // Game feel
                if (GameFeelEnhancer.Instance != null)
                {
                    GameFeelEnhancer.Instance.OnReloadComplete();
                }
            }
        }

        // Public method to register enemy kills
        public void OnEnemyKilled(GameObject enemy)
        {
            // Hit marker
            if (HitMarker.Instance != null)
            {
                HitMarker.Instance.ShowKillHit();
            }

            // Audio feedback
            if (AudioFeedbackSystem.Instance != null)
            {
                AudioFeedbackSystem.Instance.PlayKillConfirm();
            }

            // Tutorial tracking
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlayerKill();
            }
        }

        // Public method to register hits
        public void OnEnemyHit(GameObject enemy, float damage, bool isCritical = false)
        {
            // Hit marker
            if (HitMarker.Instance != null)
            {
                if (isCritical)
                    HitMarker.Instance.ShowCriticalHit();
                else
                    HitMarker.Instance.ShowNormalHit();
            }

            // Audio feedback
            if (AudioFeedbackSystem.Instance != null)
            {
                if (isCritical)
                    AudioFeedbackSystem.Instance.PlayCriticalHit();
                else
                    AudioFeedbackSystem.Instance.PlayHitConfirm();
            }

            // Tutorial tracking
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlayerHitEnemy();
            }
        }

        [ContextMenu("Test Polish Systems")]
        public void TestPolishSystems()
        {
            Debug.Log("Testing polish systems...");

            // Test camera shake
            if (CameraShakeManager.Instance != null)
            {
                CameraShakeManager.Instance.ShakeExplosion();
                Debug.Log("✓ Camera shake tested");
            }

            // Test hit marker
            if (HitMarker.Instance != null)
            {
                HitMarker.Instance.ShowNormalHit();
                Debug.Log("✓ Hit marker tested");
            }

            // Test audio
            if (AudioFeedbackSystem.Instance != null)
            {
                AudioFeedbackSystem.Instance.PlayWeaponFire();
                Debug.Log("✓ Audio feedback tested");
            }
        }
    }
}
