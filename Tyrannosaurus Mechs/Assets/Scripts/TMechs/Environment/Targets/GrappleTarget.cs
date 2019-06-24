using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class GrappleTarget : BaseTarget
    {
        public bool isSwing;
        [ConditionalHide("isSwing", true)]
        public float radius = 10F;

        public float cooldown = 1F;
        private float currentCooldown;

        public void OnGrapple()
        {
            currentCooldown = cooldown;
        }
        
        public override int GetPriority() => 0;
        public override Color GetColor() => Color.green;
        public override Color GetHardLockColor() => throw new System.NotImplementedException();

        public override bool CanTarget() => currentCooldown <= 0F;

        private GamepadLabelComponent label;

        protected override void Awake()
        {
            base.Awake();

            label = UseSpecific("GamepadLabel")?.GetComponent<GamepadLabelComponent>();
            if (label != null)
            {
                label.switcher.icon = IconMap.Icon.R2;
                label.switcher.isGeneric = false;
                label.label.text = isSwing ? "HOLD" : "";
            }
        }

        private void Update()
        {
            if(currentCooldown > 0F)
                currentCooldown -= Time.deltaTime;
        }

        private void OnDrawGizmosSelected()
        {
            if (!isSwing)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}