
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace W
{

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MapBody
    {
        [JsonProperty]
        private uint[] ids;
        public uint[] IDs => ids; // 对应位置的建筑类型

        [JsonProperty]
        private int[] levels;
        public int[] Levels => levels; // 此建筑的等级。受到相邻和科技影响

        public void Init(int width, int height) {
            A.Assert(ids == null && levels == null);
            int size = width * height;
            ids = new uint[size];
            levels = new int[size];
        }

    }

    /// <summary>
    /// 地图文件
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Map
    {

        public bool HasPosition(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        private int IndexOf(int x, int y) => x * Height + y;


        [JsonIgnore]
        private MapBody body;
        public MapBody Body => body;

        public void OnEnter() {
            if (body == null) {
                LoadBody();
            }
            if (body == null) {
                body = new MapBody();
                body.Init(Width, Height);
                MapUI.I.TryConstructInitials(this);
            }
            MapDataView.EnterMap(this);
        }
        private void ConstructInitials() {

        }


        //[JsonProperty]
        //private uint[] ids; // 对应位置的建筑类型
        public uint ID(int x, int y) => body.IDs[IndexOf(x, y)];
        public void ID(int x, int y, uint value) => body.IDs[IndexOf(x, y)] = value;

        public uint ID_Safe(int x, int y) {
            if (!HasPosition(x, y)) {
                return W.ID.Invalid;
            }
            return ID(x, y);
        }

        //[JsonProperty]
        //private int[] levels; // 此建筑的等级。受到相邻和科技影响
        public int Level(int x, int y) => body.Levels[IndexOf(x, y)];
        public void Level(int x, int y, int value) => body.Levels[IndexOf(x, y)] = value;




        [JsonProperty]
        private Dictionary<uint, Idle> resources; // 地图的资源
        public Dictionary<uint, Idle> Resources => resources;


        [JsonProperty]
        public int Width;
        [JsonProperty]
        public int Height;


        [JsonIgnore]
        public System.Random TemporaryRandomGenerator { get; set; }

        [JsonProperty]
        private uint mapDefID = W.ID.Empty;
        [JsonIgnore]
        private MapDef mapDef;


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
        private int mapLevel = 0;
        public int MapLevel => mapLevel;

        [JsonProperty]
        private int previousMapLevel = 0;
        public int PreviousMapLevel => previousMapLevel;


        [JsonProperty]
        private uint seed;
        public uint Seed => seed;

        [JsonProperty]
        private uint previousSeed;
        public uint PreviousSeed => previousSeed;




        public string Key => KeyOf(Seed, MapLevel);
        public static string KeyOf(uint seed, int mapLevel) => $"{seed}_{mapLevel}";

        public const uint SuperMapIndex = uint.MaxValue;

        public const uint NULLPreviousMap = 0;
        public bool NoPreviousMap => previousSeed == NULLPreviousMap;
        public void LoadPrevious(out Map map) {
            if (NoPreviousMap) {
                map = null;
                return;
            }

            LoadWithSeedAndLevel(previousSeed, previousMapLevel, out map);
        }

        public void LoadNext(uint index, out Map nextMap) {
            uint seed = Seed;
            int nextLevel = index == SuperMapIndex ? mapLevel + 1 : mapLevel - 1;
            if (nextLevel >= MapDefName.MaxMapLevel) {
                nextLevel = MapDefName.MaxMapLevel;
            }
            uint nextSeed = H.Hash(seed);
            LoadWithSeedAndLevel(nextSeed, nextLevel, out nextMap);
        }
        private void LoadWithSeedAndLevel(uint nextSeed, int nextLevel, out Map nextMap) {
            string key = KeyOf(nextSeed, nextLevel);
            Load(key, out nextMap);
            if (nextMap == null) {
                nextMap = new Map();
                nextMap.seed = nextSeed;
                nextMap.mapLevel = nextLevel;
                nextMap.previousSeed = seed;
                nextMap.previousMapLevel = mapLevel;

                nextMap.Init();
            }
        }

        public void LoadWithLevel(int mapLevel) {
            A.Assert(seed == 0 && mapDefID == 0);
            this.mapLevel = mapLevel;

            Init();
        }

        private void Init() {
            A.Assert(resources == null);
            resources = new Dictionary<uint, Idle>();

            mapDef = MapDefName.CalcMapDef(seed, mapLevel);
            mapDefID = mapDef.id;
            InitializeSize();
        }



        private void InitializeSize() {
            Width = mapDef.Width;
            Height = mapDef.Height;
            CameraX = (float)Width / 2;
            CameraY = (float)Height / 2;

            A.Assert(Width > 0 && Width < 0b1111111111);
            A.Assert(Height > 0 && Height < 0b1111111111);
        }



        public void Save(out string key) {
            key = Key;
            GameLoop.Save(key, this);
        }
        public void SaveBody() {
            GameLoop.Save(Key, body);
        }

        public static void Load(string key, out Map map) {
            GameLoop.Load(key, out map);
        }
        public void LoadBody() {
            A.Assert(body == null);
            GameLoop.Load(Key, out body);
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
                if (!existingRes.Contains(idValue.Key)) {
                    existingRes.Add(idValue.Key);
                }
            }
            foreach (IDValue idValue in tileDef.Max) {
                if (!existingRes.Contains(idValue.Key)) {
                    existingRes.Add(idValue.Key);
                }
            }

            foreach (ID key in existingRes) {
                if (!Game.I.Map.Resources.TryGetValue(key.id, out Idle idle)) {
                    continue;
                }
                UI.Space();
                UI.IconText(key.CN, key.Icon, key.Color);
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


