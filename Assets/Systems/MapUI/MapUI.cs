
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

        private void CanChangeText(IDValue idValue, long level, IdleReference i) {
            bool canChange = Map.CanChange(idValue, level, i);
            IconTextWithValueAndColor(idValue, level, canChange);
        }


        private static Color ColorDisable => new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private static Color ColorNormal => new Color(1f, 1f, 1f, 1f);
        private static Color ColorWarning => new Color(1f, 0, 0, 1f);

        private static Color ColorPositive => new Color(0.5f, 1f, 0.5f, 1f);
        private static Color ColorNegative => new Color(1f, 0.5f, 0.5f, 1f);






        private static Map map; // todo to refactor

        /// <summary>
        /// 是否启用UI
        /// </summary>
        private bool enableUI;

        /// <summary>
        /// 按下格子的x坐标
        /// </summary>
        private int x;
        /// <summary>
        /// 按下格子的y坐标
        /// </summary>
        private int y;


        /// <summary>
        /// 按下格子的建筑
        /// </summary>
        private TileDef existing;
        /// <summary>
        /// 按下格子的等级
        /// </summary>
        private int existingLevel;

        /// <summary>
        /// 要建造的建筑
        /// </summary>
        private TileDef replacement;
        /// <summary>
        /// 要建造的建筑等级
        /// </summary>
        private int replacementLevel;


        /// <summary>
        /// 格子按下时的行为
        /// </summary>
        private void OnTap() {
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

        private void OnTap_() {

            Audio.I.Clip = Audio.I.DefaultSound;

            x = MapTapping.I.X;
            y = MapTapping.I.Y;

            map = Game.I.Map;

            enableUI = true;

            if (!map.HasPosition(x, y)) {
                TapNothing();
                return;
            }

            uint existingID = map.ID(x, y);
            if (existingID == ID.Invalid) {
                TapNothing();
                return;
            }

            existingLevel = map.Level(x, y);

            if (existingID == 0 && existingLevel != 0) {
                A.Assert(false, () => "可能忘了更新 GameConfigReference");
            }

            TapSomething();

            if (existingID == ID.Empty) {
                existing = null;
            } else {
                existing = GameConfig.I.ID2Obj[existingID] as TileDef;
                A.Assert(existing != null);
            }

            if (existingID == ID.Empty) {
                TapEmpty();
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Destruct, x, y);
            } else {
                MapView.I.ScaleTileAt(x, y);
                TapExisting();
            }
        }

        private void TapNothing() {
            ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, x, y);
            SettingsPage.Show();
            MapTapping.I.HideIndicator();
        }

        private void TapSomething() {
            MapTapping.I.ShowIndicatorAt(x, y);
        }





















        /// <summary>
        /// 按下已有建筑
        /// </summary>
        private void TapExisting() {
            UI.Prepare();

            IconTextWithLevel(existing, existingLevel);

            //string info = existing.Description;
            //if (info != null) {
            //    UI.Space();
            //    UI.Text(info);
            //    UI.Space();
            //}

            if (existing.Techs.Count == 0) {
                UI.Space();
            } else {
                Sprites.IconButton(Sprites.TechDef, () => TapTechs(existing));
            }

            Sprites.IconButton(Sprites.Information, () => {
                UI.Prepare();
                IconTextWithLevel(existing, existingLevel);
                AddTileInformation(existing, existingLevel);
                UI.Show();
            });

            UI.Space();
            Sprites.IconButton(Sprites.Destruction, CanDestruct() ? ColorNormal : ColorDisable, Game.I.Settings.SkipConfirmation ? TryDestruct : AskDestruct);

            Map.AddRelatedResDefValue(existing);


            UI.Show();
        }

        private static void TapTechs(TileDef tileDef) {
            UI.Prepare();

            IconText(tileDef);
            Sprites.IconText(Sprites.TechDef);

            foreach (TechDef tech in tileDef.Techs) {
                AddTapTech(tech);
            }

            UI.Show();
        }

        private static void AddTapTech(TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            UI.Space();

            IconTextWithLevel(techDef, techLevel);

            if (techLevel == techDef.MaxLevel) {
                UI.IconButton("满级", ColorDisable, techDef.Icon, null);
            } else {
                bool canAfford = CanAffordTechCosts(techDef);

                UI.IconButton(LevelText(techLevel), canAfford ? ColorNormal : ColorDisable, Sprites.IconOf(Sprites.TechDef), !canAfford ? null : () => TryUpgradeTech(techDef));

                AddTechCosts(techDef);
            }
        }
        private static string LevelText(int level) => level == 0 ? "研发" : "升级";

        private static bool CanAffordTechCosts(TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);
                bool canChange = Map.CanChange(idValue, -cost, IdleReference.Val);
                if (!canChange) return false;
            }
            return true;
        }
        private static void AddTechCosts(TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);
            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);
                bool canChange = Map.CanChange(idValue, -cost, IdleReference.Val);
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

        private static void TryUpgradeTech(TechDef techDef) {
            Game.I.TechLevel(techDef.id, out int techLevel);

            foreach (IDValue idValue in techDef.Upgrade) {
                long cost = CostOf(idValue.Value, techDef.Multiplier, techLevel);

                if (!Map.CanChange(idValue, -cost, IdleReference.Val)) {
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
                    // TileDefInfoItem(idValue, existingLevel);
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







        private void TapEmpty() {
            UI.Prepare();

            // UI.Text($"坐标 {x} {y}");
            UI.IconText(map.Def.CN, GameConfig.I.Name2Obj[Sprites.MapDef].Icon);
            UI.Space();

            var unlockeds = AddUnlockeds();
            if (unlockeds.Count > 0) {
                Sprites.IconText(Sprites.Construction);
                UI.Space();

                foreach (TileDef unlocked in unlockeds) {

                    CalcReplacement(unlocked);

                    IconButtonWithLevel(replacement, replacementLevel, CanConstruct() ? ColorNormal : ColorDisable, () => {

                        CalcReplacement(unlocked);

                        if (Game.I.Settings.SkipConfirmation) {
                            TryConstruct();
                        } else {
                            AskConstruct();
                        }
                    });
                }
            } else {
                UI.Text("无法建造");
            }
            UI.Show();
        }

        private void CalcReplacement(TileDef replacement) {
            if (replacement == null) {
                A.Error($"建设的物体不能为空");
            }
            this.replacement = replacement; // capture 
            replacementLevel = CalcBonus(x, y, replacement) * CalcTech(replacement);
        }


        private readonly HashSet<TileDef> constructables = new HashSet<TileDef>();
        private HashSet<TileDef> AddUnlockeds() {
            constructables.Clear();
            AddUnlocked(0, +1);
            AddUnlocked(0, -1);
            AddUnlocked(+1, 0);
            AddUnlocked(-1, 0);
            if (constructables.Count == 0) {
                foreach (TileDef unlocked in map.Def.Constructables) {
                    if (!constructables.Contains(unlocked)) {
                        constructables.Add(unlocked);
                    }
                }
            }
            return constructables;
        }
        private void AddUnlocked(int dx, int dy) {
            uint neighborID = map.ID_Safe(x + dx, y + dy);
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

        private int CalcBonus(int x, int y, TileDef tileDef) {
            int bonus = 0;
            bonus += AddBonus(x, y + 1, tileDef);
            bonus += AddBonus(x, y - 1, tileDef);
            bonus += AddBonus(x + 1, y, tileDef);
            bonus += AddBonus(x - 1, y, tileDef);
            if (bonus < 1) bonus = 1;
            return bonus;
        }

        private static int AddBonus(int dx, int dy, TileDef tileDef) {
            uint bonusID = map.ID_Safe(dx, dy);
            if (ID.IsInvalid(bonusID)) return 0;
            TileDef neighbor = GameConfig.I.ID2Obj[bonusID] as TileDef;
            A.Assert(neighbor != null);
            foreach (var bonus in tileDef.Bonus) {
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






















        private void AskConstruct() {
            UI.Prepare();

            IconTextWithLevel(replacement, replacementLevel);

            Sprites.IconButton(Sprites.Construction, () => {
                TryConstruct();
            });

            AddConstructInfo();

            UI.Show();
        }
        private void AddConstructInfo() {
            if (replacement.Construction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (IDValue idValue in replacement.Construction) {
                    CanChangeText(idValue, 1, IdleReference.Val);
                }
            }

            if (replacement.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (IDValue idValue in replacement.Inc) {
                    CanChangeText(idValue, replacementLevel, IdleReference.Inc);
                }
            }

            if (replacement.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (IDValue idValue in replacement.Max) {
                    CanChangeText(idValue, replacementLevel, IdleReference.Max);
                }
            }
        }

        private bool CanConstruct() {
            foreach (IDValue idValue in replacement.Construction) {
                if (!Map.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (IDValue idValue in replacement.Inc) {
                if (!Map.CanChange(idValue, replacementLevel, IdleReference.Inc)) return false;
            }
            foreach (IDValue idValue in replacement.Max) {
                if (!Map.CanChange(idValue, replacementLevel, IdleReference.Max)) return false;
            }
            if (!CanConstructByNeighbor(0, +1)) return false;
            if (!CanConstructByNeighbor(0, -1)) return false;
            if (!CanConstructByNeighbor(+1, 0)) return false;
            if (!CanConstructByNeighbor(-1, 0)) return false;
            return true;
        }

        private bool CanConstructByNeighbor(int dx, int dy) {
            TileDef neighbor = NeighborOf(dx, dy);
            if (neighbor == null) return true;
            foreach (TileDef repel in replacement.Repels) {
                if (repel == neighbor) return false;
            }
            foreach (TileDef repel in neighbor.Repels) {
                if (repel == replacement) return false;
            }
            return true;
        }
        private TileDef NeighborOf(int dx, int dy) {
            uint neighborID = map.ID_Safe(x + dx, y + dy);
            if (ID.IsInvalid(neighborID)) return null;
            TileDef neighbor = GameConfig.I.ID2Obj[neighborID] as TileDef;
            return neighbor;
        }

        private bool TryConstruct() {
            if (!CanConstruct()) {
                FailConstruct();
                return false;
            }

            SucceedConstruct();
            map.ID(x, y, replacement.id);
            map.Level(x, y, replacementLevel);
            if (enableUI) MapDataView.ShowTile(x, y, replacement, replacementLevel, true);

            foreach (IDValue idValue in replacement.Construction) {
                Map.DoChange(idValue, 1, IdleReference.Val);
            }

            foreach (IDValue idValue in replacement.Inc) {
                Map.DoChange(idValue, replacementLevel, IdleReference.Inc);
            }

            foreach (IDValue idValue in replacement.Max) {
                Map.DoChange(idValue, replacementLevel, IdleReference.Max);
            }

            return true;
        }

        private void SucceedConstruct() {
            if (!enableUI) return;

            Audio.I.Clips = Audio.I.ConstructSound;

            UI.Prepare();

            IconText(replacement);
            UI.Space();
            Sprites.IconText(Sprites.Construction);
            UI.Space();
            Sprites.IconText(Sprites.Success);

            AddConstructInfo();

            UI.Show();
        }

        private readonly HashSet<TileDef> neighbors = new HashSet<TileDef>();
        private void FailConstruct() {
            if (!enableUI) return;

            UI.Prepare();

            IconText(replacement);
            UI.Space();
            Sprites.IconText(Sprites.Construction);
            UI.Space();
            Sprites.IconText(Sprites.Failure);

            neighbors.Clear();
            FailConstructRepelText(0, +1);
            FailConstructRepelText(0, -1);
            FailConstructRepelText(+1, 0);
            FailConstructRepelText(-1, 0);
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
            foreach (IDValue idValue in replacement.Construction) {
                FailConstructText(idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (IDValue idValue in replacement.Inc) {
                FailConstructText(idValue, replacementLevel, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (IDValue idValue in replacement.Max) {
                FailConstructText(idValue, replacementLevel, IdleReference.Max, ref title);
            }
            title = false;

            UI.Show();
        }
        private void FailConstructText(IDValue idValue, long level, IdleReference i, ref bool title) {
            bool canConstruct = Map.CanChange(idValue, level, i);
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

        private void FailConstructRepelText(int dx, int dy) {
            TileDef neighbor = NeighborOf(dx, dy);
            if (neighbor == null) return;

            if (neighbors.Contains(neighbor)) return;
            foreach (TileDef repel in replacement.Repels) {
                if (repel == neighbor) {
                    neighbors.Add(neighbor);
                }
            }
            if (neighbors.Contains(neighbor)) return;
            foreach (TileDef repel in neighbor.Repels) {
                if (repel == replacement) {
                    neighbors.Add(neighbor);
                }
            }
        }

















        private void AskDestruct() {
            UI.Prepare();

            IconTextWithLevel(existing, existingLevel);

            Sprites.IconButton(Sprites.Destruction, CanDestruct() ? ColorNormal : ColorDisable, () => {
                TryDestruct();
            });

            AddDestructInfo();

            UI.Show();
        }

        private void AddDestructInfo() {
            if (existing.Destruction.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Val);
                foreach (IDValue idValue in existing.Destruction) {
                    CanChangeText(idValue, 1, IdleReference.Val);
                }
            }

            if (existing.Inc.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Inc);
                foreach (IDValue idValue in existing.Inc) {
                    CanChangeText(idValue, -existingLevel, IdleReference.Inc);
                }
            }

            if (existing.Max.Count > 0) {
                UI.Space();
                Sprites.IconText(Sprites.Max);
                foreach (IDValue idValue in existing.Max) {
                    CanChangeText(idValue, -existingLevel, IdleReference.Max);
                }
            }
        }

        private bool CanDestruct() {
            foreach (IDValue idValue in existing.Destruction) {
                if (!Map.CanChange(idValue, 1, IdleReference.Val)) return false;
            }
            foreach (IDValue idValue in existing.Inc) {
                if (!Map.CanChange(idValue, -existingLevel, IdleReference.Inc)) return false;
            }
            foreach (IDValue idValue in existing.Max) {
                if (!Map.CanChange(idValue, -existingLevel, IdleReference.Max)) return false;
            }
            return true;
        }

        private void TryDestruct() {
            if (!CanDestruct()) {
                FailDestruct();
                return;
            }

            SucceedDestruct();
            map.ID(x, y, ID.Empty);
            map.Level(x, y, 0);
            if (enableUI) MapDataView.ShowTile(x, y, null, 0, true);

            foreach (IDValue idValue in existing.Inc) {
                Map.DoChange(idValue, -existingLevel, IdleReference.Inc);
            }

            foreach (IDValue idValue in existing.Max) {
                Map.DoChange(idValue, -existingLevel, IdleReference.Max);
            }

            foreach (IDValue idValue in existing.Construction) {
                Map.DoChange(idValue, 1, IdleReference.Val);
            }
        }


        private void SucceedDestruct() {
            if (!enableUI) return;

            Audio.I.Clips = Audio.I.DestructSound;

            UI.Prepare();

            IconText(existing);
            Sprites.IconText(Sprites.Destruction);
            UI.Space();
            Sprites.IconText(Sprites.Success);

            AddDestructInfo();

            UI.Show();
        }



        private void FailDestruct() {
            if (!enableUI) return;

            UI.Prepare();

            IconText(existing);
            Sprites.IconText(Sprites.Destruction);
            UI.Space();
            Sprites.IconText(Sprites.Failure);
            UI.Space();

            bool title;
            title = false;
            foreach (IDValue idValue in existing.Destruction) {
                FailDestructText(idValue, 1, IdleReference.Val, ref title);
            }
            title = false;
            foreach (IDValue idValue in existing.Inc) {
                FailDestructText(idValue, -existingLevel, IdleReference.Inc, ref title);
            }
            title = false;
            foreach (IDValue idValue in existing.Max) {
                FailDestructText(idValue, -existingLevel, IdleReference.Max, ref title);
            }

            UI.Show();
        }

        private void FailDestructText(IDValue idValue, long level, IdleReference i, ref bool title) {
            bool canConstruct = Map.CanChange(idValue, level, i);
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
















        public void TryConstructInitials(Map map) {
            MapUI.map = map;
            map.TemporaryRandomGenerator = new System.Random((int)map.Seed);
            map.Def.ProcessMap(map);
            TryConstructInitialStructures(map);
        }

        public void TryConstructInitialStructures(Map map) {
            MapDef mapDef = map.Def;

            if (mapDef.InitialRandomStructures == null || mapDef.InitialRandomStructures.Count == 0) return;

            int width = mapDef.Width;
            int height = mapDef.Height;
            int square = width * height;

            enableUI = false;

            foreach (IDValue idValue in mapDef.InitialRandomStructures) {

                if (idValue.Key is TileDef tileDef) {

                    long times = 0;
                    long times_success = 0;

                    while (times_success < idValue.Value && times < square) {
                        x = map.TemporaryRandomGenerator.Next(width);
                        y = map.TemporaryRandomGenerator.Next(height);

                        uint id = map.ID(x, y);
                        if (id == ID.Empty) {
                            CalcReplacement(tileDef);
                            if (TryConstruct()) {
                                times_success++;
                            }
                        }

                        times++;
                    }
                }
            }
        }
    }
}


