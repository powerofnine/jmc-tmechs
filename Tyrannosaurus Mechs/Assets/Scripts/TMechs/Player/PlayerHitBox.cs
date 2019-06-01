using TMechs.Entity;
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
            EntityHealth entity = other.GetComponent<EntityHealth>();

            if (entity)
                combat.OnHitboxTrigger(this, entity);
        }
    }
}