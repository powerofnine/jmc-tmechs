using System.Collections.Generic;
using System.Linq;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Player
{
    [AddComponentMenu("")]
    public class ThrowableContainer : MonoBehaviour
    {
        public float damage = 5F;

        private GameObject containedObject;

        private Renderer[] renderCache;
        private Dictionary<Renderer, Mesh> meshCache = new Dictionary<Renderer, Mesh>();

        private Rigidbody rb;
        private bool isDead = false;
        
        public void Initialize(GameObject containedObject)
        {
            transform.position = containedObject.transform.position;

            renderCache = containedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renderCache)
            {
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

            containedObject.SetActive(false);
            containedObject.transform.SetParent(transform);
            containedObject.transform.localPosition = Vector3.zero;

            this.containedObject = containedObject;
        }

        private void Update()
        {
            if (isDead)
                return;
            
            foreach (Renderer render in renderCache)
            {
                if (!render || !meshCache.ContainsKey(render))
                    return;

                Graphics.DrawMesh(meshCache[render], render.localToWorldMatrix, render.material, render.gameObject.layer);
            }
        }

        public void Throw(Vector3 velocity)
        {
            IEnumerable<Collider> colliders = containedObject.GetComponentsInChildren<Collider>(true).Where(x => !x.isTrigger);

            foreach (Collider col in colliders)
            {
                GameObject go = new GameObject($"simcol:{col.name}");
                go.transform.SetParent(transform);
                
                go.transform.position = col.transform.position;
                go.transform.rotation = col.transform.rotation;
                go.transform.localScale = col.transform.localScale;

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
            rb.velocity = velocity;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isDead || other.collider.CompareTag("Player"))
                return;
            isDead = true;
            EntityHealth entity = other.collider.GetComponent<EntityHealth>();
            if (entity)
                entity.Damage(damage);

            containedObject.transform.SetParent(null, true);
            
            renderCache = null;
            meshCache = null;
            
            Invoke(nameof(Destroy), 1F);
        }

        private void Destroy()
        {
            DestroyImmediate(gameObject);
            containedObject.SetActive(true);
        }
    }
}