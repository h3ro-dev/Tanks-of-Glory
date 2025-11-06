using UnityEngine;

namespace TankCommander.Polish
{
    /// <summary>
    /// Adds screen shake and camera effects for impacts and explosions
    /// Makes the game feel more responsive and impactful
    /// </summary>
    public class CameraShakeManager : MonoBehaviour
    {
        public static CameraShakeManager Instance { get; private set; }

        [Header("Shake Settings")]
        [SerializeField] private float shakeDecay = 5f;
        [SerializeField] private float maxShakeMagnitude = 2f;

        [Header("Presets")]
        [SerializeField] private ShakePreset damageShake = new ShakePreset { magnitude = 0.3f, duration = 0.2f };
        [SerializeField] private ShakePreset explosionShake = new ShakePreset { magnitude = 0.8f, duration = 0.5f };
        [SerializeField] private ShakePreset fireShake = new ShakePreset { magnitude = 0.15f, duration = 0.1f };

        [System.Serializable]
        public class ShakePreset
        {
            public float magnitude = 0.5f;
            public float duration = 0.3f;
            public float roughness = 10f;
        }

        private Camera mainCamera;
        private Vector3 originalPosition;
        private float shakeTimer = 0f;
        private float shakeMagnitude = 0f;
        private float shakeRoughness = 10f;

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

            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;

                // Calculate shake
                float dampingFactor = shakeTimer / (shakeMagnitude > 0 ? shakeMagnitude : 1f);
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * shakeMagnitude * dampingFactor;

                // Apply shake
                if (mainCamera != null)
                {
                    mainCamera.transform.localPosition = shakeOffset;
                }

                // Decay magnitude
                shakeMagnitude = Mathf.MoveTowards(shakeMagnitude, 0f, shakeDecay * Time.deltaTime);
            }
            else
            {
                // Return to original position
                if (mainCamera != null)
                {
                    mainCamera.transform.localPosition = Vector3.Lerp(
                        mainCamera.transform.localPosition,
                        Vector3.zero,
                        Time.deltaTime * 10f
                    );
                }
            }
        }

        public void Shake(float magnitude, float duration, float roughness = 10f)
        {
            shakeMagnitude = Mathf.Min(magnitude, maxShakeMagnitude);
            shakeTimer = duration;
            shakeRoughness = roughness;
        }

        public void ShakeDamage()
        {
            Shake(damageShake.magnitude, damageShake.duration, damageShake.roughness);
        }

        public void ShakeExplosion()
        {
            Shake(explosionShake.magnitude, explosionShake.duration, explosionShake.roughness);
        }

        public void ShakeFire()
        {
            Shake(fireShake.magnitude, fireShake.duration, fireShake.roughness);
        }

        public void ShakeExplosionAtDistance(Vector3 explosionPosition, float maxDistance = 50f)
        {
            if (mainCamera == null) return;

            float distance = Vector3.Distance(mainCamera.transform.position, explosionPosition);
            float intensity = 1f - Mathf.Clamp01(distance / maxDistance);

            if (intensity > 0.1f)
            {
                Shake(explosionShake.magnitude * intensity, explosionShake.duration, explosionShake.roughness);
            }
        }
    }
}
