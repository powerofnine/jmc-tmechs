using JetBrains.Annotations;
using TMechs.Entity;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.Environment
{
    public class HealthPickup : MonoBehaviour, EntityHealth.IDeath
    {
        public bool restorePercentage = true;

        [ConditionalHide("restorePercentage", true)]
        public float healthRestorePercent = 0.25F;

        [ConditionalHide("restorePercentage", true, true)]
        public float healthRestoreAbsolute = 25;

        public void OnDying(ref bool customDestroy)
        {
            if (restorePercentage)
                Player.Player.Instance.Health.Heal(healthRestorePercent, true);
            else
                Player.Player.Instance.Health.Heal(healthRestoreAbsolute, false);
        }
    }
}
