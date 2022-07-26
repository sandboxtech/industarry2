
using UnityEngine;

namespace W
{
    public interface _IDValue
    {
        public void SetValue(long value);
    }

    [CreateAssetMenu(fileName = "__IDValue__", menuName = "创建 IDValue 数值", order = 1)]
    public class IDValue : ID, _IDValue
    {
        // [UnityEngine.Serialization.FormerlySerializedAs("ResDef")]
        [Header("数值")]
        [SerializeField]
        private ID key;
        public ID Key => key;

        [SerializeField]
        private long value;
        public long Value => value;

        void _IDValue.SetValue(long value) {
            this.value = value;
        }
    }
}
