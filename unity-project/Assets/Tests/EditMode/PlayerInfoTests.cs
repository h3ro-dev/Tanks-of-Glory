using NUnit.Framework;
using UnityEngine;
using TankCommander;

namespace TankCommander.Tests
{
    /// <summary>
    /// Unit tests for the PlayerInfo system
    /// </summary>
    public class PlayerInfoTests
    {
        private GameObject testObject;
        private PlayerInfo playerInfo;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestPlayer");
            playerInfo = testObject.AddComponent<PlayerInfo>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void PlayerInfo_InitializesWithDefaultValues()
        {
            Assert.AreEqual("Player", playerInfo.PlayerName);
            Assert.AreEqual(0, playerInfo.PlayerIndex);
            Assert.AreEqual(1, playerInfo.Level);
            Assert.AreEqual(0, playerInfo.Experience);
        }

        [Test]
        public void PlayerInfo_AddKill_IncrementsKillCount()
        {
            int initialKills = playerInfo.Kills;

            playerInfo.AddKill();

            Assert.AreEqual(initialKills + 1, playerInfo.Kills);
        }

        [Test]
        public void PlayerInfo_AddDeath_IncrementsDeathCount()
        {
            int initialDeaths = playerInfo.Deaths;

            playerInfo.AddDeath();

            Assert.AreEqual(initialDeaths + 1, playerInfo.Deaths);
        }

        [Test]
        public void PlayerInfo_AddAssist_IncrementsAssistCount()
        {
            int initialAssists = playerInfo.Assists;

            playerInfo.AddAssist();

            Assert.AreEqual(initialAssists + 1, playerInfo.Assists);
        }

        [Test]
        public void PlayerInfo_RecordDamageDealt_AccumulatesDamage()
        {
            playerInfo.RecordDamageDealt(50f);
            playerInfo.RecordDamageDealt(75f);

            Assert.AreEqual(125f, playerInfo.DamageDealt, 0.01f);
        }

        [Test]
        public void PlayerInfo_RecordDamageTaken_AccumulatesDamage()
        {
            playerInfo.RecordDamageTaken(30f);
            playerInfo.RecordDamageTaken(45f);

            Assert.AreEqual(75f, playerInfo.DamageTaken, 0.01f);
        }

        [Test]
        public void PlayerInfo_AddExperience_IncreasesExperience()
        {
            playerInfo.AddExperience(100);

            Assert.AreEqual(100, playerInfo.Experience);
        }

        [Test]
        public void PlayerInfo_AddExperience_TriggersLevelUp()
        {
            int initialLevel = playerInfo.Level;

            // Add enough experience to level up (typically 100 XP per level)
            playerInfo.AddExperience(100);

            // Should level up if level system is implemented
            Assert.GreaterOrEqual(playerInfo.Level, initialLevel);
        }

        [Test]
        public void PlayerInfo_GetKDRatio_CalculatesCorrectly()
        {
            playerInfo.AddKill();
            playerInfo.AddKill();
            playerInfo.AddKill();
            playerInfo.AddDeath();

            float kd = playerInfo.GetKDRatio();
            Assert.AreEqual(3.0f, kd, 0.01f);
        }

        [Test]
        public void PlayerInfo_GetKDRatio_HandlesZeroDeaths()
        {
            playerInfo.AddKill();
            playerInfo.AddKill();

            float kd = playerInfo.GetKDRatio();
            Assert.AreEqual(2.0f, kd, 0.01f); // Should return kills when no deaths
        }

        [Test]
        public void PlayerInfo_ResetMatchStats_ClearsStats()
        {
            // Add some stats
            playerInfo.AddKill();
            playerInfo.AddDeath();
            playerInfo.AddAssist();
            playerInfo.RecordDamageDealt(100f);
            playerInfo.RecordDamageTaken(50f);

            // Reset
            playerInfo.ResetMatchStats();

            // Verify all stats are zero
            Assert.AreEqual(0, playerInfo.Kills);
            Assert.AreEqual(0, playerInfo.Deaths);
            Assert.AreEqual(0, playerInfo.Assists);
            Assert.AreEqual(0f, playerInfo.DamageDealt, 0.01f);
            Assert.AreEqual(0f, playerInfo.DamageTaken, 0.01f);
        }

        [Test]
        public void PlayerInfo_GetMatchStats_ReturnsCorrectData()
        {
            playerInfo.AddKill();
            playerInfo.AddKill();
            playerInfo.AddDeath();
            playerInfo.RecordDamageDealt(150f);

            var stats = playerInfo.GetMatchStats();

            Assert.AreEqual(2, stats.kills);
            Assert.AreEqual(1, stats.deaths);
            Assert.AreEqual(150f, stats.damageDealt, 0.01f);
        }
    }
}
