using TMechs.Entity;
using UnityEngine;

namespace TMechs.Enemy
{
    public class TankyloRock : MonoBehaviour
    {
        public int damage = 15;

        public GameObject effect;
        public EntityHealth.DamageSource damageSource;
        
        private void OnCollisionEnter(Collision other)
        {
            if(other.collider.CompareTag("Player"))
                Player.Player.Instance.Health.Damage(damage, damageSource.GetWithSource(transform));

            if (effect)
                Instantiate(effect, transform.position, transform.rotation);
            
            Destroy(gameObject);
        }
    }
}
