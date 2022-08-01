
using Newtonsoft.Json;
using UnityEngine;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public static class SettingsPage
	{
        public static void Settings() {
            UI.Prepare();

            UI.Button("保存", () => {
                Game.I.Save();
            });
            UI.Button("删档", () => {
                Persistence.ClearSaves();
                GameLoop.Quit();
            });
            UI.Button("退出", () => {
                GameLoop.Quit();
            });

            if (Application.platform == RuntimePlatform.WindowsEditor) {
                UI.Space();
                UI.Text("无限资源");
                UI.Button(Game.I.Settings.InfiniteResourceCheat ? "已开启" : "已关闭", () => {
                    Game.I.Settings.InfiniteResourceCheat = !Game.I.Settings.InfiniteResourceCheat;
                    Settings();
                });
            }

            UI.Show();
        }
    }
}
