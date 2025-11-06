using UnityEngine;
using UnityEngine.UI;

namespace TankCommander.Polish
{
    /// <summary>
    /// Enhances game feel with dynamic crosshair, reticle feedback, and visual polish
    /// Small details that make the game feel more responsive and professional
    /// </summary>
    public class GameFeelEnhancer : MonoBehaviour
    {
        public static GameFeelEnhancer Instance { get; private set; }

        [Header("Dynamic Crosshair")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private float baseSize = 20f;
        [SerializeField] private float expandSize = 30f;
        [SerializeField] private float expandSpeed = 10f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color enemyColor = Color.red;

        [Header("Reload Indicator")]
        [SerializeField] private Image reloadIndicator;
        [SerializeField] private float reloadIndicatorSize = 60f;

        [Header("Low Health Warning")]
        [SerializeField] private Image vignetteImage;
        [SerializeField] private float lowHealthThreshold = 0.3f;
        [SerializeField] private float vignetteIntensity = 0.5f;
        [SerializeField] private float pulsespeed = 2f;

        [Header("Speed Lines")]
        [SerializeField] private GameObject speedLinesEffect;
        [SerializeField] private float speedLinesThreshold = 8f;

        private float targetCrosshairSize;
        private Color targetCrosshairColor;
        private bool isReloading = false;
        private float reloadProgress = 0f;
        private float currentHealth = 1f;
        private float currentSpeed = 0f;

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

            // Setup components if not assigned
            if (crosshairImage == null)
            {
                SetupCrosshair();
            }

            if (reloadIndicator == null)
            {
                SetupReloadIndicator();
            }

            if (vignetteImage == null)
            {
                SetupVignette();
            }

            targetCrosshairSize = baseSize;
            targetCrosshairColor = normalColor;
        }

        private void Update()
        {
            UpdateCrosshair();
            UpdateReloadIndicator();
            UpdateVignette();
            UpdateSpeedLines();
        }

        private void UpdateCrosshair()
        {
            if (crosshairImage == null) return;

            // Smooth size transition
            RectTransform rect = crosshairImage.rectTransform;
            Vector2 currentSize = rect.sizeDelta;
            Vector2 targetSize = Vector2.one * targetCrosshairSize;
            rect.sizeDelta = Vector2.Lerp(currentSize, targetSize, Time.deltaTime * expandSpeed);

            // Smooth color transition
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetCrosshairColor, Time.deltaTime * expandSpeed);
        }

        private void UpdateReloadIndicator()
        {
            if (reloadIndicator == null) return;

            reloadIndicator.gameObject.SetActive(isReloading);

            if (isReloading)
            {
                reloadIndicator.fillAmount = reloadProgress;

                // Pulse effect
                float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
                reloadIndicator.transform.localScale = Vector3.one * pulse;
            }
        }

        private void UpdateVignette()
        {
            if (vignetteImage == null) return;

            if (currentHealth < lowHealthThreshold)
            {
                float healthPercent = currentHealth / lowHealthThreshold;
                float pulse = Mathf.Sin(Time.time * pulsespeed) * 0.5f + 0.5f;
                float targetAlpha = (1f - healthPercent) * vignetteIntensity * pulse;

                Color color = vignetteImage.color;
                color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * 5f);
                vignetteImage.color = color;
            }
            else
            {
                Color color = vignetteImage.color;
                color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 5f);
                vignetteImage.color = color;
            }
        }

        private void UpdateSpeedLines()
        {
            if (speedLinesEffect == null) return;

            if (currentSpeed > speedLinesThreshold)
            {
                if (!speedLinesEffect.activeSelf)
                    speedLinesEffect.SetActive(true);

                // Adjust intensity based on speed
                float intensity = Mathf.Clamp01((currentSpeed - speedLinesThreshold) / 5f);
                // Could modify particle system emission rate here
            }
            else
            {
                if (speedLinesEffect.activeSelf)
                    speedLinesEffect.SetActive(false);
            }
        }

        private void SetupCrosshair()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject crosshairObj = new GameObject("DynamicCrosshair");
            crosshairObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = crosshairObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * baseSize;

            crosshairImage = crosshairObj.AddComponent<Image>();
            crosshairImage.color = normalColor;
            crosshairImage.raycastTarget = false;

            // Create simple cross
            CreateCrosshairLines(crosshairObj.transform);
        }

        private void CreateCrosshairLines(Transform parent)
        {
            // Top line
            CreateLine(parent, new Vector2(0, 8), new Vector2(2, 6), "Top");
            // Bottom line
            CreateLine(parent, new Vector2(0, -8), new Vector2(2, 6), "Bottom");
            // Left line
            CreateLine(parent, new Vector2(-8, 0), new Vector2(6, 2), "Left");
            // Right line
            CreateLine(parent, new Vector2(8, 0), new Vector2(6, 2), "Right");
        }

        private void CreateLine(Transform parent, Vector2 position, Vector2 size, string name)
        {
            GameObject line = new GameObject(name);
            line.transform.SetParent(parent, false);

            RectTransform rect = line.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = line.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private void SetupReloadIndicator()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject indicatorObj = new GameObject("ReloadIndicator");
            indicatorObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = indicatorObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * reloadIndicatorSize;

            reloadIndicator = indicatorObj.AddComponent<Image>();
            reloadIndicator.type = Image.Type.Filled;
            reloadIndicator.fillMethod = Image.FillMethod.Radial360;
            reloadIndicator.fillOrigin = (int)Image.Origin360.Top;
            reloadIndicator.color = new Color(1f, 1f, 0f, 0.7f);
            reloadIndicator.raycastTarget = false;

            indicatorObj.SetActive(false);
        }

        private void SetupVignette()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject vignetteObj = new GameObject("LowHealthVignette");
            vignetteObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = vignetteObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            vignetteImage = vignetteObj.AddComponent<Image>();
            vignetteImage.color = new Color(1f, 0f, 0f, 0f);
            vignetteImage.raycastTarget = false;

            // Create radial gradient effect
            // In actual Unity, you'd use a vignette texture
        }

        // Public methods for external systems to call

        public void OnFire()
        {
            ExpandCrosshair();
        }

        public void OnTargetingEnemy(bool targeting)
        {
            targetCrosshairColor = targeting ? enemyColor : normalColor;
        }

        public void OnReloadStart(float duration)
        {
            isReloading = true;
            reloadProgress = 0f;
            StartCoroutine(AnimateReload(duration));
        }

        public void OnReloadComplete()
        {
            isReloading = false;
            reloadProgress = 0f;
        }

        public void UpdateHealth(float healthPercent)
        {
            currentHealth = healthPercent;
        }

        public void UpdateSpeed(float speed)
        {
            currentSpeed = speed;
        }

        private void ExpandCrosshair()
        {
            targetCrosshairSize = expandSize;
            Invoke(nameof(ContractCrosshair), 0.1f);
        }

        private void ContractCrosshair()
        {
            targetCrosshairSize = baseSize;
        }

        private System.Collections.IEnumerator AnimateReload(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration && isReloading)
            {
                elapsed += Time.deltaTime;
                reloadProgress = elapsed / duration;
                yield return null;
            }

            reloadProgress = 1f;
        }
    }
}
