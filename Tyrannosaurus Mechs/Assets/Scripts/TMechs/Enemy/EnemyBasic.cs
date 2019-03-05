using TMechs.Player;
using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyBasic : MonoBehaviour
    {
        public float trackRange = 25F;
        public float stopRange = 5F;

        public float moveSpeed = 5F;
        
        private Transform player;
        
        private void Awake()
        {
            player = FindObjectOfType<PlayerMovement>().transform;
        }

        private void Update()
        {
            float dist = Vector3.Distance(player.position, transform.position);
            
            if (dist < trackRange)
            {
                transform.LookAt(player.position.Set(transform.position.y, Utility.Axis.Y));

                if (dist > stopRange)
                    transform.Translate(transform.forward * moveSpeed * Time.deltaTime);
            }
        }
    }
}
