
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace W
{
    public abstract class MapLike : IPersistent
    {
        public abstract void OnCreate();
    }


    public interface IMap
    {
        public void _Init(Map previousMap, uint index);
    }
    /// <summary>
    /// 地图文件
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Map : MapLike, IMap
    {


        public bool HasPosition(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        private int IndexOf(int x, int y) => x * Height + y;

        [JsonProperty]
        private uint[] ids; // 对应位置的建筑类型
        public uint ID(int x, int y) => ids[IndexOf(x, y)];
        public void ID(int x, int y, uint value) => ids[IndexOf(x, y)] = value;
        public uint ID_Safe(int x, int y) {
            if (!HasPosition(x, y)) {
                return W.ID.Invalid;
            }
            return ID(x, y);
        }

        [JsonProperty]
        private int[] levels; // 此建筑的等级。受到相邻和科技影响
        public int Level(int x, int y) => levels[IndexOf(x, y)];
        public void Level(int x, int y, int value) => levels[IndexOf(x, y)] = value;



        [JsonProperty]
        private Dictionary<uint, Idle> resources; // 地图的资源
        public Dictionary<uint, Idle> Resources => resources;


        public override void OnCreate() {
            resources = new Dictionary<uint, Idle>();
        }



        [JsonProperty]
        public int Width;
        [JsonProperty]
        public int Height;
        [JsonIgnore]
        private int Size => Width * Height;

        [JsonIgnore]
        public System.Random RandomGenerator { get; set; }
        [JsonIgnore]
        public (bool[,], bool[,]) Buffer { get; set; }


        [JsonProperty]
        private uint mapDefID = W.ID.Empty;
        [JsonIgnore]
        private MapDef mapDef;

        public bool NotInitialized => mapDefID == W.ID.Empty;

        public MapDef Def {
            get {
                if (mapDef == null) {
                    mapDef = GameConfig.I.ID2Obj[mapDefID] as MapDef;
                    A.Assert(mapDef != null);
                }
                return mapDef;
            }
        }

        [JsonProperty]
        private uint previousSeed;
        public uint PreviousSeed => previousSeed;

        [JsonProperty]
        private uint seed;
        public uint Seed => seed;



        private const string InitialPlanetMapDefConfigName = "Continental";
        private const string SpaceshipConfigName = "Spaceship";

        void IMap._Init(Map previousMap, uint index) {
            previousSeed = previousMap == null ? 0 : previousMap.seed;
            if (previousMap == null) {
                if (index == Game.SpaceShipIndex) {
                    seed = Game.SpaceShipIndex;
                    mapDef = GameConfig.I.Name2Obj[SpaceshipConfigName] as MapDef;
                }
                else if (index == Game.PlanetMapIndex) {
                    seed = Game.PlanetMapIndex;
                    mapDef = GameConfig.I.Name2Obj[InitialPlanetMapDefConfigName] as MapDef;
                }
                else {
                    A.Error();
                }
            }
            else {
                seed = H.Hash(previousSeed, index);
                mapDef = previousMap.Def;
            }
            mapDefID = mapDef.id;
            InitializeSize();

            UnityEngine.Debug.Log($"init {seed} {previousSeed}");

            MapUI.I.TryConstructInitials(this);
        }


        private void InitializeSize() {
            Width = mapDef.Width;
            Height = mapDef.Height;
            CameraX = (float)Width / 2;
            CameraY = (float)Height / 2;

            A.Assert(Width > 0 && Width < 0b1111111111);
            A.Assert(Height > 0 && Height < 0b1111111111);
            int squareSize = Size;
            ids = new uint[squareSize];
            levels = new int[squareSize];
        }



        [JsonProperty]
        private int CameraZoom = 4;
        [JsonProperty]
        private float CameraX;
        [JsonProperty]
        private float CameraY;
        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
            LoadCameraPosition();
            CameraControl.I.Zoom = CameraZoom;
        }
        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            SaveCameraPosition();
            CameraZoom = CameraControl.I.Zoom;
        }

        public void LoadCameraPosition() {
            CameraControl.I.Position = new UnityEngine.Vector2(CameraX, CameraY);
        }

        public void SaveCameraPosition() {
            UnityEngine.Vector2 pos = CameraControl.I.Position;
            CameraX = pos.x;
            CameraY = pos.y;
        }


        public static bool CanChange(IDValue idValue, long multiplier, IdleReference i) {
            ResDef resDef = idValue.Key as ResDef;
            A.Assert(resDef != null);
            uint resDefID = resDef.id;
            if (multiplier * idValue.Value == 0) {
                return true;
            } else if (Game.I.Map.Resources.TryGetValue(resDefID, out Idle idle)) {
                long idleValue = long.MinValue;
                switch (i) {
                    case IdleReference.Val:
                        idleValue = idle.Value;
                        break;
                    case IdleReference.Inc:
                        idleValue = idle.Inc;
                        break;
                    case IdleReference.Max:
                        idleValue = idle.Max;
                        break;
                    default:
                        A.Assert(false);
                        break;
                }
                if (idleValue + idValue.Value * multiplier < 0) {
                    // 建造资源不够
                    return false;
                }
                return true;
            } else {
                return multiplier * idValue.Value > 0;
            }
        }
        public static void DoChange(IDValue idValue, long multiplier, IdleReference i) {
            ResDef resDef = idValue.Key as ResDef;
            A.Assert(resDef != null);
            uint resDefID = resDef.id;
            if (multiplier == 0) {
                return;
            } else {
                if (!Game.I.Map.Resources.TryGetValue(resDefID, out Idle idle)) {
                    long del = resDef.DeltaTicks;
                    idle = new Idle(0, 0, del, 0);
                    Game.I.Map.Resources.Add(resDefID, idle);
                }
                switch (i) {
                    case IdleReference.Val:
                        idle.Value += multiplier * idValue.Value;
                        break;
                    case IdleReference.Inc:
                        idle.Inc += multiplier * idValue.Value;
                        break;
                    case IdleReference.Max:
                        idle.Max += multiplier * idValue.Value;
                        break;
                    default:
                        A.Assert(false);
                        break;
                }
                return;
            }
        }

        private readonly static HashSet<ID> existingRes = new HashSet<ID>();
        public static void AddRelatedResDefValue(TileDef tileDef) {
            existingRes.Clear();
            foreach (IDValue idValue in tileDef.Inc) {
                if (!existingRes.Contains(idValue)) {
                    existingRes.Add(idValue.Key);
                }
            }
            foreach (IDValue idValue in tileDef.Max) {
                if (!existingRes.Contains(idValue)) {
                    existingRes.Add(idValue.Key);
                }
            }

            foreach (ID key in existingRes) {
                if (!Game.I.Map.Resources.TryGetValue(key.id, out Idle idle)) {
                    continue;
                }
                UI.Space();
                UI.IconText(key.CN, key.Icon, Game.Color(key));
                UI.Progress(() => $"{idle.Value}/{idle.Max}  +{idle.Inc}/{idle.DelSecond}s", () => idle.Progress);
            }

            existingRes.Clear();
        }

    }

    public enum IdleReference
    {
        Val, Inc, Max
    }
}


