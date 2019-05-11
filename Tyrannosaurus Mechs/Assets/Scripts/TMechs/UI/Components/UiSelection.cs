using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TMechs.Attributes;
using TMPro;
using UnityEngine;

namespace TMechs.UI.Components
{
    public class UiSelection : UiSelectable
    {
        public string[] values = { };
        public TextMeshProUGUI selectionText;

        public int Value
        {
            get => Mathf.Clamp(value, 0, values.Length - 1);
            set
            {
                this.value = value;
                UpdateState_Pre();
                NotifyValueChange();
            }
        }

        [SerializeField]
        private int value;

        protected override void UpdateState(bool instant)
        {
            base.UpdateState(instant);

            if (value >= values.Length)
                value = 0;
            else if (value < 0)
                value = values.Length - 1;

            if (selectionText)
                selectionText.text = values[Value];
        }

        public override bool DirectionPressed(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    Value--;
                    return true;
                case Direction.Right:
                    Value++;
                    return true;
                case Direction.Up:
                case Direction.Down:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            return false;
        }

        [NotNull]
        public Dictionary<int, T> SetEnum<T>(bool requireFriendlyName = true) where T : Enum
        {
            Dictionary<int, T> map = new Dictionary<int, T>();
            IEnumerable<string> values = FriendlyNameAttribute.GetNames(requireFriendlyName, map);
            
            this.values = values.ToArray();
            return map;
        }
    }
}