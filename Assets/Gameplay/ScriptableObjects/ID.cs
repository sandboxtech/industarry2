
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
        public Sprite Sprite => sprite ?? GameConfigReference.I.DefaultSprite;
        public Sprite Icon => Sprite;

        [Header("帧动画")]
        [SerializeField]
        private Sprite[] sprites;
        public Sprite[] Sprites => sprites;
        [SerializeField]
        private float spritesDuration = 1;
        public float SpritesDuration => spritesDuration;

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
