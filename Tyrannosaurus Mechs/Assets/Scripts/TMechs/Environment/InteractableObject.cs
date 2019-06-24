using TMechs.UI.GamePad;
using UIEventDelegate;
using UnityEngine;

namespace TMechs.Environment
{
    public class InteractableObject : MonoBehaviour
    {
        public static bool eventToggleValue;

        public string displayText = "Press B to Blow";
        public bool isOn;
        public ReorderableEventList onToggle;
        public bool isSingleUse = true;

        [Header("Animation")]
        public Animator animator;
        public string propertyName = "IsOn";

        private bool used;
        private int overrideLabel;

        private void Awake()
        {
            if (animator)
                animator.SetBool(propertyName, isOn);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || !IsWithinAngle() || (isSingleUse && used))
                return;

            overrideLabel = 3; 
            
            if (Player.Player.Input.GetButtonDown(Controls.Action.INTERACT) && onToggle != null)
            {
                OnToggle();
            }
        }

        private void LateUpdate()
        {
            if(overrideLabel-- > 0)
                GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow2, displayText);
        }

        public virtual void OnToggle()
        {
            used = true;

            isOn = !isOn;

            if (animator)
                animator.SetBool(propertyName, isOn);

            eventToggleValue = isOn;
            if (onToggle != null)
                EventDelegate.Execute(onToggle.List);
        }

        public virtual bool IsWithinAngle()
        {
            Vector3 direction = (transform.position - Player.Player.Instance.transform.position).normalized;
            return Vector3.Angle(direction, Player.Player.Instance.transform.forward) < 90F;
        }
    }
}