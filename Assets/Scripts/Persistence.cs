
using System.IO;

namespace W
{
    public interface IPersistent
    {
        void OnConstruct();
        void OnCreate();
        void OnLoad();
        void AfterConstruct();
    }

    //public abstract class APersistent : IPersistent
    //{
    //    void IPersistent.AfterConstruct() { }
    //    void IPersistent.OnConstruct() { }
    //    void IPersistent.OnCreate() { }
    //    void IPersistent.OnLoad() { }
    //}

    /// <summary>
    /// 包装System.IO里的一些方法
    /// 用于文件和文件夹增删改查
    /// </summary>
    public static class Persistence
    {
        public static T Create<T>(string contents = null) where T : class, new() {
            T t;
            IPersistent persistent;
            if (contents == null) {
                t = new T();
                persistent = t as IPersistent;
                persistent?.OnConstruct();
                persistent?.OnCreate();
            } else {
                if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor) {
                    t = Serialization.Deserialize(contents) as T;
                    persistent = t as IPersistent;
                    persistent?.OnConstruct();
                    persistent?.OnLoad();
                } else {
                    try {
                        t = Serialization.Deserialize(contents) as T;
                        persistent = t as IPersistent;
                        persistent?.OnConstruct();
                        persistent?.OnLoad();
                    } catch {
                        t = null;
                        return t;
                    }
                }
            }
            persistent?.AfterConstruct();
            return t;
        }


        public static string Saves {
            get {
                if (saves == null) {
                    saves = Combine(UnityEngine.Application.persistentDataPath, "saves/");
                }
                return saves;
            }
        }
        private static string saves = null;

        public static void Save(string directory, string file, in string contents) {
            string dir = DirOf(directory);
            CreateDirectory(dir);
            string path = Combine(dir, file);
            Write(path, contents);
        }
        public static void Load(string directory, string file, out string contents) {
            string dir = DirOf(directory);
            CreateDirectory(dir);
            contents = Read(Combine(dir, file));
        }
        private static string DirOf(string directory) => directory == null ? Saves : Combine(Saves, directory);

        public static string Combine(string path, string file) => System.IO.Path.Combine(path, file);




        public static void ClearSaves() {
            Clear(Saves);
        }
        private static void Clear(string directory) {
            DirectoryInfo dir = new DirectoryInfo(directory);
            if (!dir.Exists) {
                return;
            }
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            if (fileinfo.Length == 0) {
                return;
            }
            foreach (FileSystemInfo info in fileinfo) {
                if (info is DirectoryInfo) {
                    Clear(info.FullName);
                } else {
                    info.Delete();
                }
            }
        }




        public static DirectoryInfo CreateDirectory(string path) {
            return Directory.CreateDirectory(path);
        }

        public static string Read(string path) {
            if (File.Exists(path)) {
                return File.ReadAllText(path);
            } else {
                return null;
            }
        }
        public static void Write(string path, string contents) {
            File.WriteAllText(path, contents);
        }
    }
}

