using TMechs.Player.Behavior;
using UnityEngine;

namespace TMechs.Environment.Interactables
{
    public abstract class Interactable : MonoBehaviour
    {
        public string displayText;
        public GameObject display;

        [Space]
        public AudioSource interactSound;
        
        private int inRange;
        private int available;

        protected virtual void Update()
        {
            if (inRange > 0)
            {
                inRange--;
                Player.Player.Instance.interaction.AddInteraction(this);
            }

            if (available > 0)
                available--;
            
            if (display)
            {
                bool shouldShow = inRange > 0 && available > 0;
                
                if(display.activeSelf != shouldShow)
                    display.SetActive(shouldShow);
            }     
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || !IsInteractable() || !IsWithinAngle())
                return;

            inRange = 3;
        }
        
        public virtual bool IsWithinAngle()
        {
            Vector3 direction = (transform.position - Player.Player.Instance.transform.position).normalized;
            return Vector3.Angle(direction, Player.Player.Instance.transform.forward) < 90F;
        }

        public void OnInteractAvailable()
        {
            available = 3;
        }
        
        public virtual bool IsInteractable() => true;

        public virtual void OnInteract()
        {
            if(interactSound)
                interactSound.Play();
        }
        
        public virtual PlayerBehavior GetPushBehavior() => null;
        public virtual int GetSortPriority() => 0;
    }
}
