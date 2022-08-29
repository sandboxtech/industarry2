
using System.Collections.Generic;
using UnityEngine;

namespace W
{

    [CreateAssetMenu(fileName = "__ResDef__", menuName = "创建 ResDef 资源定义", order = 1)]
    public class ResDef : ID
    {

        [Header("生产间隔")]
        public long del;
        public long DeltaSecond => del == 0 ? 100 : del;
        public long DeltaTicks => DeltaSecond * Constants.Second;

        public void Inspect() {
            Game.I.Map.InspectResPage(this);
        }
    }
}
