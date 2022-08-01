
using System.Collections.Generic;
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "__TechDef__", menuName = "创建 TechDef 科技定义", order = 1)]
    public class TechDef : ID
	{
        [SerializeField]
        private List<ResDefValue> upgrade;
        public List<ResDefValue> Upgrade => upgrade;

        [SerializeField]
        private long multiplier = 10;
        public long Multiplier => multiplier;

        [SerializeField]
        private long max_level = 10;
        public long MaxLevel => max_level;
    }
}
