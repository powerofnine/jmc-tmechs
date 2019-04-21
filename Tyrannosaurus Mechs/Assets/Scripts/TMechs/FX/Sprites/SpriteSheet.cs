using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.FX.Sprites
{
    [RequireComponent(typeof(Image))]
    public class SpriteSheet : MonoBehaviour
    {
        private static Dictionary<string, Sprite[]> registeredSprites = new Dictionary<string, Sprite[]>();

        public Sprite Sheet
        {
            get => sheet;
            set
            {
                sheet = value;
                UpdateSprite();
            }
        }

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                UpdateSprite();
            }
        }

        public int Index
        {
            get => index;
            set
            {
                index = value;
                UpdateSprite();
            }
        }

        public int SpriteCount
        {
            get
            {
                if (!Sheet)
                    return -1;

                return Mathf.CeilToInt((Sheet.rect.width / Size.x) * (Sheet.rect.height / Size.y));
            }
        }

        [Tooltip("The pivot point of the sprite as a percentage, keep in mind that this only updates when the sprite gets changed")]
        public Vector2 pivot = new Vector2(0.5F, 0.5F);

        [SerializeField]
        [Tooltip("The sprite sheet to use, this variable is not listened to, so updating it during runtime will do nothing until a refresh")]
        private Sprite sheet;
        [SerializeField]
        [Tooltip("The size of each sprite, this variable is not listened to, so updating it during runtime will do nothing until a refresh")]
        private Vector2 size = new Vector2(64, 64);
        [SerializeField]
        [Tooltip("The index of the current sprite, counted starting at top-left, this variable is not listened to, so updating it during runtime will do nothing until a refresh")]
        private int index;

        private Image image;

        private void Start()
        {
            image = GetComponent<Image>();

            UpdateSprite();
        }

        private void BuildSheet()
        {
            Sprite[] sprites = new Sprite[SpriteCount];

            for (int i = 0; i < SpriteCount; i++)
            {
                sprites[i] = Sprite.Create(Sheet.texture, new Rect(Sheet.rect.x + (i % (Sheet.rect.width / Size.x)) * Size.x, Sheet.rect.y + Sheet.rect.height - Size.y - Mathf.FloorToInt(i / (Sheet.rect.height / Size.y)) * Size.y, Size.x, Size.y), new Vector2(0.5F, 0.5F));
                sprites[i].name = Sheet.name + "." + i;
            }

            registeredSprites.Add(Sheet.GetInstanceID() + ";" + Size.x + ";" + Size.y, sprites);
        }

        private void UpdateSprite()
        {
            if (!registeredSprites.ContainsKey(Sheet.GetInstanceID() + ";" + Size.x + ";" + Size.y))
            {
                BuildSheet();
            }

            if (image == null)
                return;

            if (Index >= SpriteCount)
                throw new IndexOutOfRangeException("The sprite index " + Index + " is out of range of the sheet '" + Sheet.name + "'");

            image.sprite = registeredSprites[Sheet.GetInstanceID() + ";" + Size.x + ";" + Size.y][Index];
        }
    }
}