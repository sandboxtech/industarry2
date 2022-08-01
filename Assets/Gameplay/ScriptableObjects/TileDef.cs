
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

        [Space]

        [Header("相邻动画")]
        [SerializeField]
        private ID bonusAnim;
        public ID BonusAnim => bonusAnim;


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



        [Header("科技")]
        [SerializeField]
        private List<TechDef> techs;
        public List<TechDef> Techs => techs;

    }
}
