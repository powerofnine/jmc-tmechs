using TMechs.Player.Behavior;
using UnityEngine;

namespace TMechs.Environment.Interactables
{
    public abstract class Interactable : MonoBehaviour
    {
        public string displayText;
        
        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || !IsInteractable() || !IsWithinAngle())
                return;
            
            Player.Player.Instance.interaction.AddInteraction(this);
        }
        
        public virtual bool IsWithinAngle()
        {
            Vector3 direction = (transform.position - Player.Player.Instance.transform.position).normalized;
            return Vector3.Angle(direction, Player.Player.Instance.transform.forward) < 90F;
        }

        public virtual bool IsInteractable() => true;
        public abstract void OnInteract();
        public virtual PlayerBehavior PushBehavior() => null;
    }
}
