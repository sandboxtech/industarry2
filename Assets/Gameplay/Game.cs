
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace W
{
    public interface IGame
    {
        void Start();
    }

    [ExecuteAfter(typeof(GameConfigReference))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Game : IPersistent, IGame
    {
        public static Game I => i;
        private static Game i;

        public Game() {
            A.Singleton(ref i, this);
        }


        [JsonIgnore]
        private bool created = false;

        [JsonProperty]
        private GameConfig config;
        public GameConfig Config => config;

        void IPersistent.OnCreate() {
            // create
            created = true;

            if (config == null) {
                config = new GameConfig();
                (config as IGameConfig).Init();
            }

            settings = new Settings();
            techs = new Dictionary<uint, int>();

            on_ship = false;
            spaceshipMap = new Map();
            thisMap = new Map();
            superMap = new Map();

            (spaceshipMap as IMap).Init(null, 0);
            (thisMap as IMap).Init(spaceshipMap, 0);
            (superMap as IMap).Init(thisMap, 0);


            thisMapKey = thisMap.Key;

            CameraControl.I.Position = new Vector2(thisMap.Width / 2, thisMap.Height / 2);
        }


        private const long VERSION = 3;
        [JsonProperty]
        private readonly long version = VERSION;
        private void CheckVersion() {
            if (version != VERSION) {
                UI.Text("存档版本不兼容");
                UI.Text("需要重启");
                Persistence.ClearSaves();
            }
        }
        void IGame.Start() {
            CheckVersion();
            if (!created) {
                config.Load();
            }
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
        public void Save() {
            string key = GameFilename;
            GameLoop.Save(null, key, this);
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

