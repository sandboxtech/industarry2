
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
                Game.Load(out Game game);
                (game as IGame).Start();
            }
            else {
                try {
                    Game.Load(out Game game);
                    (game as IGame).Start();
                } catch (System.Exception e) {
                    UI.Prepare();
                    UI.Text(e.Message);
                    UI.Text(new System.Diagnostics.StackTrace().ToString());
                    UI.Show();
                    throw e;
                }
            }
        }


        public static void Load<T>(string directory, string filename, out T t) where T : class, new() {
            A.Assert(filename != null);
            Persistence.Load(directory, filename, out string contents);
            t = Persistence.Deserialize<T>(contents);
        }

        /// <summary>
        /// 序列化后延迟保存，保证存档统一性
        /// </summary>
        public static void Save(string directory, string filename, object obj) {
            A.Assert(filename != null, () => filename);
            string json = Serialization.Serialize(obj);
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

