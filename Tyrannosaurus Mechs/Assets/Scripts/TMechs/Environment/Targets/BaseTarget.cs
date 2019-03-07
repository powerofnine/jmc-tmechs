using TMechs.Player;
using UnityEngine;

namespace TMechs.Environment.Targets
{
    public abstract class BaseTarget : MonoBehaviour
    {
        private void OnEnable()
        {
            TargetController.Add(this);
        }

        private void OnDisable()
        {
            TargetController.Remove(this);
        }

        public abstract int GetPriority();
        public abstract Color GetColor();
        public abstract Color GetHardLockColor();
        public abstract bool CanHardLock();
    }
}
