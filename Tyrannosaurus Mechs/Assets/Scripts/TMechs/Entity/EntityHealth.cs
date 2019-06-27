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
        private bool isDead;

        private void UpdateHealth()
        {
            if (health <= 0F && !isDead)
            {
                isDead = true;
                bool customDestroy = false;
                
                foreach (IDeath evt in GetComponentsInChildren<IDeath>(true))
                    evt.OnDying(ref customDestroy);
                
                if(customDestroy)
                    Destroy(this);
                else
                    Destroy(gameObject);
            }
        }

        public void Damage(float damage)
        {
            bool cancel = false;

            foreach (IDamage evt in GetComponentsInChildren<IDamage>(true))
            {
                evt.OnDamaged(this, ref cancel);

                if (cancel)
                    return;
            }

            Health -= damage / maxHealth;
        }

        public interface IDamage
        {
            void OnDamaged(EntityHealth health, ref bool cancel);
        }

        public interface IDeath
        {
            void OnDying(ref bool customDestroy);
        }
    }
}