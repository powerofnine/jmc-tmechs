using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class GrappleTarget : BaseTarget
    {
        public bool isSwing;
        
        public override int GetPriority() => 0;
        public override Color GetColor() => Color.green;
        public override Color GetHardLockColor() => throw new System.NotImplementedException();
    }
}