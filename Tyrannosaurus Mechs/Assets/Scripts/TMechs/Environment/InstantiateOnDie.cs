using JetBrains.Annotations;
using UnityEngine;

namespace TMechs.Environment
{
    public class InstantiateOnDie : MonoBehaviour
    {
        public GameObject template;

        public bool explode;
        [ConditionalHide("explode", true)]
        public float explosionForce;
        [ConditionalHide("explode", true)]
        public Vector3 explosionCenter;
        [ConditionalHide("explode", true)]
        public float explosionRadius;

        [UsedImplicitly]
        private void OnDied()
        {
            GameObject go = Instantiate(template, transform.position, transform.rotation);

            if (!explode)
                return;

            foreach (Rigidbody rb in go.GetComponentsInChildren<Rigidbody>())
                rb.AddExplosionForce(explosionForce, transform.position + explosionCenter, explosionRadius);
        }

        private void OnDrawGizmosSelected()
        {
            if (!explode)
                return;

            Gizmos.DrawCube(transform.position + explosionCenter, Vector3.one * .25F);
            Gizmos.DrawWireSphere(transform.position + explosionCenter, explosionRadius);
        }
    }
}