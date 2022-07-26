
using System.Collections.Generic;

namespace W
{

    /// <summary>
    /// 依赖：Persistence Serialization
    /// 依赖回调：GameEntry
    /// </summary>
    public static class GameLoop
    {
        public static void Awake() {

        }

        private static string gameFile = "game.json";
        public static void Start() {
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor) {
                LoadGame();
            } else {
                try {
                    LoadGame();
                } catch (System.Exception e) {
                    UI.Prepare();
                    UI.Text(e.Message);
                    UI.Show();
                    throw e; // rethrow
                }
            }
        }

        private static void LoadGame() {
            Load(null, gameFile, out Game _);
            Game.I.Load();
        }

        public static void SaveGame() {
            Save(null, gameFile, Game.I);
        }

        public static void Load<T>(string directory, string filename, out T t) where T : class, IPersistent, new() {
            A.Assert(filename != null);
            Persistence.Load(directory, filename, out string contents);
            t = Persistence.Create<T>(contents);
        }

        /// <summary>
        /// 序列化后延迟保存，保证存档统一性
        /// </summary>
        public static void Save(string directory, string filename, object obj) {
            A.Assert(filename != null, () => filename);
            string json = Serialization.Serialize(obj);
            // Persistence.Save(directory, filename, json);
            saves.Add((directory, filename, json));
        }
        private static HashSet<(string, string, string)> saves = new HashSet<(string, string, string)>();
        private static void SaveDelayed() {
            if (saves.Count > 0) {
                foreach (var save in saves) {
                    Persistence.Save(save.Item1, save.Item2, save.Item3);
                }
                saves.Clear();
            }
        }

        public static void Update() {
            SaveDelayed();
        }

        public static void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}

