
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace W
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Tutorials
    {
        public static void ShowPage() {
            UI.Prepare();


            UI.Show();
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context) {
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context) {
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context) {
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context) {
        }

        public bool Cheat = false;

    }
}
