
namespace W
{
    public static class A
    {
        public static void Singleton<T>(ref T instance, T self) where T : class {
            Assert(instance == null);
            instance = self;
        }
        public static void Assert(bool assertion) {
            if (!assertion) throw new System.Exception("assertion failed");
        }
        public static void Assert(bool assertion, string message) {
            if (!assertion) throw new System.Exception(message);
        }
        public static void Assert(bool assertion, System.Func<string> messageGetter) {
            if (!assertion) throw new System.Exception(messageGetter.Invoke());
        }

        public static void Error(string message) {
            throw new System.Exception(message);
        }

        public static void Log(object obj) => UnityEngine.Debug.Log(obj.ToString());
    }
}
