
using System;
using System.Collections.Generic;
using UnityEngine;

namespace W
{
    public interface __ID
    {
        public void SetID(uint id);
    }


    [CreateAssetMenu(fileName = "__ID__", menuName = "创建 ID 定义", order = 1)]
    public class ID : ScriptableObject, __ID
    {
        public static bool IsInvalid(uint id) => id == Empty || id == Invalid;
        public const uint Invalid = uint.MaxValue;
        public const uint Empty = 0;
        public uint id { get; private set; }
        void __ID.SetID(uint id) {
            this.id = id;
        }

        public void IconText() => UI.IconGlowText(CN, Icon, Color, Glow);
        public void IconButton(Action action) => UI.IconGlowButton(CN, Icon, Color, Glow, action);
        public void IconTextWithLevel(int level) => UI.IconGlowText(NameAndLevel(level), Icon, Color, Glow);
        public void IconTextWithNumber(long number) => UI.IconGlowText(number > 0 ? $"{CN} +{number}" : $"{CN} {number}", number > 0 ? UI.ColorPositive : UI.ColorNegative, Icon, Color, Glow);

        public void IconButtonWithLevel(int level, Color textColor, Action action)
            => UI.IconGlowButton(NameAndLevel(level), textColor, Icon, Color, Glow, action);
        private string NameAndLevel(int level) => level <= 1 ? CN : $"{CN} * {level}";




        [Header("中文")]
        [SerializeField]
        private string cn;
        public string CN => string.IsNullOrEmpty(cn) ? name : cn;

        //[Header("描述")]
        //[SerializeField]
        //private string description;
        //public string Description => description;

        [Header("贴图")]
        [SerializeField]
        private Sprite sprite;
        public Sprite Sprite => sprite != null ? sprite : GameConfigReference.I.DefaultSprite;
        public Sprite Icon {
            get {
                if (sprite != null) return sprite;
                if (this is MapDef mapDef && mapDef.Sprites != null && mapDef.Sprites.Length > 0) {
                    return mapDef.Sprites[0];
                }
                return GameConfigReference.I.DefaultSprite;
            }
        }



        [Header("高光贴图")]
        [SerializeField]
        private Sprite glow;
        public Sprite Glow => glow;

        [Header("颜色")]
        [SerializeField]
        private Color color = Color.white;
        public Color Color => color;

    }
}
