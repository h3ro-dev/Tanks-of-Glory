using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace TankCommander
{
    /// <summary>
    /// Manages all UI elements in the game including HUD, menus, and overlays
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Elements")]
        [SerializeField] private GameObject hudContainer;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image crosshair;
        [SerializeField] private RawImage minimapImage;

        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Game Over Elements")]
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Notifications")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;

        private Coroutine notificationCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        #region HUD Management

        public void ShowHUD()
        {
            if (hudContainer != null)
                hudContainer.SetActive(true);

            HideAllMenus();
        }

        public void HideHUD()
        {
            if (hudContainer != null)
                hudContainer.SetActive(false);
        }

        public void UpdateHealth(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.value = current / max;
            }
        }

        public void UpdateAmmo(int current, int max)
        {
            if (ammoText != null)
            {
                if (max == -1) // Infinite ammo
                    ammoText.text = $"∞";
                else
                    ammoText.text = $"{current}/{max}";
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateTime(float timeRemaining)
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void SetCrosshairColor(Color color)
        {
            if (crosshair != null)
            {
                crosshair.color = color;
            }
        }

        #endregion

        #region Menu Management

        public void ShowMainMenu()
        {
            HideAllMenus();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            HideHUD();
            Time.timeScale = 1f;
        }

        public void ShowPauseMenu()
        {
            HideAllMenus();
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            Time.timeScale = 0f;
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            ShowHUD();
            Time.timeScale = 1f;
        }

        public void ShowGameOver(bool victory, PlayerInfo playerInfo)
        {
            HideAllMenus();
            HideHUD();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverTitle != null)
                {
                    gameOverTitle.text = victory ? "VICTORY!" : "DEFEAT";
                    gameOverTitle.color = victory ? Color.green : Color.red;
                }

                if (finalScoreText != null && playerInfo != null)
                {
                    finalScoreText.text = $"Final Score: {GameManager.Instance.GetPlayerScore(playerInfo.PlayerIndex)}";
                }

                if (statsText != null && playerInfo != null)
                {
                    var stats = playerInfo.GetMatchStats();
                    statsText.text = $"Kills: {stats.kills}\n" +
                                   $"Deaths: {stats.deaths}\n" +
                                   $"Assists: {stats.assists}\n" +
                                   $"Damage Dealt: {stats.damageDealt:F0}\n" +
                                   $"Damage Taken: {stats.damageTaken:F0}";
                }
            }

            Time.timeScale = 0f;
        }

        public void ShowSettings()
        {
            HideAllMenus();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void HideAllMenus()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        #endregion

        #region Notifications

        public void ShowNotification(string message)
        {
            if (notificationText != null)
            {
                if (notificationCoroutine != null)
                    StopCoroutine(notificationCoroutine);

                notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
            }
        }

        private IEnumerator ShowNotificationCoroutine(string message)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);

            yield return new WaitForSeconds(notificationDuration);

            notificationText.gameObject.SetActive(false);
        }

        #endregion

        #region Button Handlers

        public void OnStartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameMode.Arcade);
            }
        }

        public void OnResumeGame()
        {
            HidePauseMenu();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        public void OnRestartGame()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
        }

        public void OnQuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion
    }
}
