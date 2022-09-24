
using Newtonsoft.Json;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Idle
    {
        [JsonProperty] private long val;
        [JsonProperty] private double progress;

        [JsonProperty] private long del;
        [JsonProperty] private long inc;
        [JsonProperty] private long time;
        [JsonProperty] private long max;

        public Idle(long val = 0, long inc = 1, long del = Constants.Second, long max = long.MaxValue) {
            this.val = val;
            this.inc = inc;
            this.del = del;
            this.max = max;
            time = Now;
        }


        private static long Clamp(long min, long max, long x) => x < min ? min : (x > max ? max : x);
        private static float Clamp(float min, float max, float x) => x < min ? min : (x > max ? max : x);

        private long Now => Game.I.Relativity.ScaleNow;
        private void Sync() {
            //long now = Now;
            //long turn = (now - time) / Del;
            //val += turn * Inc;
            //val = Clamp(0, Max, val);
            //time += turn * Del;

            long now = Now;

            progress = inc == 0 ? progress : (double)(now - time) * inc / del % 1;

            long delta = (now - time) * inc / del;
            val += delta;
            val = Clamp(0, max, val);

            // time += ((now - time) / del) * del ;
            time = now;
        }
        private void ReSync() {
            if (progress != 0) {
                time -= (long)(progress * del / inc);
            }
        }


        public long Max {
            get => max;
            set {
                Sync(); 
                max = value;
                A.Assert(max >= 0);
                ReSync();
            }
        }
        public long Inc {
            get => inc;
            set {
                Sync(); 
                inc = value;
                A.Assert(inc >= 0);
                ReSync();
            }
        }
        public long Del {
            get => del;
            set {
                Sync(); 
                del = value;
                A.Assert(del > 0);
                ReSync();
            }
        }
        public long DelSecond => Del / Constants.Second;


        public long Value {
            get {
                // long turn = (Now - time) / Del;
                // long result = turn * Inc + val;
                long result = (Now - time) * Inc / Del + val;
                return Clamp(0, max, result);
            }
            set {
                val = Clamp(0, max, value);
                time = Now;
            }
        }

        public void Clear() {
            time = Now;
            val = 0;
        }


        public bool Empty => Value <= 0;
        public bool Maxed => Value >= Max;

        public bool FastSlider => Inc * Constants.Second >= 10 * Del;

        public float Progress => Max == 0 ? 0 : Value >= max ? 1 : Inc == 0 ? (float)(progress) : (float)((double)(Now - time) * inc / del) % 1;
        public float TotalProgress {
            get {
                return Max == 0 ? 0 : Clamp(0, 1, (float)((double)(Value + Progress) / Max));
            }
        }
        public void RandomizeAllProgress(uint hash) {
            Value = hash % (Max + 1); // value
            time -= (long)(H.HashFloat(hash, Salt.Idle) * Del); // progress
        }
    }
}

