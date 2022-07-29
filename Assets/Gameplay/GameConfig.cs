
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace W
{
    public interface IGameConfig
    {
        void Init();
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GameConfig : IGameConfig
    {

        public static GameConfig I => Game.I.Config;
        /// <summary>
        /// 每次读取
        /// </summary>
        [JsonIgnore]
        private readonly Dictionary<string, ID> name2obj;
        public IReadOnlyDictionary<string, ID> Name2Obj => name2obj;

        /// <summary>
        /// 真正保存
        /// </summary>
        [JsonProperty]
        private Dictionary<uint, string> id2name;
        protected IReadOnlyDictionary<uint, string> ID2Name => id2name;


        /// <summary>
        /// 用于查询
        /// </summary>
        [JsonIgnore]
        private Dictionary<uint, ID> id2obj;
        public IReadOnlyDictionary<uint, ID> ID2Obj => id2obj;


        public GameConfig() {
            name2obj = GameConfigReference.I.__Name2Config();
            CheckIDValue();
            BindBonusAndConditions();
        }

        private void CheckIDValue() {
            if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor) {
                return;
            }
            foreach (var pair in name2obj) {
                if (pair.Value is IDValue idValue) {
                    A.Assert(idValue.Key != null, () => $"未配key {idValue.CN}");
                    A.Assert(idValue.Value != 0, () => $"未配value {idValue.CN}");
                }
            }
        }

        private void BindBonusAndConditions() {
            // unlockRelation = new Dictionary<TileDef, HashSet<TileDef>>();
            foreach (var pair in name2obj) {
                if (pair.Value is TileDef value) {
                    foreach (TileDef bonusKey in value.Bonus) {
                        HashSet<TileDef> set = bonusKey.BonusReverse as HashSet<TileDef>;
                        if (!set.Contains(value)) {
                            set.Add(value);
                        }
                    }
                    foreach (TileDef conditionKey in value.Conditions) {
                        HashSet<TileDef> set = conditionKey.ConditionsReverse as HashSet<TileDef>;
                        if (!set.Contains(value)) {
                            set.Add(value);
                        }
                    }
                }
            }
        }


        void IGameConfig.Init() {
            id2name = new Dictionary<uint, string>();
            uint i = 1;
            foreach (var pair in name2obj) {
                id2name.Add(i, pair.Key);
                i++;
            }
        }

        public void Prepare() {
            A.Assert(id2obj == null);

            id2obj = new Dictionary<uint, ID>();
            foreach (var pair in id2name) {
                ID obj = name2obj[pair.Value];
                id2obj.Add(pair.Key, obj);
                (obj as __ID).SetID(pair.Key);
            }
        }

        public void TryLogAll() {
            UnityEngine.Debug.Log($"logs  ");
            foreach (var pair in id2name) {
                UnityEngine.Debug.Log($"{pair.Key}   {pair.Value}   ");
            }
        }
    }
}
