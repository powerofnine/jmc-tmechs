using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI.GamePad
{
    public class IconSwitcher : MonoBehaviour
    {
        public bool isGeneric;

        [ConditionalHide("isGeneric", true, true)]
        public IconMap.Icon icon;
        [ConditionalHide("isGeneric", true, false)]
        public IconMap.IconGeneric genericIcon;

        private ControllerDef.ButtonLayout currentLayout = ControllerDef.ButtonLayout.Unsupported;
        private IconMap.Icon lastIcon = IconMap.Icon.L1;
        private IconMap.IconGeneric lastGeneric = IconMap.IconGeneric.RightAnalog;
        private bool lastIsGeneric;

        private Image image;
        private SpriteRenderer sr;

        private void Awake()
        {
            image = GetComponent<Image>();
            sr = GetComponent<SpriteRenderer>();

            UpdateIcon();
        }

        private void LateUpdate()
        {
            ControllerDef.ButtonLayout layout = ButtonDisplayController.Instance.ButtonLayout;

            if (layout != currentLayout || isGeneric != lastIsGeneric || icon != lastIcon || genericIcon != lastGeneric)
            {
                currentLayout = layout;
                UpdateIcon();
            }
        }

        private void UpdateIcon()
        {
            Sprite sprite;

            if (isGeneric)
                sprite = IconMap.Get(genericIcon);
            else
                sprite = IconMap.Get(currentLayout, icon);

            if (image)
                image.sprite = sprite;
            if (sr)
                sr.sprite = sprite;

            lastIcon = icon;
            lastGeneric = genericIcon;
            lastIsGeneric = isGeneric;
        }
    }
}