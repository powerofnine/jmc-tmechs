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
        public Dictionary<int, T> SetEnum<T>(bool requireFriendlyName = true)
        {
            Dictionary<int, T> map = new Dictionary<int, T>();

            T[] items = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            List<string> values = new List<string>();

            foreach (T item in items)
            {
                MemberInfo info = item.GetType().GetMember(item.ToString()).SingleOrDefault();
                if (info == null)
                    continue;

                FriendlyNameAttribute fn = (FriendlyNameAttribute) info.GetCustomAttributes(typeof(FriendlyNameAttribute)).SingleOrDefault();

                string name;

                if (fn == null)
                {
                    if (requireFriendlyName)
                        continue;

                    name = info.Name;
                }
                else
                    name = fn.name;

                map.Add(values.Count, item);
                values.Add(name);
            }

            this.values = values.ToArray();

            return map;
        }
    }
}