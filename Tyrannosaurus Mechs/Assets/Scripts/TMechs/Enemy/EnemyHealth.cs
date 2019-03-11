using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp01(value); 
                UpdateHealth();
            }
        }

        public float maxHealth;

        private float health = 1F;

        private void UpdateHealth()
        {
            if(health <= 0F)
                Destroy(gameObject);
        }

        public void Damage(float damage)
        {
            Health -= damage / maxHealth;
        }
    }
}
