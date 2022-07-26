
using System.Collections.Generic;
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "__MapDef__", menuName = "创建 MapDef 地图定义", order = 1)]
    public class MapDef : ID
    {
        [Header("是否是星球")]
        [SerializeField]
        private bool isPlanet;
        public bool IsPlanet => isPlanet;

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


        [Header("破碎海岸")]
        [SerializeField]
        private int edge;
        public int Edge => edge == 0 ? (M.Min(Width, Height) / 4) : edge;




        [Header("初始随机建筑")]
        [SerializeField]
        private List<IDValue> initialRandomStructures;
        public List<IDValue> InitialRandomStructures => initialRandomStructures;

        [Header("测试：可造建筑")]
        [SerializeField]
        private List<TileDef> constructables;
        public IReadOnlyList<TileDef> Constructables => constructables;

    }
}
