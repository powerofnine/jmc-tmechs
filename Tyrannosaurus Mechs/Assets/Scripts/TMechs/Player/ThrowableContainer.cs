using System.Collections.Generic;
using System.Linq;
using TMechs.Entity;
using TMechs.Environment;
using UnityEngine;

namespace TMechs.Player
{
    [AddComponentMenu("")]
    public class ThrowableContainer : MonoBehaviour
    {
        /// <summary>
        /// Amount of damage that an entity that this entity collides with is dealth
        /// </summary>
        public float recepientDamage = 10F;

        /// <summary>
        /// Amount of damage that this entity is dealt when it collides with something
        /// </summary>
        public float sourceDamage = 10F;

        private GameObject containedObject;
        private Vector3 startScale;
        
        private Renderer[] renderCache;
        private readonly Dictionary<Renderer, Mesh> meshCache = new Dictionary<Renderer, Mesh>();

        private Rigidbody rb;
        private bool isDead;
        
        public void Initialize(GameObject containedObject)
        {
            transform.position = containedObject.transform.position;
            transform.rotation = containedObject.transform.rotation;

            renderCache = containedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renderCache)
            {
                if(!render.enabled)
                    continue;
                
                switch (render)
                {
                    case MeshRenderer _:
                    {
                        MeshFilter filter = render.GetComponent<MeshFilter>();
                        if (filter)
                            meshCache.Add(render, filter.mesh);
                        break;
                    }

                    case SkinnedMeshRenderer skinned:
                        meshCache.Add(render, skinned.sharedMesh);
                        break;
                }
            }

            startScale = containedObject.transform.localScale;
            
            containedObject.SetActive(false);
            containedObject.transform.SetParent(transform);
            containedObject.transform.localPosition = Vector3.zero;
            containedObject.transform.localRotation = Quaternion.identity;

            this.containedObject = containedObject;
        }

        private void Update()
        {
            if (isDead)
                return;

            if(!containedObject)
                Destroy(gameObject);
            
            foreach (Renderer render in renderCache)
            {
                if (!render || !meshCache.ContainsKey(render))
                    return;

                Graphics.DrawMesh(meshCache[render], render.localToWorldMatrix, render.material, render.gameObject.layer);
            }
        }

        public void Throw(Vector3 target, float angle, float speed)
        {
            if (!containedObject)
            {
                Debug.LogError("Contained object has been destroyed");
                return;
            }

            IEnumerable<Collider> colliders = containedObject.GetComponentsInChildren<Collider>(false).Where(x => !x.isTrigger);

            foreach (Collider col in colliders)
            {
                GameObject go = new GameObject($"simcol:{col.name}");
                go.transform.SetParent(transform);
                
                go.transform.position = col.transform.position;
                go.transform.rotation = col.transform.rotation;
                go.transform.localScale = col.transform.localScale;

                // Terrible, but outside of reflection, best way to copy a collider
                switch (col)
                {
                    case SphereCollider sph:
                        SphereCollider simSph = go.AddComponent<SphereCollider>();
                        simSph.center = sph.center;
                        simSph.radius = sph.radius;
                        break;
                    case CharacterController cc:
                        CapsuleCollider simCc = go.AddComponent<CapsuleCollider>();
                        simCc.center = cc.center;
                        simCc.radius = cc.radius;
                        simCc.height = cc.height;
                        break;
                    case CapsuleCollider cap:
                        CapsuleCollider simCap = go.AddComponent<CapsuleCollider>();
                        simCap.center = cap.center;
                        simCap.radius = cap.radius;
                        simCap.height = cap.height;
                        break;
                    case BoxCollider box:
                        BoxCollider simBox = go.AddComponent<BoxCollider>();
                        simBox.center = box.center;
                        simBox.size = box.size;
                        break;
                }
            }

            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;

            ParabolicThrow th = gameObject.AddComponent<ParabolicThrow>();
            th.target = target;
            th.inAngle = angle;
            th.outAngle = angle;
            th.speed = speed;

            Vector3 dir = target - transform.position;
            th.onEnd = () =>
            {
                rb.velocity = dir * speed + Vector3.down * 30F;
                rb.constraints = RigidbodyConstraints.None;
            };

            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
                t.gameObject.layer = LayerMask.NameToLayer("Thrown");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isDead || other.collider.CompareTag("Player"))
                return;
            
            Destroy(gameObject.GetComponent<ParabolicThrow>());
            
            isDead = true;
            EntityHealth entity = other.collider.GetComponent<EntityHealth>();
            if (entity)
                entity.Damage(recepientDamage);

            if (containedObject)
            {
                EntityHealth containedEntity = containedObject.GetComponent<EntityHealth>();
                if (containedEntity)
                    containedEntity.Damage(sourceDamage);

                Rigidbody containedRb = containedObject.GetComponent<Rigidbody>();
                if (containedRb)
                    containedRb.velocity = rb.velocity;
                
                gameObject.SetActive(false);
                containedObject.transform.SetParent(null, true);
                containedObject.SetActive(true);

                containedObject.transform.localScale = startScale;
                containedObject.transform.rotation = transform.rotation;
                
                foreach(IOnThrowableReleased e in containedObject.GetComponentsInChildren<IOnThrowableReleased>())
                    e.OnThrowableReleased();
            }

            // This is stupid, but Unity crashes here if there is no delay before destroying this object
            // Something to do with unparenting an object before destroying the parent crashes unity?
            Invoke(nameof(Destroy), .25F);
        }

        public void DamageContainedObject(float damage)
        {
            if (containedObject)
            {
                EntityHealth containedEntity = containedObject.GetComponent<EntityHealth>();
                if (containedEntity)
                {
                    containedEntity.Damage(damage);
                    if(containedEntity.Health <= 0F) // Destroy regardless of how object handles death internally
                        Destroy(containedEntity.gameObject);
                }
            }
        }

        private void Destroy()
        {
            DestroyImmediate(gameObject);
        }
        
        public interface IOnThrowableReleased
        {
            void OnThrowableReleased();
        }
    }
}