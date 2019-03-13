using TMechs.Enemy;
using UnityEngine;

namespace TMechs.Player
{
    public class PlayerHitBox : MonoBehaviour
    {
        public string id;
        
        private PlayerCombat combat;

        private void Awake()
        {
            combat = GetComponentInParent<PlayerCombat>();

            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            
            if(enemy)
                combat.OnHitboxTrigger(this, enemy);
        }
    }
}
