using System;
using System.Collections.Generic;
using System.Linq;
using InspectorGadgets.Attributes;
using JetBrains.Annotations;
using TMechs.InspectorAttributes;
using TMPro;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class GamepadLabels : MonoBehaviour
    {
        private static GamepadLabels instance;

        [LabelledCollection(typeof(ButtonLabel))]
        public GamepadLabelComponent[] labels = { };
        private string[] defaultText;

        private readonly HashSet<ButtonLabel> activeLabels = new HashSet<ButtonLabel>();

        private void Start()
        {
            instance = this;

            defaultText = labels.Select(x => x.LabelText).ToArray();
        }

        private void LateUpdate()
        {
            foreach (ButtonLabel label in Enum.GetValues(typeof(ButtonLabel)))
            {
                if ((int) label >= labels.Length)
                    continue;

                labels[(int) label].labelActive = activeLabels.Contains(label);

                if (!activeLabels.Contains(label))
                    labels[(int) label].LabelText = defaultText[(int) label];
            }

            activeLabels.Clear();
        }

        public static void EnableLabel(ButtonLabel label, string text)
        {
            if (!instance)
                return;

            instance._EnableLabel(label, text);
        }

        private void _EnableLabel(ButtonLabel label, string text)
        {
            activeLabels.Add(label);
            if ((int) label < labels.Length)
                labels[(int) label].LabelText = text;
        }

        public enum ButtonLabel
        {
            Action,
            Attack,
            Jump
        }
    }
}