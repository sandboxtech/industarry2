
using System.Collections.Generic;
using UnityEngine;

namespace W
{

    [CreateAssetMenu(fileName = "__ResDef__", menuName = "创建 ResDef 资源定义", order = 1)]
    public class ResDef : ID
    {

        [Header("生产间隔")]
        public long del;
        public long DeltaSecond => del == 0 ? 10 : del;
        public long DeltaTicks => DeltaSecond * Constants.Second;
        public long DeltaMinutes => DeltaTicks / Constants.Minute;
        public long DeltaHours => DeltaTicks / Constants.Hour;

        public string GetSpeedDescriptionString(long number) {
            number = number < 0 ? -number : number;
            string descriptionString;
            double perTime;
            long deltaTick = number * DeltaTicks;
            if (deltaTick <= Constants.Second) {
                return "0";
            } else if (deltaTick > Constants.Second) {
                perTime = ((double)number / DeltaSecond);
                descriptionString = $"{(number >= 0 ? '+' : '-')} {perTime} 每秒";
            } else if (deltaTick > Constants.Minute) {
                perTime = ((double)number / DeltaSecond);
                descriptionString = $"{(number >= 0 ? '+' : '-')} {perTime} 每分";
            } else {
                perTime = ((double)number / DeltaHours);
                descriptionString = $"{(number >= 0 ? '+' : '-')} {perTime} 每时";
            }
            return descriptionString;
        }

        public void AddIncText(long number) {
            UI.Text(GetSpeedDescriptionString(number));
            //UI.IconGlowText(number > 0 ? $"{CN} +{number}" : $"{CN} -{-number}",
            //    UI.ColorNormal,
            //    Icon, Color, Glow);
        }
        public void AddMaxText(long number) {
            UI.Text(GetSpeedDescriptionString(number));
            //UI.IconGlowText(number > 0 ? $"{CN} +{number}" : $"{CN} -{-number}",
            //    UI.ColorNormal,
            //    Icon, Color, Glow);
        }
        public void AddIncButton(long number) {
            UI.IconGlowButton(number > 0 ? $"{CN} +{number}" : $"{CN} -{-number}",
                UI.ColorNormal,
                Icon, Color, Glow, Inspect);
        }
        public void AddValButton(long number) {
            UI.IconGlowButton(number > 0 ? $"{CN} {number}" : $"{CN} -{-number}",
                UI.ColorNormal,
                Icon, Color, Glow, Inspect);
        }
        public void AddMaxButton(long number = 1) {
            UI.IconGlowButton(number > 0 ? $"{CN} {number}" : $"{CN} -{-number}",
                UI.ColorNormal,
                Icon, Color, Glow, Inspect);
        }


        public void Inspect() {
            Game.I.Map.InspectResPage(this);
        }
    }
}
