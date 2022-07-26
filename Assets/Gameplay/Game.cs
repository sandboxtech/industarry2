
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace W
{

    [ExecuteAfter(typeof(GameConfigReference))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Game : IPersistent
    {
        public static Game I => i;
        private static Game i;

        public Game() {
            A.Singleton(ref i, this);
        }



        [JsonProperty]
        private GameConfig config;
        public GameConfig Config => config;

        public void OnCreate() {
            config = Persistence.Create<GameConfig>();
            config.Load();

            settings = Persistence.Create<Settings>();
            techs = new Dictionary<uint, int>();

            on_ship = false;
            planetMap = CreateMap(Seed2MapDefName(planetMap_seed), planetMap_seed);
            spaceshipMap = CreateMap(SpaceshipConfigName, int.MaxValue);

            MapUI.I.TryConstructInitials(planetMap);
            MapUI.I.TryConstructInitials(spaceshipMap);

            CameraControl.I.Position = new Vector2(planetMap.Width / 2, planetMap.Height / 2);

            UI.Prepare();
            UI.Text("挂机工厂2");
            CheckVersion();
            UI.Show();
        }
        private Map CreateMap(string mapConfigfile, int seed) {
            Map map = Persistence.Create<Map>();
            MapDef mapDef = config.Name2Obj[mapConfigfile] as MapDef;
            if (mapDef == null) {
                A.Error($"找不到地图配置: {mapConfigfile}");
            }
            map.Init(mapDef, seed);
            return map;
        }

        private string Seed2MapDefName(int seed) => "Continental";

        public void Load() {
            config.Load();
        }

        private const long VERSION = 2;
        [JsonProperty]
        private long version = VERSION;
        private void CheckVersion() {
            if (version != VERSION) {
                UI.Text("存档版本不兼容");
                UI.Text("需要重启");
                Persistence.ClearSaves();
            }
        }


        [JsonProperty]
        private Settings settings;
        public Settings Settings => settings;


        [JsonProperty]
        private Dictionary<uint, int> techs;
        // public Dictionary<uint, int> Techs => techs;
        public void TechLevel(uint id, out int level) {
            if (!techs.TryGetValue(id, out level)) {
                techs.Add(id, 0);
                level = 0;
            }
        }
        public void TechLevel(uint id, int level) {
            if (!techs.TryGetValue(id, out _)) {
                techs.Add(id, 0);
            }
            techs[id] = level;
        }



        [JsonProperty]
        private bool on_ship = false;
        public bool IsOnSpaceship {
            get => on_ship;
            set {
                if (on_ship) {
                    planetMap.SaveCameraPosition();
                    spaceshipMap.LoadCameraPosition();
                }
                else {
                    spaceshipMap.SaveCameraPosition();
                    planetMap.LoadCameraPosition();
                }
                on_ship = value;
                MapUI.EnterMap();
            }
        }

        [JsonIgnore]
        private Map planetMap;
        [JsonIgnore]
        private Map spaceshipMap;

        public Map Map => on_ship ? spaceshipMap : planetMap;
        public static Color Color(ID id) => I.Map.Def.Theme.ColorOfID(id);




        [OnDeserializing]
        private void OnDeserializingMethod(StreamingContext context) {
            // 反序列化之后，什么都不做
        }

        [JsonProperty]
        private int planetMap_seed = 0;

        private const string SpaceshipConfigName = "Spaceship";
        private const string MapsFolder = "maps";
        private const string SpaceshipMapFilename = "spaceship.json";
        private static string Seed2Filename(int seed) => $"{seed}.json";

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
            // 反序列化之后，恢复Map
            GameLoop.Load(MapsFolder, Seed2Filename(planetMap_seed), out planetMap);
            GameLoop.Load(null, SpaceshipMapFilename, out spaceshipMap);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            // 序列化之前，保存Map
            GameLoop.Save(MapsFolder, Seed2Filename(planetMap.Seed), planetMap);
            GameLoop.Save(null, SpaceshipMapFilename, spaceshipMap);
        }

        [OnSerialized]
        private void OnSerializedMethod(StreamingContext context) {
            // 序列化之前，什么都不做
        }

    }
}

