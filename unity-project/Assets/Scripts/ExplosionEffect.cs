using UnityEngine;

namespace TankCommander
{
    /// <summary>
    /// Creates a procedural explosion effect without requiring particle system assets
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private float explosionSize = 5f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private Color explosionColor = new Color(1f, 0.5f, 0f, 1f);
        [SerializeField] private int debrisCount = 20;

        [Header("Physics Settings")]
        [SerializeField] private float debrisSpeed = 10f;
        [SerializeField] private float upwardForce = 5f;

        private float elapsedTime = 0f;
        private Light explosionLight;
        private GameObject[] debrisObjects;

        private void Start()
        {
            CreateExplosion();
        }

        private void CreateExplosion()
        {
            // Create explosion light
            GameObject lightObj = new GameObject("ExplosionLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;

            explosionLight = lightObj.AddComponent<Light>();
            explosionLight.type = LightType.Point;
            explosionLight.color = explosionColor;
            explosionLight.range = explosionSize * 2f;
            explosionLight.intensity = 8f;

            // Create debris
            debrisObjects = new GameObject[debrisCount];
            for (int i = 0; i < debrisCount; i++)
            {
                GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debris.transform.SetParent(transform);
                debris.transform.localPosition = Vector3.zero;
                debris.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);

                // Set color
                Renderer renderer = debris.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.Lerp(Color.black, explosionColor, Random.Range(0.3f, 0.7f));
                mat.SetFloat("_Metallic", 0.5f);
                renderer.material = mat;

                // Add physics
                Rigidbody rb = debris.AddComponent<Rigidbody>();
                rb.mass = 0.1f;

                // Random direction with upward bias
                Vector3 randomDir = Random.onUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y) + upwardForce;
                rb.velocity = randomDir.normalized * debrisSpeed * Random.Range(0.5f, 1.5f);
                rb.angularVelocity = Random.insideUnitSphere * 10f;

                debrisObjects[i] = debris;
            }

            // Create core sphere
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = Vector3.one * explosionSize;

            Renderer coreRenderer = core.GetComponent<Renderer>();
            Material coreMat = new Material(Shader.Find("Standard"));
            coreMat.SetFloat("_Mode", 3); // Transparent mode
            coreMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            coreMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            coreMat.SetInt("_ZWrite", 0);
            coreMat.DisableKeyword("_ALPHATEST_ON");
            coreMat.EnableKeyword("_ALPHABLEND_ON");
            coreMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            coreMat.renderQueue = 3000;
            coreMat.color = new Color(explosionColor.r, explosionColor.g, explosionColor.b, 0.5f);
            coreMat.SetFloat("_Metallic", 0f);
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_EmissionColor", explosionColor * 2f);
            coreRenderer.material = coreMat;

            Destroy(core.GetComponent<Collider>());

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition(null, transform.position, 1f, Random.Range(0.8f, 1.2f));
            }
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Fade out light
            if (explosionLight != null)
            {
                explosionLight.intensity = Mathf.Lerp(8f, 0f, progress);
            }

            // Fade out debris
            foreach (GameObject debris in debrisObjects)
            {
                if (debris != null)
                {
                    Renderer renderer = debris.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = 1f - progress;
                        renderer.material.color = color;
                    }
                }
            }
        }

        /// <summary>
        /// Create an explosion at a specific position
        /// </summary>
        public static void CreateExplosion(Vector3 position, float size = 5f)
        {
            GameObject explosionObj = new GameObject("Explosion");
            explosionObj.transform.position = position;

            ExplosionEffect effect = explosionObj.AddComponent<ExplosionEffect>();
            effect.explosionSize = size;
        }
    }
}
