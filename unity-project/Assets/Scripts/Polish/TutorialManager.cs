using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace TankCommander.Polish
{
    /// <summary>
    /// In-game tutorial system that guides new players
    /// Shows contextual tips and tracks player progress
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private Image tutorialIcon;
        [SerializeField] private float tipDisplayDuration = 5f;

        [Header("Settings")]
        [SerializeField] private bool showTutorial = true;
        [SerializeField] private bool autoProgress = true;
        [SerializeField] private KeyCode skipKey = KeyCode.H;

        [Header("Tutorial Steps")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

        [System.Serializable]
        public class TutorialStep
        {
            public string stepName;
            [TextArea(3, 5)]
            public string message;
            public float duration = 5f;
            public bool requiresAction = false;
            public TutorialTrigger trigger = TutorialTrigger.Time;
        }

        public enum TutorialTrigger
        {
            Time,
            Movement,
            Fire,
            Hit,
            TakeDamage,
            Kill,
            CameraChange
        }

        private int currentStep = 0;
        private bool tutorialCompleted = false;
        private bool isShowingTip = false;
        private Coroutine currentTipCoroutine;

        // Progress tracking
        private bool hasMovedForward = false;
        private bool hasTurned = false;
        private bool hasFired = false;
        private bool hasHitEnemy = false;
        private bool hasTakenDamage = false;
        private bool hasKilled = false;
        private bool hasChangedCamera = false;

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

            // Check if player has already completed tutorial
            if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
            {
                showTutorial = false;
            }

            // Setup default tutorial steps if none defined
            if (tutorialSteps.Count == 0)
            {
                SetupDefaultTutorial();
            }

            // Create UI if not assigned
            if (tutorialPanel == null)
            {
                CreateTutorialUI();
            }
        }

        private void Start()
        {
            if (showTutorial && !tutorialCompleted)
            {
                StartCoroutine(RunTutorial());
            }
        }

        private void Update()
        {
            // Allow skipping tutorial
            if (Input.GetKeyDown(skipKey))
            {
                if (isShowingTip)
                {
                    HideTip();
                }
                else
                {
                    SkipTutorial();
                }
            }
        }

        private void SetupDefaultTutorial()
        {
            tutorialSteps = new List<TutorialStep>
            {
                new TutorialStep
                {
                    stepName = "Welcome",
                    message = "Welcome to Tank Commander!\nPress [H] to hide tips.",
                    duration = 3f,
                    trigger = TutorialTrigger.Time
                },
                new TutorialStep
                {
                    stepName = "Movement",
                    message = "Use [W][A][S][D] to move your tank.\n[W] forward, [S] back, [A][D] to turn.",
                    duration = 5f,
                    requiresAction = true,
                    trigger = TutorialTrigger.Movement
                },
                new TutorialStep
                {
                    stepName = "Aiming",
                    message = "Move your [MOUSE] to aim the turret.\nThe turret rotates independently!",
                    duration = 4f,
                    trigger = TutorialTrigger.Time
                },
                new TutorialStep
                {
                    stepName = "Firing",
                    message = "[LEFT CLICK] to fire your cannon.\nWatch your ammo count!",
                    duration = 5f,
                    requiresAction = true,
                    trigger = TutorialTrigger.Fire
                },
                new TutorialStep
                {
                    stepName = "Combat",
                    message = "Hit an enemy to see the hit marker.\nDestroy all enemies to win!",
                    duration = 5f,
                    requiresAction = false,
                    trigger = TutorialTrigger.Hit
                },
                new TutorialStep
                {
                    stepName = "Camera",
                    message = "Press [C] to change camera views.\nFind the view that works for you!",
                    duration = 4f,
                    trigger = TutorialTrigger.CameraChange
                },
                new TutorialStep
                {
                    stepName = "Complete",
                    message = "You're ready! Good luck, Commander!\nPress [ESC] to pause.",
                    duration = 3f,
                    trigger = TutorialTrigger.Time
                }
            };
        }

        private void CreateTutorialUI()
        {
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("TutorialCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 150;

                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create tutorial panel
            GameObject panel = new GameObject("TutorialPanel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.15f);
            panelRect.anchorMax = new Vector2(0.5f, 0.15f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 100);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);

            tutorialPanel = panel;

            // Create text
            GameObject textObj = new GameObject("TutorialText");
            textObj.transform.SetParent(panel.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -20);

            tutorialText = textObj.AddComponent<TextMeshProUGUI>();
            tutorialText.text = "";
            tutorialText.fontSize = 20;
            tutorialText.color = Color.white;
            tutorialText.alignment = TextAlignmentOptions.Center;

            tutorialPanel.SetActive(false);
        }

        private IEnumerator RunTutorial()
        {
            yield return new WaitForSeconds(1f); // Initial delay

            while (currentStep < tutorialSteps.Count && !tutorialCompleted)
            {
                TutorialStep step = tutorialSteps[currentStep];

                // Show tip
                ShowTip(step.message, step.duration);

                // Wait for requirement
                if (step.requiresAction)
                {
                    yield return StartCoroutine(WaitForTrigger(step.trigger));
                }
                else
                {
                    yield return new WaitForSeconds(step.duration);
                }

                HideTip();
                yield return new WaitForSeconds(1f); // Pause between steps

                currentStep++;
            }

            CompleteTutorial();
        }

        private IEnumerator WaitForTrigger(TutorialTrigger trigger)
        {
            bool triggered = false;

            while (!triggered)
            {
                switch (trigger)
                {
                    case TutorialTrigger.Movement:
                        triggered = hasMovedForward || hasTurned;
                        break;
                    case TutorialTrigger.Fire:
                        triggered = hasFired;
                        break;
                    case TutorialTrigger.Hit:
                        triggered = hasHitEnemy;
                        break;
                    case TutorialTrigger.TakeDamage:
                        triggered = hasTakenDamage;
                        break;
                    case TutorialTrigger.Kill:
                        triggered = hasKilled;
                        break;
                    case TutorialTrigger.CameraChange:
                        triggered = hasChangedCamera;
                        break;
                }

                yield return null;
            }
        }

        public void ShowTip(string message, float duration = 5f)
        {
            if (!showTutorial || tutorialCompleted) return;

            if (currentTipCoroutine != null)
            {
                StopCoroutine(currentTipCoroutine);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
            }

            if (tutorialText != null)
            {
                tutorialText.text = message;
            }

            isShowingTip = true;
        }

        public void HideTip()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            isShowingTip = false;
        }

        public void SkipTutorial()
        {
            tutorialCompleted = true;
            HideTip();

            if (currentTipCoroutine != null)
            {
                StopCoroutine(currentTipCoroutine);
            }

            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            Debug.Log("Tutorial skipped");
        }

        private void CompleteTutorial()
        {
            tutorialCompleted = true;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            Debug.Log("Tutorial completed!");
        }

        // Public methods for tracking actions
        public void OnPlayerMoved() => hasMovedForward = true;
        public void OnPlayerTurned() => hasTurned = true;
        public void OnPlayerFired() => hasFired = true;
        public void OnPlayerHitEnemy() => hasHitEnemy = true;
        public void OnPlayerTookDamage() => hasTakenDamage = true;
        public void OnPlayerKill() => hasKilled = true;
        public void OnCameraChanged() => hasChangedCamera = true;

        [ContextMenu("Reset Tutorial Progress")]
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            tutorialCompleted = false;
            currentStep = 0;
            showTutorial = true;

            // Reset progress
            hasMovedForward = false;
            hasTurned = false;
            hasFired = false;
            hasHitEnemy = false;
            hasTakenDamage = false;
            hasKilled = false;
            hasChangedCamera = false;

            Debug.Log("Tutorial progress reset");
        }
    }
}
