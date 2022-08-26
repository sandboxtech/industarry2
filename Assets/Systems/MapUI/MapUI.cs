
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
                MapView.I.ScaleTileAt(info.X, info.Y);
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

            // info.TileDef.IconButtonWithLevel(info.Level, Color.white, info.TileDef.ShowPage);
            info.TileDef.IconButton(info.TileDef.ShowPage);

            if (info.TileDef.Techs.Count == 0 && info.TileDef.TechsRelavant.Count == 0) {
                UI.Space();
            } else {
                SpriteUI.IconButton(SpriteUI.TechDef, () => TapTechs(info));
            }

            if (info.TileDef.Inc.Count > 0 || info.TileDef.Max.Count > 0) {
                // UI.IconButton("相关资源", SpriteUI.IconOf(SpriteUI.ResDef), () => info.Map.AllLocalRelatedResourcesPage(info.TileDef));
                SpriteUI.IconButton("资源", SpriteUI.ResDef, () => info.Map.AllLocalRelatedResourcesPage(info.TileDef));
            } else {
                UI.Space();
            }
            UI.Space();

            if (info.TileDef.NotDestructable) {
                UI.Space();
            } else {
                SpriteUI.IconButton(SpriteUI.Destruction, CanDestruct(info) ? UI.ColorNormal : UI.ColorDisable, () => TryDestruct(info));
            }


            // AddTileInformationIncMax(info);

            UI.Show();
        }

        private static void TapTechs(MapTileInfo info) {
            UI.Prepare();

            info.TileDef.IconText();
            SpriteUI.IconText(SpriteUI.TechDef);

            foreach (TechDef tech in info.TileDef.Techs) {
                AddTapTech(info.Map, tech);
            }
            foreach (TechDef tech in info.TileDef.TechsRelavant) {
                AddTapTech(info.Map, tech);
            }

            UI.Show();
        }

        private static void AddTapTech(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            UI.Space();

            techDef.IconTextWithLevel(techLevel);

            if (techLevel == techDef.MaxLevel) {
                UI.IconButton(techDef.MaxLevel == 1 ? "已研究" : "已满级", UI.ColorDisable, techDef.Icon, null);
            } else {
                bool canAfford = CanAffordTechCosts(map, techDef);

                UI.IconButton(LevelText(techLevel), canAfford ? UI.ColorNormal : UI.ColorDisable, SpriteUI.IconOf(SpriteUI.TechDef), !canAfford ? null : () => TryUpgradeTech(map, techDef));

                AddTechCosts(map, techDef);
            }
        }
        private static string LevelText(int level) => level == 0 ? "研发" : "升级";

        private static bool CanAffordTechCosts(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (ResDefValue idValue in techDef.Upgrade) {

                long costMultiplier = CostMultiplierOf(techDef.Multiplier, techLevel);
                bool canChange = map.CanChange(idValue, -costMultiplier, IdleReference.Val);

                if (!canChange) return false;
            }
            return true;
        }
        private static void AddTechCosts(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (ResDefValue idValue in techDef.Upgrade) {
                long costMultiplier = CostMultiplierOf(techDef.Multiplier, techLevel);
                bool canChange = map.CanChange(idValue, -costMultiplier, IdleReference.Val);
                UI.IconText($"{idValue.Key.CN} {idValue.Value * costMultiplier}", canChange ? UI.ColorNormal : UI.ColorWarning,
                    idValue.Key.Icon, idValue.Key.Color);
            }
        }

        private static long CostMultiplierOf(long multiplier, int techLevel) {
            long cost = 1;
            techLevel = M.Clamp(0, 30, techLevel);
            for (int i = 0; i < techLevel; i++) {
                cost *= multiplier;
            }
            return cost;
        }

        private static void TryUpgradeTech(Map map, TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            //foreach (ResDefValue idValue in techDef.Upgrade) {
            //    long costMultiplier = CostMultiplierOf(techDef.Multiplier, techLevel);
            //    if (!map.CanChange(idValue, -costMultiplier, IdleReference.Val)) {
            //        FailUpgrade(techDef);
            //        return;
            //    }
            //}
            if (!CanAffordTechCosts(map, techDef)) {
                FailUpgrade(techDef);
                return;
            }

            Game.I.TechLevel(techDef.id, techLevel + 1);
            foreach (ResDefValue idValue in techDef.Upgrade) {
                long costMultiplier = CostMultiplierOf(techDef.Multiplier, techLevel);
                map.DoChange(idValue, -costMultiplier, IdleReference.Val);
            }

            SucceedUpgrade(techDef);
        }

        private static void SucceedUpgrade(TechDef techDef) {
            UI.Prepare();
            techDef.IconText();
            Game.I.TechLevel(techDef.id, out int techLevel);
            Audio.I.Clip = Audio.I.PositiveSound;
            if (techLevel == 1) {
                UI.IconText($"研究成功", SpriteUI.IconOf(SpriteUI.Success));
            } else {
                UI.IconText($"升级 {techLevel} 成功", SpriteUI.IconOf(SpriteUI.Success));
            }
            UI.Show();
        }
        private static void FailUpgrade(TechDef techDef) {
            UI.Prepare();
            techDef.IconText();
            Game.I.TechLevel(techDef.id, out int techLevel);
            Audio.I.Clip = Audio.I.NegativeSound;
            if (techLevel == 0) {
                UI.IconText($"研究失败", SpriteUI.IconOf(SpriteUI.Success));
            } else {
                UI.IconText($"升级 {techLevel + 1} 失败", SpriteUI.IconOf(SpriteUI.Failure));
            }
            UI.Show();
        }






        private static void TapEmpty(MapTileInfo info) {
            UI.Prepare();

            // info.Map.AddButton();
            // UI.Space();

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
            if (Game.I.Settings.LevelOne) {
                info.Level = 1;
            } else if (info.PlayerBuild_OrAutoBuild) {
                int bonus = CalcBonus(info);
                int tech = CalcTech(info.TileDef);
                info.Level = bonus * tech;
            } else {
                info.Level = 1;
            }
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
                    A.Assert(unlocked != null);
                    if (!constructables.Contains(unlocked)) {
                        constructables.Add(unlocked);
                    }
                }
            }
            return constructables;
        }
        private static void AddUnlocked(MapTileInfo info, int dx, int dy) {
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
                // 查询普通解锁
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

            info.TileDef.IconTextWithLevel(info.Level);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Success);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Construction);

            // AddConstructInfo

            UI.Show();
        }

        private static readonly HashSet<TileDef> neighbors = new HashSet<TileDef>();
        private static void FailConstruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clip = Audio.I.NegativeSound;

            UI.Prepare();

            info.TileDef.IconText();
            UI.Space();
            SpriteUI.IconText(SpriteUI.Construction);
            UI.Space();

            if (!CanConstructTech(info.TileDef)) {
                SpriteUI.IconText("需要科技", SpriteUI.Failure);
                info.TileDef.TechRequirementForConstruction.IconText();
                UI.Show();
                return;
            }

            neighbors.Clear();
            FailConstructRepelText(info, 0, +1);
            FailConstructRepelText(info, 0, -1);
            FailConstructRepelText(info, +1, 0);
            FailConstructRepelText(info, -1, 0);
            if (neighbors.Count > 0) {
                SpriteUI.IconText("不能相邻", SpriteUI.Failure);

                UI.Space();
                SpriteUI.IconText(SpriteUI.Repel);
                foreach (TileDef neighbor in neighbors) {
                    neighbor.IconText();
                }
            } else {
                SpriteUI.IconText("缺少资源", SpriteUI.Failure);
            }
            neighbors.Clear();



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
                            break;
                        case IdleReference.Inc:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.IncSuper : SpriteUI.Inc);
                            break;
                        case IdleReference.Max:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.MaxSuper : SpriteUI.Max);
                            break;
                    }
                }
                idValue.CannotChangeUIItem(level);
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

            info.TileDef.IconTextWithLevel(info.Level);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Success);
            UI.Space();
            SpriteUI.IconText(SpriteUI.Destruction);

            // AddDestructInfo(info);

            UI.Show();
        }



        private static void FailDestruct(MapTileInfo info) {
            if (!info.PlayerBuild_OrAutoBuild) return;
            Audio.I.Clip = Audio.I.NegativeSound;

            UI.Prepare();

            info.TileDef.IconText();
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
                            break;
                        case IdleReference.Inc:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.IncSuper : SpriteUI.Inc);
                            break;
                        case IdleReference.Max:
                            SpriteUI.IconText(isMapSuper ? SpriteUI.MaxSuper : SpriteUI.Max);
                            break;
                        default:
                            A.Assert(false);
                            break;
                    }
                }
                idValue.CannotChangeUIItem(level);
            }
        }
    }
}


