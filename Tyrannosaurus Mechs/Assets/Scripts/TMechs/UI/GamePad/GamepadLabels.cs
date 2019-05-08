using TMechs.InspectorAttributes;
using TMPro;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class GamepadLabels : MonoBehaviour
    {
        private static GamepadLabels instance;

        [Header("Labels")]
        [SerializeField]
        [Name("Action Bottom Row 1")]
        private TextMeshProUGUI abr1;
        [SerializeField]
        [Name("Action Bottom Row 2")]
        private TextMeshProUGUI abr2;
        [SerializeField]
        [Name("Action Top Row 1")]
        private TextMeshProUGUI atr1;
        [SerializeField]
        [Name("Action Top Row 2")]
        private TextMeshProUGUI atr2;

        private void Awake()
        {
            instance = this;
        }

        public static void SetLabel(ButtonLabel label, string text)
        {
            if (instance)
                instance._SetLabel(label, text);
        }

        private void _SetLabel(ButtonLabel label, string text)
        {
            TextMeshProUGUI lbl = GetLabel(label);

            if (lbl)
                lbl.text = text;
        }

        private TextMeshProUGUI GetLabel(ButtonLabel label)
        {
            switch (label)
            {
                case ButtonLabel.ActionBottomRow1:
                    return abr1;
                case ButtonLabel.ActionBottomRow2:
                    return abr2;
                case ButtonLabel.ActionTopRow1:
                    return atr1;
                case ButtonLabel.ActionTopRow2:
                    return atr2;
            }

            return null;
        }

        public enum ButtonLabel
        {
            ActionBottomRow1,
            ActionBottomRow2,
            ActionTopRow1,
            ActionTopRow2
        }
    }
}