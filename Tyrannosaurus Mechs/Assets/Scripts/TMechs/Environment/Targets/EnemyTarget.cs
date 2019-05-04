using System.Collections.Generic;
using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class EnemyTarget : BaseTarget
    {
        public PickupType pickup;

        private RigidbodyConstraints constraints;
        private readonly Dictionary<Behaviour, bool> states = new Dictionary<Behaviour, bool>();

        public override int GetPriority() => 100;
        public override Color GetHardLockColor() => Color.red;
        public override Color GetColor() => Color.yellow;

        public void HandlePickup()
        {
            states.Clear();

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
            {
                constraints = rb.constraints;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            foreach (Behaviour beh in GetComponentsInChildren<Behaviour>())
            {
                states[beh] = beh.enabled;
                beh.enabled = false;
            }
        }

        public void HandleThrow()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
                rb.constraints = constraints;

            foreach (KeyValuePair<Behaviour, bool> kvp in states)
                if (kvp.Key)
                    kvp.Key.enabled = kvp.Value;
        }

        public enum PickupType
        {
            Prohibit,
            Light,
            Heavy
        }
    }
}