
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
                Game.I.SaveDelayed();
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
                UI.Button(Game.I.Settings.Cheat ? "作弊:已开启" : "作弊:已关闭", () => {
                    Game.I.Settings.Cheat = !Game.I.Settings.Cheat;
                    Settings();
                });
            }

            UI.Space();
            // UI.Text("目前游戏流程就5分钟，3个建筑。飞出月球就没什么东西了。9月1日更新大量内容");


            UI.Show();
        }
    }
}
