
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
}
