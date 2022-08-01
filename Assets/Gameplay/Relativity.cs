
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Relativity
	{
        public Relativity() {
            lastRealTicks = RealNow;
            ticks = lastRealTicks;
            TimeScale = 1;
        }


        [JsonProperty]
        private long lastRealTicks;

        [JsonProperty]
        private long ticks;

        [JsonProperty]
        private int timeScale;
        public int TimeScale { 
            get => timeScale; 
            set {
                A.Assert(value >= 1 && value <= 100);
                timeScale = value;
            }
        }

        public long RealNow => System.DateTime.UtcNow.Ticks;
        public long ScaleNow {
            get {
                long nowRealTicks = RealNow;
                long deltaRealTicks = nowRealTicks - lastRealTicks;
                if (deltaRealTicks == 0) return ticks;

                lastRealTicks = nowRealTicks;

                ticks += deltaRealTicks * TimeScale;
                return ticks;
            }
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
            // 序列化之前，保存Map
            ticks = ScaleNow;
        }
    }
}
