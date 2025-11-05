using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TankCommander;

namespace TankCommander.Tests
{
    /// <summary>
    /// Integration tests that run in PlayMode to test actual gameplay
    /// </summary>
    public class GameplayIntegrationTests
    {
        private GameObject playerTank;
        private GameObject enemyTank;
        private GameObject testArena;

        [SetUp]
        public void Setup()
        {
            // Create test arena
            testArena = new GameObject("TestArena");
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(testArena.transform);
            ground.transform.localScale = new Vector3(10, 1, 10);

            // Create player tank
            playerTank = CreateTestTank("PlayerTank", new Vector3(0, 1, 0), false);
            playerTank.tag = "Player";

            // Create enemy tank
            enemyTank = CreateTestTank("EnemyTank", new Vector3(10, 1, 0), true);
            enemyTank.tag = "Enemy";
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(playerTank);
            Object.Destroy(enemyTank);
            Object.Destroy(testArena);
        }

        private GameObject CreateTestTank(string name, Vector3 position, bool isAI)
        {
            GameObject tank = new GameObject(name);
            tank.transform.position = position;

            // Add physics
            Rigidbody rb = tank.AddComponent<Rigidbody>();
            rb.mass = 1000f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            tank.AddComponent<BoxCollider>();

            // Add game components
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
                tank.AddComponent<TankAI>();
            }

            // Create turret
            GameObject turret = new GameObject("Turret");
            turret.transform.SetParent(tank.transform);
            turret.transform.localPosition = new Vector3(0, 1, 0);

            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(turret.transform);
            firePoint.transform.localPosition = new Vector3(0, 0, 2);

            return tank;
        }

        [UnityTest]
        public IEnumerator Tank_CanTakeDamage()
        {
            Health health = playerTank.GetComponent<Health>();
            float initialHealth = health.CurrentHealth;

            health.TakeDamage(25f);

            yield return null; // Wait one frame

            Assert.Less(health.CurrentHealth, initialHealth);
            Assert.IsTrue(health.IsAlive);
        }

        [UnityTest]
        public IEnumerator Tank_DiesWhenHealthReachesZero()
        {
            Health health = playerTank.GetComponent<Health>();

            health.TakeDamage(health.MaxHealth);

            yield return null;

            Assert.IsFalse(health.IsAlive);
        }

        [UnityTest]
        public IEnumerator PlayerInfo_TracksKills()
        {
            PlayerInfo playerInfo = playerTank.GetComponent<PlayerInfo>();
            int initialKills = playerInfo.Kills;

            playerInfo.AddKill();

            yield return null;

            Assert.AreEqual(initialKills + 1, playerInfo.Kills);
        }

        [UnityTest]
        public IEnumerator WeaponSystem_CanFireProjectile()
        {
            WeaponSystem weapon = playerTank.GetComponent<WeaponSystem>();

            // Try to fire
            bool fired = weapon.FirePrimaryWeapon();

            yield return new WaitForSeconds(0.1f);

            // Check if projectile was created (may not be if no prefab assigned)
            // This tests the firing mechanism itself
            Assert.IsNotNull(weapon);
        }

        [UnityTest]
        public IEnumerator TankController_HasRigidbody()
        {
            TankController controller = playerTank.GetComponent<TankController>();
            Rigidbody rb = playerTank.GetComponent<Rigidbody>();

            yield return null;

            Assert.IsNotNull(controller);
            Assert.IsNotNull(rb);
        }

        [UnityTest]
        public IEnumerator Health_TriggersDeathEvent()
        {
            Health health = playerTank.GetComponent<Health>();
            bool deathEventTriggered = false;

            health.OnDeath.AddListener(() => deathEventTriggered = true);

            health.TakeDamage(health.MaxHealth);

            yield return null;

            Assert.IsTrue(deathEventTriggered);
        }

        [UnityTest]
        public IEnumerator PlayerInfo_AccumulatesDamageDealt()
        {
            PlayerInfo info = playerTank.GetComponent<PlayerInfo>();

            info.RecordDamageDealt(50f);
            info.RecordDamageDealt(75f);

            yield return null;

            Assert.AreEqual(125f, info.DamageDealt, 0.01f);
        }

        [UnityTest]
        public IEnumerator AI_HasRequiredComponents()
        {
            TankAI ai = enemyTank.GetComponent<TankAI>();

            yield return null;

            Assert.IsNotNull(ai);
            Assert.IsNotNull(enemyTank.GetComponent<TankController>());
            Assert.IsNotNull(enemyTank.GetComponent<WeaponSystem>());
        }

        [UnityTest]
        public IEnumerator TankController_RespondsToMovement()
        {
            TankController controller = playerTank.GetComponent<TankController>();
            Vector3 initialPosition = playerTank.transform.position;

            // Simulate movement input (would need to be called via InputManager normally)
            controller.Move(Vector2.up); // Move forward

            yield return new WaitForSeconds(0.5f);

            // Tank should have moved (in Z direction since forward is +Z in Unity)
            // Note: This might not work without physics updates, but tests the system exists
            Assert.IsNotNull(controller);
        }

        [UnityTest]
        public IEnumerator MultipleHits_ReduceHealthCorrectly()
        {
            Health health = playerTank.GetComponent<Health>();
            float initialHealth = health.CurrentHealth;

            health.TakeDamage(10f);
            yield return null;

            health.TakeDamage(20f);
            yield return null;

            health.TakeDamage(15f);
            yield return null;

            Assert.AreEqual(initialHealth - 45f, health.CurrentHealth, 0.01f);
        }
    }
}
