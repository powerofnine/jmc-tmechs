using UnityEngine;

namespace TMechs.Entity
{
    public class EntityHealth : MonoBehaviour
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
        private bool isDead = false;

        private void UpdateHealth()
        {
            if (health <= 0F && !isDead)
            {
                isDead = true;
                SendMessage("OnDied", SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }
        }

        public void Damage(float damage)
        {
            Health -= damage / maxHealth;
        }
    }
}
