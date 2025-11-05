using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TankCommander
{
    /// <summary>
    /// Controls the main menu UI and scene transitions
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Settings UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        private void Start()
        {
            // Show main panel by default
            ShowMainPanel();

            // Load settings
            LoadSettings();

            // Play menu music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        #region Button Handlers

        public void OnPlayGame()
        {
            Debug.Log("Starting game...");

            // Load game scene
            SceneManager.LoadScene("GameScene");
        }

        public void OnSettings()
        {
            ShowSettingsPanel();
        }

        public void OnCredits()
        {
            ShowCreditsPanel();
        }

        public void OnQuit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void OnBack()
        {
            ShowMainPanel();
        }

        #endregion

        #region Panel Management

        private void ShowMainPanel()
        {
            HideAllPanels();
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }

        private void ShowSettingsPanel()
        {
            HideAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void ShowCreditsPanel()
        {
            HideAllPanels();
            if (creditsPanel != null)
                creditsPanel.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            // Load master volume
            if (masterVolumeSlider != null)
            {
                float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeSlider.value = masterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            // Load music volume
            if (musicVolumeSlider != null)
            {
                float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
                musicVolumeSlider.value = musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            // Load SFX volume
            if (sfxVolumeSlider != null)
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.value = sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // Load fullscreen
            if (fullscreenToggle != null)
            {
                bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
                fullscreenToggle.isOn = fullscreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            // Load quality
            if (qualityDropdown != null)
            {
                int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
                qualityDropdown.value = quality;
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }

        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnQualityChanged(int value)
        {
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt("Quality", value);
            PlayerPrefs.Save();
        }

        #endregion

        #region Auto-Setup (for testing in editor)

        [ContextMenu("Create Complete Main Menu")]
        public void CreateCompleteMainMenu()
        {
            // This would be called from editor to auto-setup the main menu
            // Implementation would create all UI elements programmatically
            Debug.Log("Auto-creating main menu UI...");

            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Canvas");
                Canvas c = canvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvas.AddComponent<GraphicRaycaster>();
            }

            CreateMenuUI(canvas);
        }

        private void CreateMenuUI(GameObject canvas)
        {
            // Create title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(canvas.transform, false);
            RectTransform titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(800, 150);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "TANK COMMANDER";
            titleText.fontSize = 72;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Create buttons container
            GameObject buttonsContainer = new GameObject("ButtonsContainer");
            buttonsContainer.transform.SetParent(canvas.transform, false);
            RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonsRect.pivot = new Vector2(0.5f, 0.5f);
            buttonsRect.sizeDelta = new Vector2(400, 400);

            // Create buttons
            CreateMenuButton(buttonsContainer, "PlayButton", "PLAY", 0, OnPlayGame);
            CreateMenuButton(buttonsContainer, "SettingsButton", "SETTINGS", -80, OnSettings);
            CreateMenuButton(buttonsContainer, "CreditsButton", "CREDITS", -160, OnCredits);
            CreateMenuButton(buttonsContainer, "QuitButton", "QUIT", -240, OnQuit);

            Debug.Log("Main menu UI created!");
        }

        private void CreateMenuButton(GameObject parent, string name, string text, float yOffset, UnityEngine.Events.UnityAction onClick)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent.transform, false);

            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, yOffset);
            rect.sizeDelta = new Vector2(300, 60);

            Image image = button.AddComponent<Image>();
            image.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);

            Button btn = button.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 32;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontStyle = FontStyles.Bold;
        }

        #endregion
    }
}
