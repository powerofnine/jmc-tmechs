using System.Collections.Generic;
using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class EnemyTarget : BaseTarget
    {
        public PickupType pickup;

        private RigidbodyConstraints constraints;
        private readonly Dictionary<Component, bool> states = new Dictionary<Component, bool>();

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

            foreach (Component cmp in GetComponentsInChildren<Component>())
            {
                states[cmp] = GetSetValue(cmp, false);
            }
        }

        public void HandleThrow()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
                rb.constraints = constraints;

            foreach (KeyValuePair<Component, bool> kvp in states)
                if (kvp.Key)
                    GetSetValue(kvp.Key, kvp.Value);
        }

        private bool GetSetValue(Component cmp, bool enabled)
        {
            bool val = false;

            switch (cmp)
            {
                case Behaviour beh:
                    val = beh.enabled;
                    beh.enabled = enabled;
                    break;
                case Collider col:
                    val = col.enabled;
                    col.enabled = enabled;
                    break;
            }

            return val;
        }

        public enum PickupType
        {
            Prohibit,
            Light,
            Heavy
        }
    }
}