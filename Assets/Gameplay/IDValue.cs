
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


        public void AddValButton(long multiplier = 1) {
            Key.AddValButton(multiplier * value);
            //ID id = Key;
            //long v = Value;
            //long product = v * multiplier;
            //bool positive = v >= 0;

            //UI.IconGlowButton(positive ? $"{id.CN} -{product}" : $"{id.CN} {-product}",
            //    UI.ColorNormal,
            //    id.Icon, id.Color, id.Glow, key.Inspect);
        }
        public void AddMaxButton(long multiplier = 1) {
            Key.AddMaxButton(multiplier * value);
            //ID id = Key;
            //long v = Value;
            //long product = v * multiplier;
            //bool positive = v >= 0;

            //UI.IconGlowButton((positive ? $"{id.CN} {product}" : $"{id.CN} -{-product}"),
            //    UI.ColorNormal,
            //    id.Icon, id.Color, id.Glow, key.Inspect);
        }

        public void AddIncButton(long multiplier = 1) {
            Key.AddIncButton(multiplier * Value);
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
