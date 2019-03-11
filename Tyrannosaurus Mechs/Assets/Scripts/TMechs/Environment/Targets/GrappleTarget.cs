using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class GrappleTarget : BaseTarget
    {
        public bool isSwing;
        public float radius;

        public float maxSwingDistance = 10F;
        
        public override int GetPriority() => 0;
        public override Color GetColor() => Color.green;
        public override Color GetHardLockColor() => throw new System.NotImplementedException();

        private void OnDrawGizmosSelected()
        {
            if (!isSwing)
                return;
            
            Gizmos.color = GetColor();
            
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxSwingDistance);
        }
    }
}