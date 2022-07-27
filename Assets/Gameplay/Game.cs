
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


        [JsonProperty]
        private GameConfig config;
        public GameConfig Config => config;
        void IPersistent.OnConstruct() {
            if (config == null) {
                config = Persistence.Create<GameConfig>();
            }
            config.Load();
            CheckVersion();
        }


        void IPersistent.OnCreate() {
            // create

            settings = Persistence.Create<Settings>();
            techs = new Dictionary<uint, int>();

            spaceshipMap = Persistence.Create<Map>();
            thisMap = Persistence.Create<Map>();
            superMap = Persistence.Create<Map>();

            (spaceshipMap as IMap)._Init(null, 0);
            (thisMap as IMap)._Init(spaceshipMap, 0);
            (superMap as IMap)._Init(thisMap, 0);

            on_ship = false;

            thisMapKey = thisMap.Key;

            CameraControl.I.Position = new Vector2(thisMap.Width / 2, thisMap.Height / 2);
        }
        void IPersistent.OnLoad() {
        }
        void IPersistent.AfterConstruct() {
            thisMap.Enter();
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
                    thisMap.SaveCameraPosition();
                    spaceshipMap.LoadCameraPosition();
                } else {
                    spaceshipMap.SaveCameraPosition();
                    thisMap.LoadCameraPosition();
                }
                on_ship = value;
                Map.Enter();
            }
        }


        [JsonIgnore]
        private Map thisMap;
        [JsonProperty]
        private string thisMapKey;

        [JsonIgnore]
        private Map superMap;
        [JsonProperty]
        private string superMapKey;

        [JsonIgnore]
        private Map spaceshipMap;
        [JsonProperty]
        private string spaceshipMapKey;


        public Map Map => on_ship ? spaceshipMap : thisMap;



        private const string GameFilename = "game.json";
        public string Save(out string key) {
            key = GameFilename;
            GameLoop.Save(null, key, this);
            return key;
        }
        public static Game Load(out Game game) {
            GameLoop.Load(null, GameFilename, out game);
            return game;
        }



        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
            Map.Load(spaceshipMapKey, out spaceshipMap);
            Map.Load(thisMapKey, out thisMap);
            Map.Load(superMapKey, out superMap);
            //GameLoop.Load(MapsFolder, thisMapKey, out thisMap);
            //GameLoop.Load(MapsFolder, superMapKey, out superMap);
            //GameLoop.Load(null, SpaceshipMapKey, out spaceshipMap);
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            // 序列化之前，保存Map
            SaveMaps();
        }
        private void SaveMaps() {
            thisMap.Save(out thisMapKey);
            superMap.Save(out superMapKey);
            spaceshipMap.Save(out spaceshipMapKey);
            //GameLoop.Save(MapsFolder, thisMap.Key, thisMap);
            //GameLoop.Save(MapsFolder, superMap.Key, superMap);
            //GameLoop.Save(null, SpaceshipMapKey, spaceshipMap);
        }

        [OnDeserializing]
        private void OnDeserializingMethod(StreamingContext context) {
            // 反序列化之后，什么都不做
        }
        [OnSerialized]
        private void OnSerializedMethod(StreamingContext context) {
            // 序列化之前，什么都不做
        }

    }
}

