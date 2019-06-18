using TMechs.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.Environment.Targets
{
    public abstract class BaseTarget : MonoBehaviour
    {
        private Transform lookTarget;

        private byte lastPing;
        private bool hardLock;

        protected Transform targetRoot;
        private Transform lookAnchor;
        private Image targetImage;

        private void OnEnable()
        {
            TargetController.Add(this);
        }

        private void OnDisable()
        {
            TargetController.Remove(this);

            targetRoot.gameObject.SetActive(false);
        }

        public abstract int GetPriority();
        public abstract Color GetColor();
        public abstract Color GetHardLockColor();

        public virtual bool CanTarget() => true;

        protected virtual void Awake()
        {
            lookAnchor = new GameObject("Look Anchor").transform;
            lookAnchor.SetParent(transform, false);

            targetRoot = Instantiate(Resources.Load<GameObject>("Prefabs/TargetRender"), lookAnchor).transform;
            targetImage = targetRoot.Find("Target").GetComponent<Image>();
            targetRoot.gameObject.SetActive(false);

            lookTarget = Player.Player.Instance.Camera.camera.transform;
        }

        private void LateUpdate()
        {
            bool shouldShow = lastPing > 0;
            if (shouldShow != targetRoot.gameObject.activeSelf)
                targetRoot.gameObject.SetActive(shouldShow);
            targetImage.color = hardLock ? GetHardLockColor() : GetColor();

            if (!shouldShow)
                return;
            lastPing--;
            
            if (lookTarget)
            {
                lookAnchor.transform.LookAt(lookTarget);
                lookAnchor.Rotate(0F, 180F, 0F);
            }
        }

        public void Ping(bool hardLock = false)
        {
            this.hardLock = hardLock;
            lastPing = 1;
        }
    }
}