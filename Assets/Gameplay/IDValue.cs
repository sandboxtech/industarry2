
using UnityEngine;

namespace W
{


    //[System.Serializable]
    //public class IDValue
    //{
    //    // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
    //    [Header("数值")]
    //    [SerializeField]
    //    private ID key;
    //    public ID Key => key;

    //    [SerializeField]
    //    private long value;
    //    public long Value => value;

    //}

    [System.Serializable]
    public class ResDefValue
    {
        // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
        [Header("数值")]
        [SerializeField]
        private ResDef key;
        public ResDef Key => key;

        [SerializeField]
        private long value;
        public long Value => value;

        public void AddButton() {
            ID id = Key;
            long v = Value;
            bool positive = v > 0;

            UI.IconGlowButton($"{id.CN} {(positive ? "+" : "-")}{(v > 0 ? v : -v)}",
                positive ? UI.ColorPositive : UI.ColorNegative,
                id.Icon, id.Color, id.Glow, key.Inspect);
        }
        public void AddText(long multiplier) {
            ID id = Key;
            long v = Value;
            long product = v * multiplier;
            bool positive = product > 0;
            UI.IconGlowText($"{id.CN} {(positive ? "+" : "-")}{(v > 0 ? v : -v)}{(multiplier == 1 ? "" : $"*{(multiplier > 0 ? multiplier : -multiplier)}")}",
                UI.ColorNegative,
                id.Icon, id.Color, id.Glow);
        }
        public void AddButton(long multiplier) {
            ID id = Key;
            long v = Value;
            long product = v * multiplier;
            bool positive = product > 0;
            UI.IconGlowButton($"{id.CN} {(positive ? "+" : "-")}{(v > 0 ? v : -v)}{(multiplier == 1 ? "" : $"*{(multiplier > 0 ? multiplier : -multiplier)}")}",
                UI.ColorNegative,
                id.Icon, id.Color, id.Glow, key.Inspect);
        }
    }


    [System.Serializable]
    public class MapDefValue
    {
        // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
        [Header("数值")]
        [SerializeField]
        private MapDef key;
        public MapDef Key => key;

        [SerializeField]
        private long value;
        public long Value => value;

    }

    [System.Serializable]
    public class TileDefValue
    {
        // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
        [Header("数值")]
        [SerializeField]
        private TileDef key;
        public TileDef Key => key;

        [SerializeField]
        private long value;
        public long Value => value;
    }

    [System.Serializable]
    public class TechDefValue
    {
        // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
        [Header("数值")]
        [SerializeField]
        private TechDef key;
        public TechDef Key => key;

        [SerializeField]
        private long value;
        public long Value => value;
    }
}
