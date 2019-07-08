using System;
using System.Collections;
using UnityEngine;

namespace TMechs.Entity
{
    public class RocketFist : MonoBehaviour
    {
        public float maxTime = 5F;
        public float speed = 10F;
        [NonSerialized]
        public float damage = 10F;

        [HideInInspector]
        public Transform target;

        private bool done;

        private void Update()
        {
            if (done)
                return;

            if (!target)
                OnReturn();

            Vector3 relativePos = target.position - transform.position;
            Quaternion rotation = transform.rotation;
            rotation.SetLookRotation(relativePos);
            transform.eulerAngles = rotation.eulerAngles;

            transform.Translate(speed * Time.deltaTime * Vector3.forward);

            maxTime -= Time.deltaTime;
            if (maxTime <= 0F)
            {
                OnReturn();
                return;
            }

            if (Vector3.Distance(transform.position, target.position) < 2F)
            {
                OnReturn();
                
                EntityHealth entity = target.GetComponent<EntityHealth>();
                if (entity)
                    entity.Damage(damage);
            }
        }

        private void OnReturn()
        {
            Player.Player.Instance.rocketFist.rocketReturned = true;
            Destroy(gameObject);
            done = true;
        }
    }
}