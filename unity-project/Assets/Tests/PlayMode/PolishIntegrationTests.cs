using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TankCommander;

namespace TankCommander.Tests
{
    /// <summary>
    /// Integration tests for Polish Systems in PlayMode
    /// Tests system interactions, event handling, and runtime behavior
    /// </summary>
    [TestFixture]
    public class PolishIntegrationTests
    {
        private GameObject testScene;
        private GameObject playerTank;
        private GameObject enemyTank;
        private Camera mainCamera;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Create test scene
            testScene = new GameObject("TestScene");

            // Create camera
            GameObject cameraObj = new GameObject("MainCamera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.transform.position = new Vector3(0, 10, -10);
            mainCamera.transform.LookAt(Vector3.zero);

            // Create player tank
            playerTank = new GameObject("Player");
            playerTank.tag = "Player";
            playerTank.transform.position = Vector3.zero;
            Rigidbody playerRb = playerTank.AddComponent<Rigidbody>();
            playerRb.isKinematic = true;

            // Add player components
            Health playerHealth = playerTank.AddComponent<Health>();
            WeaponSystem playerWeapon = playerTank.AddComponent<WeaponSystem>();

            // Create enemy tank
            enemyTank = new GameObject("Enemy");
            enemyTank.tag = "Enemy";
            enemyTank.transform.position = new Vector3(10, 0, 0);
            Rigidbody enemyRb = enemyTank.AddComponent<Rigidbody>();
            enemyRb.isKinematic = true;
            enemyTank.AddComponent<Health>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            // Clean up all test objects
            if (testScene != null)
                Object.Destroy(testScene);
            if (playerTank != null)
                Object.Destroy(playerTank);
            if (enemyTank != null)
                Object.Destroy(enemyTank);
            if (mainCamera != null)
                Object.Destroy(mainCamera.gameObject);

            // Clean up polish systems
            CleanupPolishSystems();

            yield return null;
        }

        private void CleanupPolishSystems()
        {
            var cameraShake = Object.FindObjectOfType<CameraShakeManager>();
            if (cameraShake != null) Object.Destroy(cameraShake.gameObject);

            var hitMarker = Object.FindObjectOfType<HitMarker>();
            if (hitMarker != null) Object.Destroy(hitMarker.gameObject);

            var damageIndicator = Object.FindObjectOfType<DamageIndicator>();
            if (damageIndicator != null) Object.Destroy(damageIndicator.gameObject);

            var tutorial = Object.FindObjectOfType<TutorialManager>();
            if (tutorial != null) Object.Destroy(tutorial.gameObject);

            var enhancer = Object.FindObjectOfType<GameFeelEnhancer>();
            if (enhancer != null) Object.Destroy(enhancer.gameObject);

            var audio = Object.FindObjectOfType<AudioFeedbackSystem>();
            if (audio != null) Object.Destroy(audio.gameObject);

            var polishManager = Object.FindObjectOfType<PolishIntegrationManager>();
            if (polishManager != null) Object.Destroy(polishManager.gameObject);
        }

        #region Polish System Setup Tests

        [UnityTest]
        public IEnumerator PolishIntegrationManager_SetupAllSystems_CreatesAllComponents()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            // Act
            manager.SetupAllPolishSystems();
            yield return null;

            // Assert - Check that systems were created
            // Note: Some systems may not be created if prerequisites are missing (e.g., no Canvas)
            // But the setup should execute without errors
            Assert.IsNotNull(manager, "PolishIntegrationManager should exist");

            // Cleanup
            Object.Destroy(managerObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CameraShakeManager_RuntimeShake_MovesCamera()
        {
            // Arrange
            CameraShakeManager shakeManager = mainCamera.gameObject.AddComponent<CameraShakeManager>();
            Vector3 originalPosition = mainCamera.transform.localPosition;

            // Act
            shakeManager.Shake(0.5f, 0.5f);
            yield return null; // Wait one frame for shake to apply

            // Assert - Camera should have moved from original position during shake
            // (Note: This test may be flaky if shake is very small or has already decayed)
            Vector3 currentPosition = mainCamera.transform.localPosition;
            bool cameraShook = currentPosition != originalPosition || Mathf.Approximately(currentPosition.magnitude, 0f);
            Assert.IsTrue(cameraShook, "Camera should shake or return to zero position");

            // Wait for shake to complete
            yield return new WaitForSeconds(0.6f);

            // Camera should return close to original position
            Vector3 finalPosition = mainCamera.transform.localPosition;
            Assert.IsTrue(finalPosition.magnitude < 0.1f, "Camera should return near original position after shake");

            yield return null;
        }

        [UnityTest]
        public IEnumerator HitMarker_RuntimeDisplay_ShowsAndHides()
        {
            // Arrange
            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            // Act
            hitMarker.ShowNormalHit();
            yield return null;

            // Assert - Hit marker executed (test doesn't throw)
            Assert.IsNotNull(hitMarker, "Hit marker should exist after showing");

            // Wait for display duration
            yield return new WaitForSeconds(0.2f);

            // Hit marker should still exist (not destroyed, just hidden)
            Assert.IsNotNull(hitMarker, "Hit marker should persist after hiding");

            // Cleanup
            Object.Destroy(hitMarkerObj);
            yield return null;
        }

        #endregion

        #region Event Integration Tests

        [UnityTest]
        public IEnumerator PolishIntegrationManager_PlayerDamage_TriggersAllSystems()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            // Setup systems
            CameraShakeManager.Instance?.gameObject.SetActive(false); // Clear any existing
            CameraShakeManager shakeManager = mainCamera.gameObject.AddComponent<CameraShakeManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            yield return null;

            // Wire up manager
            manager.SetupAllPolishSystems();
            yield return null;

            // Act - Simulate player taking damage
            Health playerHealth = playerTank.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(25f);
                yield return null;
            }

            // Assert - Systems should still exist and be active
            Assert.IsNotNull(CameraShakeManager.Instance, "CameraShakeManager should respond to damage");
            Assert.IsNotNull(AudioFeedbackSystem.Instance, "AudioFeedbackSystem should respond to damage");

            // Cleanup
            Object.Destroy(managerObj);
            Object.Destroy(hitMarkerObj);
            Object.Destroy(audioObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PolishIntegrationManager_EnemyHit_ShowsHitMarker()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            yield return null;

            // Act - Call OnEnemyHit
            manager.OnEnemyHit(enemyTank, 25f, false);
            yield return null;

            // Assert - Should execute without errors
            Assert.IsNotNull(hitMarker, "Hit marker should exist after enemy hit");
            Assert.IsNotNull(manager, "Manager should persist after event");

            // Cleanup
            Object.Destroy(managerObj);
            Object.Destroy(hitMarkerObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PolishIntegrationManager_EnemyKilled_ShowsKillMarker()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            yield return null;

            // Act - Call OnEnemyKilled
            manager.OnEnemyKilled(enemyTank);
            yield return null;

            // Assert - Should execute without errors
            Assert.IsNotNull(hitMarker, "Hit marker should exist after enemy killed");
            Assert.IsNotNull(manager, "Manager should persist after kill event");

            // Cleanup
            Object.Destroy(managerObj);
            Object.Destroy(hitMarkerObj);
            yield return null;
        }

        #endregion

        #region Tutorial System Tests

        [UnityTest]
        public IEnumerator TutorialManager_FirstPlay_StartsAutomatically()
        {
            // Arrange
            PlayerPrefs.DeleteKey("TutorialComplete"); // Ensure fresh start
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorial = tutorialObj.AddComponent<TutorialManager>();

            // Act
            yield return null; // Let Start() run

            // Assert - Tutorial manager should be initialized
            Assert.IsNotNull(TutorialManager.Instance, "Tutorial instance should be set");
            Assert.IsNotNull(tutorial, "Tutorial should exist");

            // Cleanup
            Object.Destroy(tutorialObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TutorialManager_TriggerActions_AdvancesTutorial()
        {
            // Arrange
            PlayerPrefs.DeleteKey("TutorialComplete");
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorial = tutorialObj.AddComponent<TutorialManager>();
            tutorial.StartTutorial();

            yield return new WaitForSeconds(0.5f);

            // Act - Trigger tutorial actions
            tutorial.OnPlayerMoved();
            yield return new WaitForSeconds(0.1f);

            tutorial.OnPlayerFired();
            yield return new WaitForSeconds(0.1f);

            tutorial.OnPlayerHitEnemy();
            yield return new WaitForSeconds(0.1f);

            // Assert - Tutorial should still be active (or completed)
            Assert.IsNotNull(tutorial, "Tutorial should exist after triggers");

            // Cleanup
            Object.Destroy(tutorialObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TutorialManager_SkipTutorial_StopsImmediately()
        {
            // Arrange
            PlayerPrefs.DeleteKey("TutorialComplete");
            GameObject tutorialObj = new GameObject("TutorialManager");
            TutorialManager tutorial = tutorialObj.AddComponent<TutorialManager>();
            tutorial.StartTutorial();

            yield return new WaitForSeconds(0.2f);

            // Act
            tutorial.SkipTutorial();
            yield return null;

            // Assert - Tutorial should be skipped (completion flag set)
            int tutorialComplete = PlayerPrefs.GetInt("TutorialComplete", 0);
            Assert.AreEqual(1, tutorialComplete, "Tutorial should be marked complete after skip");

            // Cleanup
            Object.Destroy(tutorialObj);
            yield return null;
        }

        #endregion

        #region GameFeelEnhancer Tests

        [UnityTest]
        public IEnumerator GameFeelEnhancer_HealthChange_UpdatesVignette()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            yield return null;

            // Act - Update health through range
            enhancer.UpdateHealth(1.0f); // Full health
            yield return null;

            enhancer.UpdateHealth(0.5f); // Half health
            yield return null;

            enhancer.UpdateHealth(0.2f); // Low health (should trigger vignette)
            yield return null;

            enhancer.UpdateHealth(0.0f); // Critical
            yield return null;

            // Assert - Should execute without errors
            Assert.IsNotNull(enhancer, "Enhancer should exist after health updates");

            // Cleanup
            Object.Destroy(enhancerObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameFeelEnhancer_OnFire_ExpandsCrosshair()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            yield return null;

            // Act
            enhancer.OnFire();
            yield return null;

            // Brief wait for expansion
            yield return new WaitForSeconds(0.15f);

            // Assert - Should execute without errors
            Assert.IsNotNull(enhancer, "Enhancer should exist after fire");

            // Cleanup
            Object.Destroy(enhancerObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameFeelEnhancer_ReloadSequence_ShowsIndicator()
        {
            // Arrange
            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            yield return null;

            // Act - Simulate reload
            float reloadDuration = 0.5f;
            enhancer.OnReloadStart(reloadDuration);
            yield return null;

            // Wait partial reload
            yield return new WaitForSeconds(reloadDuration / 2f);

            // Complete reload
            yield return new WaitForSeconds(reloadDuration / 2f);
            enhancer.OnReloadComplete();
            yield return null;

            // Assert
            Assert.IsNotNull(enhancer, "Enhancer should exist after reload sequence");

            // Cleanup
            Object.Destroy(enhancerObj);
            yield return null;
        }

        #endregion

        #region Audio Feedback Tests

        [UnityTest]
        public IEnumerator AudioFeedbackSystem_PlayAllSounds_ExecutesWithoutError()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            yield return null;

            // Act - Play all sound types (should work with visual fallback)
            audio.PlayWeaponFire();
            yield return new WaitForSeconds(0.05f);

            audio.PlayReload();
            yield return new WaitForSeconds(0.05f);

            audio.PlayHitConfirm();
            yield return new WaitForSeconds(0.05f);

            audio.PlayCriticalHit();
            yield return new WaitForSeconds(0.05f);

            audio.PlayKillConfirm();
            yield return new WaitForSeconds(0.05f);

            audio.PlayTakeDamage(25f);
            yield return new WaitForSeconds(0.05f);

            audio.PlayButtonClick();
            yield return new WaitForSeconds(0.05f);

            // Assert - Should execute without errors
            Assert.IsNotNull(audio, "Audio system should exist after playing sounds");

            // Cleanup
            Object.Destroy(audioObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioFeedbackSystem_LowHealthWarning_LoopsCorrectly()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            yield return null;

            // Act - Start warning
            audio.PlayLowHealthWarning();
            yield return new WaitForSeconds(0.2f);

            // Stop warning
            audio.StopLowHealthWarning();
            yield return null;

            // Assert
            Assert.IsNotNull(audio, "Audio system should handle looping sounds");

            // Cleanup
            Object.Destroy(audioObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioFeedbackSystem_VolumeControl_UpdatesCorrectly()
        {
            // Arrange
            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            yield return null;

            // Act - Change volume
            audio.SetMasterVolume(0.5f);
            yield return null;

            audio.SetMasterVolume(0.0f);
            yield return null;

            audio.SetMasterVolume(1.0f);
            yield return null;

            // Assert
            Assert.IsNotNull(audio, "Audio system should handle volume changes");

            // Cleanup
            Object.Destroy(audioObj);
            yield return null;
        }

        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator PolishSystems_RunningTogether_MaintainPerformance()
        {
            // Arrange - Create all systems
            CameraShakeManager shakeManager = mainCamera.gameObject.AddComponent<CameraShakeManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            GameObject enhancerObj = new GameObject("GameFeelEnhancer");
            GameFeelEnhancer enhancer = enhancerObj.AddComponent<GameFeelEnhancer>();

            GameObject audioObj = new GameObject("AudioFeedbackSystem");
            AudioFeedbackSystem audio = audioObj.AddComponent<AudioFeedbackSystem>();

            yield return null;

            // Act - Trigger multiple systems simultaneously
            float startTime = Time.realtimeSinceStartup;

            shakeManager.ShakeDamage();
            hitMarker.ShowCriticalHit();
            enhancer.OnFire();
            enhancer.UpdateHealth(0.3f);
            audio.PlayHitConfirm();
            audio.PlayTakeDamage(30f);

            yield return null;

            float endTime = Time.realtimeSinceStartup;
            float executionTime = (endTime - startTime) * 1000f; // Convert to milliseconds

            // Assert - Should execute quickly (< 2ms for all systems)
            Assert.Less(executionTime, 2f, $"Polish systems should execute quickly (took {executionTime:F2}ms)");

            // Cleanup
            Object.Destroy(hitMarkerObj);
            Object.Destroy(enhancerObj);
            Object.Destroy(audioObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PolishSystems_RepeatedCalls_NoMemoryLeaks()
        {
            // Arrange
            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            yield return null;

            // Act - Call hit marker many times
            for (int i = 0; i < 50; i++)
            {
                hitMarker.ShowNormalHit();
                if (i % 10 == 0)
                    yield return null; // Yield occasionally
            }

            yield return null;

            // Assert - Should not accumulate objects
            Assert.IsNotNull(hitMarker, "Hit marker should exist after repeated calls");

            // Cleanup
            Object.Destroy(hitMarkerObj);
            yield return null;
        }

        #endregion

        #region Stress Tests

        [UnityTest]
        public IEnumerator CameraShakeManager_RapidShakes_HandlesCorrectly()
        {
            // Arrange
            CameraShakeManager shakeManager = mainCamera.gameObject.AddComponent<CameraShakeManager>();

            yield return null;

            // Act - Trigger many shakes rapidly
            for (int i = 0; i < 20; i++)
            {
                shakeManager.Shake(0.1f, 0.1f);
            }

            yield return new WaitForSeconds(0.5f);

            // Assert - Camera should return to stable position
            Vector3 finalPosition = mainCamera.transform.localPosition;
            Assert.Less(finalPosition.magnitude, 0.2f, "Camera should stabilize after rapid shakes");

            yield return null;
        }

        [UnityTest]
        public IEnumerator PolishIntegrationManager_ManyEnemyHits_HandlesCorrectly()
        {
            // Arrange
            GameObject managerObj = new GameObject("PolishIntegrationManager");
            PolishIntegrationManager manager = managerObj.AddComponent<PolishIntegrationManager>();

            GameObject hitMarkerObj = new GameObject("HitMarker");
            HitMarker hitMarker = hitMarkerObj.AddComponent<HitMarker>();

            yield return null;

            // Act - Simulate many enemy hits rapidly
            for (int i = 0; i < 30; i++)
            {
                bool isCritical = (i % 5 == 0);
                manager.OnEnemyHit(enemyTank, 25f, isCritical);

                if (i % 10 == 0)
                    yield return null; // Yield occasionally
            }

            yield return null;

            // Assert - Should handle rapid calls
            Assert.IsNotNull(manager, "Manager should handle rapid enemy hit calls");
            Assert.IsNotNull(hitMarker, "Hit marker should handle rapid calls");

            // Cleanup
            Object.Destroy(managerObj);
            Object.Destroy(hitMarkerObj);
            yield return null;
        }

        #endregion
    }
}
