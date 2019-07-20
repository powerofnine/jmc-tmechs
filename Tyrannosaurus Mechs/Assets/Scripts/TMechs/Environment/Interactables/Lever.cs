using UltEvents;
using UnityEngine;

namespace TMechs.Environment.Interactables
{
    public class Lever : Interactable
    {
        public bool isOn;
        public UltEvent<bool> onToggle;
        public bool isSingleUse = true;

        [Header("Animation")]
        public Animator animator;
        public string propertyName = "IsOn";

        private bool used;

        private void Awake()
        {
            if (animator)
                animator.SetBool(propertyName, isOn);
        }

        public override void OnInteract()
        {
            used = true;

            isOn = !isOn;

            if (animator)
                animator.SetBool(propertyName, isOn);

            onToggle.InvokeX(isOn);
        }

        public override bool IsInteractable() => !isSingleUse || !used;
    }
}