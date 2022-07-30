
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


            UI.Button(Game.I.OnShip ? "离开飞船" : "进入飞船", () => {
                Game.I.OnShip = !Game.I.OnShip;
            });

            if (Game.I.OnShip) {

            }
            else {
                UI.Button("上天", () => Game.I.EnterMap(Map.SuperMapIndex));
                if (Game.I.Map.PreviousSeed != Map.NullSeed && Game.I.Map.PreviousMapLevel < Game.I.Map.MapLevel) {
                    UI.Button("入地", () => Game.I.EnterPreviousMap());
                }
            }


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
                Game.I.Save();
            });
            UI.Button("删档", () => {
                Persistence.ClearSaves();
                GameLoop.Quit();
            });
            UI.Button("退出", () => {
                GameLoop.Quit();
            });

            UI.Show();
        }
    }
}
