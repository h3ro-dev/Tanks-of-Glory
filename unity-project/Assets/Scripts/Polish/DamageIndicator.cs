using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TankCommander.Polish
{
    /// <summary>
    /// Visual damage indicators showing direction of incoming damage
    /// Improves player awareness and feedback
    /// </summary>
    public class DamageIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float indicatorDistance = 150f;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private int maxIndicators = 5;

        [Header("Colors")]
        [SerializeField] private Color lowDamageColor = new Color(1f, 1f, 0f, 0.7f);
        [SerializeField] private Color mediumDamageColor = new Color(1f, 0.5f, 0f, 0.8f);
        [SerializeField] private Color highDamageColor = new Color(1f, 0f, 0f, 1f);

        private Transform playerTransform;
        private List<IndicatorInstance> activeIndicators = new List<IndicatorInstance>();

        private class IndicatorInstance
        {
            public GameObject gameObject;
            public Image image;
            public RectTransform rectTransform;
            public float timeRemaining;
            public Vector3 damageDirection;
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Create canvas if not assigned
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DamageIndicatorCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create default indicator prefab if not assigned
            if (indicatorPrefab == null)
            {
                CreateDefaultIndicatorPrefab();
            }
        }

        private void CreateDefaultIndicatorPrefab()
        {
            indicatorPrefab = new GameObject("DamageIndicatorPrefab");

            RectTransform rect = indicatorPrefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 40);

            Image image = indicatorPrefab.AddComponent<Image>();
            image.color = highDamageColor;

            // Create simple arrow using UI
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(indicatorPrefab.transform);

            RectTransform arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(30, 30);
            arrowRect.anchoredPosition = Vector2.zero;

            Image arrowImage = arrow.AddComponent<Image>();
            arrowImage.color = Color.white;

            indicatorPrefab.SetActive(false);
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Update all active indicators
            for (int i = activeIndicators.Count - 1; i >= 0; i--)
            {
                IndicatorInstance indicator = activeIndicators[i];

                indicator.timeRemaining -= Time.deltaTime;

                // Fade out
                float alpha = indicator.timeRemaining / fadeDuration;
                Color color = indicator.image.color;
                color.a = alpha;
                indicator.image.color = color;

                // Position indicator
                UpdateIndicatorPosition(indicator);

                // Remove if expired
                if (indicator.timeRemaining <= 0)
                {
                    Destroy(indicator.gameObject);
                    activeIndicators.RemoveAt(i);
                }
            }
        }

        private void UpdateIndicatorPosition(IndicatorInstance indicator)
        {
            // Get direction from player to damage source
            Vector3 direction = indicator.damageDirection;

            // Convert to screen space
            Vector3 forward = playerTransform.forward;
            float angle = Mathf.Atan2(direction.x, direction.z) - Mathf.Atan2(forward.x, forward.z);

            // Position around screen edge
            Vector2 position = new Vector2(
                Mathf.Sin(angle) * indicatorDistance,
                Mathf.Cos(angle) * indicatorDistance
            );

            indicator.rectTransform.anchoredPosition = position;

            // Rotate to point inward
            indicator.rectTransform.localRotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);
        }

        public void ShowDamageIndicator(Vector3 damageSourcePosition, float damageAmount)
        {
            if (playerTransform == null) return;

            // Don't create more than max indicators
            if (activeIndicators.Count >= maxIndicators)
            {
                // Remove oldest
                if (activeIndicators.Count > 0)
                {
                    Destroy(activeIndicators[0].gameObject);
                    activeIndicators.RemoveAt(0);
                }
            }

            // Calculate direction
            Vector3 direction = (damageSourcePosition - playerTransform.position).normalized;

            // Create indicator
            GameObject indicatorObj = Instantiate(indicatorPrefab, canvas.transform);
            indicatorObj.SetActive(true);

            IndicatorInstance indicator = new IndicatorInstance
            {
                gameObject = indicatorObj,
                image = indicatorObj.GetComponent<Image>(),
                rectTransform = indicatorObj.GetComponent<RectTransform>(),
                timeRemaining = fadeDuration,
                damageDirection = direction
            };

            // Set color based on damage
            Color color = GetDamageColor(damageAmount);
            indicator.image.color = color;

            activeIndicators.Add(indicator);
        }

        private Color GetDamageColor(float damage)
        {
            if (damage < 20f)
                return lowDamageColor;
            else if (damage < 50f)
                return mediumDamageColor;
            else
                return highDamageColor;
        }

        public void ClearAllIndicators()
        {
            foreach (var indicator in activeIndicators)
            {
                if (indicator.gameObject != null)
                {
                    Destroy(indicator.gameObject);
                }
            }
            activeIndicators.Clear();
        }
    }
}
