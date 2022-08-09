
using System;
using UnityEngine;

namespace W
{
    public enum ItemType
    {
        None,

        Space,
        Text,
        Button,

        Progress,
        Slider,
    }

    public struct Item
    {
        public ItemType Type;

        public string Text;
        public Func<string> TextDynamic;

        public Action Button;

        public Func<float> Progress;
        public Action<float> Slider;

        public bool TextColorDefined;
        public Color TextColor;

        public bool IconColorDefined;
        public Color IconColor;

        public Sprite Icon;
        public Sprite IconGlow;
    }

    public partial class UI
    {
        public static Color ColorDisable => new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public static Color ColorNormal => new Color(1f, 1f, 1f, 1f);
        public static Color ColorWarning => new Color(1f, 0, 0, 1f);

        public static Color ColorPositive => new Color(0.5f, 1f, 0.5f, 1f);
        public static Color ColorNegative => new Color(1f, 0.5f, 0.5f, 1f);


        public static Item Space() {
            Item item = new Item {
                Type = ItemType.Space,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Text(string text) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Text(Func<string> textDynamic) {
            Item item = new Item {
                Type = ItemType.Text,
                TextDynamic = textDynamic,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Text(string text, Func<string> textDynamic) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                TextDynamic = textDynamic,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item Text(string text, Color color) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                TextColorDefined = true,
                TextColor = color,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconText(string text, Sprite icon) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconText(string text, Color textColor, Sprite icon) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
                
                TextColorDefined = true,
                TextColor = textColor,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconText(string text, Sprite icon, Color spriteColor) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,

                IconColorDefined = true,
                IconColor = spriteColor,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconGlowText(string text, Sprite icon, Color spriteColor, Sprite glow) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
                IconGlow = glow,
                IconColorDefined = true,
                IconColor = spriteColor,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconText(string text, Color textColor, Sprite icon, Color spriteColor) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,

                TextColorDefined = true,
                TextColor = textColor,

                IconColorDefined = true,
                IconColor = spriteColor,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item IconGlowText(string text, Color textColor, Sprite icon, Color spriteColor, Sprite glow) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
                IconGlow = glow,

                TextColorDefined = true,
                TextColor = textColor,

                IconColorDefined = true,
                IconColor = spriteColor,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item Button(string text, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item IconButton(string text, Sprite icon, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item Button(string text, Color color, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                TextColorDefined = true,
                TextColor = color,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item IconButton(string text, Color color, Sprite icon, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,

                TextColorDefined = true,
                TextColor = color,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconButton(string text, Sprite icon, Color iconColor, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,

                IconColorDefined = true,
                IconColor = iconColor,
            };
            if (items != null) items.Add(item);
            return item;
        }

        public static Item IconButton(string text, Color color, Sprite icon, Color iconColor, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,

                TextColorDefined = true,
                TextColor = color,

                IconColorDefined = true,
                IconColor = iconColor,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item IconGlowButton(string text, Color color, Sprite icon, Color iconColor, Sprite glow, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,
                IconGlow = glow,

                TextColorDefined = true,
                TextColor = color,

                IconColorDefined = true,
                IconColor = iconColor,
            };
            if (items != null) items.Add(item);
            return item;
        }


        public static Item Button(Func<string> textDynamic, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                TextDynamic = textDynamic,
                Button = button,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Button(string text, Func<string> textDynamic, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                TextDynamic = textDynamic,
                Button = button,
            };
            if (items != null) items.Add(item);
            return item;
        }


        public static Item Progress(string text, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                Text = text,
                Progress = progress,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Progress(Func<string> textDynamic, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                TextDynamic = textDynamic,
                Progress = progress,
            };
            if (items != null) items.Add(item);
            return item;
        }
        public static Item Progress(string text, Func<string> textDynamic, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                Text = text,
                TextDynamic = textDynamic,
                Progress = progress,
            };
            if (items != null) items.Add(item);
            return item;
        }
    }
}
