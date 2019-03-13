using TMechs.Player;
using UnityEngine;

namespace TMechs.Environment.Targets
{
    public abstract class BaseTarget : MonoBehaviour
    {
        private Transform player;
        
        private float lastPing = -1337F;
        private bool hardLock;

        private Transform lookAnchor;
        private SpriteRenderer target;
        
        private void OnEnable()
        {
            TargetController.Add(this);
        }

        private void OnDisable()
        {
            TargetController.Remove(this);
            
            target.gameObject.SetActive(false);
        }

        public abstract int GetPriority();
        public abstract Color GetColor();
        public abstract Color GetHardLockColor();

        private void Awake()
        {
            lookAnchor = new GameObject("Look Anchor").transform;
            lookAnchor.SetParent(transform, false);
            
            target = Instantiate(Resources.Load<GameObject>("Prefabs/TargetRender"), lookAnchor).GetComponent<SpriteRenderer>();
            target.gameObject.SetActive(false);

            target.transform.localScale = target.transform.lossyScale.InverseScale();
            target.sharedMaterial = Resources.Load<Material>("Sprite-AlwaysOnTop");
            
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void LateUpdate()
        {
            bool shouldShow = Time.time - lastPing < 1;
            if (shouldShow != target.gameObject.activeSelf)
                target.gameObject.SetActive(shouldShow);
            target.color = hardLock ? GetHardLockColor() : GetColor();
            
            if(player)
                lookAnchor.transform.LookAt(player);
            
            target.transform.Rotate(0F, 0F, 25F * Time.deltaTime);
        }

        public void Ping(bool hardLock = false)
        {
            this.hardLock = hardLock;
            lastPing = Time.time;
        }
    }
}
