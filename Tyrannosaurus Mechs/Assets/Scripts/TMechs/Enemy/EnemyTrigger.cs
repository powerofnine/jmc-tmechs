using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyTrigger : MonoBehaviour
    {
        public string id;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                foreach (ITriggerListener listener in GetComponentsInParent<ITriggerListener>())
                    listener.OnTrigger(id);
        }

        public interface ITriggerListener
        {
            void OnTrigger(string id);
        }
    }
}