
using UnityEngine;

namespace W
{

    [ExecuteAfter(typeof(UI))]
    public class GameEntry : MonoBehaviour
    {
        private void Awake() {
            GameLoop.Awake();
        }

        private void Start() {
            GameLoop.Start();
        }

        private void Update() {
            GameLoop.Update();
        }

        [ContextMenu("清除存档")]
        private void ClearSaves() {
            Persistence.ClearSaves();
        }
    }
}
