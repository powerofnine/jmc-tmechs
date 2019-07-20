using UnityEngine;

namespace TMechs.Environment
{
    public class OneWayWall : MonoBehaviour
    {
        public Collider[] colliders = {};

        private void OnTriggerEnter(Collider other)
        {
            SetTrigger(other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            SetTrigger(other, false);
        }

        private void SetTrigger(Collider other, bool ignore)
        {
            foreach (Collider col in colliders)
                Physics.IgnoreCollision(col, other, ignore);
        }
    }
}
