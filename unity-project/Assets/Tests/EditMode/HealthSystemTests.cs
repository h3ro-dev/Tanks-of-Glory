using NUnit.Framework;
using UnityEngine;
using TankCommander;

namespace TankCommander.Tests
{
    /// <summary>
    /// Unit tests for the Health system
    /// </summary>
    public class HealthSystemTests
    {
        private GameObject testObject;
        private Health healthComponent;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestTank");
            healthComponent = testObject.AddComponent<Health>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void Health_InitializesWithMaxHealth()
        {
            // Default max health should be 100
            Assert.AreEqual(100f, healthComponent.CurrentHealth, 0.01f);
            Assert.AreEqual(100f, healthComponent.MaxHealth, 0.01f);
        }

        [Test]
        public void Health_TakeDamage_ReducesHealth()
        {
            float initialHealth = healthComponent.CurrentHealth;
            healthComponent.TakeDamage(25f);

            Assert.AreEqual(initialHealth - 25f, healthComponent.CurrentHealth, 0.01f);
        }

        [Test]
        public void Health_TakeDamage_DoesNotGoBelowZero()
        {
            healthComponent.TakeDamage(200f); // Overkill damage

            Assert.AreEqual(0f, healthComponent.CurrentHealth, 0.01f);
        }

        [Test]
        public void Health_Heal_IncreasesHealth()
        {
            healthComponent.TakeDamage(50f);
            float damagedHealth = healthComponent.CurrentHealth;

            healthComponent.Heal(25f);

            Assert.AreEqual(damagedHealth + 25f, healthComponent.CurrentHealth, 0.01f);
        }

        [Test]
        public void Health_Heal_DoesNotExceedMaxHealth()
        {
            healthComponent.Heal(50f); // Trying to heal above max

            Assert.AreEqual(healthComponent.MaxHealth, healthComponent.CurrentHealth, 0.01f);
        }

        [Test]
        public void Health_IsAlive_ReturnsTrueWhenHealthAboveZero()
        {
            Assert.IsTrue(healthComponent.IsAlive);

            healthComponent.TakeDamage(50f);
            Assert.IsTrue(healthComponent.IsAlive);
        }

        [Test]
        public void Health_IsAlive_ReturnsFalseWhenHealthIsZero()
        {
            healthComponent.TakeDamage(healthComponent.MaxHealth);

            Assert.IsFalse(healthComponent.IsAlive);
        }

        [Test]
        public void Health_GetHealthPercentage_ReturnsCorrectValue()
        {
            Assert.AreEqual(1.0f, healthComponent.GetHealthPercentage(), 0.01f);

            healthComponent.TakeDamage(50f);
            Assert.AreEqual(0.5f, healthComponent.GetHealthPercentage(), 0.01f);

            healthComponent.TakeDamage(50f);
            Assert.AreEqual(0.0f, healthComponent.GetHealthPercentage(), 0.01f);
        }
    }
}
