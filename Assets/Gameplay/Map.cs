
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace W
{

    /// <summary>
    /// 地块信息记录
    /// 数组体积 16 * width * height 字节
    /// </summary>
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
            A.Assert(size > 0);
            ids = new uint[size];
            levels = new int[size];
        }
    }

    /// <summary>
    /// 地图
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Map
    {

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
        private uint previousMapDefID = 0;
        public uint PreviousMapDefID => previousMapDefID;

        [JsonProperty]
        private int mapLevel = 0;
        public int MapLevel => mapLevel;


        [JsonProperty]
        private int previousMapLevel = int.MaxValue;
        public int PreviousMapLevel => previousMapLevel;


        /// <summary>
        /// 地图种子，来自于上个地图哈希值
        /// 用于此地图内容生成，以及之后前往的地图的种子
        /// </summary>
        [JsonProperty]
        private uint seed;
        public uint Seed => seed;

        /// <summary>
        /// 上个地图的种子
        /// 用于返回上个地图
        /// </summary>
        [JsonProperty]
        private uint previousSeed;
        public uint PreviousSeed => previousSeed;



        /// <summary>
        /// 一个地图由种子和地图等级确定
        /// 若同等级地图种子哈希冲突，可能产生虫洞效应
        /// </summary>
        public string Key => KeyOf(Seed, MapLevel);
        public static string KeyOf(uint seed, int mapLevel) => $"{seed}_{mapLevel}";

        public const uint SuperMapIndex = uint.MaxValue;


        public const uint NullSeed = 0;
        public bool NoPreviousMap => previousSeed == NullSeed;


        /// <summary>
        /// 加载前一个地图
        /// </summary>
        public void LoadPrevious(out Map map) {
            if (NoPreviousMap) {
                map = null;
                return;
            }
            LoadWithSeedAndLevel(SuperMapIndex, W.ID.Invalid, previousSeed, previousMapLevel, out map);
        }

        public void LoadSuper(out Map map) => LoadMap(SuperMapIndex, W.ID.Invalid, out map);
        /// <summary>
        /// 加载后一个地图，index决定位置。SuerMapIndex代表加载高级地图
        /// </summary>
        public void LoadMap(uint index, uint mapDefID, out Map map) {
            uint seed = Seed;
            int nextLevel = index == SuperMapIndex ? mapLevel + 1 : mapLevel - 1;
            if (nextLevel >= MapDefName.MaxMapLevel) {
                nextLevel = MapDefName.MaxMapLevel;
            }
            uint nextSeed = H.Hash(seed, index);
            if (nextSeed == NullSeed) H.Hashed(ref nextSeed, index); // in case hash collision to 0

            LoadWithSeedAndLevel(index, mapDefID, nextSeed, nextLevel, out map);
        }

        /// <summary>
        /// 先读取地图文件，如果没有，则内存创建
        /// </summary>
        private void LoadWithSeedAndLevel(uint index, uint mapDefID, uint seed, int mapLevel, out Map map) {
            string key = KeyOf(seed, mapLevel);
            Load(key, out map);
            if (map == null) {
                map = new Map {
                    seed = seed,
                    mapLevel = mapLevel,
                    previousSeed = this.seed,
                    previousMapLevel = this.mapLevel,
                    previousMapDefID = this.mapDefID,

                    mapDef = index == SuperMapIndex ? Def.SuperMapDef : W.ID.IsInvalid(mapDefID) ? null : (GameConfig.I.ID2Obj[mapDefID] as MapDef),
                };
                map.Init();
            }
        }

        /// <summary>
        /// 内存创建。只有飞船地图和母星地图用到了
        /// </summary>
        public static Map Create(uint seed, int mapLevel) {
            A.Assert(seed != NullSeed);

            Map map = new Map {
                seed = seed,
                mapLevel = mapLevel,
                previousSeed = NullSeed,
            };
            map.Init();
            return map;
        }


        /// <summary>
        /// 初始化：资源，地图定义
        /// </summary>
        private void Init() {
            A.Assert(resources == null);
            resources = new Dictionary<uint, Idle>();
            tileDefCountOnThisMap = new Dictionary<uint, int>();
            tileDefCountOnSubmap = new Dictionary<uint, int>();

            if (mapDef == null) {
                mapDef = MapDefName.CalcMapDef(seed, mapLevel);
            }
            mapDefID = mapDef.id;
            InitializeSize();
        }



        #region size
        [JsonProperty]
        public int Width;
        [JsonProperty]
        public int Height;
        /// <summary>
        /// 初始化地图大小
        /// </summary>
        private void InitializeSize() {
            Width = mapDef.Width;
            Height = mapDef.Height;
            CameraX = (float)Width / 2;
            CameraY = (float)Height / 2;

            A.Assert(Width > 0 && Width < 0b1111111111);
            A.Assert(Height > 0 && Height < 0b1111111111);
        }
        #endregion





        #region body
        [JsonIgnore]
        private MapBody body;
        public MapBody Body => body;


        public void OnExit() {
            SaveCameraPosition();
        }

        /// <summary>
        /// 进入地图时进行处理
        /// </summary>
        public void OnEnter() {
            if (body == null) {
                LoadBody();
            }
            if (body == null) {
                body = new MapBody();
                body.Init(Width, Height);
                TryConstructInitials();
            }
            LoadCameraPosition();
            Game.I.Relativity.TimeScale = Def.TimeScale;
            MapDataView.EnterMap(this);
        }
        /// <summary>
        /// 离开地图时，保存相机的位置和缩放。下次来到地图时，读取
        /// </summary>
        [JsonProperty]
        private int CameraZoom = 5;
        [JsonProperty]
        private float CameraX;
        [JsonProperty]
        private float CameraY;

        public void LoadCameraPosition() {
            CameraControl.I.Position = new UnityEngine.Vector2(CameraX, CameraY);
            CameraControl.I.Zoom = CameraZoom;
        }

        public void SaveCameraPosition() {
            UnityEngine.Vector2 pos = CameraControl.I.Position;
            CameraX = pos.x;
            CameraY = pos.y;
            CameraZoom = CameraControl.I.Zoom;
        }


        [JsonIgnore]
        public System.Random TemporaryRandomGenerator { get; set; }
        private void TryConstructInitials() {
            TemporaryRandomGenerator = new System.Random((int)Seed);
            Def.ProcessMapTerrain(this);
            TryConstructPreviousMapIndex();
            TryConstructSubMapPortals();
            TryConstructInitialStructures();
        }

        [JsonProperty]
        private uint previousMapIndex = uint.MaxValue;
        public uint PreviousMapIndex => previousMapIndex;

        public const int MysteriousRadius = 8;
        private void TryConstructPreviousMapIndex() {
            if (previousMapLevel > mapLevel) return;
            RandomOrbitPositionType1((int)mapDef.PreviousMapOrbitRadius, mapDef.Width / 2, mapDef.Height / 2, out int x, out int y);
            uint index = IndexOf(x, y);
            previousMapIndex = index;
        }


        [JsonProperty]
        private Dictionary<uint, uint> subMaps;
        public IReadOnlyDictionary<uint, uint> SubMaps => subMaps;

        /// <summary>
        /// 构建前往其他地图的传送门。比如星球。无法被拆除
        /// </summary>
        private void TryConstructSubMapPortals() {
            int width = mapDef.Width;
            int height = mapDef.Height;
            int centerX = width / 2;
            int centerY = height / 2;

            foreach (MapDefOrbit orbit in mapDef.MapDefOrbits) {
                for (int i = 0; i < orbit.Repeat; i++) {
                    bool hasOrbit = orbit.Possibility == 1 || TemporaryRandomGenerator.NextDouble() < orbit.Possibility;
                    if (!hasOrbit) return;

                    if (subMaps == null) subMaps = new Dictionary<uint, uint>();

                    int radius = orbit.Radius;
                    A.Assert(radius < centerX && radius < centerY);

                    RandomOrbitPositionRound(radius, centerX, centerY, out int x, out int y);
                    uint index = IndexOf(x, y);
                    if (index == previousMapIndex) return;

                    if (subMaps.ContainsKey(index)) continue;
                    subMaps.Add(index, orbit.Key.id);

                    if (orbit.Satellite != null && (TemporaryRandomGenerator.NextDouble()) < orbit.SatellitePossibility) {
                        uint satellite = RandomSatellitePosition(x, y);
                        if (!subMaps.ContainsKey(satellite)) {
                            subMaps.Add(satellite, orbit.Satellite.id);
                        }
                    }
                }
            }
        }

        private void RandomOrbitPositionType1(int radius, int centerX, int centerY, out int x, out int y) {
            int randomX = TemporaryRandomGenerator.Next(radius + 1);
            int randomY = radius - randomX;
            int sign = TemporaryRandomGenerator.Next(0, 3);
            if (sign > 2) { randomX = -randomX; }
            if (sign % 2 == 0) { randomY = -randomY; }

            x = centerX + randomX;
            y = centerY + randomY;
        }
        private void RandomOrbitPositionRound(int radius, int centerX, int centerY, out int x, out int y) {
            double angle = TemporaryRandomGenerator.NextDouble() * 2 * System.Math.PI;
            int randomX = (int)(radius * System.Math.Cos(angle));
            int randomY = (int)(radius * System.Math.Sin(angle));
            x = centerX + randomX;
            y = centerY + randomY;
        }

        private uint RandomSatellitePosition(int x, int y) {
            switch (TemporaryRandomGenerator.Next(0, 4)) {
                case 0:
                    return IndexOf(x + 1, y);
                case 1:
                    return IndexOf(x - 1, y);
                case 2:
                    return IndexOf(x, y + 1);
                case 3:
                    return IndexOf(x, y - 1);
                default:
                    return IndexOf(x, y);
            }
        }

        private void TryConstructInitialStructures() {
            A.Assert(Game.I.ThisMap == this);
            MapTileInfo info = new MapTileInfo();
            info.Map = this;
            info.MapSuper = Game.I.SuperMap;

            MapDef mapDef = Def;
            if (mapDef.InitialRandomStructures == null || mapDef.InitialRandomStructures.Count == 0) return;

            int width = mapDef.Width;
            int height = mapDef.Height;
            int square = width * height;

            info.PlayerBuild_OrAutoBuild = false;

            foreach (TileDefValue pair in mapDef.InitialRandomStructures) {

                if (pair.Key is TileDef tileDef) {

                    long times = 0;
                    long times_success = 0;

                    while (times_success < pair.Value && times < square) {
                        info.X = TemporaryRandomGenerator.Next(width);
                        info.Y = TemporaryRandomGenerator.Next(height);

                        uint id = ID(info.X, info.Y);
                        if (id == W.ID.Empty) {
                            info.TileDef = tileDef;
                            MapUI.CalcLevel(info);
                            if (MapUI.TryConstruct(info)) {
                                times_success++;
                            } else {
                            }
                        }

                        times++;
                    }
                }
            }
        }


        public bool HasPosition(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public uint IndexOf(int x, int y) => (uint)(x * Height + y);

        //[JsonProperty]
        //private uint[] ids; // 对应位置的建筑类型
        public uint ID(int x, int y) {
            return body.IDs[IndexOf(x, y)];
        }
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


        #endregion






        #region serialization
        /// <summary>
        /// 存档
        /// </summary>
        public void Save(out string key) {
            key = Key;
            GameLoop.Save(key, this);
        }
        public void SaveBody() {
            GameLoop.Save(Key, body);
        }

        /// <summary>
        /// 读档
        /// </summary>
        public static void Load(string key, out Map map) {
            GameLoop.Load(key, out map);
        }
        public void LoadBody() {
            A.Assert(body == null);
            GameLoop.Load(Key, out body);
        }



        #endregion


        #region techs

        public bool CanEnter() {
            foreach (TechDef techDef in Def.TechRequirementForEntrence) {
                Game.I.TechLevel(techDef.id, out int level);
                if (level == 0) {
                    return false;
                }
            }
            return true;
        }

        #endregion


        #region resources_and_counts

        [JsonProperty]
        private Dictionary<uint, Idle> resources; // 地图的资源
        public IDictionary<uint, Idle> Resources => resources;


        [JsonProperty]
        private Dictionary<uint, int> tileDefCountOnThisMap; // 地图的建筑数量
        [JsonProperty]
        private Dictionary<uint, int> tileDefCountOnSubmap; // 下级地图的建筑数量

        public void ChangeTileDefCount(uint tileDefID, int count) => AddDict(tileDefCountOnThisMap, tileDefID, count);

        public void ChangeTileDefCountOnSubmap(uint tileDefID, int count) => AddDict(tileDefCountOnSubmap, tileDefID, count);

        private static void AddDict(Dictionary<uint, int> dict, uint tileDefID, int count) {
            if (dict.TryGetValue(tileDefID, out int sum)) {
                int newCount = sum + count;
                if (newCount > 0) {
                    dict[tileDefID] = newCount;
                } else if (newCount == 0) {
                    dict.Remove(tileDefID);
                } else {
                    A.Assert(false);
                }
            } else if (count > 0) {
                dict.Add(tileDefID, count);
            } else {
                A.Assert(false);
            }
        }

        public bool CanChangeValue(ResDef resDef, long value) {
            if (Game.I.Settings.Cheat) return true;

            A.Assert(resDef != null);
            uint resDefID = resDef.id;
            if (value == 0) {
                return true;
            } else if (Resources.TryGetValue(resDefID, out Idle idle)) {
                if (idle.Value + value < 0) {
                    // 建造资源不够
                    return false;
                }
                return true;
            } else {
                return value > 0;
            }
        }

        public bool CanChange(ResDefValue idValue, long multiplier, IdleReference i) {
            if (i == IdleReference.Val && Game.I.Settings.Cheat) return true;
            ResDef resDef = idValue.Key;
            A.Assert(resDef != null);
            uint resDefID = resDef.id;
            if (multiplier * idValue.Value == 0) {
                return true;
            } else if (Resources.TryGetValue(resDefID, out Idle idle)) {
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

        public void DoChangeValue(ResDef resDef, long value) {
            if (Game.I.Settings.Cheat) return;

            uint resDefID = resDef.id;
            if (value == 0) {
            } else {
                if (!Resources.TryGetValue(resDefID, out Idle idle)) {
                    long del = resDef.DeltaTicks / Def.TimeScale;
                    idle = new Idle(0, 0, del, 0);
                    Resources.Add(resDefID, idle);
                }
                idle.Value += value;
            }
        }

        public void DoChange(ResDefValue idValue, long multiplier, IdleReference i) {
            if (i == IdleReference.Val && Game.I.Settings.Cheat) return;

            ResDef resDef = idValue.Key;
            A.Assert(resDef != null);
            uint resDefID = resDef.id;
            if (multiplier == 0) {
                return;
            } else {
                if (!Resources.TryGetValue(resDefID, out Idle idle)) {
                    long del = resDef.DeltaTicks / Def.TimeScale;
                    idle = new Idle(0, 0, del, 0);
                    Resources.Add(resDefID, idle);
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

        private void AllSubmapsPage() {
            UI.Prepare();
            UI.IconText("下级地图", SpriteUI.IconOf(SpriteUI.MapDef));
            UI.Space();
            foreach (var pair in SubMaps) {
                ID id = GameConfig.I.ID2Obj[pair.Value];
                MapDef mapDef = id as MapDef;
                A.Assert(mapDef != null);
                UI.IconGlowText(mapDef.CN, mapDef.Icon, mapDef.Color, mapDef.Glow);
            }
            UI.Space();
            UI.Show();
        }

        private void AllTilesPage(Dictionary<uint, int> dict) {
            UI.Prepare();
            AddIcon();
            UI.Space();

            if (dict.Count == 0) {
                UI.Text("暂无地块");
                UI.Show();
                return;
            }

            foreach (var pair in dict) {
                ID id = GameConfig.I.ID2Obj[pair.Key];
                TileDef tileDef = id as TileDef;
                A.Assert(tileDef != null);
                UI.IconGlowButton($"{tileDef.CN} {pair.Value}", tileDef.Icon, tileDef.Color, tileDef.Glow, tileDef.ShowPage);
            }
            UI.Show();
        }

        //private readonly static Dictionary<ResDef, ResDefValue> inc = new Dictionary<ResDef, ResDefValue>();
        //private readonly static Dictionary<ResDef, ResDefValue> max = new Dictionary<ResDef, ResDefValue>();
        //private readonly static Dictionary<ResDef, ResDefValue> inc_or_max = new Dictionary<ResDef, ResDefValue>();
        //public void AllLocalRelatedResourcesPage(TileDef tileDef) {
        //    UI.Prepare();

        //    tileDef.TileIconText();

        //    inc.Clear();
        //    max.Clear();
        //    inc_or_max.Clear();
        //    foreach (ResDefValue idValue in tileDef.Inc) {
        //        inc.Add(idValue.Key, idValue);
        //        inc_or_max.Add(idValue.Key, idValue);
        //    }
        //    foreach (ResDefValue idValue in tileDef.Max) {
        //        max.Add(idValue.Key, idValue);
        //        if (!inc_or_max.ContainsKey(idValue.Key)) {
        //            inc_or_max.Add(idValue.Key, idValue);
        //        }
        //    }

        //    foreach (var pair in inc_or_max) {
        //        ResDef key = pair.Key;
        //        ResDefValue idValue = pair.Value;

        //        if (!Resources.TryGetValue(key.id, out Idle idle)) {
        //            continue;
        //        }
        //        UI.Space();

        //        idValue.Key.IconText();

        //        UI.Progress(idle);
        //        if (inc.ContainsKey(key)) {
        //            SpriteUI.IconText($"{SpriteUI.CNOf(SpriteUI.Inc)} {(idValue.Value > 0 ? "+" : null)}{idValue.Value}", idValue.Value > 0 ? UI.ColorPositive : UI.ColorNegative, SpriteUI.Inc);
        //        }
        //        if (max.ContainsKey(key)) {
        //            SpriteUI.IconText($"{SpriteUI.CNOf(SpriteUI.Max)} {(idValue.Value > 0 ? "+" : null)}{idValue.Value}", idValue.Value > 0 ? UI.ColorPositive : UI.ColorNegative, SpriteUI.Max);
        //        }
        //    }
        //    inc.Clear();
        //    max.Clear();
        //    inc_or_max.Clear();
        //    UI.Show();
        //}


        public void AllResourcesPage() {
            UI.Prepare();
            Def.IconText();
            UI.Space();
            if (Resources.Count == 0) {
                UI.Text("暂无资源");
                UI.Show();
                return;
            }
            foreach (var pair in Resources) {
                // if (pair.Value.Inc == 0) continue;

                ResDef key = GameConfig.I.ID2Obj[pair.Key] as ResDef;
                key.IconButton(key.Inspect);
                UI.Progress(pair.Value);
                UI.Space();
            }
            UI.Show();
        }

        /// <summary>
        /// 效率略低，查询本土所有建筑，并且考察对此资源有无影响
        /// </summary>
        public void InspectResPage(ResDef resDef) {
            if (resDef == null) return; // ?

            if (!resources.TryGetValue(resDef.id, out Idle idle)) {
                return;
            }

            UI.Prepare();

            resDef.IconText();
            UI.Progress(idle);
            if (idle.Inc > 0 && idle.Max > 0) {
                // UI.Text($"+ {((double)idle.Inc / idle.DelSecond)} 每秒");
                resDef.AddIncText(idle.Inc);
            }

            bool title;

            title = false;
            foreach (var pair in tileDefCountOnThisMap) {
                TileDef tileDef = GameConfig.I.ID2Obj[pair.Key] as TileDef;
                foreach (ResDefValue resDefValue in tileDef.Inc) {
                    if (resDefValue.Key == resDef) {
                        if (!title) {
                            UI.Space();
                            SpriteUI.IconText(SpriteUI.Inc);
                            title = true;
                        }
                        tileDef.IconTextWithNumber(resDefValue.Value * pair.Value);
                        // resDefValue.Key.AddMaxText(resDefValue.Value * pair.Value);
                    }
                }
            }

            title = false;
            foreach (var pair in tileDefCountOnThisMap) {
                TileDef tileDef = GameConfig.I.ID2Obj[pair.Key] as TileDef;
                foreach (ResDefValue resDefValue in tileDef.Max) {
                    if (resDefValue.Key == resDef) {
                        if (!title) {
                            UI.Space();
                            SpriteUI.IconText(SpriteUI.Max);
                            title = true;
                        }
                        tileDef.IconTextWithNumber(resDefValue.Value * pair.Value);
                        // resDefValue.Key.AddMaxButton(resDefValue.Value * pair.Value);
                    }
                }
            }

            title = false;
            foreach (var pair in tileDefCountOnSubmap) {
                TileDef tileDef = GameConfig.I.ID2Obj[pair.Key] as TileDef;
                foreach (ResDefValue resDefValue in tileDef.IncSuper) {
                    if (resDefValue.Key == resDef) {
                        if (!title) {
                            UI.Space();
                            SpriteUI.IconText(SpriteUI.IncSuper);
                            title = true;
                        }
                        tileDef.IconTextWithNumber(resDefValue.Value * pair.Value);
                        // resDefValue.Key.AddMaxText(resDefValue.Value * pair.Value);
                    }
                }
            }

            title = false;
            foreach (var pair in tileDefCountOnSubmap) {
                TileDef tileDef = GameConfig.I.ID2Obj[pair.Key] as TileDef;
                foreach (ResDefValue resDefValue in tileDef.MaxSuper) {
                    if (resDefValue.Key == resDef) {
                        if (!title) {
                            UI.Space();
                            SpriteUI.IconText(SpriteUI.MaxSuper);
                            title = true;
                        }
                        tileDef.IconTextWithNumber(resDefValue.Value * pair.Value);
                        // resDefValue.Key.AddMaxButton(resDefValue.Value * pair.Value);
                    }
                }
            }
            UI.Show();
        }

        public void AddButton() {
            UI.IconButton(Def.CN, Def.Icon, ShowPage);
        }
        public void AddIcon() {
            UI.IconText(Def.CN, Def.Icon);
        }

        public void ShowPage() {
            UI.Prepare();

            Def.IconText();
            UI.Space();
            bool canEnter = Game.I.Settings.Cheat || Game.I.SuperMap.CanEnter();
            UI.IconButton("离开地图", SpriteUI.IconOf(SpriteUI.MapDef), canEnter ? UI.ColorNormal : UI.ColorDisable, () => {
                if (canEnter) {
                    Game.I.EnterSuperMap();
                } else {
                    FailGotoSuperMapPage();
                }
            });
            UI.Space();

            UI.IconButton("已有地块", SpriteUI.IconOf(SpriteUI.TileDef), () => AllTilesPage(tileDefCountOnThisMap));
            UI.IconButton("已有资源", SpriteUI.IconOf(SpriteUI.ResDef), AllResourcesPage);
            if (tileDefCountOnSubmap.Count > 0) UI.IconButton("下级地块", SpriteUI.IconOf(SpriteUI.TileDef), () => AllTilesPage(tileDefCountOnSubmap));
            if (SubMaps != null && SubMaps.Count > 0) UI.IconButton("下级地图", SpriteUI.IconOf(SpriteUI.MapDef), AllSubmapsPage);

            UI.Space();

            //if (Game.I.Map.PreviousSeed != Map.NullSeed && Game.I.Map.PreviousMapLevel <= Game.I.Map.MapLevel) {
            //    UI.Button("返回", () => Game.I.EnterPreviousMap());
            //}
            UI.IconButton("地块百科", SpriteUI.IconOf(SpriteUI.TileDef), AllConstructablePage);

            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor) {
                UI.Text("地图种子");
                UI.Text(Seed.ToString());

                UI.Space();
                UI.Text("钟慢效应");
                UI.Text(Def.TimeScale.ToString());
            }

            UI.Show();
        }

        private void AllConstructablePage() {
            UI.Prepare();
            AddIcon();
            UI.Space();
            foreach (TileDef tileDef in Def.Constructables) {
                UI.IconGlowButton(tileDef.CN, tileDef.Icon, tileDef.Color, tileDef.Glow, tileDef.ShowPage);
            }
            UI.Show();
        }

        private void FailGotoSuperMapPage() {
            UI.Prepare();

            SpriteUI.IconText(SpriteUI.Failure);

            foreach (TechDef techDef in Game.I.SuperMap.Def.TechRequirementForEntrence) {
                Game.I.TechLevel(techDef.id, out int level);
                if (level == 0) {
                    // return false;
                    UI.Space();

                    Audio.I.Clip = Audio.I.NegativeSound;
                    UI.IconText($"需要科技", SpriteUI.IconOf(SpriteUI.Failure));
                    UI.IconGlowText(techDef.CN, techDef.Icon, techDef.Color, techDef.Glow);
                }
            }

            UI.Show();
        }

        #endregion

    }

    /// <summary>
    /// 指向一个数值的哪个属性：数量值，增速，容量/最大值
    /// </summary>
    public enum IdleReference
    {
        Val, Inc, Max
    }
}


