
using UnityEngine;

namespace W
{
    public class RandomGenerator : MonoBehaviour
	{
        public static RandomGenerator I => i;
        private static RandomGenerator i;

        private void Awake() {
            A.Singleton(ref i, this);
            random = new System.Random();
        }
        private System.Random random;
        public int Int { get; private set; }
        public uint UInt { get => (uint)Int; }
        public double Double { get; private set; }
        public float Float { get => (float)Double; }
        private void Update() {
            Int = random.Next();
            Double = random.NextDouble();
        }
    }
}
