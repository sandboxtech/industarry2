

namespace W
{
    public static class MapDefName
	{
        public const int SpaceshipLevel = 0;
        public const int PlanetLevel = SpaceshipLevel + 1;
        public const int StarSystemLevel = PlanetLevel + 1;
        public const int GalaxyLevel = StarSystemLevel + 1;
        public const int ClusterLevel = GalaxyLevel + 1;
        public const int UniverseLevel = ClusterLevel + 1;
        public const int MaxMapLevel = UniverseLevel;
        public static MapDef CalcMapDef(uint mapSeed, int mapLevel) {
            MapDef def;
            if (mapLevel >= UniverseLevel) {
                def = GameConfig.I.Name2Obj["Universe"] as MapDef;
            } else {
                switch (mapLevel) {
                    case SpaceshipLevel:
                        def = GameConfig.I.Name2Obj["Spaceship"] as MapDef;
                        break;
                    case PlanetLevel:
                        def = GameConfig.I.Name2Obj["Mars"] as MapDef;
                        break;
                    case StarSystemLevel:
                        def = GameConfig.I.Name2Obj["Stellar"] as MapDef;
                        break;
                    case GalaxyLevel:
                        def = GameConfig.I.Name2Obj["Galaxy"] as MapDef;
                        break;
                    case ClusterLevel:
                        def = GameConfig.I.Name2Obj["Cluster"] as MapDef;
                        break;
                    default:
                        def = null;
                        A.Error($"mapLevel 没有对应的 config : {mapLevel}");
                        break;
                }
            }
            return def;
        }
    }
}
