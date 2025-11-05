using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TankCommander
{
    /// <summary>
    /// Automatically sets up the game scene with all required components and UI
    /// Run this in the editor to instantly create a playable game
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [Header("Game Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private int numberOfEnemies = 3;
        [SerializeField] private float arenaSize = 50f;

        [Header("Prefab References")]
        [SerializeField] private GameObject tankPrefab;
        [SerializeField] private GameObject projectilePrefab;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupCompleteGame();
            }
        }

        [ContextMenu("Setup Complete Game")]
        public void SetupCompleteGame()
        {
            Debug.Log("Setting up complete game...");

            // 1. Setup managers
            SetupManagers();

            // 2. Setup UI
            SetupUI();

            // 3. Setup arena
            SetupArena();

            // 4. Setup spawn points
            SetupSpawnPoints();

            // 5. Setup player
            SetupPlayer();

            // 6. Setup enemies
            SetupEnemies();

            // 7. Setup camera
            SetupCamera();

            Debug.Log("Game setup complete! Press Play to start.");
        }

        private void SetupManagers()
        {
            // Create GameManager object if it doesn't exist
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj == null)
            {
                gmObj = new GameObject("GameManager");
            }

            // Add managers
            if (gmObj.GetComponent<GameManager>() == null)
                gmObj.AddComponent<GameManager>();

            if (gmObj.GetComponent<UIManager>() == null)
                gmObj.AddComponent<UIManager>();

            if (gmObj.GetComponent<AudioManager>() == null)
                gmObj.AddComponent<AudioManager>();

            if (gmObj.GetComponent<ObjectPooler>() == null)
            {
                ObjectPooler pooler = gmObj.AddComponent<ObjectPooler>();

                // Setup projectile pool if prefab exists
                if (projectilePrefab != null)
                {
                    // This would be configured via serialized fields in actual Unity editor
                    Debug.Log("Object pooler added - configure pools in inspector");
                }
            }

            Debug.Log("✓ Managers setup complete");
        }

        private void SetupUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("✓ Canvas created");
            }

            // Create HUD Container
            GameObject hudContainer = new GameObject("HUD");
            hudContainer.transform.SetParent(canvas.transform, false);
            RectTransform hudRect = hudContainer.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.sizeDelta = Vector2.zero;

            // Health Bar
            CreateHealthBar(hudContainer);

            // Ammo Counter
            CreateAmmoCounter(hudContainer);

            // Crosshair
            CreateCrosshair(hudContainer);

            // Score Text
            CreateScoreText(hudContainer);

            // Time Text
            CreateTimeText(hudContainer);

            // Create EventSystem if missing
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("✓ UI setup complete");
        }

        private void CreateHealthBar(GameObject parent)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(parent.transform, false);

            RectTransform rt = healthBarObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.02f);
            rt.anchorMax = new Vector2(0.02f, 0.02f);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(300, 30);

            Slider slider = healthBarObj.AddComponent<Slider>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(healthBarObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(healthBarObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0f, 1f, 0f, 1f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            slider.fillRect = fillRect;
            slider.value = 1f;
        }

        private void CreateAmmoCounter(GameObject parent)
        {
            GameObject ammoObj = new GameObject("AmmoText");
            ammoObj.transform.SetParent(parent.transform, false);

            RectTransform rt = ammoObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.98f, 0.02f);
            rt.anchorMax = new Vector2(0.98f, 0.02f);
            rt.pivot = new Vector2(1, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI text = ammoObj.AddComponent<TextMeshProUGUI>();
            text.text = "999/999";
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Right;
            text.fontStyle = FontStyles.Bold;
        }

        private void CreateCrosshair(GameObject parent)
        {
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(parent.transform, false);

            RectTransform rt = crosshairObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(32, 32);

            Image image = crosshairObj.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.7f);

            // Create simple crosshair using lines
            CreateCrosshairLine(crosshairObj, new Vector2(0, 16), new Vector2(2, 8));
            CreateCrosshairLine(crosshairObj, new Vector2(0, -16), new Vector2(2, 8));
            CreateCrosshairLine(crosshairObj, new Vector2(16, 0), new Vector2(8, 2));
            CreateCrosshairLine(crosshairObj, new Vector2(-16, 0), new Vector2(8, 2));
        }

        private void CreateCrosshairLine(GameObject parent, Vector2 position, Vector2 size)
        {
            GameObject line = new GameObject("Line");
            line.transform.SetParent(parent.transform, false);

            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image image = line.AddComponent<Image>();
            image.color = Color.white;
        }

        private void CreateScoreText(GameObject parent)
        {
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(parent.transform, false);

            RectTransform rt = scoreObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.98f);
            rt.anchorMax = new Vector2(0.5f, 0.98f);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI text = scoreObj.AddComponent<TextMeshProUGUI>();
            text.text = "Score: 0";
            text.fontSize = 32;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
        }

        private void CreateTimeText(GameObject parent)
        {
            GameObject timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(parent.transform, false);

            RectTransform rt = timeObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.98f);
            rt.anchorMax = new Vector2(0.02f, 0.98f);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI text = timeObj.AddComponent<TextMeshProUGUI>();
            text.text = "05:00";
            text.fontSize = 32;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.fontStyle = FontStyles.Bold;
        }

        private void SetupArena()
        {
            // Create ground if it doesn't exist
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.localScale = new Vector3(arenaSize / 10f, 1, arenaSize / 10f);
                ground.transform.position = Vector3.zero;

                // Add material
                Renderer renderer = ground.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.3f, 0.4f, 0.3f);
                renderer.material = mat;
            }

            // Create boundary walls
            CreateWalls();

            Debug.Log("✓ Arena setup complete");
        }

        private void CreateWalls()
        {
            float wallHeight = 3f;
            float wallThickness = 1f;
            float halfSize = arenaSize / 2f;

            // North wall
            CreateWall("Wall_North", new Vector3(0, wallHeight / 2f, halfSize), new Vector3(arenaSize, wallHeight, wallThickness));

            // South wall
            CreateWall("Wall_South", new Vector3(0, wallHeight / 2f, -halfSize), new Vector3(arenaSize, wallHeight, wallThickness));

            // East wall
            CreateWall("Wall_East", new Vector3(halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, arenaSize));

            // West wall
            CreateWall("Wall_West", new Vector3(-halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, arenaSize));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.Find(name);
            if (wall == null)
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = name;
                wall.transform.position = position;
                wall.transform.localScale = scale;

                Renderer renderer = wall.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.5f, 0.5f, 0.5f);
                renderer.material = mat;
            }
        }

        private void SetupSpawnPoints()
        {
            // Create spawn points in a circle
            int totalSpawnPoints = numberOfEnemies + 1; // +1 for player
            float radius = arenaSize * 0.3f;

            for (int i = 0; i < totalSpawnPoints; i++)
            {
                float angle = (360f / totalSpawnPoints) * i;
                Vector3 position = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    1f,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius
                );

                GameObject spawnPoint = new GameObject($"SpawnPoint_{i:00}");
                spawnPoint.transform.position = position;
                spawnPoint.transform.rotation = Quaternion.LookRotation(-position.normalized);

                SpawnPoint sp = spawnPoint.AddComponent<SpawnPoint>();
            }

            Debug.Log($"✓ Created {totalSpawnPoints} spawn points");
        }

        private void SetupPlayer()
        {
            // Find first spawn point
            SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points found!");
                return;
            }

            Vector3 spawnPos = spawnPoints[0].Position;
            Quaternion spawnRot = spawnPoints[0].Rotation;

            GameObject player = CreateTank("Player", spawnPos, spawnRot, false);
            player.tag = "Player";

            Debug.Log("✓ Player created");
        }

        private void SetupEnemies()
        {
            SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

            for (int i = 1; i <= numberOfEnemies && i < spawnPoints.Length; i++)
            {
                Vector3 spawnPos = spawnPoints[i].Position;
                Quaternion spawnRot = spawnPoints[i].Rotation;

                GameObject enemy = CreateTank($"Enemy_{i:00}", spawnPos, spawnRot, true);
                enemy.tag = "Enemy";
            }

            Debug.Log($"✓ Created {numberOfEnemies} enemies");
        }

        private GameObject CreateTank(string name, Vector3 position, Quaternion rotation, bool isAI)
        {
            GameObject tank = new GameObject(name);
            tank.transform.position = position;
            tank.transform.rotation = rotation;

            // Add Rigidbody
            Rigidbody rb = tank.AddComponent<Rigidbody>();
            rb.mass = 1000f;
            rb.drag = 0.5f;
            rb.angularDrag = 1f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Add collider
            BoxCollider collider = tank.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 1f, 3f);
            collider.center = new Vector3(0, 0.5f, 0);

            // Create body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(tank.transform);
            body.transform.localPosition = new Vector3(0, 0.5f, 0);
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(2, 1, 3);
            Destroy(body.GetComponent<Collider>());

            // Color based on team
            Material bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = isAI ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.2f, 0.5f, 0.8f);
            body.GetComponent<Renderer>().material = bodyMat;

            // Create turret
            GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turret.name = "Turret";
            turret.transform.SetParent(tank.transform);
            turret.transform.localPosition = new Vector3(0, 1.2f, 0);
            turret.transform.localRotation = Quaternion.identity;
            turret.transform.localScale = new Vector3(1.5f, 0.6f, 1.5f);
            Destroy(turret.GetComponent<Collider>());

            Material turretMat = new Material(Shader.Find("Standard"));
            turretMat.color = isAI ? new Color(0.6f, 0.1f, 0.1f) : new Color(0.1f, 0.3f, 0.6f);
            turret.GetComponent<Renderer>().material = turretMat;

            // Create barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(turret.transform);
            barrel.transform.localPosition = new Vector3(0, 0, 1.2f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            barrel.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
            Destroy(barrel.GetComponent<Collider>());
            barrel.GetComponent<Renderer>().material = turretMat;

            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(turret.transform);
            firePoint.transform.localPosition = new Vector3(0, 0, 2f);
            firePoint.transform.localRotation = Quaternion.identity;

            // Add muzzle flash
            firePoint.AddComponent<MuzzleFlash>();

            // Add components
            tank.AddComponent<TankController>();
            tank.AddComponent<WeaponSystem>();
            tank.AddComponent<Health>();
            tank.AddComponent<PlayerInfo>();

            if (!isAI)
            {
                tank.AddComponent<InputManager>();
            }
            else
            {
                TankAI ai = tank.AddComponent<TankAI>();
                // Configure AI behavior - would be set in inspector
            }

            return tank;
        }

        private void SetupCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            // Add camera controller
            CameraController camController = mainCam.GetComponent<CameraController>();
            if (camController == null)
            {
                camController = mainCam.AddComponent<CameraController>();
            }

            // Find player and assign as target
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                // Would set target via serialized field in actual Unity
                Debug.Log("✓ Camera setup - assign Player as target in inspector");
            }

            Debug.Log("✓ Camera setup complete");
        }
    }
}
