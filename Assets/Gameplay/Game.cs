
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

        private const long VERSION = 3;
        [JsonProperty]
        private long version = VERSION;
        private void CheckVersion() {
            if (version != VERSION) {
                UI.Text("存档版本不兼容");
                UI.Text("需要重启");
                Persistence.ClearSaves();
            }
        }

        public void OnCreate() {
            config = Persistence.Create<GameConfig>();
            config.Load();

            CheckVersion();

            settings = Persistence.Create<Settings>();
            techs = new Dictionary<uint, int>();

            on_ship = false;

            LoadOrCreateMap(SpaceShipIndex);
            LoadOrCreateMap(PlanetMapIndex);

            planetMapPreviousSeed = planetMap.PreviousSeed;

            CameraControl.I.Position = new Vector2(planetMap.Width / 2, planetMap.Height / 2);

            MapUI.EnterMap();
        }

        public const uint SpaceShipIndex = uint.MaxValue;
        public const uint PlanetMapIndex = uint.MinValue;
        public Map LoadOrCreateMap(uint index) {
            Map previousMap = planetMap;
            Map map;
            if (previousMap == null) {
                if (index == SpaceShipIndex) {
                    map = spaceshipMap = Persistence.Create<Map>();
                }
                else if (index == PlanetMapIndex) {
                    map = planetMap = Persistence.Create<Map>();
                }
                else {
                    throw new System.Exception();
                }
            } else {
                GameLoop.Load(MapsFolder, FilenameOf(previousMap.Seed), out map);
                planetMap = map;
            }
            if (map.NotInitialized) {
                (map as IMap)._Init(previousMap, index);
            }
            planetMapPreviousSeed = map.PreviousSeed;
            return map;
        }


        public void Load() {
            config.Load();
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
                A.Assert(on_ship != value);
                if (on_ship) {
                    planetMap.SaveCameraPosition();
                    spaceshipMap.LoadCameraPosition();
                } else {
                    spaceshipMap.SaveCameraPosition();
                    planetMap.LoadCameraPosition();
                }
                on_ship = value;
                MapUI.EnterMap();
            }
        }


        [JsonIgnore]
        private Map planetMap;
        [JsonProperty]
        private uint planetMapPreviousSeed;
        [JsonIgnore]
        private Map spaceshipMap;

        public Map Map => on_ship ? spaceshipMap : planetMap;
        public static Color Color(ID id) => I.Map.Def.Theme.ColorOfID(id);




        [OnDeserializing]
        private void OnDeserializingMethod(StreamingContext context) {
            // 反序列化之后，什么都不做
        }


        private const string SpaceshipMapFilename = "spaceship.json";

        private const string MapsFolder = "maps";
        private static string FilenameOf(uint seed) => $"{seed}.json";

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
            // 反序列化之后，恢复Map
            GameLoop.Load(MapsFolder, FilenameOf(planetMapPreviousSeed), out planetMap);
            GameLoop.Load(null, SpaceshipMapFilename, out spaceshipMap);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            // 序列化之前，保存Map
            GameLoop.Save(MapsFolder, FilenameOf(planetMap.PreviousSeed), planetMap);
            GameLoop.Save(null, SpaceshipMapFilename, spaceshipMap);
        }

        [OnSerialized]
        private void OnSerializedMethod(StreamingContext context) {
            // 序列化之前，什么都不做
        }

    }
}

