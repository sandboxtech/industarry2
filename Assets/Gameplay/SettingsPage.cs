
using Newtonsoft.Json;
using UnityEngine;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public static class SettingsPage
	{
        public static void Show() {
            UI.Prepare();

            UI.Text($"挂机工厂2");


            UI.Button($"前往{(Game.I.IsOnSpaceship ? "地面" : "飞船")}", () => {
                Game.I.IsOnSpaceship = !Game.I.IsOnSpaceship;
            });

            UI.Space();

            UI.Button("设置", Settings);

            UI.Space();

            //foreach (var pair in Game.I.Map.Resources) {
            //    ID id = GameConfig.I.ID2Obj[pair.Key];
            //    Idle idle = pair.Value;
            //    UI.IconText(id.CN, id.Icon);
            //    UI.Progress(() => $"{idle.Value}/{idle.Max}  +{idle.Inc}/{idle.DelSecond}s", () => idle.Progress);
            //}

            UI.Show();
        }
        private static void Settings() {
            UI.Prepare();

            UI.Button("保存", () => {
                GameLoop.SaveGame();
            });
            UI.Button("删档", () => {
                Persistence.ClearSaves();
            });
            UI.Button("退出", () => {
                GameLoop.Quit();
            });

            UI.Show();
        }
    }
}
