using UnityEngine;

namespace TankCommander
{
    /// <summary>
    /// Creates a procedural muzzle flash effect
    /// </summary>
    public class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private float lightIntensity = 3f;
        [SerializeField] private float lightRange = 10f;
        [SerializeField] private Color flashColor = new Color(1f, 0.8f, 0.3f);

        private Light flashLight;
        private float timer = 0f;
        private bool isFlashing = false;

        private void Start()
        {
            // Create light component
            flashLight = gameObject.AddComponent<Light>();
            flashLight.type = LightType.Point;
            flashLight.color = flashColor;
            flashLight.range = lightRange;
            flashLight.intensity = 0f;
        }

        private void Update()
        {
            if (isFlashing)
            {
                timer += Time.deltaTime;

                if (timer >= flashDuration)
                {
                    flashLight.intensity = 0f;
                    isFlashing = false;
                }
                else
                {
                    // Fade out
                    float progress = timer / flashDuration;
                    flashLight.intensity = Mathf.Lerp(lightIntensity, 0f, progress);
                }
            }
        }

        public void Flash()
        {
            timer = 0f;
            isFlashing = true;
            flashLight.intensity = lightIntensity;

            // Create a temporary flash sprite
            CreateFlashSprite();
        }

        private void CreateFlashSprite()
        {
            GameObject flashObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flashObj.transform.SetParent(transform);
            flashObj.transform.localPosition = Vector3.forward * 0.5f;
            flashObj.transform.localRotation = Quaternion.identity;
            flashObj.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);

            // Random rotation
            flashObj.transform.Rotate(0, 0, Random.Range(0f, 360f));

            Renderer renderer = flashObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", flashColor * 5f);
            mat.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.8f);
            renderer.material = mat;

            Destroy(flashObj.GetComponent<Collider>());
            Destroy(flashObj, flashDuration);
        }
    }
}
