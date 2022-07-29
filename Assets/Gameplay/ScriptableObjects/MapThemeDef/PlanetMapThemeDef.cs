
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "__PlanetMapThemeDef__", menuName = "创建 PlanetMapThemeDef 星球地图样式定义", order = 1)]
    public class PlanetMapThemeDef : MapThemeDef
    {
        [Header("星球光照")]


        [SerializeField]
        private float dayDuration = 12f;
        public float DayDuration => dayDuration;

        [SerializeField]
        private bool activateLight = true;
        public bool ActivateLight => activateLight;

        [SerializeField]
        private Gradient defaultDaylightGradient;
        public Gradient DefaultDaylightGradient => defaultDaylightGradient;

    }
}
