
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
                // Game.I.Relativity.TimeScale = Game.I.Relativity.TimeScale > 1 ? 1 : 3;
            }
        }



        private static void IconText(ID cn) => UI.IconText(cn.CN, cn.Icon, cn.Color);

        private static void IconTextWithLevel(ID cn, int level) => UI.IconText(NameAndLevel(cn, level), cn.Icon, cn.Color);
        private static void IconButtonWithLevel(ID cn, int level, Color textColor, Action action)
            => UI.IconButton(NameAndLevel(cn, level), textColor, cn.Icon, cn.Color, action);
        private static string NameAndLevel(ID cn, int level) => level <= 1 ? cn.CN : $"{cn.CN} * {level}";


        private static void IconTextWithValueAndColor(ID id, long v, long multiplier, bool canChange) {
            long product = v * multiplier;
            bool positive = product > 0;
            UI.IconText($"{id.CN} {(positive ? "+" : "-")}{(v > 0 ? v : -v)}{(multiplier == 1 ? "" : $"*{(multiplier > 0 ? multiplier : -multiplier)}")}",
                !canChange ? ColorWarning : positive ? ColorPositive : ColorNegative,
                id.Icon, id.Color);
        }

        private static void CanChangeText(Map map, ResDefValue idValue, long level, IdleReference i) {
            bool canChange = map.CanChange(idValue, level, i);
            IconTextWithValueAndColor(idValue.Key, idValue.Value, level, canChange);
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
            } else {
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
                TapNothing(info);
                return;
            }

            uint existingID = info.Map.ID(info.X, info.Y);
            if (existingID == ID.Invalid) {
                TapNothing(info);
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
                uint index = info.Map.IndexOf(info.X, info.Y);
                if (index == info.Map.PreviousMapIndex) {
                    MapDef mapDef = GameConfig.I.ID2Obj[info.Map.PreviousMapDefID] as MapDef;
                    A.Assert(mapDef != null);
                    TapPortal(info, mapDef);
                } else if (info.Map.SubMaps != null && info.Map.SubMaps.TryGetValue(index, out uint mapDefID)) {
                    MapDef mapDef = GameConfig.I.ID2Obj[mapDefID] as MapDef;
                    A.Assert(mapDef != null);
                    TapPortal(info, mapDef);
                } else {
                    TapEmpty(info);
                }
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Destruct, info.X, info.Y);

            } else {
                MapView.I.ScaleTileAt(info.X, info.Y);
                TapExisting(info);
            }
        }

        private static void TapPortal(MapTileInfo info, MapDef mapDef) {
            UI.Prepare();

            IconText(mapDef);
            UI.Space();

            if (mapDef.NotAccessible) {
                Sprites.IconText("无法进入", Sprites.Information);
            } else {
                UI.Button("进入", () => Game.I.EnterMap(info.Map.IndexOf(info.X, info.Y), mapDef.id));
            }

            UI.Show();
        }

        private static void TapNothing(MapTileInfo info) {
            ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, info.X, info.Y);
            MapTapping.I.HideIndicator();

            UI.Prepare();

            AddMapHeadInfo(info.Map);

            //if (false) {
            //    UI.Button(Game.I.OnShip ? "离开飞船" : "进入飞船", () => {
            //        Game.I.OnShip = !Game.I.OnShip;
            //    });
            //    UI.Space();
            //}

            UI.Button("设置", SettingsPage.Settings);
            UI.Space();

            //foreach (var pair in Game.I.Map.Resources) {
            //    ID id = GameConfig.I.ID2Obj[pair.Key];
            //    Idle idle = pair.Value;
            //    UI.IconText(id.CN, id.Icon);
            //    UI.Progress(() => $"{idle.Value}/{idle.Max}  +{idle.Inc}/{idle.DelSecond}s", () => idle.Progress);
            //}

            UI.Show();
        }

        private static void AddMapHeadInfo(Map map) {
            // UI.IconText(map.Def.CN, GameConfig.I.Name2Obj[Sprites.MapDef].Icon);
            UI.IconButton(map.Def.CN, map.Def.Sprite, () => ShowMapPage(map));

            UI.Space();
        }
        private static void ShowMapPage(Map map) {
            UI.Prepare();

            UI.IconText(map.Def.CN, map.Def.Sprite);
            UI.Space();

            if (Game.I.OnShip) {

            } else {
                UI.IconButton("航行", Game.I.SuperMap.CanEnter() ? ColorNormal : ColorNegative, Sprites.IconOf(Sprites.Failure), () => {
                    if (Game.I.SuperMap.CanEnter()) {
                        Game.I.EnterSuperMap();
                    }
                    else {
                        FailGotoSuperMapPage();
                    }
                });

                if (Game.I.Map.PreviousSeed != Map.NullSeed && Game.I.Map.PreviousMapLevel <= Game.I.Map.MapLevel) {
                    UI.Button("返回", () => Game.I.EnterPreviousMap());
                }
                UI.Space();
            }

            UI.Text("地图种子");
            UI.Text(map.Seed.ToString());

            UI.Space();
            UI.Text("钟慢效应");
            UI.Text(map.Def.TimeScale.ToString());

            UI.Show();
        }

        private static void FailGotoSuperMapPage() {
            UI.Prepare();

            Sprites.IconText(Sprites.Failure);

            foreach (TechDefValue techDefValue in Game.I.SuperMap.Def.TechRequirementForEntrence) {
                Game.I.TechLevel(techDefValue.Key.id, out int level);
                if (level < techDefValue.Value) {
                    // return false;
                    UI.Space();

                    IconText(techDefValue.Key);
                    UI.IconText($"等级 {level} < {techDefValue.Value}", Sprites.IconOf(Sprites.Level));
                }
            }

            UI.Show();
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
            if (info.TileDef.NotDestructable) {
                UI.Space();
            } else {
                Sprites.IconButton(Sprites.Destruction, CanDestruct(info) ? ColorNormal : ColorDisable, Game.I.Settings.SkipConfirmation ? () => TryDestruct(info) : () => AskDestruct(info));
            }

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
            foreach (ResDefValue idValue in techDef.Upgrade) {

                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);
                bool canChange = map.CanChange(idValue, -cost, IdleReference.Val);
                if (!canChange) return false;
            }
            return true;
        }
        private static void AddTechCosts(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (ResDefValue idValue in techDef.Upgrade) {
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

            foreach (ResDefValue idValue in techDef.Upgrade) {
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
                foreach (ResDefValue idValue in existing.Inc) {
                    IconTextWithValueAndColor(idValue.Key, idValue.Value, existingLevel, true);
                }
            }

            if (existing.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (ResDefValue idValue in existing.Max) {
                    IconTextWithValueAndColor(idValue.Key, idValue.Value, existingLevel, true);
                }
            }

            if (existing.Construction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Construction);
                foreach (ResDefValue idValue in existing.Construction) {
                    IconTextWithValueAndColor(idValue.Key, idValue.Value, existingLevel, true);
                }
            }

            if (existing.Destruction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Destruction);
                foreach (ResDefValue idValue in existing.Destruction) {
                    IconTextWithValueAndColor(idValue.Key, idValue.Value, existingLevel, true);
                }
            }
            if (existing.NotDestructable) {
                UI.Space();
                Sprites.IconText("无法拆除", Sprites.Failure);
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

            if ((existing.BonusReverse != null && existing.BonusReverse.Count > 0)) {
                UI.Space();
                Sprites.IconText(Sprites.BonusReverse);
                foreach (TileDef tile in existing.BonusReverse) {
                    IconText(tile);
                }
            }
            //if ((existing.BonusReverse != null && existing.BonusReverse.Count > 0) || (existing.ConditionsReverse != null && existing.ConditionsReverse.Count > 0)) {
            //    UI.Space();
            //    Sprites.IconText(Sprites.BonusReverse);
            //    foreach (TileDef tile in existing.BonusReverse) {
            //        IconText(tile);
            //    }
            //    foreach (TileDef tile in existing.ConditionsReverse) {
            //        IconText(tile);
            //    }
            //}
        }





        private static void TapEmpty(MapTileInfo info) {
            UI.Prepare();

            AddMapHeadInfo(info.Map);

            var unlockeds = AddUnlockeds(info);
            if (unlockeds.Count > 0) {
                Sprites.IconText(Sprites.Construction);
                UI.Space();

                foreach (TileDef unlocked in unlockeds) {

                    info.TileDef = unlocked;
                    CalcReplacement(info);

                    IconButtonWithLevel(info.TileDef, info.Level, CanConstruct(info, false) ? ColorNormal : ColorDisable, () => {

                        info.TileDef = unlocked;
                        CalcReplacement(info);

                        if (Game.I.Settings.SkipConfirmation) {
                            TryConstruct(info, false);
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
            //if (constructables.Count == 0) {
            foreach (TileDef unlocked in info.Map.Def.Constructables) {
                if (!constructables.Contains(unlocked)) {
                    constructables.Add(unlocked);
                }
            }
            //}
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

            UI.Space();

            Sprites.IconButton(Sprites.Construction, () => {
                TryConstruct(info, false);
            });

            AddConstructInfo(info);

            UI.Show();
        }
        private static void AddConstructInfo(MapTileInfo info) {
            if (info.TileDef.Construction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (ResDefValue idValue in info.TileDef.Construction) {
                    CanChangeText(info.Map, idValue, 1, IdleReference.Val);
                }
            }

            if (info.TileDef.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (ResDefValue idValue in info.TileDef.Inc) {
                    CanChangeText(info.Map, idValue, info.Level, IdleReference.Inc);
                }
            }

            if (info.TileDef.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (ResDefValue idValue in info.TileDef.Max) {
                    CanChangeText(info.Map, idValue, info.Level, IdleReference.Max);
                }
            }
        }

        private static bool CanConstruct(MapTileInfo info, bool ignoreConstruction = false) {
            if (!ignoreConstruction) {
                foreach (ResDefValue idValue in info.TileDef.Construction) {
                    if (!info.Map.CanChange(idValue, 1, IdleReference.Val)) return false;
                }
            }

            foreach (ResDefValue idValue in info.TileDef.Inc) {
                if (!info.Map.CanChange(idValue, info.Level, IdleReference.Inc)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.Max) {
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

        public static bool TryConstruct(MapTileInfo info, bool ignoreConstruction) {
            if (!CanConstruct(info, ignoreConstruction)) {
                FailConstruct(info, ignoreConstruction);
                return false;
            }

            SucceedConstruct(info);
            info.Map.ID(info.X, info.Y, info.TileDef.id);
            info.Map.Level(info.X, info.Y, info.Level);
            if (info.EnableUI) MapDataView.ShowTile(info.X, info.Y, info.TileDef, info.Level, true);

            foreach (ResDefValue idValue in info.TileDef.Construction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (ResDefValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.Max) {
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
        private static void FailConstruct(MapTileInfo info, bool ignoreConstruction) {
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
            if (!ignoreConstruction) {
                title = false;
                foreach (ResDefValue idValue in info.TileDef.Construction) {
                    FailConstructText(info.Map, idValue, 1, IdleReference.Val, ref title);
                }
            }

            title = false;
            foreach (ResDefValue idValue in info.TileDef.Inc) {
                FailConstructText(info.Map, idValue, info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Max) {
                FailConstructText(info.Map, idValue, info.Level, IdleReference.Max, ref title);
            }
            title = false;

            UI.Show();
        }
        private static void FailConstructText(Map map, ResDefValue idValue, long level, IdleReference i, ref bool title) {
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
                IconTextWithValueAndColor(idValue.Key, idValue.Value, level, canConstruct);
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

            UI.Space();

            if (info.TileDef.NotDestructable) {
                UI.Space();
            } else {
                Sprites.IconButton(Sprites.Destruction, CanDestruct(info) ? ColorNormal : ColorDisable, () => {
                    TryDestruct(info);
                });
            }

            AddDestructInfo(info);

            UI.Show();
        }

        private static void AddDestructInfo(MapTileInfo info) {
            if (info.TileDef.Destruction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (ResDefValue idValue in info.TileDef.Destruction) {
                    CanChangeText(info.Map, idValue, 1, IdleReference.Val);
                }
            }

            if (info.TileDef.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (ResDefValue idValue in info.TileDef.Inc) {
                    CanChangeText(info.Map, idValue, -info.Level, IdleReference.Inc);
                }
            }

            if (info.TileDef.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (ResDefValue idValue in info.TileDef.Max) {
                    CanChangeText(info.Map, idValue, -info.Level, IdleReference.Max);
                }
            }
        }

        private static bool CanDestruct(MapTileInfo info) {
            if (info.TileDef.NotDestructable) return false;

            foreach (ResDefValue idValue in info.TileDef.Destruction) {
                if (!info.Map.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.Inc) {
                if (!info.Map.CanChange(idValue, -info.Level, IdleReference.Inc)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.Max) {
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

            foreach (ResDefValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.Max) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Max);
            }

            foreach (ResDefValue idValue in info.TileDef.Construction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }
        }


        private static void SucceedDestruct(MapTileInfo info) {
            if (!info.EnableUI) return;

            Audio.I.Clips = Audio.I.DestructSound;

            UI.Prepare();

            IconText(info.TileDef);
            UI.Space();
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
            foreach (ResDefValue idValue in info.TileDef.Destruction) {
                FailDestructText(info.Map, idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Inc) {
                FailDestructText(info.Map, idValue, -info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Max) {
                FailDestructText(info.Map, idValue, -info.Level, IdleReference.Max, ref title);
            }

            UI.Show();
        }

        private static void FailDestructText(Map map, ResDefValue idValue, long level, IdleReference i, ref bool title) {
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
                IconTextWithValueAndColor(idValue.Key, idValue.Value, level, canConstruct);
            }
        }

















    }
}


