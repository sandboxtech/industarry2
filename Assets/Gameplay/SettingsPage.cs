
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
 
            UI.Button("退出", () => {
                Game.I.SaveDelayed();
                GameLoop.Quit();
            });

            UI.Button("地块堆叠上限", LevelMaxPage);

            UI.Space();

            UI.Button("删档", () => {
                Persistence.ClearSaves();
                GameLoop.Quit();
            });

            if (true || Application.platform == RuntimePlatform.WindowsEditor) {
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

        private static int delta = 0;
        private static int Valid(int x) => M.Clamp(1, int.MaxValue, x);
        private static void LevelMaxPage() {
            UI.Prepare();

            int maxLevel = Valid(Game.I.Settings.MaxLevel);
            Game.I.Settings.MaxLevel = maxLevel;

            UI.Text(() => $"地块最大数目 {(Game.I.Settings.MaxLevel == int.MaxValue ? "max" : Game.I.Settings.MaxLevel)}");
            UI.Space();

            const int small = 1000;
            UI.Slider(small.ToString(), (float x) => {
                Game.I.Settings.MaxLevel = (int)((M.Pow(2f, x) - 1) * small);
            });
            const int large = 100000;
            UI.Slider(large.ToString(), (float x) => {
                Game.I.Settings.MaxLevel = (int)((M.Pow(2f, x) - 1) * large);
            });


            UI.Button(" * 2", () => {
                delta = Game.I.Settings.MaxLevel / 2;
                Game.I.Settings.MaxLevel *= 2;
                LevelMaxPage();
            });
            UI.Button(" / 2", () => {
                delta = Game.I.Settings.MaxLevel / 4;
                Game.I.Settings.MaxLevel /= 2;
                LevelMaxPage();
            });

            delta = Valid(delta);
            UI.Button($" + {delta}", () => {
                Game.I.Settings.MaxLevel += delta;
                delta *= 2;
                LevelMaxPage();
            });
            UI.Button($" - {delta}", () => {
                Game.I.Settings.MaxLevel -= delta;
                delta /= 2;
                LevelMaxPage();
            });

            UI.Button(" + 1", () => {
                Game.I.Settings.MaxLevel += 1;
                LevelMaxPage();
            });
            UI.Button(" - 1", () => {
                Game.I.Settings.MaxLevel -= 1;
                LevelMaxPage();
            });

            UI.Button(" min", () => {
                Game.I.Settings.MaxLevel = 1;
                LevelMaxPage();
            });
            UI.Button(" max", () => {
                Game.I.Settings.MaxLevel = maxLevel;
                LevelMaxPage();
            });

            UI.Show();
        }
    }
}
