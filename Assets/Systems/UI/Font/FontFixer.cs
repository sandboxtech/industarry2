
using UnityEngine;

namespace W {
	public class FontFixer : MonoBehaviour
	{
        private void Awake() {
			Fix();
        }
        public Font FontToFix;

		[ContextMenu("Fix")]
		private void Fix() {
			FontToFix.material.mainTexture.filterMode = FilterMode.Point;
        }
	}
}
