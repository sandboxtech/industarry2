
using System.Collections.Generic;

namespace W
{

    /// <summary>
    /// 依赖：Persistence Serialization
    /// 被依赖：目前 GameEntry
    /// 用于游戏循环回调：开始，保存
    /// 用于创建保存物品路由，存档位置控制
    /// </summary>
    public static class GameLoop
    {
        public static void Start() {
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor) {
                LoadGame();
            }
            else {
                try {
                    LoadGame();
                } catch (System.Exception e) {
                    UI.Prepare();
                    UI.Text(e.Message);
                    UI.Text(new System.Diagnostics.StackTrace().ToString());
                    UI.Show();
                    throw e;
                }
            }
        }
        private static void LoadGame() {
            Game.TryLoad(out Game game);
            if (game == null) {
                game = new Game();
                (game as IGame).Init();
            }
            (game as IGame).Start();
        }


        public static void Load<T>(string filename, out T t) where T : class, new () {
            A.Assert(filename != null);
            Persistence.Load(typeof(T).FullName, filename, out string contents);
            t = contents == null ? null : Persistence.Deserialize<T>(contents);
        }

        /// <summary>
        /// 序列化后延迟保存，保证存档统一性
        /// </summary>
        public static void Save(string filename, object obj) {
            A.Assert(filename != null, () => filename);
            string json = Serialization.Serialize(obj);
            // Persistence.Save(obj.GetType().FullName, filename, json);
            saves.Add((obj.GetType().FullName, filename, json));
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
            QuitDelayed();
        }

        private static bool doQuit = false;
        private static void QuitDelayed() {
            if (!doQuit) { return; }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        public static void Quit() {
            doQuit = true;
        }
    }
}

