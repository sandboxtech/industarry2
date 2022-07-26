
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BiDictionary<TFirst, TSecond>
    {
        [JsonProperty]
        private Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        [JsonProperty]
        private Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public void Add(TFirst first, TSecond second) {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second)) {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second) {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first) {
            return secondToFirst.TryGetValue(second, out first);
        }
    }
}
