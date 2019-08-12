using System;
using TMPro;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class GamepadLabelComponent : MonoBehaviour
    {
        public bool labelActive;
        public string LabelText
        {
            get => label.text;
            set => label.text = value;
        }

        private CanvasGroup group;
        private TextMeshProUGUI label;
        
        private float alphaVelocity;

        // Required by GrappleTarget
        [Space]
        public IconSwitcher switcher;
        
        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void LateUpdate()
        {
            if(group)
                group.alpha = Mathf.SmoothDamp(group.alpha, labelActive ? 1F : 0.25F, ref alphaVelocity, .1F);
        }
    }
}