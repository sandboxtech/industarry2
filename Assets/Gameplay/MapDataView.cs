
using UnityEngine;

namespace W
{
    public static class MapDataView
    {

        private static Map map;
        public static void EnterMap(Map m) {

            A.Assert(m != null);
            map = m;

            CameraControl.I.Area = new Rect(0, 0, map.Width, map.Height);
            MapView.I.EnterMap(map.Def);

            UI.Prepare();
            map.AddInfoButton();
            // UI.Text("进入地图");
            UI.Show();

            // 地形
            if (map.Def.Theme.Tileset != null && map.Def.Theme.Tileset.Length != 0) {
                A.Assert(map.Def.Theme.Tileset.Length == TileUtility.Size8x6);
                for (int i = -1; i <= map.Width; i++) {
                    for (int j = -1; j <= map.Height; j++) {
                        int spriteIndex = TileUtility.Index_8x6((Vec2 delta) => {
                            Vec2 pos = delta + new Vec2(i, j);
                            if (pos.x <= -1 || pos.x >= map.Width) return true;
                            if (pos.y <= -1 || pos.y >= map.Height) return true;

                            if (map.ID(pos.x, pos.y) == ID.Invalid) {
                                return true;
                            }
                            return false;
                        });
                        if (spriteIndex != TileUtility.Full8x6) {
                            MapView.I.SetBackSpriteAt(i, j, map.Def.Theme.Tileset[spriteIndex], map.Def.Theme.GroundColor);
                        }
                    }
                }
            } else {
                MapView.I.Lightness = 1;
            }

            for (int i = 0; i < map.Width; i++) {
                for (int j = 0; j < map.Height; j++) {
                    uint id = map.ID(i, j);
                    if (ID.IsInvalid(id)) {
                        uint index = map.IndexOf(i, j);
                        if (!map.NoPreviousMap && index == map.PreviousMapIndex) {
                            MapDef mapDef = GameConfig.I.ID2Obj[map.PreviousMapDefID] as MapDef;
                            A.Assert(mapDef != null);
                            ShowSubMapPortal(i, j, mapDef);
                        } else if (map.SubMaps != null && map.SubMaps.TryGetValue(index, out uint mapDefID)) {
                            MapDef mapDef = GameConfig.I.ID2Obj[mapDefID] as MapDef;
                            A.Assert(mapDef != null);
                            ShowSubMapPortal(i, j, mapDef);
                        }
                    } else {
                        TileDef tileDef = GameConfig.I.ID2Obj[id] as TileDef;
                        A.Assert(tileDef != null);
                        int level = map.Level(i, j);
                        ShowTile(i, j, tileDef, level, false);
                    }
                }
            }
        }


        public static void ShowSubMapPortal(int x, int y, MapDef id) {

            if (id == null) {
                MapView.I.ClearFrontSpriteAt(x, y);
            } else if (id.Sprites != null && id.Sprites.Length > 0) {
                MapView.I.SetFrontSpritesAt(x, y, id.Sprites, id.SpritesDuration, id.Color);
            } else if (id.Sprite != null) {
                MapView.I.SetFrontSpriteAt(x, y, id.Sprite, id.Color);
            } else {
                MapView.I.ClearFrontSpriteAt(x, y);
            }

            MapView.I.SetGlowSpriteAt(x, y, id == null ? null : id.Glow);
            TranslateTiles5(x, y);
        }


        public static void ShowTile(int x, int y, TileDef id, int level, bool effect = false) {
            MapView.I.PlayParticleEffect = effect;

            if (id == null) {
                MapView.I.ClearFrontSpriteAt(x, y);
            } else if (id.Sprites != null && id.Sprites.Length > 0) {
                MapView.I.SetFrontSpritesAt(x, y, id.Sprites, id.SpritesDuration, id.Color);
            } else if (id.Sprite != null) {
                MapView.I.SetFrontSpriteAt(x, y, id.Sprite, id.Color);
            } else {
                MapView.I.ClearFrontSpriteAt(x, y);
            }

            MapView.I.SetGlowSpriteAt(x, y, id == null ? null : id.Glow);
            MapView.I.SetGlowDayNightSpriteAt(x, y, id == null ? null : id.GlowDayNight);

            MapView.I.SetIndexSpriteAt(x, y, level);
            TranslateTiles5(x, y);
        }

        /// <summary>
        /// 计算相邻物品。丛SetFrontSpriteAt路由至此
        /// </summary>
        private static void RuleTiles5(int x, int y) {
            // TODO
        }



        /// <summary>
        /// 计算相邻运输动画
        /// </summary>
        private static void TranslateTiles5(int x, int y) {
            TranslateTiles(x, y);
            TranslateTiles(x, y + 1);
            TranslateTiles(x, y - 1);
            TranslateTiles(x + 1, y);
            TranslateTiles(x - 1, y);
        }

        public static void TranslateTiles(int x, int y) {
            uint id = map.ID_Safe(x, y);
            TileDef tile = ID.IsInvalid(id) ? null : GameConfig.I.ID2Obj[id] as TileDef;

            TranslateTile(x, y, x, y + 1, MapView.Dir.Up, tile);
            TranslateTile(x, y, x, y - 1, MapView.Dir.Down, tile);
            TranslateTile(x, y, x + 1, y, MapView.Dir.Right, tile);
            TranslateTile(x, y, x - 1, y, MapView.Dir.Left, tile);
        }

        public static void TranslateTile(int x, int y, int dx, int dy, MapView.Dir dir, TileDef self) {
            if (self == null) {
                MapView.I.SetAnimSpriteAt(x, y, null, Color.white, null, dir);
                return;
            }

            uint id = map.ID_Safe(dx, dy);
            if (ID.IsInvalid(id)) return;
            TileDef neighbor = GameConfig.I.ID2Obj[id] as TileDef;
            foreach (TileDef bonus in self.Bonus) {
                if (bonus == neighbor) {
                    foreach (ResDefValue input in self.Inc) {
                        if (input.Value > 0) continue;
                        foreach (ResDefValue output in neighbor.Inc) {
                            if (output.Value < 0) continue;
                            if (input.Key == output.Key) {
                                MapView.I.SetAnimSpriteAt(x, y, input.Key.Sprite, input.Key.Color, input.Key.Glow, dir);
                                return;
                            }
                        }
                    }
                }
            }
            //foreach (TileDef bonus in self.Bonus) {
            //    if (bonus == neighbor) {
            //        foreach (ResDefValue input in self.Inc) {
            //            if (input.Value < 0) continue;
            //            MapView.I.SetAnimSpriteAt(x, y, input.Key.Sprite, input.Key.Color, input.Key.Glow, dir);
            //            return;
            //        }
            //    }
            //}

            //foreach (TileDef bonus in self.Conditions) {
            //    if (bonus == neighbor) {
            //        //MapView.I.SetAnimSpriteAt(x, y, self.BonusAnim.Sprite, self.BonusAnim.Color, dir);
            //        //return;
            //        foreach (ResDefValue input in self.Inc) {
            //            if (input.Value > 0) continue;
            //            foreach (ResDefValue output in neighbor.Inc) {
            //                if (output.Value < 0) continue;
            //                if (input.Key == output.Key) {
            //                    MapView.I.SetAnimSpriteAt(x, y, input.Key.Sprite, input.Key.Color, input.Key.Glow, dir);
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
        }

    }
}
