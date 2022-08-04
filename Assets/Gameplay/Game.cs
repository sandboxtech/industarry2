
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace W
{
    public interface IGame
    {
        void Init();
        void Start();
    }

    [ExecuteAfter(typeof(GameConfigReference))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Game : IGame
    {
        public static Game I => i;
        private static Game i;

        /// <summary>
        /// 最开始被触发，无论读取还是创建
        /// </summary>
        public Game() {
            A.Singleton(ref i, this);
        }

        [JsonIgnore]
        private bool created = false;
        void IGame.Init() {
            // create
            created = true;

            if (config == null) {
                config = new GameConfig();
                (config as IGameConfig).Init();
                // gameConfig 的 Prepare 需要在创建地图前执行，并且在读取后执行
                config.Prepare();
            }

            relativity = new Relativity();
            settings = new Settings();
            techs = new Dictionary<uint, int>();

            //on_ship = false;
            //spaceshipMap = Map.Create(1, MapDefName.SpaceshipLevel);
            thisMap = Map.Create(1, MapDefName.PlanetLevel);
            thisMap.LoadSuper(out superMap);

            thisMapKey = thisMap.Key;
            superMapKey = superMap.Key;
        }

        [JsonProperty]
        private GameConfig config;
        public GameConfig Config => config;
        void IGame.Start() {
            CheckVersion();
            if (!created) {
                config.Prepare();
            }
            Map.OnEnter();
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

        private const string GameFilename = "game.json";
        public void Save() {
            string key = GameFilename;
            GameLoop.Save(key, this);
        }
        public static Game TryLoad(out Game game) {
            GameLoop.Load(GameFilename, out game);
            return game;
        }



        [JsonProperty]
        private Relativity relativity;
        public Relativity Relativity => relativity;


        [JsonProperty]
        private Settings settings;
        public Settings Settings => settings;
        [JsonProperty]
        private Dictionary<uint, int> techs;
        // public Dictionary<uint, int> Techs => techs;
        public void TechLevel(uint id, out int level) {
            if (!techs.TryGetValue(id, out level)) {
                // techs.Add(id, 0);
                level = 0;
            }
        }
        public void TechLevel(uint id, int level) {
            if (!techs.TryGetValue(id, out _)) {
                techs.Add(id, level);
            }
            else {
                techs[id] = level;
            }
        }





        [JsonIgnore]
        private Map thisMap;
        public Map ThisMap => thisMap;
        [JsonProperty]
        private string thisMapKey;

        [JsonIgnore]
        private Map superMap;
        public Map SuperMap => superMap;
        [JsonProperty]
        private string superMapKey;

        //[JsonIgnore]
        //private Map spaceshipMap;
        //[JsonProperty]
        //private string spaceshipMapKey;

        //private Map OtherMap => on_ship ? thisMap : spaceshipMap;
        //public Map Map => on_ship ? spaceshipMap : thisMap;
        public Map Map => thisMap;
        //public Map SpaceshipMap => spaceshipMap;

        //[JsonProperty]
        //private bool on_ship = false;

        //public bool OnShip {
        //    get => on_ship;
        //    set {
        //        A.Assert(on_ship != value);
        //        //if (on_ship) {
        //        //    thisMap.SaveCameraPosition();
        //        //    spaceshipMap.LoadCameraPosition();
        //        //} else {
        //        //    spaceshipMap.SaveCameraPosition();
        //        //    thisMap.LoadCameraPosition();
        //        //}
        //        Save();
        //        on_ship = value;

        //        OtherMap.OnExit();
        //        Map.OnEnter();
        //    }
        //}


        public void EnterSuperMap() => EnterMap(Map.SuperMapIndex, ID.Invalid);
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="index"></param>
        public void EnterMap(uint index, uint mapDefID) {
            thisMap.OnExit();
            Save();
            if (index == Map.SuperMapIndex) {
                thisMap = superMap;
                thisMap.LoadMap(index, mapDefID, out superMap);
            } else {
                superMap = thisMap;
                superMap.LoadMap(index, mapDefID, out thisMap);
            }
            thisMap.OnEnter();
        }

        public void EnterPreviousMap() {
            thisMap.OnExit();
            Save();

            thisMap.LoadPrevious(out Map map);
            if (map == null) {
                return;
            }
            thisMap = map;
            thisMap.LoadSuper(out superMap);

            thisMap.OnEnter();

            // on_ship = false;
        }



        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
            Map.Load(thisMapKey, out thisMap);
            Map.Load(superMapKey, out superMap);
            //Map.Load(spaceshipMapKey, out spaceshipMap);

            Map.LoadBody();
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            // 序列化之前，保存Map
            SaveMaps();
        }
        private void SaveMaps() {
            thisMap.Save(out thisMapKey);
            superMap.Save(out superMapKey);
            //spaceshipMap.Save(out spaceshipMapKey);

            Map.SaveBody();
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

