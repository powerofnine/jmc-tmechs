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
        
        private void Awake()
        {
            anchor = Player.Player.Instance.rocketFistAnchor;
            target = TargetController.Instance.GetTarget(true).transform;

            transform.position = anchor.position;
            transform.forward = anchor.forward;
        }

        private void Update()
        {
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
                isReturning = true;

            if (Vector3.Distance(transform.position, target.position) < 1F)
            {
                transform.position = target.position;
                if (!isReturning)
                    isReturning = true;
                else
                {
                    Player.Player.Instance.Animator.SetTrigger("Rocket Fist Return");
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                return;
            isReturning = true;
        }
    }
}
