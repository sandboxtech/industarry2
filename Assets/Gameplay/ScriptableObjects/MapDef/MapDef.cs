
using System.Collections.Generic;
using UnityEngine;

namespace W
{
    //public enum StarType
    //{
    //    None,
    //}
    //public enum MapType
    //{
    //    None, 

    //    Galaxy,
    //    Sector,
    //    StarSystem,

    //    Continental,

    //    Asteroid,

    //    Wet,
    //    Dry,
    //    Cold,


    //    Gaia,
    //    Tomb,

    //    Molten,
    //    Barren,
    //    Frozen,

    //    Artificial,
    //    RingWorld,

    //    SpaceShip,
    //    SpaceStation,

    //    GasGaint,
    //    Star,
    //    NeutronStar,
    //    BlackHole,
    //}

    [System.Serializable]
    public class MapDefOrbit
    {
        
        [Header("类型")]
        [SerializeField]
        private MapDef key;
        public MapDef Key => key;

        [Header("轨道半径")]
        [SerializeField]
        private int radius;
        public int Radius => radius;

        [Header("概率")]
        [SerializeField]
        private double possiblity;
        public double Possibility => possiblity == 0 ? 1 : possiblity;

        [Header("重复次数")]
        [SerializeField]
        private long repeat;
        public long Repeat => repeat == 0 ? 1 : repeat;

        [Header("卫星")]
        [SerializeField]
        private MapDef satellite;
        public MapDef Satellite => satellite;

        [Header("卫星概率")]
        [SerializeField]
        private double satellitePossibility;
        public double SatellitePossibility => satellitePossibility == 0 ? 1 : possiblity;


    }


    [CreateAssetMenu(fileName = "__MapDef__", menuName = "创建 MapDef 地图定义", order = 1)]
    public class MapDef : ID
    {
        [Header("帧动画")]
        [SerializeField]
        private Sprite[] sprites;
        public Sprite[] Sprites => sprites;
        [SerializeField]
        private float spritesDuration = 1;
        public float SpritesDuration => spritesDuration;



        [Header("无法进入")]
        [SerializeField]
        private bool notAccessible;
        public bool NotAccessible => notAccessible;


        [Header("相对论效应")]
        [SerializeField]
        private int relativityTimeScale = 1;
        public int TimeScale => relativityTimeScale < 0 ? 1 : relativityTimeScale > 100 ? 100 : relativityTimeScale;


        [Header("地图样式")]
        [SerializeField]
        private MapThemeDef mapThemeDef;
        public MapThemeDef Theme => mapThemeDef;

        [Header("地图宽度")]
        [SerializeField]
        private int width;
        [Header("地图长度")]
        [SerializeField]
        private int height;

        public int Width => Validate(width);
        public int Height => Validate(height);

        private static int Validate(int size) => size <= 0 ? 16 : size >= 1024 ? 1024 : size;


        [Header("进入科技要求")]
        [SerializeField]
        private List<TechDef> techRequirementForEntrence;
        public IReadOnlyList<TechDef> TechRequirementForEntrence => techRequirementForEntrence;






        [Header("上级地图类型")]
        [SerializeField]
        private MapDef superMapDef;
        public MapDef SuperMapDef => superMapDef;
        [Header("下级地图轨道半径")]
        [SerializeField]
        private uint previousMapOrbitRadius;
        public uint PreviousMapOrbitRadius => previousMapOrbitRadius == 0 ? 6 : previousMapOrbitRadius;

        [Header("轨道定义")]
        [SerializeField]
        private List<MapDefOrbit> mapDefOrbits;
        public IReadOnlyList<MapDefOrbit> MapDefOrbits => mapDefOrbits;




        [Header("初始随机建筑")]
        [SerializeField]
        private List<TileDefValue> initialRandomStructures;
        public IReadOnlyList<TileDefValue> InitialRandomStructures => initialRandomStructures;


        [Header("无条件可造建筑")]
        [SerializeField]
        private List<TileDef> constructables;
        public IReadOnlyList<TileDef> Constructables => constructables;

        [System.NonSerialized]
        private List<TileDef> paidConstructables;
        [System.NonSerialized]
        private List<TileDef> freeConstructables;
        [System.NonSerialized]
        private bool bothConstructablesClassified = false;
        public IReadOnlyList<TileDef> ConstructablesPaid {
            get {
                if (!bothConstructablesClassified) Classify();
                return paidConstructables;
            }
        }
        public IReadOnlyList<TileDef> ConstructablesFree {
            get {
                if (!bothConstructablesClassified) Classify();
                return freeConstructables;
            }
        }
        private void Classify() {
            paidConstructables = new List<TileDef>();
            freeConstructables = new List<TileDef>();
            foreach (var item in Constructables) {
                if (item == null) A.Assert(false);
                if (item.ConditionsSubmap.Count > 0) {
                    continue;
                }
                else if (!item.IsFreeOrPaid) {
                    paidConstructables.Add(item);
                }
                else {
                    freeConstructables.Add(item);
                }
            }
            bothConstructablesClassified = true;
        }


        // 周边可造建筑。自动生成属性
        public IReadOnlyCollection<TileDef> ConditionsSubmapReverse { get; private set; } = new HashSet<TileDef>();



        public virtual void ProcessMapTerrain(Map map) {

        }
    }
}
