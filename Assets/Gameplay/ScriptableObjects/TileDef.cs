
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
        private List<IDValue> construction; // val
        public IReadOnlyList<IDValue> Construction => construction;

        [Header("拆除成本")]
        [SerializeField]
        private List<IDValue> destruction; // val
        public IReadOnlyList<IDValue> Destruction => destruction;


        [Space]

        [Header("增速影响")]
        [SerializeField]
        private List<IDValue> inc; // inc
        public IReadOnlyList<IDValue> Inc => inc;

        [Header("容量影响")]
        [SerializeField]
        private List<IDValue> max; // max
        public IReadOnlyList<IDValue> Max => max;


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
