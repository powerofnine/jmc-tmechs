using UnityEngine;

namespace TMechs.Environment.Targets
{
    public class EnemyTarget : BaseTarget
    {
        public override int GetPriority() => 100;
        public override Color GetHardLockColor() => Color.red;
        public override Color GetColor() => Color.yellow;
    }
}