using System.Collections;
using TMechs.Environment.Targets;
using TMechs.Player;
using UnityEngine;

namespace TMechs.Entity
{
    public class RocketFist : MonoBehaviour
    {
        public float maxTime = 5F;
        public float speed = 10F;
        public float rotateDamp = .25F;
        
        private Transform anchor;
        private Transform target;

        private bool isReturning;

        private Vector3 dampVelocity;

        private bool done;
        
        private static readonly int ANIM_RETURN = Animator.StringToHash("Rocket Fist Return");

        private void Awake()
        {
            anchor = Player.Player.Instance.rocketFistAnchor;
            target = TargetController.Instance.GetTarget<EnemyTarget>().transform;

            transform.position = anchor.position;
            transform.forward = anchor.forward;
        }

        private void Update()
        {
            if (done)
                return;
            
            Transform target = this.target;
            if (isReturning)
                target = anchor;

            Vector3 relativePos = target.position - transform.position;
            if (isReturning)
                relativePos = -relativePos;
            Quaternion rotation = transform.rotation;
            rotation.SetLookRotation(relativePos);
            Vector3 rot = rotation.eulerAngles;
            
            transform.eulerAngles = transform.eulerAngles.SmoothDampAngle(rot, ref dampVelocity, rotateDamp);

            transform.Translate(Vector3.forward * speed * Time.deltaTime * (isReturning ? -1 : 1));

            maxTime -= Time.deltaTime;
            if (maxTime <= 0F)
            {
                isReturning = true;
                rotateDamp = 0F;
            }

            if (Vector3.Distance(transform.position, target.position) < 1F)
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
            
            while (progress < 1F)
            {
                progress += Time.deltaTime / 2F;

                transform.position = Vector3.Lerp(startPos, target.position, progress);
                transform.rotation = Quaternion.Lerp(startRot, target.rotation, progress);

                yield return null;
            }
            
            Player.Player.Instance.Animator.SetTrigger(ANIM_RETURN);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                return;
            isReturning = true;
        }
    }
}
