
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
        private const float magic = 3/4f;
        /// <summary>
        /// 禁用按钮颜色
        /// </summary>
        public static Color ColorDisable => new Color(magic, magic, magic, magic);
        /// <summary>
        /// 可用按钮颜色
        /// </summary>
        public static Color ColorNormal => new Color(1f, 1f, 1f, 1f);
        /// <summary>
        /// 正面按钮颜色
        /// </summary>
        public static Color ColorPositive => new Color(1f, 1f, 1f, 1f);
        /// <summary>
        /// 负面按钮颜色
        /// </summary>
        public static Color ColorNegative => new Color(1f, magic, magic, 1f);

        public static void Space() {
            Item item = new Item {
                Type = ItemType.Space,
            };
            items.Add(item);
        }
        public static void Text(string text) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
            };
            items.Add(item);
        }
        public static void Text(Func<string> textDynamic) {
            Item item = new Item {
                Type = ItemType.Text,
                TextDynamic = textDynamic,
            };
            items.Add(item);
        }
        public static void Text(string text, Func<string> textDynamic) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                TextDynamic = textDynamic,
            };
            items.Add(item);
        }

        public static void Text(string text, Color color) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                TextColorDefined = true,
                TextColor = color,
            };
            items.Add(item);
            
        }

        public static void IconText(string text, Sprite icon) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
            };
            items.Add(item);
            
        }

        public static void IconText(string text, Color textColor, Sprite icon) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
                
                TextColorDefined = true,
                TextColor = textColor,
            };
            items.Add(item);
            
        }

        public static void IconText(string text, Sprite icon, Color spriteColor) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,

                IconColorDefined = true,
                IconColor = spriteColor,
            };
            items.Add(item);
            
        }

        public static void IconGlowText(string text, Sprite icon, Color spriteColor, Sprite glow) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,
                IconGlow = glow,
                IconColorDefined = true,
                IconColor = spriteColor,
            };
            items.Add(item);
            
        }

        public static void IconText(string text, Color textColor, Sprite icon, Color spriteColor) {
            Item item = new Item {
                Type = ItemType.Text,
                Text = text,
                Icon = icon,

                TextColorDefined = true,
                TextColor = textColor,

                IconColorDefined = true,
                IconColor = spriteColor,
            };
            items.Add(item);
            
        }
        public static void IconGlowText(string text, Color textColor, Sprite icon, Color spriteColor, Sprite glow) {
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
            items.Add(item);
            
        }

        public static void Button(string text, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,
            };
            items.Add(item);
            
        }
        public static void IconButton(string text, Sprite icon, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,
            };
            items.Add(item);
            
        }

        public static void Button(string text, Color color, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                TextColorDefined = true,
                TextColor = color,
            };
            items.Add(item);
            
        }
        public static void IconButton(string text, Color color, Sprite icon, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,

                TextColorDefined = true,
                TextColor = color,
            };
            items.Add(item);
            
        }

        public static void IconButton(string text, Sprite icon, Color iconColor, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,

                IconColorDefined = true,
                IconColor = iconColor,
            };
            items.Add(item);
            
        }

        public static void IconButton(string text, Color color, Sprite icon, Color iconColor, Action button) {
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
            items.Add(item);
            
        }
        public static void IconGlowButton(string text, Sprite icon, Color iconColor, Sprite glow, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                Button = button,

                Icon = icon,
                IconGlow = glow,

                IconColorDefined = true,
                IconColor = iconColor,
            };
            items.Add(item);
            
        }

        public static void IconGlowButton(string text, Color color, Sprite icon, Color iconColor, Sprite glow, Action button) {
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
            items.Add(item);
            
        }


        public static void Button(Func<string> textDynamic, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                TextDynamic = textDynamic,
                Button = button,
            };
            items.Add(item);
            
        }
        public static void Button(string text, Func<string> textDynamic, Action button) {
            Item item = new Item {
                Type = ItemType.Button,
                Text = text,
                TextDynamic = textDynamic,
                Button = button,
            };
            items.Add(item);
            
        }


        public static void Progress(string text, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                Text = text,
                Progress = progress,
            };
            items.Add(item);
            
        }
        public static void Progress(Func<string> textDynamic, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                TextDynamic = textDynamic,
                Progress = progress,
            };
            items.Add(item);
            
        }
        public static void Progress(Idle idle) {
            if (idle == null) {
                return; // Button(" 0", null);
            } else if (idle.Max == 0) {
                Progress(() => $" +{idle.Inc}", () => idle.Progress);
            }
            else {
                Progress(() => $" +{idle.Inc}  {idle.Value}/{idle.Max}", () => idle.Progress);
            }
        }
        public static void Progress(string text, Func<string> textDynamic, Func<float> progress) {
            Item item = new Item {
                Type = ItemType.Progress,
                Text = text,
                TextDynamic = textDynamic,
                Progress = progress,
            };
            items.Add(item);
        }

        public static void Slider(string text, Action<float> slider) {
            Item item = new Item {
                Type = ItemType.Slider,
                Text = text,
                Slider = slider,
            };
            items.Add(item);
        }
        public static void Slider(Func<string> textDynamic, Action<float> slider) {
            Item item = new Item {
                Type = ItemType.Slider,
                TextDynamic = textDynamic,
                Slider = slider,
            };
            items.Add(item);
        }
    }
}
