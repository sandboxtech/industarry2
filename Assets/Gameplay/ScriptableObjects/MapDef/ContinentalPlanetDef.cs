
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "__ContinentalMapDef__", menuName = "MapDef/创建 ContinentalMapDef 地图定义", order = 1)]
    public class ContinentalPlanetDef
        : PlanetMapDef
    {
        public override void ProcessMapTerrain(Map map) {

            int width = map.Width;
            int height = map.Height;

            (bool[,], bool[,]) buffer = CellularAutomataUtility.GetArray(width, height);
            
            float distMax = (width + height) / 2;
            for (int i = 0; i < width; i++) {
                float distX = M.Abs((width - 1) / 2f - i);
                for (int j = 0; j < height; j++) {
                    float distY = M.Abs((height - 1) / 2f - j);
                    float t = (distX + distY) / distMax;

                    bool isEmpty = t < map.TemporaryRandomGenerator.NextDouble();
                    buffer.Item1[i, j] = isEmpty;
                }
            }

            buffer = CellularAutomataUtility.GameCustomized(buffer, (bool source, int count) => {
                if (count > 5) {
                    return true;
                } else {
                    return source;
                }
            }, 3);

            buffer = CellularAutomataUtility.GameCustomized(buffer, (bool source, int count) => {
                if (count > 6) {
                    return true;
                } else {
                    return source;
                }
            }, 2);

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    map.ID(i, j, buffer.Item1[i, j] ? ID.Empty : ID.Invalid);
                }
            }
        }
    }
}
