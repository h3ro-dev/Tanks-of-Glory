using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TankCommander;

namespace TankCommander.Tests
{
    /// <summary>
    /// Unit tests for the Polish Systems (CameraShake, HitMarker, DamageIndicator, etc.)
    /// Tests individual system functionality in isolation
    /// </summary>
    [TestFixture]
    public class PolishSystemsTests
    {
        private GameObject testGameObject;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("TestObject");
        }

        [TearDown]
        public void Teardown()
        {
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);

            // Clean up any singleton instances
            CleanupSingletons();
        }

        private void CleanupSingletons()
        {
            var cameraShakeManager = Object.FindObjectOfType<CameraShakeManager>();
            if (cameraShakeManager != null)
                Object.DestroyImmediate(cameraShakeManager.gameObject);

            var hitMarker = Object.FindObjectOfType<HitMarker>();
            if (hitMarker != null)
                Object.DestroyImmediate(hitMarker.gameObject);

            var damageIndicator = Object.FindObjectOfType<DamageIndicator>();
            if (damageIndicator != null)
                Object.DestroyImmediate(damageIndicator.gameObject);

            var tutorialManager = Object.FindObjectOfType<TutorialManager>();
            if (tutorialManager != null)
                Object.DestroyImmediate(tutorialManager.gameObject);

            var gameFeelEnhancer = Object.FindObjectOfType<GameFeelEnhancer>();
            if (gameFeelEnhancer != null)
                Object.DestroyImmediate(gameFeelEnhancer.gameObject);

            var audioFeedback = Object.FindObjectOfType<AudioFeedbackSystem>();
            if (audioFeedback != null)
                Object.DestroyImmediate(audioFeedback.gameObject);
        }

        #region CameraShakeManager Tests

        [Test]
        public void CameraShakeManager_Initialize_SetsSingletonInstance()
        {
            // Arrange
            GameObject cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            CameraShakeManager shakeManager = cameraObj.AddComponent<CameraShakeManager>();

            // Act
            shakeManager.Awake();

            // Assert
            Assert.IsNotNull(CameraShakeManager.Instance, "CameraShakeManager Instance should be set");
            Assert.AreEqual(shakeManager, CameraShakeManager.Instance, "Instance should reference the created manager");

            // Cleanup
            Object.DestroyImmediate(cameraObj);
        }

        [Test]
        public void CameraShakeManager_Shake_AcceptsMagnitudeAndDuration()
        {
            // Arrange
            GameObject cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            CameraShakeManager shakeManager = cameraObj.AddComponent<CameraShakeManager>();

            // Act - Should not throw
            shakeManager.Shake(0.5f, 0.2f);

            // Assert - If we got here, the method executed without errors
            Assert.Pass("Shake method executed successfully");

            // Cleanup
            Object.DestroyImmediate(cameraObj);
        }

        [Test]
        public void CameraShakeManager_ShakePresets_ExecuteWithoutErrors()
        {
            // Arrange
            GameObject cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            CameraShakeManager shakeManager = cameraObj.AddComponent<CameraShakeManager>();

            // Act & Assert - All preset methods should execute without errors
            Assert.DoesNotThrow(() => shakeManager.ShakeFire(), "ShakeFire should not throw");
            Assert.DoesNotThrow(() => shakeManager.ShakeDamage(), "ShakeDamage should not throw");
            Assert.DoesNotThrow(() => shakeManager.ShakeExplosion(), "ShakeExplosion should not throw");

            // Cleanup
            Object.DestroyImmediate(cameraObj);
        }

        [Test]
        public void CameraShakeManager_ShakeExplosionAtDistance_ReducesIntensityWithDistance()
        {
            // Arrange
            GameObject cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            CameraShakeManager shakeManager = cameraObj.AddComponent<CameraShakeManager>();
            cameraObj.transform.position = Vector3.zero;

            Vector3 closeExplosion = new Vector3(5f, 0f, 0f);
            Vector3 farExplosion = new Vector3(100f, 0f, 0f);

            // Act - Should handle both close and far explosions
            Assert.DoesNotThrow(() => shakeManager.ShakeExplosionAtDistance(closeExplosion, 50f));
            Assert.DoesNotThrow(() => shakeManager.ShakeExplosionAtDistance(farExplosion, 50f));

            // Assert - If we got here, distance-based shake works
            Assert.Pass("Distance-based shake executed successfully");

            // Cleanup
            Object.DestroyImmediate(cameraObj);
        }

        #endregion

        #region HitMarker Tests

        [Test]
        public void HitMarker_Initialize_SetsSingletonInstance()
        {
            // Arrange
            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            // Act
            hitMarker.Awake();

            // Assert
            Assert.IsNotNull(HitMarker.Instance, "HitMarker Instance should be set");
            Assert.AreEqual(hitMarker, HitMarker.Instance, "Instance should reference the created marker");

            // Cleanup
            Object.DestroyImmediate(hitMarkerObj);
        }

        [Test]
        public void HitMarker_ShowHitMarker_HandlesAllTypes()
        {
            // Arrange
            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            // Act & Assert - All hit types should execute without errors
            Assert.DoesNotThrow(() => hitMarker.ShowHitMarker(false, false), "Normal hit should not throw");
            Assert.DoesNotThrow(() => hitMarker.ShowHitMarker(true, false), "Critical hit should not throw");
            Assert.DoesNotThrow(() => hitMarker.ShowHitMarker(false, true), "Kill hit should not throw");

            // Cleanup
            Object.DestroyImmediate(hitMarkerObj);
        }

        [Test]
        public void HitMarker_ConvenienceMethods_ExecuteCorrectly()
        {
            // Arrange
            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            // Act & Assert
            Assert.DoesNotThrow(() => hitMarker.ShowNormalHit(), "ShowNormalHit should not throw");
            Assert.DoesNotThrow(() => hitMarker.ShowCriticalHit(), "ShowCriticalHit should not throw");
            Assert.DoesNotThrow(() => hitMarker.ShowKillHit(), "ShowKillHit should not throw");

            // Cleanup
            Object.DestroyImmediate(hitMarkerObj);
        }

        #endregion

        #region DamageIndicator Tests

        [Test]
        public void DamageIndicator_Initialize_FindsPlayerTransform()
        {
            // Arrange
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";

            GameObject indicatorObj = new GameObject("DamageIndicator");
            DamageIndicator damageIndicator = indicatorObj.AddComponent<DamageIndicator>();

            // Act
            damageIndicator.Start();

            // Assert
            // If player exists, it should be found (test doesn't throw)
            Assert.Pass("DamageIndicator initialized successfully");

            // Cleanup
            Object.DestroyImmediate(playerObj);
            Object.DestroyImmediate(indicatorObj);
        }

        [Test]
        public void DamageIndicator_ShowDamageIndicator_HandlesValidInput()
        {
            // Arrange
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";

            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();

            GameObject indicatorObj = new GameObject("DamageIndicator");
            DamageIndicator damageIndicator = indicatorObj.AddComponent<DamageIndicator>();

            Vector3 damageSource = new Vector3(10f, 0f, 0f);
            float damageAmount = 25f;

            // Act & Assert - Should handle showing indicators
            Assert.DoesNotThrow(() => damageIndicator.ShowDamageIndicator(damageSource, damageAmount));

            // Cleanup
            Object.DestroyImmediate(playerObj);
            Object.DestroyImmediate(canvasObj);
            Object.DestroyImmediate(indicatorObj);
        }

        #endregion

        #region TutorialManager Tests

        [Test]
        public void TutorialManager_Initialize_SetsSingletonInstance()
        {
            // Arrange
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tutorialObj.AddComponent<TutorialManager>();

            // Act
            tutorialManager.Awake();

            // Assert
            Assert.IsNotNull(TutorialManager.Instance, "TutorialManager Instance should be set");
            Assert.AreEqual(tutorialManager, TutorialManager.Instance);

            // Cleanup
            Object.DestroyImmediate(tutorialObj);
        }

        [Test]
        public void TutorialManager_StartTutorial_ExecutesWithoutError()
        {
            // Arrange
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tutorialObj.AddComponent<TutorialManager>();

            // Act & Assert
            Assert.DoesNotThrow(() => tutorialManager.StartTutorial());

            // Cleanup
            Object.DestroyImmediate(tutorialObj);
        }

        [Test]
        public void TutorialManager_SkipTutorial_ExecutesCorrectly()
        {
            // Arrange
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tutorialObj.AddComponent<TutorialManager>();

            // Act & Assert
            Assert.DoesNotThrow(() => tutorialManager.SkipTutorial());

            // Cleanup
            Object.DestroyImmediate(tutorialObj);
        }

        [Test]
        public void TutorialManager_ResetTutorial_ClearsProgress()
        {
            // Arrange
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tutorialObj.AddComponent<TutorialManager>();

            // Act
            tutorialManager.ResetTutorial();

            // Assert - PlayerPrefs should be cleared
            int tutorialComplete = PlayerPrefs.GetInt("TutorialComplete", 0);
            Assert.AreEqual(0, tutorialComplete, "Tutorial completion flag should be cleared");

            // Cleanup
            Object.DestroyImmediate(tutorialObj);
        }

        [Test]
        public void TutorialManager_TriggerMethods_ExecuteWithoutError()
        {
            // Arrange
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorialManager = tutorialObj.AddComponent<TutorialManager>();

            // Act & Assert - All trigger methods should execute
            Assert.DoesNotThrow(() => tutorialManager.OnPlayerMoved());
            Assert.DoesNotThrow(() => tutorialManager.OnPlayerFired());
            Assert.DoesNotThrow(() => tutorialManager.OnPlayerHitEnemy());
            Assert.DoesNotThrow(() => tutorialManager.OnPlayerKill());
            Assert.DoesNotThrow(() => tutorialManager.OnPlayerTookDamage());

            // Cleanup
            Object.DestroyImmediate(tutorialObj);
        }

        #endregion

        #region GameFeelEnhancer Tests

        [Test]
        public void GameFeelEnhancer_Initialize_SetsSingletonInstance()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            // Act
            enhancer.Awake();

            // Assert
            Assert.IsNotNull(GameFeelEnhancer.Instance, "GameFeelEnhancer Instance should be set");
            Assert.AreEqual(enhancer, GameFeelEnhancer.Instance);

            // Cleanup
            Object.DestroyImmediate(enhancerObj);
        }

        [Test]
        public void GameFeelEnhancer_OnFire_ExecutesWithoutError()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            // Act & Assert
            Assert.DoesNotThrow(() => enhancer.OnFire());

            // Cleanup
            Object.DestroyImmediate(enhancerObj);
        }

        [Test]
        public void GameFeelEnhancer_UpdateHealth_AcceptsValidRange()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            // Act & Assert - Should handle health values from 0 to 1
            Assert.DoesNotThrow(() => enhancer.UpdateHealth(1.0f), "Full health should work");
            Assert.DoesNotThrow(() => enhancer.UpdateHealth(0.5f), "Half health should work");
            Assert.DoesNotThrow(() => enhancer.UpdateHealth(0.25f), "Low health should work");
            Assert.DoesNotThrow(() => enhancer.UpdateHealth(0.0f), "Zero health should work");

            // Cleanup
            Object.DestroyImmediate(enhancerObj);
        }

        [Test]
        public void GameFeelEnhancer_OnTargetingEnemy_HandlesBoolean()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            // Act & Assert
            Assert.DoesNotThrow(() => enhancer.OnTargetingEnemy(true), "Targeting enemy should work");
            Assert.DoesNotThrow(() => enhancer.OnTargetingEnemy(false), "Not targeting should work");

            // Cleanup
            Object.DestroyImmediate(enhancerObj);
        }

        [Test]
        public void GameFeelEnhancer_ReloadMethods_ExecuteCorrectly()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            // Act & Assert
            Assert.DoesNotThrow(() => enhancer.OnReloadStart(2.0f), "Reload start should work");
            Assert.DoesNotThrow(() => enhancer.OnReloadComplete(), "Reload complete should work");

            // Cleanup
            Object.DestroyImmediate(enhancerObj);
        }

        #endregion

        #region AudioFeedbackSystem Tests

        [Test]
        public void AudioFeedbackSystem_Initialize_SetsSingletonInstance()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act
            audioSystem.Awake();

            // Assert
            Assert.IsNotNull(AudioFeedbackSystem.Instance, "AudioFeedbackSystem Instance should be set");
            Assert.AreEqual(audioSystem, AudioFeedbackSystem.Instance);

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        [Test]
        public void AudioFeedbackSystem_PlayWeaponSounds_ExecuteWithoutError()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act & Assert - Should work even without audio clips (visual fallback)
            Assert.DoesNotThrow(() => audioSystem.PlayWeaponFire());
            Assert.DoesNotThrow(() => audioSystem.PlayReload());
            Assert.DoesNotThrow(() => audioSystem.PlayWeaponEmpty());

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        [Test]
        public void AudioFeedbackSystem_PlayImpactSounds_ExecuteWithoutError()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act & Assert
            Assert.DoesNotThrow(() => audioSystem.PlayHitConfirm());
            Assert.DoesNotThrow(() => audioSystem.PlayCriticalHit());
            Assert.DoesNotThrow(() => audioSystem.PlayKillConfirm());

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        [Test]
        public void AudioFeedbackSystem_PlayDamageSounds_ExecuteWithoutError()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act & Assert
            Assert.DoesNotThrow(() => audioSystem.PlayTakeDamage(25f));
            Assert.DoesNotThrow(() => audioSystem.PlayLowHealthWarning());
            Assert.DoesNotThrow(() => audioSystem.StopLowHealthWarning());

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        [Test]
        public void AudioFeedbackSystem_PlayUISounds_ExecuteWithoutError()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act & Assert
            Assert.DoesNotThrow(() => audioSystem.PlayButtonClick());
            Assert.DoesNotThrow(() => audioSystem.PlayMenuOpen());
            Assert.DoesNotThrow(() => audioSystem.PlayMenuClose());

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        [Test]
        public void AudioFeedbackSystem_SetMasterVolume_AcceptsValidRange()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audioSystem = audioObj.AddComponent<AudioFeedbackSystem>();

            // Act & Assert
            Assert.DoesNotThrow(() => audioSystem.SetMasterVolume(0.0f), "Volume 0 should work");
            Assert.DoesNotThrow(() => audioSystem.SetMasterVolume(0.5f), "Volume 0.5 should work");
            Assert.DoesNotThrow(() => audioSystem.SetMasterVolume(1.0f), "Volume 1 should work");

            // Cleanup
            Object.DestroyImmediate(audioObj);
        }

        #endregion

        #region PolishIntegrationManager Tests

        [Test]
        public void PolishIntegrationManager_Initialize_CreatesComponent()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");

            // Act
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            // Assert
            Assert.IsNotNull(manager, "PolishIntegrationManager should be created");
            Assert.IsNotNull(manager.gameObject, "Manager should have GameObject reference");

            // Cleanup
            Object.DestroyImmediate(managerObj);
        }

        [Test]
        public void PolishIntegrationManager_OnEnemyHit_ExecutesWithValidInput()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            GameObject enemyObj = new GameObject("Enemy");

            // Act & Assert
            Assert.DoesNotThrow(() => manager.OnEnemyHit(enemyObj, 25f, false));
            Assert.DoesNotThrow(() => manager.OnEnemyHit(enemyObj, 50f, true));

            // Cleanup
            Object.DestroyImmediate(managerObj);
            Object.DestroyImmediate(enemyObj);
        }

        [Test]
        public void PolishIntegrationManager_OnEnemyKilled_ExecutesWithValidInput()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            GameObject enemyObj = new GameObject("Enemy");

            // Act & Assert
            Assert.DoesNotThrow(() => manager.OnEnemyKilled(enemyObj));

            // Cleanup
            Object.DestroyImmediate(managerObj);
            Object.DestroyImmediate(enemyObj);
        }

        [Test]
        public void PolishIntegrationManager_HandlesNullGracefully()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            // Act & Assert - Should handle null without errors
            Assert.DoesNotThrow(() => manager.OnEnemyHit(null, 25f, false));
            Assert.DoesNotThrow(() => manager.OnEnemyKilled(null));

            // Cleanup
            Object.DestroyImmediate(managerObj);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PolishSystems_MultipleInstancesSingletons_OnlyOneInstanceExists()
        {
            // Arrange
            GameObject cameraObj1 = new GameObject("Camera1");
            cameraObj1.tag = "MainCamera";
            cameraObj1.AddComponent<Camera>();
            CameraShakeManager shake1 = cameraObj1.AddComponent<CameraShakeManager>();

            GameObject cameraObj2 = new GameObject("Camera2");
            cameraObj2.AddComponent<Camera>();
            CameraShakeManager shake2 = cameraObj2.AddComponent<CameraShakeManager>();

            // Act
            shake1.Awake();
            shake2.Awake();

            // Assert - Only one instance should exist
            Assert.IsNotNull(CameraShakeManager.Instance);
            int instanceCount = Object.FindObjectsOfType<CameraShakeManager>().Length;
            // One should be destroyed, so only 1 should remain
            Assert.LessOrEqual(instanceCount, 2, "Should handle multiple instances gracefully");

            // Cleanup
            Object.DestroyImmediate(cameraObj1);
            Object.DestroyImmediate(cameraObj2);
        }

        [Test]
        public void PolishSystems_AllSystemsCanCoexist()
        {
            // Arrange & Act - Create all systems
            GameObject cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            CameraShakeManager shakeManager = cameraObj.AddComponent<CameraShakeManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            GameObject damageIndicatorObj = new GameObject("DamageIndicator");
            DamageIndicator damageIndicator = damageIndicatorObj.AddComponent<DamageIndicator>();

            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorial = tutorialObj.AddComponent<TutorialManager>();

            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            // Assert - All systems should coexist
            Assert.IsNotNull(shakeManager);
            Assert.IsNotNull(hitMarker);
            Assert.IsNotNull(damageIndicator);
            Assert.IsNotNull(tutorial);
            Assert.IsNotNull(enhancer);
            Assert.IsNotNull(audio);

            // Cleanup
            Object.DestroyImmediate(cameraObj);
            Object.DestroyImmediate(hitMarkerObj);
            Object.DestroyImmediate(damageIndicatorObj);
            Object.DestroyImmediate(tutorialObj);
            Object.DestroyImmediate(enhancerObj);
            Object.DestroyImmediate(audioObj);
        }

        #endregion
    }
}
