
using System.Collections.Generic;
using UnityEngine;

namespace W
{


    [CreateAssetMenu(fileName = "__MapDefTheme__", menuName = "创建 MapDefTheme 地图样式定义", order = 1)]
    public class MapThemeDef : ID
    {
        [Space]

        [Header("地面纹理")]
        [SerializeField]
        private Sprite[] tileset;
        public Sprite[] Tileset => tileset;


        [Space]


        [Header("背景颜色")]
        [SerializeField]
        private Color backgroundColor;
        public Color BackgroundColor => backgroundColor;

        [Header("背景纹理")]
        [SerializeField]
        private Sprite background;
        public Sprite Background => background;

        [Header("背景材料")]
        [SerializeField]
        private Material backgroundMaterial;
        public Material BackgroundMaterial => backgroundMaterial;



        [Space]

        [Header("地面颜色")]
        [SerializeField]
        private Color groundColor;
        public Color GroundColor => groundColor;

        [Header("地面纹理")]
        [SerializeField]
        private Sprite ground;
        public Sprite Ground => ground;

        [Header("背景材料")]
        [SerializeField]
        private Material groundMaterial;
        public Material GroundMaterial => groundMaterial;


    }
}
