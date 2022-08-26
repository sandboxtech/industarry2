
using System.Collections.Generic;
using UnityEngine;

namespace W
{

    [System.Serializable]
    public struct Pair
    {
        public ID ID;
        public long value;
    }


    [CreateAssetMenu(fileName = "__TileDef__", menuName = "创建 TileDef 建筑定义", order = 1)]
    public class TileDef : ID
    {
        public void AddButton() {
            UI.IconButton(CN, Icon, Color, ShowPage);
        }
        public void AddIcon() {
            UI.IconText(CN, Icon);
        }

        private void AddTileInformationIncMax() {

            if (Inc.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Inc);
                foreach (ResDefValue idValue in Inc) {
                    idValue.AddItem();
                }
            }

            if (Max.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Max);
                foreach (ResDefValue idValue in Max) {
                    idValue.AddItem();
                }
            }

            if (IncSuper.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.IncSuper);
                foreach (ResDefValue idValue in IncSuper) {
                    idValue.AddItem();
                }
            }

            if (MaxSuper.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.MaxSuper);
                foreach (ResDefValue idValue in MaxSuper) {
                    idValue.AddItem();
                }
            }
        }

        public void ShowPage() {

            UI.Prepare();

            IconText();

            AddTileInformationIncMax();

            if (Construction.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Construction);
                foreach (ResDefValue idValue in Construction) {
                    idValue.AddItem();
                }
            }

            if (Destruction.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Destruction);
                foreach (ResDefValue idValue in Destruction) {
                    idValue.AddItem();
                }
            }

            if (ConstructionSuper.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.ConstructionSuper);
                foreach (ResDefValue idValue in ConstructionSuper) {
                    idValue.AddItem();
                }
            }

            if (DestructionSuper.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.DestructionSuper);
                foreach (ResDefValue idValue in DestructionSuper) {
                    idValue.AddItem();
                }
            }


            if (NotDestructable) {
                UI.Space();
                SpriteUI.IconText("无法拆除", SpriteUI.Failure);
            }


            if (Techs.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.TechDef);
                foreach (TechDef tech in Techs) {
                    tech.IconText();
                }
            }
             

            if (BonusReverse.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.BonusReverse);
                foreach (TileDef tile in BonusReverse) {
                    tile.IconButton(tile == this ? null : tile.ShowPage);
                }
            }

            if (ConditionsReverse.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.ConditionsReverse);
                foreach (TileDef tile in ConditionsReverse) {
                    tile.IconButton(tile == this ? null : tile.ShowPage);
                }
            }

            if (Bonus.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Bonus);
                foreach (TileDef tile in Bonus) {
                    tile.IconButton(tile == this ? null : tile.ShowPage);
                }
            }

            if (Conditions.Count > 0 || ConditionsSubmap.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Conditions);
                foreach (TileDef tile in Conditions) {
                    tile.IconButton(tile == this ? null : tile.ShowPage);
                }
            }

            if (Repels.Count > 0) {
                UI.Space();
                SpriteUI.IconText(SpriteUI.Repel);
                foreach (TileDef tile in Repels) {
                    tile.IconButton(tile == this ? null : tile.ShowPage);
                }
            }

            if (ConditionsSubmap.Count > 0) {
                UI.Space();
                foreach (MapDef map in ConditionsSubmap) {
                    map.IconText();
                }
            }

            UI.Show();
        }



        [Header("帧动画")]
        [SerializeField]
        private Sprite[] sprites;
        public Sprite[] Sprites => sprites;
        [SerializeField]
        private float spritesDuration = 1;
        public float SpritesDuration => spritesDuration;

        [Header("高光帧动画")]
        [SerializeField]
        private Sprite[] spritesGlow;
        public Sprite[] SpritesGlow => spritesGlow;
        [SerializeField]
        private float spritesGlowDuration = 1;
        public float SpritesGlowDuration => spritesGlowDuration;


        [Header("地块44")]
        [SerializeField]
        private Sprite[] ruleTile44;
        public Sprite[] RuleTile44 => ruleTile44;

        [Header("地块86")]
        [SerializeField]
        private Sprite[] ruleTile86;
        public Sprite[] RuleTile86 => ruleTile86;

        [Header("高光贴图昼夜")]
        [SerializeField]
        private Sprite glowDayNight;
        public Sprite GlowDayNight => glowDayNight;



        [Space]


        [Header("建造成本")]

        [SerializeField]
        private List<ResDefValue> construction; // val
        public IReadOnlyList<ResDefValue> Construction => construction;

        [Header("拆除成本")]
        [SerializeField]
        private List<ResDefValue> destruction; // val
        public IReadOnlyList<ResDefValue> Destruction => destruction;

        [Header("无法拆除")]
        [SerializeField]
        private bool notDestructable;
        public bool NotDestructable => notDestructable;

        [Space]

        [Header("增速影响")]
        [SerializeField]
        private List<ResDefValue> inc; // inc
        public IReadOnlyList<ResDefValue> Inc => inc;

        [Header("容量影响")]
        [SerializeField]
        private List<ResDefValue> max; // max
        public IReadOnlyList<ResDefValue> Max => max;


        //[Header("相邻动画")]
        //[SerializeField]
        //private ID bonusAnim;
        //public ID BonusAnim => bonusAnim;

        [Space]

        [Header("相邻奖励")]
        [SerializeField]
        private List<TileDef> bonus;
        public IReadOnlyList<TileDef> Bonus => bonus;
        public IReadOnlyCollection<TileDef> BonusReverse { get; private set; } = new HashSet<TileDef>();


        [Header("相邻解锁")]
        [SerializeField]
        private List<TileDef> conditions;
        public IReadOnlyList<TileDef> Conditions => conditions;
        public IReadOnlyCollection<TileDef> ConditionsReverse { get; private set; } = new HashSet<TileDef>();


        [Header("相邻禁止")]
        [SerializeField]
        private List<TileDef> repels;
        public IReadOnlyList<TileDef> Repels => repels;



        [Header("地图相邻解锁")]
        [SerializeField]
        private List<MapDef> conditionsSubmap;
        public IReadOnlyList<MapDef> ConditionsSubmap => conditionsSubmap;



        [Header("建造科技要求")]
        [SerializeField]
        private TechDef techRequirementForConstruction;
        public TechDef TechRequirementForConstruction => techRequirementForConstruction;



        [Header("加成科技")]
        [SerializeField]
        private List<TechDef> techs;
        public List<TechDef> Techs => techs;


        [Header("研究科技")]
        [SerializeField]
        private List<TechDef> techsRelavant;
        public List<TechDef> TechsRelavant => techsRelavant;



        [Space]

        [Header("建造成本(上级)")]

        [SerializeField]
        private List<ResDefValue> constructionSuper; // val
        public IReadOnlyList<ResDefValue> ConstructionSuper => constructionSuper;

        [Header("拆除成本(上级)")]
        [SerializeField]
        private List<ResDefValue> destructionSuper; // val
        public IReadOnlyList<ResDefValue> DestructionSuper => destructionSuper;

        [Header("增速影响(上级)")]
        [SerializeField]
        private List<ResDefValue> incSuper; // inc
        public IReadOnlyList<ResDefValue> IncSuper => incSuper;

        [Header("容量影响(上级)")]
        [SerializeField]
        private List<ResDefValue> maxSuper; // max
        public IReadOnlyList<ResDefValue> MaxSuper => maxSuper;
    }
}
