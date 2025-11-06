using UnityEngine;
using UnityEngine.UI;

namespace TankCommander.Polish
{
    /// <summary>
    /// Displays a hit marker when you successfully damage an enemy
    /// Provides instant feedback for successful hits
    /// </summary>
    public class HitMarker : MonoBehaviour
    {
        public static HitMarker Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Image hitMarkerImage;
        [SerializeField] private float displayDuration = 0.15f;
        [SerializeField] private float fadeSpeed = 10f;

        [Header("Colors")]
        [SerializeField] private Color normalHitColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color criticalHitColor = new Color(1f, 0f, 0f, 1f);
        [SerializeField] private Color killColor = new Color(1f, 0.8f, 0f, 1f);

        [Header("Scale Animation")]
        [SerializeField] private bool animateScale = true;
        [SerializeField] private float scaleMultiplier = 1.3f;
        [SerializeField] private float scaleSpeed = 15f;

        private float timer = 0f;
        private Vector3 originalScale;
        private float targetAlpha = 0f;
        private HitType currentHitType = HitType.Normal;

        private enum HitType
        {
            Normal,
            Critical,
            Kill
        }

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

            // Create hit marker if not assigned
            if (hitMarkerImage == null)
            {
                CreateHitMarker();
            }

            if (hitMarkerImage != null)
            {
                originalScale = hitMarkerImage.transform.localScale;
                SetAlpha(0f);
            }
        }

        private void CreateHitMarker()
        {
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("HitMarkerCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 200;

                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create hit marker image
            GameObject markerObj = new GameObject("HitMarker");
            markerObj.transform.SetParent(canvas.transform, false);

            RectTransform rect = markerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(40, 40);

            hitMarkerImage = markerObj.AddComponent<Image>();
            hitMarkerImage.color = normalHitColor;
            hitMarkerImage.raycastTarget = false;

            // Create simple X shape
            CreateHitMarkerCross(markerObj.transform);
        }

        private void CreateHitMarkerCross(Transform parent)
        {
            // Vertical line
            GameObject vertical = new GameObject("Vertical");
            vertical.transform.SetParent(parent, false);

            RectTransform vertRect = vertical.AddComponent<RectTransform>();
            vertRect.sizeDelta = new Vector2(4, 30);
            vertRect.anchoredPosition = Vector2.zero;

            Image vertImage = vertical.AddComponent<Image>();
            vertImage.color = Color.white;
            vertImage.raycastTarget = false;

            // Horizontal line
            GameObject horizontal = new GameObject("Horizontal");
            horizontal.transform.SetParent(parent, false);

            RectTransform horizRect = horizontal.AddComponent<RectTransform>();
            horizRect.sizeDelta = new Vector2(30, 4);
            horizRect.anchoredPosition = Vector2.zero;

            Image horizImage = horizontal.AddComponent<Image>();
            horizImage.color = Color.white;
            horizImage.raycastTarget = false;
        }

        private void Update()
        {
            if (hitMarkerImage == null) return;

            // Timer countdown
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                targetAlpha = 1f;

                // Animate scale
                if (animateScale)
                {
                    Vector3 targetScale = originalScale * scaleMultiplier;
                    hitMarkerImage.transform.localScale = Vector3.Lerp(
                        hitMarkerImage.transform.localScale,
                        targetScale,
                        Time.deltaTime * scaleSpeed
                    );
                }
            }
            else
            {
                targetAlpha = 0f;

                // Return to original scale
                if (animateScale)
                {
                    hitMarkerImage.transform.localScale = Vector3.Lerp(
                        hitMarkerImage.transform.localScale,
                        originalScale,
                        Time.deltaTime * scaleSpeed
                    );
                }
            }

            // Fade in/out
            Color currentColor = hitMarkerImage.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * fadeSpeed);
            hitMarkerImage.color = currentColor;
        }

        public void ShowHitMarker(bool isCritical = false, bool isKill = false)
        {
            if (hitMarkerImage == null) return;

            timer = displayDuration;

            // Set color based on hit type
            Color newColor;
            if (isKill)
            {
                newColor = killColor;
                currentHitType = HitType.Kill;
            }
            else if (isCritical)
            {
                newColor = criticalHitColor;
                currentHitType = HitType.Critical;
            }
            else
            {
                newColor = normalHitColor;
                currentHitType = HitType.Normal;
            }

            hitMarkerImage.color = newColor;

            // Reset scale
            if (animateScale)
            {
                hitMarkerImage.transform.localScale = originalScale;
            }

            // Camera shake for feedback
            if (CameraShakeManager.Instance != null)
            {
                if (isKill)
                    CameraShakeManager.Instance.Shake(0.2f, 0.15f);
                else if (isCritical)
                    CameraShakeManager.Instance.Shake(0.1f, 0.1f);
            }
        }

        public void ShowNormalHit()
        {
            ShowHitMarker(false, false);
        }

        public void ShowCriticalHit()
        {
            ShowHitMarker(true, false);
        }

        public void ShowKillHit()
        {
            ShowHitMarker(false, true);
        }

        private void SetAlpha(float alpha)
        {
            if (hitMarkerImage != null)
            {
                Color color = hitMarkerImage.color;
                color.a = alpha;
                hitMarkerImage.color = color;
            }
        }
    }
}
