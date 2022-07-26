
using System;
using UnityEngine;

namespace W
{
    public static class Sprites
    {
        public static void IconText(string name) {
            ID id = GameConfig.I.Name2Obj[name];
            UI.IconText(id.CN, id.Icon, id.Color);
        }
        public static void IconButton(string name, Action action) {
            ID id = GameConfig.I.Name2Obj[name];
            UI.IconButton(id.CN, id.Icon, id.Color, action);
        }
        public static void IconButton(string name, Color color, Action action) {
            ID id = GameConfig.I.Name2Obj[name];
            UI.IconButton(id.CN, color, id.Icon, id.Color, action);
        }

        public static Sprite IconOf(string name) => GameConfig.I.Name2Obj[name].Icon;

        public const string Construction = "Construction";
        public const string Destruction = "Destruction";
        public const string Success = "Success";
        public const string Failure = "Failure";

        public const string Val = "Val";
        public const string Inc = "Inc";
        public const string Max = "Max";
        public const string Bonus = "Bonus";
        public const string Repel = "Repel";
        public const string Condition = "Condition";

        public const string Information = "Information";
        public const string Warning = "Warning";

        public const string Level = "Level";

        public const string TechDef = "TechDef";
        public const string TileDef = "TileDef";
        public const string ResDef = "ResDef";
        public const string MapDef = "MapDef";
    }
}
