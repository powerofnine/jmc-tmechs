using System;
using System.Collections;
using TMechs.Enemy;
using TMechs.Environment.Targets;
using TMechs.Player;
using UnityEngine;

namespace TMechs.Entity
{
    public class RocketFist : MonoBehaviour
    {
        public float maxTime = 5F;
        public float speed = 10F;
        [NonSerialized]
        public float damage = 10F;
        
        private Transform anchor;
        [HideInInspector]
        public Transform target;

        private bool isReturning;

        private bool done;
        
        private void Awake()
        {
            anchor = Player.Player.Instance.rocketFistAnchor;

            transform.position = anchor.position;
            transform.forward = anchor.forward;
        }

        private void Update()
        {
            if (done)
                return;

            Transform target = GetTarget();

            Vector3 relativePos = target.position - transform.position;
            if (isReturning)
                relativePos = -relativePos;
            Quaternion rotation = transform.rotation;
            rotation.SetLookRotation(relativePos);
            transform.eulerAngles = rotation.eulerAngles;
            
            transform.Translate(Vector3.forward * speed * Time.deltaTime * (isReturning ? -1 : 1));

            maxTime -= Time.deltaTime;
            if (maxTime <= 0F)
                isReturning = true;

            if (Vector3.Distance(transform.position, target.position) < 2F)
            {
                transform.position = target.position;
                if (!isReturning)
                    isReturning = true;
                else
                {
                    StartCoroutine(MatchPositionAndRotation(target));
                    done = true;
                }
            }
        }

        private IEnumerator MatchPositionAndRotation(Transform target)
        {
            float progress = 0F;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            
            while (progress <= 1F)
            {
                progress += Time.deltaTime * 2F;

                transform.position = Vector3.Lerp(startPos, target.position, progress);
                transform.rotation = Quaternion.Slerp(startRot, target.rotation, progress);

                yield return null;
            }
            
            Player.Player.Instance.Animator.SetTrigger(Anim.ROCKET_RETURN);
            Destroy(gameObject);
        }

        private Transform GetTarget() => isReturning ? anchor : target;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                return;

            EntityHealth entity = other.GetComponent<EntityHealth>();
            if(entity)
                entity.Damage(damage);
            
            if(other.transform == GetTarget())
                isReturning = true;
        }
    }
}
