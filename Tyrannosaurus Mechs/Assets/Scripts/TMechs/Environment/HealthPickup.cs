using JetBrains.Annotations;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.Environment
{
    public class HealthPickup : MonoBehaviour
    {
        public bool restorePercentage = true;

        [ConditionalHide("restorePercentage", true)]
        public float healthRestorePercent = 0.25F;

        [ConditionalHide("restorePercentage", true, true)]
        public float healthRestoreAbsolute = 25;
        
        [UsedImplicitly]
        private void OnDied()
        {
            if (restorePercentage)
                Player.Player.Instance.Health += healthRestorePercent;
            else
                Player.Player.Instance.Health += healthRestoreAbsolute / Player.Player.Instance.maxHealth;
        }
    }
}
