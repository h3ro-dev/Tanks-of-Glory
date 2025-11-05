using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TankCommander.Tests
{
    /// <summary>
    /// Automated test runner for validating the complete game build
    /// Can be run from Editor menu or via command line for CI/CD
    /// </summary>
    public class AutomatedTestRunner : MonoBehaviour
    {
        private static AutomatedTestRunner instance;

        [Header("Test Configuration")]
        [SerializeField] private bool runAllTests = true;
        [SerializeField] private bool testGameScene = true;
        [SerializeField] private bool testMainMenu = true;
        [SerializeField] private bool validateBuild = true;

        [Header("Test Results")]
        [SerializeField] private int totalTests = 0;
        [SerializeField] private int passedTests = 0;
        [SerializeField] private int failedTests = 0;

        private List<TestResult> results = new List<TestResult>();

        private struct TestResult
        {
            public string testName;
            public bool passed;
            public string message;

            public TestResult(string name, bool pass, string msg = "")
            {
                testName = name;
                passed = pass;
                message = msg;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tank Commander/Run All Tests 🧪")]
        public static void RunAllTestsFromMenu()
        {
            // Create test runner in scene
            GameObject testRunner = new GameObject("AutomatedTestRunner");
            AutomatedTestRunner runner = testRunner.AddComponent<AutomatedTestRunner>();
            runner.StartTests();
        }

        [MenuItem("Tank Commander/Validate Game Build 🔍")]
        public static void ValidateBuildFromMenu()
        {
            // Create validator in scene
            GameObject validator = new GameObject("GameBuildValidator");
            GameBuildValidator buildValidator = validator.AddComponent<GameBuildValidator>();
            buildValidator.ValidateGameBuild();
        }

        [MenuItem("Tank Commander/Generate Test Report 📊")]
        public static void GenerateTestReport()
        {
            GameObject testRunner = new GameObject("TestReportGenerator");
            AutomatedTestRunner runner = testRunner.AddComponent<AutomatedTestRunner>();
            runner.GenerateFullReport();
        }
#endif

        private void Start()
        {
            if (runAllTests)
            {
                StartTests();
            }
        }

        [ContextMenu("▶ Start All Tests")]
        public void StartTests()
        {
            Debug.Log("========================================");
            Debug.Log("🧪 AUTOMATED TEST SUITE STARTED");
            Debug.Log("========================================");

            results.Clear();
            totalTests = 0;
            passedTests = 0;
            failedTests = 0;

            StartCoroutine(RunAllTestsCoroutine());
        }

        private IEnumerator RunAllTestsCoroutine()
        {
            // Test 1: Validate current scene
            yield return StartCoroutine(ValidateCurrentScene());

            // Test 2: Check managers
            yield return StartCoroutine(TestManagerSystems());

            // Test 3: Test game build
            if (validateBuild)
            {
                yield return StartCoroutine(TestGameBuildValidation());
            }

            // Test 4: Test scene loading (if applicable)
            if (testGameScene || testMainMenu)
            {
                yield return StartCoroutine(TestSceneLoading());
            }

            // Test 5: Component validation
            yield return StartCoroutine(TestComponentIntegrity());

            // Generate final report
            GenerateTestReport();

            Debug.Log("========================================");
            Debug.Log("🧪 AUTOMATED TEST SUITE COMPLETED");
            Debug.Log($"✅ Passed: {passedTests}/{totalTests}");
            Debug.Log($"❌ Failed: {failedTests}/{totalTests}");
            Debug.Log("========================================");
        }

        private IEnumerator ValidateCurrentScene()
        {
            AddTest("Scene Validation", "Checking current scene setup");

            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.IsValid())
            {
                PassTest("Scene Validation", $"Scene '{currentScene.name}' is valid");
            }
            else
            {
                FailTest("Scene Validation", "Current scene is invalid!");
            }

            yield return null;
        }

        private IEnumerator TestManagerSystems()
        {
            AddTest("Manager Systems", "Checking for required managers");

            bool allManagersPresent = true;

            // GameManager
            if (FindObjectOfType<GameManager>() == null)
            {
                FailTest("GameManager", "GameManager not found in scene");
                allManagersPresent = false;
            }
            else
            {
                PassTest("GameManager", "GameManager found");
            }

            // UIManager
            if (FindObjectOfType<UIManager>() == null)
            {
                Debug.LogWarning("UIManager not found - UI features may not work");
            }
            else
            {
                PassTest("UIManager", "UIManager found");
            }

            // AudioManager
            if (FindObjectOfType<AudioManager>() == null)
            {
                Debug.LogWarning("AudioManager not found - audio features may not work");
            }
            else
            {
                PassTest("AudioManager", "AudioManager found");
            }

            if (allManagersPresent)
            {
                PassTest("Manager Systems", "All critical managers present");
            }
            else
            {
                FailTest("Manager Systems", "Some managers are missing");
            }

            yield return null;
        }

        private IEnumerator TestGameBuildValidation()
        {
            AddTest("Build Validation", "Running GameBuildValidator");

            // Create validator
            GameObject validatorObj = new GameObject("TempValidator");
            GameBuildValidator validator = validatorObj.AddComponent<GameBuildValidator>();

            // Run validation
            validator.ValidateGameBuild();

            yield return new WaitForSeconds(0.5f);

            if (validator.IsValid())
            {
                PassTest("Build Validation", $"Build is valid ({validator.GetSuccessCount()} successes)");
            }
            else
            {
                FailTest("Build Validation", $"Build has {validator.GetErrorCount()} errors, {validator.GetWarningCount()} warnings");
            }

            Destroy(validatorObj);

            yield return null;
        }

        private IEnumerator TestSceneLoading()
        {
            AddTest("Scene Loading", "Checking available scenes");

            int sceneCount = SceneManager.sceneCountInBuildSettings;

            if (sceneCount > 0)
            {
                PassTest("Scene Loading", $"{sceneCount} scenes in build settings");
            }
            else
            {
                FailTest("Scene Loading", "No scenes in build settings!");
            }

            yield return null;
        }

        private IEnumerator TestComponentIntegrity()
        {
            AddTest("Component Integrity", "Testing component interactions");

            // Find all tanks in scene
            TankController[] tanks = FindObjectsOfType<TankController>();

            if (tanks.Length > 0)
            {
                foreach (TankController tank in tanks)
                {
                    // Check if tank has all required components
                    bool hasAllComponents = true;

                    if (tank.GetComponent<Rigidbody>() == null)
                    {
                        FailTest($"Tank {tank.name}", "Missing Rigidbody");
                        hasAllComponents = false;
                    }

                    if (tank.GetComponent<Collider>() == null)
                    {
                        FailTest($"Tank {tank.name}", "Missing Collider");
                        hasAllComponents = false;
                    }

                    if (tank.GetComponent<Health>() == null)
                    {
                        FailTest($"Tank {tank.name}", "Missing Health component");
                        hasAllComponents = false;
                    }

                    if (hasAllComponents)
                    {
                        PassTest($"Tank {tank.name}", "All components present");
                    }
                }

                PassTest("Component Integrity", $"Tested {tanks.Length} tanks");
            }
            else
            {
                Debug.LogWarning("No tanks found in scene to test");
            }

            yield return null;
        }

        private void AddTest(string testName, string description)
        {
            totalTests++;
            Debug.Log($"[TEST {totalTests}] {testName}: {description}");
        }

        private void PassTest(string testName, string message)
        {
            passedTests++;
            results.Add(new TestResult(testName, true, message));
            Debug.Log($"✅ PASS: {testName} - {message}");
        }

        private void FailTest(string testName, string message)
        {
            failedTests++;
            results.Add(new TestResult(testName, false, message));
            Debug.LogError($"❌ FAIL: {testName} - {message}");
        }

        private void GenerateTestReport()
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("========================================");
            report.AppendLine("📊 COMPREHENSIVE TEST REPORT");
            report.AppendLine("========================================");
            report.AppendLine();

            report.AppendLine("SUMMARY:");
            report.AppendLine($"Total Tests: {totalTests}");
            report.AppendLine($"Passed: {passedTests} ({(totalTests > 0 ? (passedTests * 100f / totalTests) : 0):F1}%)");
            report.AppendLine($"Failed: {failedTests}");
            report.AppendLine();

            if (failedTests > 0)
            {
                report.AppendLine("FAILED TESTS:");
                foreach (TestResult result in results)
                {
                    if (!result.passed)
                    {
                        report.AppendLine($"❌ {result.testName}: {result.message}");
                    }
                }
                report.AppendLine();
            }

            if (passedTests > 0)
            {
                report.AppendLine("PASSED TESTS:");
                foreach (TestResult result in results)
                {
                    if (result.passed)
                    {
                        report.AppendLine($"✅ {result.testName}: {result.message}");
                    }
                }
                report.AppendLine();
            }

            report.AppendLine("VERDICT:");
            if (failedTests == 0)
            {
                report.AppendLine("🎉 ALL TESTS PASSED! Game is ready to play!");
            }
            else
            {
                report.AppendLine($"⚠️  {failedTests} test(s) failed. Please fix before release.");
            }

            report.AppendLine("========================================");

            Debug.Log(report.ToString());

            // Save report to file (optional)
            #if UNITY_EDITOR
            string reportPath = Application.dataPath + "/../test_report.txt";
            System.IO.File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"Test report saved to: {reportPath}");
            #endif
        }

        [ContextMenu("📊 Generate Full Report")]
        public void GenerateFullReport()
        {
            Debug.Log("========================================");
            Debug.Log("📊 GENERATING COMPREHENSIVE GAME REPORT");
            Debug.Log("========================================");

            StringBuilder report = new StringBuilder();

            // Game Information
            report.AppendLine("GAME INFORMATION:");
            report.AppendLine($"Product Name: {Application.productName}");
            report.AppendLine($"Version: {Application.version}");
            report.AppendLine($"Unity Version: {Application.unityVersion}");
            report.AppendLine($"Platform: {Application.platform}");
            report.AppendLine();

            // Scene Information
            Scene activeScene = SceneManager.GetActiveScene();
            report.AppendLine("SCENE INFORMATION:");
            report.AppendLine($"Active Scene: {activeScene.name}");
            report.AppendLine($"Scene Path: {activeScene.path}");
            report.AppendLine($"Scenes in Build: {SceneManager.sceneCountInBuildSettings}");
            report.AppendLine();

            // Object Counts
            report.AppendLine("OBJECT COUNTS:");
            report.AppendLine($"Total GameObjects: {FindObjectsOfType<GameObject>().Length}");
            report.AppendLine($"Tanks: {FindObjectsOfType<TankController>().Length}");
            report.AppendLine($"AI Tanks: {FindObjectsOfType<TankAI>().Length}");
            report.AppendLine($"Spawn Points: {FindObjectsOfType<SpawnPoint>().Length}");
            report.AppendLine($"Cameras: {FindObjectsOfType<Camera>().Length}");
            report.AppendLine();

            // Component Status
            report.AppendLine("MANAGERS:");
            report.AppendLine($"GameManager: {(FindObjectOfType<GameManager>() != null ? "✓ Present" : "✗ Missing")}");
            report.AppendLine($"UIManager: {(FindObjectOfType<UIManager>() != null ? "✓ Present" : "✗ Missing")}");
            report.AppendLine($"AudioManager: {(FindObjectOfType<AudioManager>() != null ? "✓ Present" : "✗ Missing")}");
            report.AppendLine($"ObjectPooler: {(FindObjectOfType<ObjectPooler>() != null ? "✓ Present" : "✗ Missing")}");
            report.AppendLine();

            // System Status
            report.AppendLine("SYSTEMS:");
            report.AppendLine($"Physics Engine: Active");
            report.AppendLine($"Time Scale: {Time.timeScale}");
            report.AppendLine($"Target Frame Rate: {Application.targetFrameRate}");
            report.AppendLine();

            report.AppendLine("========================================");

            Debug.Log(report.ToString());
        }
    }
}
