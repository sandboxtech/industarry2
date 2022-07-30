
using System;
using System.Collections.Generic;
using UnityEngine;

namespace W
{

    /// <summary>
    /// 代码大混合
    /// </summary>
    public class MapUI : MonoBehaviour
    {
        public static MapUI I => i;
        private static MapUI i;
        private void Awake() {
            A.Singleton(ref i, this);

            MapTapping.I.OnTap = OnTap;
        }



        private void Update() {
            if (Input.GetKeyDown(KeyCode.Z)) {
                // Map.DoChange(GameConfig.I.Name2Obj["Population_1"] as IDValue, 1, IdleReference.Inc);
            }
            if (Input.GetKeyDown(KeyCode.X)) {
                // Map.DoChange(GameConfig.I.Name2Obj["Population_N1"] as IDValue, 1, IdleReference.Inc);
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                // Game.I.EnterPreviousMap();
                Game.I.Relativity.TimeScale = Game.I.Relativity.TimeScale > 1 ? 1 : 3;
            }
        }



        private static void IconText(ID cn) => UI.IconText(cn.CN, cn.Icon, cn.Color);
        private static void IconTextWithLevel(ID cn, int level) => UI.IconText(NameAndLevel(cn, level), cn.Icon, cn.Color);
        private static void IconButtonWithLevel(ID cn, int level, Color textColor, Action action)
            => UI.IconButton(NameAndLevel(cn, level), textColor, cn.Icon, cn.Color, action);
        private static string NameAndLevel(ID cn, int level) => level <= 1 ? cn.CN : $"{cn.CN} * {level}";


        private static void IconTextWithValueAndColor(IDValue idValue, long multiplier, bool canChange) {
            long v = idValue.Value;
            long product = v * multiplier;
            bool positive = product > 0;
            UI.IconText($"{idValue.Key.CN} {(positive ? "+" : "-")}{(v > 0 ? v : -v)}{(multiplier == 1 ? "" : $"*{(multiplier > 0 ?  multiplier : -multiplier)}")}",
                !canChange ? ColorWarning : positive ? ColorPositive : ColorNegative,
                idValue.Key.Icon, idValue.Key.Color);
        }

        private static void CanChangeText(Map map, IDValue idValue, long level, IdleReference i) {
            bool canChange = map.CanChange(idValue, level, i);
            IconTextWithValueAndColor(idValue, level, canChange);
        }


        private static Color ColorDisable => new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private static Color ColorNormal => new Color(1f, 1f, 1f, 1f);
        private static Color ColorWarning => new Color(1f, 0, 0, 1f);

        private static Color ColorPositive => new Color(0.5f, 1f, 0.5f, 1f);
        private static Color ColorNegative => new Color(1f, 0.5f, 0.5f, 1f);







        /// <summary>
        /// 格子按下时的行为
        /// </summary>
        private static void OnTap() {
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                OnTap_();
            }
            else {
                try {
                    OnTap_();
                } catch (Exception e) {
                    UI.Prepare();
                    UI.Text(e.Message);
                    UI.Text(new System.Diagnostics.StackTrace().ToString());
                    UI.Show();
                    throw e;
                }
            }
        }

        private static void OnTap_() {

            Audio.I.Clip = Audio.I.DefaultSound;

            MapTileInfo info = new MapTileInfo() {
                X = MapTapping.I.X,
                Y = MapTapping.I.Y,
                Map = Game.I.Map,
                EnableUI = true,

            };

            if (!info.Map.HasPosition(info.X, info.Y)) {
                TapNothing(info.X, info.Y);
                return;
            }

            uint existingID = info.Map.ID(info.X, info.Y);
            if (existingID == ID.Invalid) {
                TapNothing(info.X, info.Y);
                return;
            }

            info.Level = info.Map.Level(info.X, info.Y);

            if (existingID == 0 && info.Level != 0) {
                A.Assert(false, () => "可能忘了更新 GameConfigReference");
            }

            TapSomething(info.X, info.Y);

            if (existingID == ID.Empty) {
                info.TileDef = null;
            } else {
                info.TileDef = GameConfig.I.ID2Obj[existingID] as TileDef;
                A.Assert(info.TileDef != null);
            }

            if (existingID == ID.Empty) {
                TapEmpty(info);
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Destruct, info.X, info.Y);
            } else {
                MapView.I.ScaleTileAt(info.X, info.Y);
                TapExisting(info);
            }
        }

        private static void TapNothing(int x, int y) {
            ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, x, y);
            SettingsPage.Show();
            MapTapping.I.HideIndicator();
        }

        private static void TapSomething(int x, int y) {
            MapTapping.I.ShowIndicatorAt(x, y);
        }





















        /// <summary>
        /// 按下已有建筑
        /// </summary>
        private static void TapExisting(MapTileInfo info) {
            UI.Prepare();

            IconTextWithLevel(info.TileDef, info.Level);

            //string info = existing.Description;
            //if (info != null) {
            //    UI.Space();
            //    UI.Text(info);
            //    UI.Space();
            //}

            if (info.TileDef.Techs.Count == 0) {
                UI.Space();
            } else {
                Sprites.IconButton(Sprites.TechDef, () => TapTechs(info));
            }

            Sprites.IconButton(Sprites.Information, () => {
                UI.Prepare();
                IconTextWithLevel(info.TileDef, info.Level);
                AddTileInformation(info.TileDef, info.Level);
                UI.Show();
            });

            UI.Space();
            Sprites.IconButton(Sprites.Destruction, CanDestruct(info) ? ColorNormal : ColorDisable, Game.I.Settings.SkipConfirmation ? () => TryDestruct(info) : () => AskDestruct(info));

            info.Map.AddRelatedResDefValue(info.TileDef);


            UI.Show();
        }

        private static void TapTechs(MapTileInfo info) {
            UI.Prepare();

            IconText(info.TileDef);
            Sprites.IconText(Sprites.TechDef);

            foreach (TechDef tech in info.TileDef.Techs) {
                AddTapTech(info.Map, tech);
            }

            UI.Show();
        }

        private static void AddTapTech(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            UI.Space();

            IconTextWithLevel(techDef, techLevel);

            if (techLevel == techDef.MaxLevel) {
                UI.IconButton("满级", ColorDisable, techDef.Icon, null);
            } else {
                bool canAfford = CanAffordTechCosts(map, techDef);

                UI.IconButton(LevelText(techLevel), canAfford ? ColorNormal : ColorDisable, Sprites.IconOf(Sprites.TechDef), !canAfford ? null : () => TryUpgradeTech(map, techDef));

                AddTechCosts(map, techDef);
            }
        }
        private static string LevelText(int level) => level == 0 ? "研发" : "升级";

        private static bool CanAffordTechCosts(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);
                bool canChange = map.CanChange(idValue, -cost, IdleReference.Val);
                if (!canChange) return false;
            }
            return true;
        }
        private static void AddTechCosts(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);
                bool canChange = map.CanChange(idValue, -cost, IdleReference.Val);
                UI.IconText($"{idValue.Key.CN} {cost}", canChange ? ColorNormal : ColorWarning,
                    idValue.Key.Icon, idValue.Key.Color);
            }
        }

        private static long CostOf(long upgradeValue, long multiplier, int techLevel) {
            long cost = 1;
            techLevel = M.Clamp(0, 30, techLevel);
            for (int i = 0; i < techLevel; i++) {
                cost *= multiplier;
            }
            return cost * upgradeValue;
        }

        private static void TryUpgradeTech(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);

                if (!map.CanChange(idValue, -cost, IdleReference.Val)) {
                    FailUpgrade(techDef);
                    return;
                }
            }

            Game.I.TechLevel(techDef.id, techLevel + 1);
            SucceedUpgrade(techDef);
        }

        private static void SucceedUpgrade(TechDef techDef) {
            UI.Prepare();
            IconText(techDef);
            Game.I.TechLevel(techDef.id, out int techLevel);
            UI.IconText($"升级 {techLevel} 成功", Sprites.IconOf(Sprites.Success));
            UI.Show();
        }
        private static void FailUpgrade(TechDef techDef) {
            UI.Prepare();
            IconText(techDef);
            Game.I.TechLevel(techDef.id, out int techLevel);
            UI.IconText($"升级 {techLevel} 失败", Sprites.IconOf(Sprites.Failure));
            UI.Show();
        }



        private static void AddTileInformation(TileDef existing, int existingLevel) {

            if (existing.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (IDValue idValue in existing.Inc) {
                    IconTextWithValueAndColor(idValue, existingLevel, true);
                }
            }

            if (existing.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (IDValue idValue in existing.Max) {
                    IconTextWithValueAndColor(idValue, existingLevel, true);
                }
            }

            if (existing.Construction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Construction);
                foreach (IDValue idValue in existing.Construction) {
                    IconTextWithValueAndColor(idValue, existingLevel, true);
                }
            }

            if (existing.Destruction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Destruction);
                foreach (IDValue idValue in existing.Destruction) {
                    IconTextWithValueAndColor(idValue, existingLevel, true);
                }
            }


            if (existing.Techs.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.TechDef);
                foreach (TechDef tech in existing.Techs) {
                    IconText(tech);
                }
            }

            if (existing.Bonus.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Bonus);
                foreach (TileDef bonus in existing.Bonus) {
                    IconText(bonus);
                }
            }

            if (existing.Conditions.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Condition);
                foreach (TileDef condition in existing.Conditions) {
                    IconText(condition);
                }
            }

            if (existing.Repels.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Repel);
                foreach (TileDef tile in existing.Repels) {
                    IconText(tile);
                }
            }
        }







        private static void TapEmpty(MapTileInfo info) {
            UI.Prepare();

            // UI.Text($"坐标 {x} {y}");
            UI.IconText(info.Map.Def.CN, GameConfig.I.Name2Obj[Sprites.MapDef].Icon);
            UI.Space();

            var unlockeds = AddUnlockeds(info);
            if (unlockeds.Count > 0) {
                Sprites.IconText(Sprites.Construction);
                UI.Space();

                foreach (TileDef unlocked in unlockeds) {

                    info.TileDef = unlocked;
                    CalcReplacement(info);

                    IconButtonWithLevel(info.TileDef, info.Level, CanConstruct(info) ? ColorNormal : ColorDisable, () => {

                        info.TileDef = unlocked;
                        CalcReplacement(info);

                        if (Game.I.Settings.SkipConfirmation) {
                            TryConstruct(info);
                        } else {
                            AskConstruct(info);
                        }
                    });
                }
            } else {
                UI.Text("无法建造");
            }
            UI.Show();
        }

        public static void CalcReplacement(MapTileInfo info) {
            if (info.TileDef == null) {
                A.Error($"建设的物体不能为空");
            }
            info.Level = CalcBonus(info) * CalcTech(info.TileDef);
        }


        private static readonly HashSet<TileDef> constructables = new HashSet<TileDef>();
        private static HashSet<TileDef> AddUnlockeds(MapTileInfo info) {
            constructables.Clear();
            AddUnlocked(info, 0, +1);
            AddUnlocked(info, 0, -1);
            AddUnlocked(info, +1, 0);
            AddUnlocked(info, -1, 0);
            if (constructables.Count == 0) {
                foreach (TileDef unlocked in info.Map.Def.Constructables) {
                    if (!constructables.Contains(unlocked)) {
                        constructables.Add(unlocked);
                    }
                }
            }
            return constructables;
        }
        private static void AddUnlocked(MapTileInfo info, int dx, int dy) {
            uint neighborID = info.Map.ID_Safe(info.X + dx, info.Y + dy);
            if (ID.IsInvalid(neighborID)) return;
            TileDef neighbor = GameConfig.I.ID2Obj[neighborID] as TileDef;
            foreach (TileDef unlocked in neighbor.BonusReverse) {
                if (!constructables.Contains(unlocked)) {
                    constructables.Add(unlocked);
                }
            }
            foreach (TileDef unlocked in neighbor.ConditionsReverse) {
                if (!constructables.Contains(unlocked)) {
                    constructables.Add(unlocked);
                }
            }
        }

        private static int CalcBonus(MapTileInfo info) {
            int bonus = 0;
            bonus += AddBonus(info, 0, 0 + 1);
            bonus += AddBonus(info, 0, 0 - 1);
            bonus += AddBonus(info, 0 + 1, 0);
            bonus += AddBonus(info, 0 - 1, 0);
            if (bonus < 1) bonus = 1;
            return bonus;
        }

        private static int AddBonus(MapTileInfo info, int dx, int dy) {
            uint bonusID = info.Map.ID_Safe(info.X + dx, info.Y + dy);
            if (ID.IsInvalid(bonusID)) return 0;
            TileDef neighbor = GameConfig.I.ID2Obj[bonusID] as TileDef;
            A.Assert(neighbor != null);
            foreach (var bonus in info.TileDef.Bonus) {
                if (bonus == neighbor) return 1;
            }
            return 0;
            // return replacement.Bonus.Contains(neighbor) ? 1 : 0;
        }

        private static int CalcTech(TileDef tileDef) {
            int result = 1;
            foreach (TechDef tech in tileDef.Techs) {
                Game.I.TechLevel(tech.id, out int level);
                result += level;
            }
            return result;
        }






















        private static void AskConstruct(MapTileInfo info) {
            UI.Prepare();

            IconTextWithLevel(info.TileDef, info.Level);

            Sprites.IconButton(Sprites.Construction, () => {
                TryConstruct(info);
            });

            AddConstructInfo(info);

            UI.Show();
        }
        private static void AddConstructInfo(MapTileInfo info) {
            if (info.TileDef.Construction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (IDValue idValue in info.TileDef.Construction) {
                    CanChangeText(info.Map, idValue, 1, IdleReference.Val);
                }
            }

            if (info.TileDef.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (IDValue idValue in info.TileDef.Inc) {
                    CanChangeText(info.Map, idValue, info.Level, IdleReference.Inc);
                }
            }

            if (info.TileDef.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (IDValue idValue in info.TileDef.Max) {
                    CanChangeText(info.Map, idValue, info.Level, IdleReference.Max);
                }
            }
        }

        private static bool CanConstruct(MapTileInfo info) {
            foreach (IDValue idValue in info.TileDef.Construction) {
                if (!info.Map.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (IDValue idValue in info.TileDef.Inc) {
                if (!info.Map.CanChange(idValue, info.Level, IdleReference.Inc)) return false;
            }
            foreach (IDValue idValue in info.TileDef.Max) {
                if (!info.Map.CanChange(idValue, info.Level, IdleReference.Max)) return false;
            }
            if (!CanConstructByNeighbor(info, 0, +1)) return false;
            if (!CanConstructByNeighbor(info, 0, -1)) return false;
            if (!CanConstructByNeighbor(info, +1, 0)) return false;
            if (!CanConstructByNeighbor(info, -1, 0)) return false;
            return true;
        }

        private static bool CanConstructByNeighbor(MapTileInfo info, int dx, int dy) {
            TileDef neighbor = NeighborOf(info, dx, dy);
            if (neighbor == null) return true;
            foreach (TileDef repel in info.TileDef.Repels) {
                if (repel == neighbor) return false;
            }
            foreach (TileDef repel in neighbor.Repels) {
                if (repel == info.TileDef) return false;
            }
            return true;
        }
        private static TileDef NeighborOf(MapTileInfo info, int dx, int dy) {
            uint neighborID = info.Map.ID_Safe(info.X + dx, info.Y + dy);
            if (ID.IsInvalid(neighborID)) return null;
            TileDef neighbor = GameConfig.I.ID2Obj[neighborID] as TileDef;
            return neighbor;
        }

        public static bool TryConstruct(MapTileInfo info) {
            if (!CanConstruct(info)) {
                FailConstruct(info);
                return false;
            }

            SucceedConstruct(info);
            info.Map.ID(info.X, info.Y, info.TileDef.id);
            info.Map.Level(info.X, info.Y, info.Level);
            if (info.EnableUI) MapDataView.ShowTile(info.X, info.Y, info.TileDef, info.Level, true);

            foreach (IDValue idValue in info.TileDef.Construction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (IDValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, info.Level, IdleReference.Inc);
            }

            foreach (IDValue idValue in info.TileDef.Max) {
                info.Map.DoChange(idValue, info.Level, IdleReference.Max);
            }

            return true;
        }

        private static void SucceedConstruct(MapTileInfo info) {
            if (!info.EnableUI) return;

            Audio.I.Clips = Audio.I.ConstructSound;

            UI.Prepare();

            IconText(info.TileDef);
            UI.Space();
            Sprites.IconText(Sprites.Construction);
            UI.Space();
            Sprites.IconText(Sprites.Success);

            AddConstructInfo(info);

            UI.Show();
        }

        private static readonly HashSet<TileDef> neighbors = new HashSet<TileDef>();
        private static void FailConstruct(MapTileInfo info) {
            if (!info.EnableUI) return;

            UI.Prepare();

            IconText(info.TileDef);
            UI.Space();
            Sprites.IconText(Sprites.Construction);
            UI.Space();
            Sprites.IconText(Sprites.Failure);

            neighbors.Clear();
            FailConstructRepelText(info, 0, +1);
            FailConstructRepelText(info, 0, -1);
            FailConstructRepelText(info, +1, 0);
            FailConstructRepelText(info, -1, 0);
            if (neighbors.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Repel);
                foreach (TileDef neighbor in neighbors) {
                    IconText(neighbor);
                }
            }
            neighbors.Clear();

            UI.Space();

            bool title;
            title = false;
            foreach (IDValue idValue in info.TileDef.Construction) {
                FailConstructText(info.Map, idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (IDValue idValue in info.TileDef.Inc) {
                FailConstructText(info.Map, idValue, info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (IDValue idValue in info.TileDef.Max) {
                FailConstructText(info.Map, idValue, info.Level, IdleReference.Max, ref title);
            }
            title = false;

            UI.Show();
        }
        private static void FailConstructText(Map map, IDValue idValue, long level, IdleReference i, ref bool title) {
            bool canConstruct = map.CanChange(idValue, level, i);
            if (!canConstruct) {
                if (!title) {
                    title = true;
                    switch (i) {
                        case IdleReference.Val:
                            Sprites.IconText(Sprites.Val);
                            break;
                        case IdleReference.Inc:
                            Sprites.IconText(Sprites.Inc);
                            break;
                        case IdleReference.Max:
                            Sprites.IconText(Sprites.Max);
                            break;
                    }
                }
                IconTextWithValueAndColor(idValue, level, canConstruct);
            }
        }

        private static void FailConstructRepelText(MapTileInfo info, int dx, int dy) {
            TileDef neighbor = NeighborOf(info, dx, dy);
            if (neighbor == null) return;

            if (neighbors.Contains(neighbor)) return;
            foreach (TileDef repel in info.TileDef.Repels) {
                if (repel == neighbor) {
                    neighbors.Add(neighbor);
                }
            }
            if (neighbors.Contains(neighbor)) return;
            foreach (TileDef repel in neighbor.Repels) {
                if (repel == info.TileDef) {
                    neighbors.Add(neighbor);
                }
            }
        }

















        private static void AskDestruct(MapTileInfo info) {
            UI.Prepare();

            IconTextWithLevel(info.TileDef, info.Level);

            Sprites.IconButton(Sprites.Destruction, CanDestruct(info) ? ColorNormal : ColorDisable, () => {
                TryDestruct(info);
            });

            AddDestructInfo(info);

            UI.Show();
        }

        private static void AddDestructInfo(MapTileInfo info) {
            if (info.TileDef.Destruction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (IDValue idValue in info.TileDef.Destruction) {
                    CanChangeText(info.Map, idValue, 1, IdleReference.Val);
                }
            }

            if (info.TileDef.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (IDValue idValue in info.TileDef.Inc) {
                    CanChangeText(info.Map, idValue, -info.Level, IdleReference.Inc);
                }
            }

            if (info.TileDef.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (IDValue idValue in info.TileDef.Max) {
                    CanChangeText(info.Map, idValue, -info.Level, IdleReference.Max);
                }
            }
        }

        private static bool CanDestruct(MapTileInfo info) {
            foreach (IDValue idValue in info.TileDef.Destruction) {
                if (!info.Map.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (IDValue idValue in info.TileDef.Inc) {
                if (!info.Map.CanChange(idValue, -info.Level, IdleReference.Inc)) return false;
            }
            foreach (IDValue idValue in info.TileDef.Max) {
                if (!info.Map.CanChange(idValue, -info.Level, IdleReference.Max)) return false;
            }
            return true;
        }

        private static void TryDestruct(MapTileInfo info) {
            if (!CanDestruct(info)) {
                FailDestruct(info);
                return;
            }

            SucceedDestruct(info);
            info.Map.ID(info.X, info.Y, ID.Empty);
            info.Map.Level(info.X, info.Y, 0);
            if (info.EnableUI) MapDataView.ShowTile(info.X, info.Y, null, 0, true);

            foreach (IDValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Inc);
            }

            foreach (IDValue idValue in info.TileDef.Max) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Max);
            }

            foreach (IDValue idValue in info.TileDef.Construction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }
        }


        private static void SucceedDestruct(MapTileInfo info) {
            if (!info.EnableUI) return;

            Audio.I.Clips = Audio.I.DestructSound;

            UI.Prepare();

            IconText(info.TileDef);
            Sprites.IconText(Sprites.Destruction);
            UI.Space();
            Sprites.IconText(Sprites.Success);

            AddDestructInfo(info);

            UI.Show();
        }



        private static void FailDestruct(MapTileInfo info) {
            if (!info.EnableUI) return;

            UI.Prepare();

            IconText(info.TileDef);
            Sprites.IconText(Sprites.Destruction);
            UI.Space();
            Sprites.IconText(Sprites.Failure);
            UI.Space();

            bool title;
            title = false;
            foreach (IDValue idValue in info.TileDef.Destruction) {
                FailDestructText(info.Map, idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (IDValue idValue in info.TileDef.Inc) {
                FailDestructText(info.Map, idValue, -info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (IDValue idValue in info.TileDef.Max) {
                FailDestructText(info.Map, idValue, -info.Level, IdleReference.Max, ref title);
            }

            UI.Show();
        }

        private static void FailDestructText(Map map, IDValue idValue, long level, IdleReference i, ref bool title) {
            bool canConstruct = map.CanChange(idValue, level, i);
            if (!canConstruct) {
                if (!title) {
                    title = true;
                    switch (i) {
                        case IdleReference.Val:
                            Sprites.IconText(Sprites.Val);
                            break;
                        case IdleReference.Inc:
                            Sprites.IconText(Sprites.Inc);
                            break;
                        case IdleReference.Max:
                            Sprites.IconText(Sprites.Max);
                            break;
                        default:
                            A.Assert(false);
                            break;
                    }
                }
                IconTextWithValueAndColor(idValue, level, canConstruct);
            }
        }

















    }
}


