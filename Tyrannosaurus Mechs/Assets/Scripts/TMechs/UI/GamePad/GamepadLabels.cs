using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMechs.InspectorAttributes;
using TMPro;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class GamepadLabels : MonoBehaviour
    {
        private static GamepadLabels instance;

        public int labelPoolSize = 10;
        public GamepadLabelComponent labelTemplate;

        private GamepadLabelComponent[] pool;
        
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

        private readonly HashSet<IconIdentifier> identifiers = new HashSet<IconIdentifier>();

        private void Awake()
        {
            instance = this;
            
            pool = new GamepadLabelComponent[labelPoolSize];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = Instantiate(labelTemplate, transform);
                pool[i].gameObject.SetActive(false);
            }
        }

        public static void AddLabel(IconMap.Icon icon, string label, int sortOrder = 0)
        {
            if(instance)
                instance.AddLabel(new IconIdentifier(icon, label, sortOrder));
        }

        public static void AddLabel(IconMap.IconGeneric icon, string label, int sortOrder = 0)
        {
            if(instance)
                instance.AddLabel(new IconIdentifier(icon, label, sortOrder));
        }

        private void AddLabel(IconIdentifier identifier)
        {
            identifiers.Add(identifier);
        }

        private void LateUpdate()
        {
            IconIdentifier[] sorted = identifiers.OrderBy(x => x.sortOrder).ThenBy(x => x.GetRelevantIcon()).ToArray();

            for (int i = 0; i < pool.Length; i++)
            {
                if (i < sorted.Length)
                {
                    if(!pool[i].gameObject.activeInHierarchy)
                        pool[i].gameObject.SetActive(true);
                    
                    sorted[i].CopyToComponent(pool[i]);
                }
                else
                {
                    if(pool[i].gameObject.activeInHierarchy)
                        pool[i].gameObject.SetActive(false);
                }
            }
            
            identifiers.Clear();
        }

//        public static void SetLabel(ButtonLabel label, string text)
//        {
//            if (instance)
//                instance._SetLabel(label, text);
//        }
//
//        private void _SetLabel(ButtonLabel label, string text)
//        {
//            TextMeshProUGUI lbl = GetLabel(label);
//
//            if (lbl)
//                lbl.text = text;
//        }
//
//        private TextMeshProUGUI GetLabel(ButtonLabel label)
//        {
//            switch (label)
//            {
//                case ButtonLabel.ActionBottomRow1:
//                    return abr1;
//                case ButtonLabel.ActionBottomRow2:
//                    return abr2;
//                case ButtonLabel.ActionTopRow1:
//                    return atr1;
//                case ButtonLabel.ActionTopRow2:
//                    return atr2;
//            }
//
//            return null;
//        }

        public enum ButtonLabel
        {
            ActionBottomRow1,
            ActionBottomRow2,
            ActionTopRow1,
            ActionTopRow2
        }

        private struct IconIdentifier
        {
            private readonly bool isGeneric;
            private readonly IconMap.Icon icon;
            private readonly IconMap.IconGeneric genericIcon;
            private readonly string label;

            public readonly int sortOrder;

            public IconIdentifier(IconMap.Icon icon, string label, int sortOrder)
            {
                isGeneric = false;
                this.icon = icon;

                this.label = label;
                
                genericIcon = 0;

                this.sortOrder = sortOrder;
            }

            public IconIdentifier(IconMap.IconGeneric icon, string label, int sortOrder)
            {
                isGeneric = true;
                genericIcon = icon;

                this.label = label;
                
                this.icon = 0;
                
                this.sortOrder = sortOrder;
            }
            
            public void CopyToComponent(GamepadLabelComponent component)
            {
                component.switcher.isGeneric = isGeneric;
                component.switcher.icon = icon;
                component.switcher.genericIcon = genericIcon;
                component.label.text = label;
            }

            [Pure]
            public object GetRelevantIcon()
            {
                if (isGeneric)
                    return genericIcon;

                return icon;
            }

            public override int GetHashCode()
            {
                int genCode = isGeneric ? int.MaxValue / 2 : 0;
                return genCode + GetRelevantIcon().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is IconIdentifier))
                    return false;
                
                IconIdentifier other = (IconIdentifier)obj;
                
                return isGeneric == other.isGeneric && (int)GetRelevantIcon() == (int)other.GetRelevantIcon();
            }
        }
    }
}