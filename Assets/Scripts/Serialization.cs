
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace W
{
    /// <summary>
    /// 利用Newtonsoft.Json
    /// 进行序列化、反序列化
    /// </summary>
    public class Serialization
    {
        private static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings {

            MaxDepth = 16,
            // ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

            MissingMemberHandling = MissingMemberHandling.Ignore,

            MetadataPropertyHandling = MetadataPropertyHandling.Default, // 序列化自动生成 attribute property getter setter
            ReferenceLoopHandling = ReferenceLoopHandling.Error, // 循环依赖

            Formatting = Formatting.Indented, // 缩进
            DefaultValueHandling = DefaultValueHandling.Ignore, // 不序列化默认值
            TypeNameHandling = TypeNameHandling.All, // 只有 object 标出类型
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple, // 短 assembly name

            PreserveReferencesHandling = PreserveReferencesHandling.None,
        };

        public static string Serialize(object obj) {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static object Deserialize(string json) {
            return JsonConvert.DeserializeObject(json, Settings);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SerializableClassTemplate
    {
        [JsonProperty]
        private int a;
        [JsonIgnore]
        private int b;

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) {
        }

        [OnSerialized]
        private void OnSerializedMethod(StreamingContext context) {
        }

        [OnDeserializing]
        private void OnDeserializingMethod(StreamingContext context) {
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) {
        }
    }
}
