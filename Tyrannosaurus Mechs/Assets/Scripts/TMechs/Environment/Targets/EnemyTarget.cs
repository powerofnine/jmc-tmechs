using TMechs.Entity;
using TMechs.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.Environment.Targets
{
    public class EnemyTarget : BaseTarget
    {
        public PickupType pickup;

        private RigidbodyConstraints constraints;

        private Image healthValue;
        private EntityHealth health;
        private float healthVelocity;
        private float alphaVelocity;
        
        public override int GetPriority() => 100;
        public override Color GetHardLockColor() => Color.red;
        public override Color GetColor() => Color.yellow;

        protected override void Awake()
        {
            base.Awake();

            healthValue = UseSpecific("EnemyHealth")?.GetComponent<Image>();
            health = GetComponentInParent<EntityHealth>();

            if (health && healthValue)
                healthValue.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!health && healthValue)
            {
                healthValue.gameObject.SetActive(false);
                return;
            }

            if (!healthValue)
                return;
            
            healthValue.fillAmount = Mathf.SmoothDamp(healthValue.fillAmount, health.Health, ref healthVelocity, .1F);

            bool shouldBeActive = BossHealthBar.activeHealthBar != health;
            
            if(healthValue.gameObject.activeSelf != shouldBeActive)
                healthValue.gameObject.SetActive(shouldBeActive);
        }

        public enum PickupType
        {
            Prohibit,
            Light,
            Heavy
        }
    }
}