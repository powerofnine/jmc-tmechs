using TMechs.Player;
using UnityEngine;
using UnityEngine.AI;

namespace TMechs.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBasicAi : MonoBehaviour
    {
        public float trackRadius = 25F;

        private Transform player;
        private NavMeshAgent agent;
        
        private void Awake()
        {
            player = FindObjectOfType<PlayerMovement>().transform;

            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            float dist = Vector3.Distance(player.position, transform.position);

            if (dist < trackRadius)
                agent.SetDestination(player.position);
            else
                agent.SetDestination(transform.position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, trackRadius);
        }
    }
}
