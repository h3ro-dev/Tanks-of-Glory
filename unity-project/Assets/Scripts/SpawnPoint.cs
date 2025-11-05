using UnityEngine;

namespace TankCommander
{
    /// <summary>
    /// Marks a position as a spawn point for players or AI
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private int teamIndex = 0; // 0 for FFA, 1+ for teams
        [SerializeField] private bool isPlayerSpawn = true;
        [SerializeField] private bool isAISpawn = true;

        public int TeamIndex => teamIndex;
        public bool IsPlayerSpawn => isPlayerSpawn;
        public bool IsAISpawn => isAISpawn;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            // Draw spawn point visualization
            Gizmos.color = teamIndex == 0 ? Color.white : (teamIndex == 1 ? Color.blue : Color.red);
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Draw direction arrow
            Gizmos.color = Color.green;
            Vector3 forward = transform.forward * 2f;
            Gizmos.DrawRay(transform.position, forward);

            // Draw cone for direction
            Vector3 right = transform.right * 0.5f;
            Gizmos.DrawRay(transform.position + forward, -forward.normalized * 0.5f + right);
            Gizmos.DrawRay(transform.position + forward, -forward.normalized * 0.5f - right);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw larger selection indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
