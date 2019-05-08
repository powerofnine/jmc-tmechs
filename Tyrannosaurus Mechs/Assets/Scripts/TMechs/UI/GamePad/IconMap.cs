using System;
using System.Collections.Generic;
using System.Reflection;
using TMechs.Attributes;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class IconMap
    {
        private const string ICON_PATH = "Controllers/Icons/";
        private const string GENERIC_TEXTURE = ICON_PATH + "buttons.gen";
        private static readonly Dictionary<ControllerDef.ButtonLayout, IconMap> map = new Dictionary<ControllerDef.ButtonLayout, IconMap>();
        private static readonly Dictionary<IconGeneric, Sprite> generic = new Dictionary<IconGeneric, Sprite>();

        private readonly Dictionary<Icon, Sprite> icons = new Dictionary<Icon, Sprite>();

        private IconMap(string location)
        {
            Sprite sprite = Resources.Load<Sprite>(location);

            foreach (Icon icon in Enum.GetValues(typeof(Icon)))
                icons.Add(icon, CreateSprite(sprite, (uint) icon, icon.ToString()));
        }

        public static Sprite Get(ControllerDef.ButtonLayout layout, Icon icon) => !map.ContainsKey(layout) ? null : map[layout].icons[icon];
        public static Sprite Get(IconGeneric icon) => generic[icon];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            foreach (ControllerDef.ButtonLayout layout in Enum.GetValues(typeof(ControllerDef.ButtonLayout)))
            {
                TextureNameAttribute tn = layout.GetType().GetMember(layout.ToString())[0].GetCustomAttribute<TextureNameAttribute>();

                if (tn == null)
                    continue;

                map.Add(layout, new IconMap(ICON_PATH + tn.name));
            }

            Sprite sprite = Resources.Load<Sprite>(GENERIC_TEXTURE);

            foreach (IconGeneric icon in Enum.GetValues(typeof(IconGeneric)))
                generic.Add(icon, CreateSprite(sprite, (uint) icon, icon.ToString()));
        }

        private static Sprite CreateSprite(Sprite sprite, uint index, string name)
        {
            const float COUNT = 4;

            Texture2D texture = sprite.texture;
            Rect rect = sprite.rect;

            Rect spRect = new Rect(
                    Mathf.CeilToInt(index % COUNT) * (rect.width / COUNT),
                    Mathf.CeilToInt(COUNT - index / COUNT - 1) * (rect.height / COUNT),
                    rect.width / COUNT,
                    rect.height / COUNT
            );

            Sprite spr = Sprite.Create(texture, spRect, Vector2.one * .5F, sprite.pixelsPerUnit, 0);
            spr.name = name;

            return spr;
        }

        public enum Icon : uint
        {
            ActionBottomRow1 = 0,
            ActionBottomRow2 = 1,
            ActionTopRow1 = 2,
            ActionTopRow2 = 3,

            L1 = 4,
            R1 = 5,
            L2 = 6,
            R2 = 7,

            Start = 8,
            Select = 9
        }

        public enum IconGeneric : uint
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
            L3 = 4,
            R3 = 5,
            LeftAnalog = 6,
            RightAnalog = 7
        }
    }
}