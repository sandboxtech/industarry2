
using UnityEditor;
using UnityEngine;

namespace W
{
    [CustomEditor(typeof(GameConfigReference))]
    public class GameConfigReferenceEditor : Editor
	{
        private GameConfigReference gcr;
        public override void OnInspectorGUI() {
            gcr = (GameConfigReference)target;
            if (GUILayout.Button("清理")) {
                gcr.___Clear();
            }
            //if (GUILayout.Button("加工")) {
            //    gcr.___TryLoad();
            //}
            base.OnInspectorGUI();
        }


    }
}
