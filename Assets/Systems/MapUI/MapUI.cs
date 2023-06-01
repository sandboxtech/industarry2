
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

            // 注册事件
            MapTapping.I.OnTap = OnTap;

        }

        private void Start() {
            UI.I.Button0.onClick.AddListener(SettingsPage.Settings);
            UI.I.Button1.onClick.AddListener(() => Game.I.Map.ShowPage());
            // UI.I.ButtonResources.onClick.AddListener(() => Game.I.Map.AllLocalResourcesPage());
        }

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
                MapSuper = Game.I.SuperMap,
                PlayerBuild_OrAutoBuild = true,
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
                    TapPortal(info, mapDef, true);
                } else if (info.Map.SubMaps != null && info.Map.SubMaps.TryGetValue(index, out uint mapDefID)) {
                    MapDef mapDef = GameConfig.I.ID2Obj[mapDefID] as MapDef;
                    A.Assert(mapDef != null);
                    TapPortal(info, mapDef, false);
                } else {
                    TapEmpty(info);
                }
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Destruct, info.X, info.Y);

            } else {
                if (!info.TileDef.NotDestructable) MapView.I.ScaleTileAt(info.X, info.Y);
                TapExisting(info);
            }
        }

        private static void TapPortal(MapTileInfo info, MapDef mapDef, bool isPreviousMap) {
            UI.Prepare();

            mapDef.IconText();
            UI.Space();

            if (mapDef.NotAccessible) {
                SpriteUI.IconText("无法进入", SpriteUI.Information);
            } else {
                if (isPreviousMap) {
                    UI.Button("进入", () => Game.I.EnterPreviousMap());
                } else {
                    UI.Button("进入", () => Game.I.EnterMap(info.Map.IndexOf(info.X, info.Y), mapDef.id));
                }
            }

            UI.Show();
        }

        private static void TapNothing(MapTileInfo info) {
            ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, info.X, info.Y);
            MapTapping.I.HideIndicator();

            UI.Hide();

            //UI.Prepare();
            //info.Map.AddButton();
            //UI.Space();
            //UI.Button("设置", SettingsPage.Settings);
            //UI.Show();
        }


        private static void TapSomething(int x, int y) {
            MapTapping.I.ShowIndicatorAt(x, y);
        }





















        /// <summary>
        /// 按下已有建筑
        /// </summary>
        private static void TapExisting(MapTileInfo info) {
            UI.Prepare();

            if (info.TileDef.NotDestructable) {
                UI.Space();
            } else {
                SpriteUI.IconButton(SpriteUI.Destruction, CanDestruct(info) ? UI.ColorNormal : UI.ColorDisable, () => TryDestruct(info));
            }

            UI.Space();

            info.TileDef.IconButton(info.TileDef.ShowPage);

            UI.Show();
        }



        private static void TapEmpty(MapTileInfo info) {
            UI.Prepare();

            var unlockeds = AddUnlockeds(info);
            if (unlockeds.Count > 0) {
                SpriteUI.IconText(SpriteUI.Construction);
                UI.Space();

                foreach (TileDef unlocked in unlockeds) {

                    info.TileDef = unlocked;
                    CalcLevel(info);

                    info.TileDef.IconButtonWithLevel(info.Level, CanConstruct(info) ? UI.ColorNormal : UI.ColorDisable, () => {
                        info.TileDef = unlocked;
                        CalcLevel(info);

                        TryConstruct(info);
                    });
                }
            } else {
                UI.Text("无法建造");
            }
            UI.Show();
        }

        public static void CalcLevel(MapTileInfo info) {
            if (info.TileDef == null) {
                A.Error($"建设的物体不能为空");
            }
            if (Game.I.Settings.MaxLevel == 1) {
                info.Level = 1;
            } else if (info.PlayerBuild_OrAutoBuild) {
                int bonus = CalcBonus(info);
                int tech = CalcTech(info.TileDef);
                info.Level = bonus * tech;
                info.Level = info.Level > Game.I.Settings.MaxLevel ? Game.I.Settings.MaxLevel : info.Level;
            } else {
                info.Level = 1;
            }
        }


        private static readonly HashSet<TileDef> constructables = new HashSet<TileDef>();
        private static HashSet<TileDef> AddUnlockeds(MapTileInfo info) {
            constructables.Clear();

            AddUnlockedForSubmap(info, 0, +1);
            AddUnlockedForSubmap(info, 0, -1);
            AddUnlockedForSubmap(info, +1, 0);
            AddUnlockedForSubmap(info, -1, 0);

            AddUnlocked(info);

            if (constructables.Count == 0) {
                foreach (TileDef unlocked in info.Map.Def.ConstructablesFree) {
                    TryAdd(info, unlocked);
                }
            }
            return constructables;
        }

        [System.NonSerialized]
        private static List<TileDef> tempNeighbors = new List<TileDef>();
        private static void AddUnlocked(MapTileInfo info) {
            tempNeighbors.Clear();
            AddTempNeighbors(info, 0, +1);
            AddTempNeighbors(info, 0, -1);
            AddTempNeighbors(info, +1, 0);
            AddTempNeighbors(info, -1, 0);

            if (tempNeighbors.Count == 0) return;

            foreach (TileDef constructable in info.Map.Def.ConstructablesPaid) {
                if (constructable.ConstructionCondition != null && !tempNeighbors.Contains(constructable.ConstructionCondition)) {
                    continue;
                }

                // 对于每个可能要造的建筑
                bool allSatisfied = true;

                if (info.Map.MapLevel > MapDefName.PlanetLevel) {
                    foreach (var input in constructable.Inc) {
                        if (input.Value > 0) continue;
                        // 如果已经满足需求，可以建造
                        if (info.Map.Resources.TryGetValue(input.Key.id, out Idle idle)) {
                            if (idle.Inc + input.Value >= 0) {
                                continue;
                            }
                        }
                    }
                }
                else {
                    foreach (var input in constructable.Inc) {
                        if (input.Value > 0) continue;
                        // 如果已经满足需求，可以建造

                        // 对于这个建筑的每种需求
                        if (input.Key == null) A.Assert(false, $"null in inc : {constructable}");
                        bool satisfied = false;
                        foreach (var neighbor in tempNeighbors) { // max count 4;
                            foreach (var output in neighbor.Inc) {
                                if (output.Value < 0) continue;
                                if (output.Key == input.Key) {
                                    satisfied = true;
                                    break;
                                }
                            }
                            if (satisfied) break;
                        }
                        if (!satisfied) {
                            allSatisfied = false;
                            break;
                        }
                    }
                }

                if (allSatisfied) {
                    TryAdd(info, constructable);
                }
            }
            tempNeighbors.Clear();
        }
        private static void AddTempNeighbors(MapTileInfo info, int dx, int dy) {
            int posX = info.X + dx;
            int posY = info.Y + dy;
            if (!info.Map.HasPosition(posX, posY)) {
                return;
            }
            uint neighborID = info.Map.ID(posX, posY);
            if (!ID.IsInvalid(neighborID)) {
                tempNeighbors.Add(GameConfig.I.ID2Obj[neighborID] as TileDef);
            }
        }


        private static void AddUnlockedForSubmap(MapTileInfo info, int dx, int dy) {
            int posX = info.X + dx;
            int posY = info.Y + dy;
            if (!info.Map.HasPosition(posX, posY)) {
                return;
            }
            uint neighborID = info.Map.ID(posX, posY);

            if (ID.IsInvalid(neighborID)) {
                // 查询上级地图以及传送门解锁
                uint index = info.Map.IndexOf(posX, posY);
                if (index == info.Map.PreviousMapIndex) {
                    MapDef neighborMap = GameConfig.I.ID2Obj[info.Map.PreviousMapDefID] as MapDef;
                    foreach (TileDef unlocked in neighborMap.ConditionsSubmapReverse) {
                        if (!constructables.Contains(unlocked)) {
                            constructables.Add(unlocked);
                        }
                    }
                }
                else if (info.Map.SubMaps != null && info.Map.SubMaps.Count > 0) {
                    if (info.Map.SubMaps.TryGetValue(index, out uint mapDefID)) {
                        MapDef neighborMap = GameConfig.I.ID2Obj[mapDefID] as MapDef;
                        foreach (TileDef unlocked in neighborMap.ConditionsSubmapReverse) {
                            if (!constructables.Contains(unlocked)) {
                                constructables.Add(unlocked);
                            }
                        }
                    }
                }
                return;
            }
            else {
                return;
                //// 查询普通解锁
                //TileDef neighbor = GameConfig.I.ID2Obj[neighborID] as TileDef;

                //foreach (var constructable in info.Map.Def.ConstructablesPaid) {
                //    foreach (var output in neighbor.Inc) {
                //        if (output.Value <= 0) continue;
                //        foreach (var input in constructable.Inc) {
                //            if (input.Value >= 0) continue;
                //            TryAdd(info, constructable);
                //        }
                //    }
                //}

                ////foreach (TileDef unlocked in neighbor.BonusReverse) {
                ////    if (!constructables.Contains(unlocked)) {
                ////        // constructables.Add(unlocked);
                ////        TryAdd(info, unlocked);
                ////    }
                ////}
                ////foreach (TileDef unlocked in neighbor.ConditionsReverse) {
                ////    if (!constructables.Contains(unlocked)) {
                ////        // constructables.Add(unlocked);
                ////        TryAdd(info, unlocked);
                ////    }
                ////}
            }
        }
        private static void TryAdd(MapTileInfo info, TileDef unlocked) {
            A.Assert(unlocked != null);
            if (!constructables.Contains(unlocked)) {
                // 没资源的建筑，完全不显示
                foreach (ResDefValue input in unlocked.Construction) {
                    if (info.Map.Resources.TryGetValue(input.Key.id, out Idle idle)) {
                        if (idle.Inc == 0 && idle.Max == 0) return;
                    } else {
                        return;
                    }
                }
                //foreach (ResDefValue input in unlocked.Inc) {
                //    if (input.Value < 0) {
                //        if (info.Map.Resources.TryGetValue(input.Key.id, out Idle idle)) {
                //            if (idle.Inc == 0 && idle.Max == 0) return;
                //        } else {
                //            return;
                //        }
                //    }
                //}
                constructables.Add(unlocked);
            }
        }


        private static int CalcBonus(MapTileInfo info) {
            return 1;
            //int bonus = 0;
            //bonus += AddBonus(info, 0, 0 + 1);
            //bonus += AddBonus(info, 0, 0 - 1);
            //bonus += AddBonus(info, 0 + 1, 0);
            //bonus += AddBonus(info, 0 - 1, 0);
            //if (bonus < 1) bonus = 1;
            //return bonus;
        }

        //private static int AddBonus(MapTileInfo info, int dx, int dy) {
        //    uint bonusID = info.Map.ID_Safe(info.X + dx, info.Y + dy);
        //    if (ID.IsInvalid(bonusID)) return 0;
        //    TileDef neighbor = GameConfig.I.ID2Obj[bonusID] as TileDef;
        //    A.Assert(neighbor != null);
        //    foreach (var bonus in info.TileDef.Bonus) {
        //        if (bonus == neighbor) return 1;
        //    }
        //    return 0;
        //    // return replacement.Bonus.Contains(neighbor) ? 1 : 0;
        //}

        private static int CalcTech(TileDef tileDef) {
            int result = 1;
            foreach (TechDef tech in tileDef.TechsBonus) {
                Game.I.TechLevel(tech.id, out int level);
                result += level;
            }
            return result;
        }



































        private static bool CanConstructTech(TileDef tileDef) {
            if (tileDef.TechRequirementForConstruction != null) {
                Game.I.TechLevel(tileDef.TechRequirementForConstruction.id, out int level);
                if (level <= 0) {
                    return false;
                }
            }
            return true;
        }

        private static bool CanConstruct(MapTileInfo info) {

            if (!CanConstructTech(info.TileDef)) {
                return false;
            }

            if (info.PlayerBuild_OrAutoBuild) {
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

            if (info.PlayerBuild_OrAutoBuild) {
                foreach (ResDefValue idValue in info.TileDef.ConstructionSuper) {
                    if (!info.MapSuper.CanChange(idValue, 1, IdleReference.Val)) return false;
                }
            }
            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                if (!info.MapSuper.CanChange(idValue, info.Level, IdleReference.Inc)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                if (!info.MapSuper.CanChange(idValue, info.Level, IdleReference.Max)) return false;
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
            //foreach (TileDef repel in info.TileDef.Repels) {
            //    if (repel == neighbor) return false;
            //}
            //foreach (TileDef repel in neighbor.Repels) {
            //    if (repel == info.TileDef) return false;
            //}
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

            if (info.PlayerBuild_OrAutoBuild) {
                MapDataView.ShowTile(info.X, info.Y, info.TileDef, info.Level, true);
            }

            info.Map.ChangeTileDefCount(info.TileDef.id, info.Level);
            info.MapSuper.ChangeTileDefCountOnSubmap(info.TileDef.id, info.Level);

            foreach (ResDefValue idValue in info.TileDef.Construction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (ResDefValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.Max) {
                info.Map.DoChange(idValue, info.Level, IdleReference.Max);
            }

            foreach (ResDefValue idValue in info.TileDef.ConstructionSuper) {
                info.MapSuper.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                info.MapSuper.DoChange(idValue, info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                info.MapSuper.DoChange(idValue, info.Level, IdleReference.Max);
            }

            return true;
        }

        private static void SucceedConstruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clips = Audio.I.ConstructSound;

            UI.Prepare();
            SpriteUI.IconText(SpriteUI.Construction);
            UI.Space();
            info.TileDef.IconTextWithLevel(info.Level);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Success);

            // AddConstructInfo

            UI.Show();
        }

        private static void FailConstruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clip = Audio.I.NegativeSound;

            UI.Prepare();

            info.TileDef.TileIconButton();
            UI.Space();
            SpriteUI.IconText(SpriteUI.Construction);
            UI.Space();

            if (!CanConstructTech(info.TileDef)) {
                SpriteUI.IconText("需要科技", SpriteUI.Failure);
                info.TileDef.TechRequirementForConstruction.IconText();
                UI.Show();
                return;
            }

            //neighbors.Clear();
            //FailConstructRepelText(info, 0, +1);
            //FailConstructRepelText(info, 0, -1);
            //FailConstructRepelText(info, +1, 0);
            //FailConstructRepelText(info, -1, 0);
            //if (neighbors.Count > 0) {
            //    SpriteUI.IconText("不能相邻", SpriteUI.Failure);

            //    UI.Space();
            //    SpriteUI.IconText(SpriteUI.Repel);
            //    foreach (TileDef neighbor in neighbors) {
            //        neighbor.IconText();
            //    }
            //} else {
            //    SpriteUI.IconText("缺少资源", SpriteUI.Failure);
            //}

            SpriteUI.IconText("缺少资源", SpriteUI.Failure);


            bool title;
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Construction) {
                FailConstructText(info.Map, false, idValue, 1, IdleReference.Val, ref title);
            }

            title = false;
            foreach (ResDefValue idValue in info.TileDef.Inc) {
                FailConstructText(info.Map, false, idValue, info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Max) {
                FailConstructText(info.Map, false, idValue, info.Level, IdleReference.Max, ref title);
            }

            title = false;
            foreach (ResDefValue idValue in info.TileDef.ConstructionSuper) {
                FailConstructText(info.MapSuper, true, idValue, 1, IdleReference.Val, ref title);
            }

            title = false;
            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                FailConstructText(info.MapSuper, true, idValue, info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                FailConstructText(info.MapSuper, true, idValue, info.Level, IdleReference.Max, ref title);
            }

            title = false;
            UI.Show();
        }

        private static void FailConstructText(Map map, bool isMapSuper, ResDefValue idValue, long level, IdleReference i, ref bool title) {
            bool canConstruct = map.CanChange(idValue, level, i);
            if (!canConstruct) {
                if (!title) {
                    title = true;
                    UI.Space();
                    switch (i) {
                        case IdleReference.Val:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.ValSuper : SpriteUI.Val);
                            idValue.AddValButton(level);
                            break;
                        case IdleReference.Inc:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.IncSuper : SpriteUI.Inc);
                            idValue.AddIncButton(level);
                            break;
                        case IdleReference.Max:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.MaxSuper : SpriteUI.Max);
                            idValue.AddMaxButton(level);
                            break;
                    }
                }
            }
        }

        //private static void FailConstructRepelText(MapTileInfo info, int dx, int dy) {
        //    TileDef neighbor = NeighborOf(info, dx, dy);
        //    if (neighbor == null) return;

        //    if (neighbors.Contains(neighbor)) return;
        //    foreach (TileDef repel in info.TileDef.Repels) {
        //        if (repel == neighbor) {
        //            neighbors.Add(neighbor);
        //        }
        //    }
        //    if (neighbors.Contains(neighbor)) return;
        //    foreach (TileDef repel in neighbor.Repels) {
        //        if (repel == info.TileDef) {
        //            neighbors.Add(neighbor);
        //        }
        //    }
        //}















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

            foreach (ResDefValue idValue in info.TileDef.DestructionSuper) {
                if (!info.MapSuper.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                if (!info.MapSuper.CanChange(idValue, -info.Level, IdleReference.Inc)) return false;
            }
            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                if (!info.MapSuper.CanChange(idValue, -info.Level, IdleReference.Max)) return false;
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

            if (info.PlayerBuild_OrAutoBuild) {
                MapDataView.ShowTile(info.X, info.Y, null, 0, true); // clear
            }

            info.Map.ChangeTileDefCount(info.TileDef.id, -info.Level);
            info.MapSuper.ChangeTileDefCountOnSubmap(info.TileDef.id, -info.Level);

            foreach (ResDefValue idValue in info.TileDef.Inc) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.Max) {
                info.Map.DoChange(idValue, -info.Level, IdleReference.Max);
            }

            foreach (ResDefValue idValue in info.TileDef.Destruction) {
                info.Map.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                info.MapSuper.DoChange(idValue, -info.Level, IdleReference.Inc);
            }

            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                info.MapSuper.DoChange(idValue, -info.Level, IdleReference.Max);
            }

            foreach (ResDefValue idValue in info.TileDef.ConstructionSuper) {
                info.MapSuper.DoChange(idValue, 1, IdleReference.Val);
            }
        }


        private static void SucceedDestruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clips = Audio.I.DestructSound;

            UI.Prepare();
            SpriteUI.IconText(SpriteUI.Destruction);
            UI.Space();
            info.TileDef.IconTextWithLevel(info.Level);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Success);

            UI.Show();
        }



        private static void FailDestruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clip = Audio.I.NegativeSound;

            UI.Prepare();

            info.TileDef.TileIconButton();
            UI.Space();
            SpriteUI.IconText(SpriteUI.Destruction);
            UI.Space();
            SpriteUI.IconText("缺少资源", SpriteUI.Failure);

            bool title;
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Destruction) {
                FailDestructText(info.Map, false, idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Inc) {
                FailDestructText(info.Map, false, idValue, -info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.Max) {
                FailDestructText(info.Map, false, idValue, -info.Level, IdleReference.Max, ref title);
            }

            title = false;
            foreach (ResDefValue idValue in info.TileDef.DestructionSuper) {
                FailDestructText(info.MapSuper, true, idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.IncSuper) {
                FailDestructText(info.MapSuper, true, idValue, -info.Level, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (ResDefValue idValue in info.TileDef.MaxSuper) {
                FailDestructText(info.MapSuper, true, idValue, -info.Level, IdleReference.Max, ref title);
            }

            UI.Show();
        }

        private static void FailDestructText(Map map, bool isMapSuper, ResDefValue idValue, long level, IdleReference i, ref bool title) {
            bool canDestruct = map.CanChange(idValue, level, i);
            if (!canDestruct) {
                if (!title) {
                    title = true;
                    UI.Space();
                    switch (i) {
                        case IdleReference.Val:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.ValSuper : SpriteUI.Val);
                            idValue.AddValButton(-level);
                            break;
                        case IdleReference.Inc:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.IncSuper : SpriteUI.Inc);
                            idValue.AddIncButton(-level);
                            break;
                        case IdleReference.Max:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.MaxSuper : SpriteUI.Max);
                            idValue.AddMaxButton(-level);
                            break;
                        default:
                            A.Assert(false);
                            break;
                    }
                }
            }
        }
    }
}


