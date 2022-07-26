
using System.Collections.Generic;
using UnityEngine;

namespace W
{

    [CreateAssetMenu(fileName = "__ResDef__", menuName = "创建 ResDef 资源定义", order = 1)]
    public class ResDef : ID
    {

        [Header("生产间隔")]
        public IDValue Del;
        public long DeltaSecond => Del == null ? 10 : Del.Value;
        public long DeltaTicks => DeltaSecond * Constants.Second;
    }
}
