using JetBrains.Annotations;
using TMechs.Entity;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.Environment
{
    public class HealthPickup : MonoBehaviour, EntityHealth.IDeath
    {
        public bool restorePercentage = true;
        public EntityHealth.DamageSource healSource; 

        [ConditionalHide("restorePercentage", true)]
        public float healthRestorePercent = 0.25F;

        [ConditionalHide("restorePercentage", true, true)]
        public float healthRestoreAbsolute = 25;

        public void OnDying(EntityHealth.DamageSource source, ref bool customDestroy)
        {
            if (restorePercentage)
                Player.Player.Instance.Health.Heal(healthRestorePercent, healSource.GetWithSource(transform), true);
            else
                Player.Player.Instance.Health.Heal(healthRestoreAbsolute, healSource.GetWithSource(transform), false);
        }
    }
}
