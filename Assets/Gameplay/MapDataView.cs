
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
            UI.Text("进入地图");
            UI.Text($"seed {map.Seed}");
            UI.Text($"type {map.Def.CN}");
            UI.Show();

            if (map.Def.Theme.Tileset != null && map.Def.Theme.Tileset.Length != 0) {
                A.Assert(map.Def.Theme.Tileset.Length == TileUtility.Size8x6);
                for (int i = -1; i <= map.Width; i++) {
                    for (int j = -1; j <= map.Height; j++) {
                        int index = TileUtility.Index_8x6((Vec2 delta) => {
                            Vec2 pos = delta + new Vec2(i, j);
                            if (pos.x <= -1 || pos.x >= map.Width) return true;
                            if (pos.y <= -1 || pos.y >= map.Height) return true;

                            if (map.ID(pos.x, pos.y) == ID.Invalid) {
                                return true;
                            }
                            return false;
                        });
                        if (index != TileUtility.Full8x6) {
                            MapView.I.SetBackSpriteAt(i, j, map.Def.Theme.Tileset[index], map.Def.Theme.GroundColor);
                        }
                    }
                }
            }

            for (int i = 0; i < map.Width; i++) {
                for (int j = 0; j < map.Height; j++) {
                    uint id = map.ID(i, j);
                    if (ID.IsInvalid(id)) continue;
                    TileDef tileDef = GameConfig.I.ID2Obj[id] as TileDef;
                    A.Assert(tileDef != null);
                    int level = map.Level(i, j);
                    ShowTile(i, j, tileDef, level, false);
                    TranslateTiles(i, j);
                }
            }
        }




        public static void ShowTile(int x, int y, ID id, int level, bool effect = false) {
            MapView.I.PlayParticleEffect = effect;

            if (id == null) {
                MapView.I.ClearFrontSpriteAt(x, y);
            } else if (id.Sprite != null) {
                MapView.I.SetFrontSpriteAt(x, y, id.Sprite, id.Color);
            } else if (id.Sprites != null && id.Sprites.Length > 0) {
                MapView.I.SetFrontSpritesAt(x, y, id.Sprites, id.SpritesDuration, id.Color);
            } else {
                MapView.I.ClearFrontSpriteAt(x, y);
            }

            MapView.I.SetGlowSpriteAt(x, y, id == null ? null : id.Glow);
            MapView.I.SetIndexSpriteAt(x, y, level);
            TranslateTiles5(x, y);
        }

        /// <summary>
        /// 计算相邻物品
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
            if (tile != null && tile.BonusAnim == null) return;

            TranslateTile(x, y, x, y + 1, MapView.Dir.Up, tile);
            TranslateTile(x, y, x, y - 1, MapView.Dir.Down, tile);
            TranslateTile(x, y, x + 1, y, MapView.Dir.Right, tile);
            TranslateTile(x, y, x - 1, y, MapView.Dir.Left, tile);
        }

        public static void TranslateTile(int x, int y, int dx, int dy, MapView.Dir dir, TileDef self) {
            if (self == null) {
                MapView.I.SetAnimSpriteAt(x, y, null, Color.white, dir);
                return;
            }

            uint id = map.ID_Safe(dx, dy);
            if (ID.IsInvalid(id)) return;
            TileDef neighbor = GameConfig.I.ID2Obj[id] as TileDef;
            foreach (TileDef bonus in self.Bonus) {
                if (bonus == neighbor) {
                    MapView.I.SetAnimSpriteAt(x, y, self.BonusAnim.Sprite, self.Color, dir);
                    return;
                }
            }
            foreach (TileDef bonus in self.Conditions) {
                if (bonus == neighbor) {
                    MapView.I.SetAnimSpriteAt(x, y, self.BonusAnim.Sprite, self.Color, dir);
                    return;
                }
            }
        }

    }
}