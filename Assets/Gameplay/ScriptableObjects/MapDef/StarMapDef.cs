
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "__StarMapDef__", menuName = "MapDef/创建 StarMapDef 地图定义", order = 1)]
    public class StarMapDef : MapDef
	{
        [Header("恒星质量")]
        [SerializeField]
        private long mass;
        public long Mass => mass;
    }
}
