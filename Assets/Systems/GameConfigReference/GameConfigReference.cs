
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace W
{
    public class GameConfigReference : MonoBehaviour
    {
        public static GameConfigReference I => i;
        private static GameConfigReference i;
        private void Awake() {
            A.Singleton(ref i, this);
        }



        [SerializeField]
        private Sprite defaultSprite;
        public Sprite DefaultSprite => defaultSprite;

        [SerializeField]
        private List<ID> ids;

        public Dictionary<string, ID> __Name2Config() {
            Dictionary<string, ID> dictioanry = new Dictionary<string, ID>();
            for (uint i = 0; i < ids.Count; i++) {
                ID id = ids[(int)i];
                dictioanry.Add(id.name, id);
            }
            return dictioanry;
        }


#if UNITY_EDITOR

        [ContextMenu("根据名字，自动生成 IDValue 数值")]
        private void __AutoWriteIDValue() {
            foreach (ID id in ids) {
                if (id is IDValue idValue) {

                    if (idValue.name.StartsWith(idValue.Key.name)) {
                        string[] pair = idValue.name.Split('_');

                        if (pair.Length != 2) continue;

                        string number = pair[1];
                        int value;
                        if (number.StartsWith('N')) {
                            value = -int.Parse(number.Substring(1));

                        } else {
                            value = int.Parse(number);
                        }
                        if (value != idValue.Value) {
                            (idValue as _IDValue).SetValue(value);
                            EditorUtility.SetDirty(id);
                            AssetDatabase.SaveAssetIfDirty(id);
                            // AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
        }


        [SerializeField]
        private Font fontToFix;
        private void FixFont() {
            fontToFix.material.mainTexture.filterMode = FilterMode.Point;
        }

        public void ___Clear() {
            ids.Clear();
        }

        [ContextMenu("加工")]
        public void ___TryLoad() {
            FixFont();

            ids = new List<ID>();

            TryLoadAtDirectory($"Assets/ID");

            EditorUtility.SetDirty(this.gameObject);
            AssetDatabase.SaveAssetIfDirty(this.gameObject);

            if (DoDeleteSave) Persistence.ClearSaves();
        }

        [SerializeField]
        private bool DoDeleteSave = true;

        private void TryLoadAtDirectory(string path) {
            // Assets/DefConfigs/

            string[] entries = Directory.GetFileSystemEntries(path);
            for (int i = 0; i < entries.Length; i++) {
                string entry = entries[i];
                if (File.Exists(entry)) {
                    if (entry.EndsWith(".meta")) continue;
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(entry);
                    if (obj is ID id) {
                        ids.Add(id);
                    }
                } else if (Directory.Exists(entry)) {
                    TryLoadAtDirectory(entry);
                }
            }

        }
#endif

    }
}
