using JetBrains.Annotations;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Environment
{
    public class InstantiateOnDie : MonoBehaviour, EntityHealth.IDeath
    {
        public GameObject template;
        [InspectorGadgets.Attributes.Toolbar("Enable", "Instantiate")]
        public bool instantiate = true;

        public bool explode;
        [ConditionalHide("explode", true)]
        public float explosionForce;
        [ConditionalHide("explode", true)]
        public Vector3 explosionCenter;
        [ConditionalHide("explode", true)]
        public float explosionRadius;

        public void OnDying(ref bool customDestroy)
        {
            GameObject go;

            if (instantiate)
                go = Instantiate(template, transform.position, transform.rotation);
            else
            {
                go = template;
                go.SetActive(true);
            }

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