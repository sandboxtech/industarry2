
using UnityEngine;

namespace W
{
    public class PostProcess : MonoBehaviour
    {
        public static PostProcess I => i;
        private static PostProcess i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }

        private void AwakeInstance() {

        }
    }
}
