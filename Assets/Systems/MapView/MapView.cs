
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace W
{
    public class MapView : MonoBehaviour
    {
        public static MapView I => i;
        private static MapView i;
        private void Awake() {
            A.Singleton(ref i, this);
            AwakeInstance();
        }

        private void AwakeInstance() {
            InitializeTiles();
        }

        [SerializeField]
        private Light2D light2D;

        [SerializeField]
        private Sprite[] Indexes;


        private SimpleTile SimpleTile;
        private AnimatedTile AnimatedTile;
        private void InitializeTiles() {
            SimpleTile = ScriptableObject.CreateInstance<SimpleTile>();
            AnimatedTile = ScriptableObject.CreateInstance<AnimatedTile>();
        }

        [SerializeField]
        private Tilemap Index;
        [SerializeField]
        private Tilemap Glow;
        private TilemapRenderer glowTilemapRenderer;
        [SerializeField]
        private Tilemap GlowDayNight;
        private TilemapRenderer glowDayNightTilemapRenderer;
        [SerializeField]
        private Tilemap Front;
        [SerializeField]
        private Tilemap Back;

        [SerializeField]
        private Tilemap Up;
        private Transform upTrans;
        [SerializeField]
        private Tilemap Down;
        private Transform downTrans;
        [SerializeField]
        private Tilemap Left;
        private Transform leftTrans;
        [SerializeField]
        private Tilemap Right;
        private Transform rightTrans;

        [SerializeField]
        private Tilemap UpGlow;
        private Transform upGlowTrans;
        [SerializeField]
        private Tilemap DownGlow;
        private Transform downGlowTrans;
        [SerializeField]
        private Tilemap LeftGlow;
        private Transform leftGlowTrans;
        [SerializeField]
        private Tilemap RightGlow;
        private Transform rightGlowTrans;


        private void Start() {
            upTrans = Up.transform;
            downTrans = Down.transform;
            leftTrans = Left.transform;
            rightTrans = Right.transform;

            upGlowTrans = UpGlow.transform;
            downGlowTrans = DownGlow.transform;
            leftGlowTrans = LeftGlow.transform;
            rightGlowTrans = RightGlow.transform;

            glowTilemapRenderer = Glow.GetComponent<TilemapRenderer>();
            glowTilemapRenderer.material = Instantiate(glowTilemapRenderer.material); // yes

            glowDayNightTilemapRenderer = GlowDayNight.GetComponent<TilemapRenderer>();
            glowDayNightTilemapRenderer.material = Instantiate(glowDayNightTilemapRenderer.material); // yes
        }

        private const long deltaT = 2 * Constants.Second;
        private const float stayT = 0.375f;
        private const float fadeT = 0.0625f;

        private void Update() {
            UpdateTranslates();
            UpdateLightAndGlow();
        }

        private void UpdateTranslates() {
            float t = (float)(G.now % deltaT) / deltaT;
            t = SmoothPosition(t);
            float motion = 1 - t;

            upTrans.position = new Vector3(0, motion, 0);
            upGlowTrans.position = new Vector3(0, motion, 0);

            downTrans.position = new Vector3(0, -motion, 0);
            downGlowTrans.position = new Vector3(0, -motion, 0);

            rightTrans.position = new Vector3(motion, 0, 0);
            rightGlowTrans.position = new Vector3(motion, 0, 0);

            leftTrans.position = new Vector3(-motion, 0, 0);
            leftGlowTrans.position = new Vector3(-motion, 0, 0);
        }

        private long lastReminder = -1;
        private void UpdateTranslatesBad() {
            float t = (float)(G.now % deltaT) / deltaT;
            t = SmoothPosition(t);
            float motion = 1 - t;

            long count = G.now / deltaT;
            count = H.Hash((uint)count);
            long reminder = count % 4;

            if (reminder != lastReminder) {
                lastReminder = reminder;

                Up.gameObject.SetActive(false);
                Down.gameObject.SetActive(false);
                Left.gameObject.SetActive(false);
                Right.gameObject.SetActive(false);
                UpGlow.gameObject.SetActive(false);
                DownGlow.gameObject.SetActive(false);
                LeftGlow.gameObject.SetActive(false);
                RightGlow.gameObject.SetActive(false);
                switch (reminder) {
                    case 0:
                        Up.gameObject.SetActive(true);
                        UpGlow.gameObject.SetActive(true);
                        break;
                    case 1:
                        Down.gameObject.SetActive(true);
                        DownGlow.gameObject.SetActive(true);
                        break;
                    case 2:
                        Right.gameObject.SetActive(true);
                        RightGlow.gameObject.SetActive(true);
                        break;
                    case 3:
                        Left.gameObject.SetActive(true);
                        LeftGlow.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }
            }


            float opacity = SmoothOpacity(t);
            Color color = new Color(1, 1, 1, opacity);
            switch (reminder) {
                case 0:
                    upTrans.position = new Vector3(0, motion, 0);
                    upGlowTrans.position = new Vector3(0, motion, 0);

                    Up.color = color;
                    UpGlow.color = color;
                    break;
                case 1:
                    downTrans.position = new Vector3(0, -motion, 0);
                    downGlowTrans.position = new Vector3(0, -motion, 0);

                    Down.color = color;
                    DownGlow.color = color;
                    break;
                case 2:
                    rightTrans.position = new Vector3(motion, 0, 0);
                    rightGlowTrans.position = new Vector3(motion, 0, 0);

                    Right.color = color;
                    RightGlow.color = color;
                    break;
                case 3:
                    leftTrans.position = new Vector3(-motion, 0, 0);
                    leftGlowTrans.position = new Vector3(-motion, 0, 0);

                    Left.color = color;
                    LeftGlow.color = color;
                    break;
                default:
                    break;
            }


            //upTrans.position = new Vector3(0, motion, 0);
            //downTrans.position = new Vector3(0, -motion, 0);
            //rightTrans.position = new Vector3(motion, 0, 0);
            //leftTrans.position = new Vector3(-motion, 0, 0);

            //upGlowTrans.position = new Vector3(0, motion, 0);
            //downGlowTrans.position = new Vector3(0, -motion, 0);
            //rightGlowTrans.position = new Vector3(motion, 0, 0);
            //leftGlowTrans.position = new Vector3(-motion, 0, 0);

            //Up.color = color;
            //Down.color = color;
            //Left.color = color;
            //Right.color = color;
            //UpGlow.color = color;
            //DownGlow.color = color;
            //LeftGlow.color = color;
            //RightGlow.color = color;
        }


        private bool lightEnabled = false;
        private void UpdateLightAndGlow() {

            if (Game.I.Map == null) return;

            if (Game.I.Map.Def.Theme is PlanetMapThemeDef theme && theme.ActivateLight) {
                lightEnabled = true;

                float day = Time.time / theme.DayDuration;
                light2D.color = theme.DefaultDaylightGradient.Evaluate(day % 1);

                const float minIntensity = 0.25f;

                float sin = (M.Cos(2 * M.PI * day) + 1) / 2f;

                sin = M.Clamp01(sin);

                float intensity = M.Lerp(minIntensity, 1f, sin);

                intensity = M.Clamp01(intensity);
                light2D.intensity = intensity;


                float glow = M.InverseLerp(minIntensity + 1/32f, minIntensity, intensity);
                glow = M.Clamp(0, 1, glow);
                if (glow > 0) {
                    GlowDayNight.enabled = true;
                    glowDayNightTilemapRenderer.sharedMaterial.SetFloat("_GlowIntensity", glow * 4);
                    GlowDayNight.color = new Color(1, 1, 1, glow);

                    BackgroundSprite.sharedMaterial.SetFloat("_Lerp", glow);
                } else {
                    GlowDayNight.enabled = false;
                    BackgroundSprite.sharedMaterial.SetFloat("_Lerp", 0);
                }

            } else {
                if (lightEnabled) {
                    lightEnabled = false;

                    light2D.intensity = 1;
                    light2D.color = Color.white;

                    glowDayNightTilemapRenderer.sharedMaterial.SetFloat("_GlowIntensity", 3);
                    GlowDayNight.color = new Color(1, 1, 1, 1);
                    GlowDayNight.enabled = true;
                }
            }
            // UnityEngine.Debug.LogWarning(Glow.GetComponent<TilemapRenderer>().sharedMaterial == GlowDayNight.GetComponent<TilemapRenderer>().sharedMaterial);
        }

        public float Lightness {
            set {
                light2D.intensity = value;
            }
        }

        private float SmoothPosition(float t) {
            return t;

            //if (t < stayT) {
            //    return 0;
            //} else if (t > 1 - stayT) {
            //    return 1;
            //} else {
            //    float t2 = (t - stayT) / (1 - 2 * stayT);
            //    return (-Mathf.Cos(t2 * Mathf.PI) + 1) / 2;
            //}
        }
        private float SmoothOpacity(float t) {
            if (t < fadeT) {
                return t / fadeT;
            } else if (t > 1 - fadeT) {
                return (-t + 1 - fadeT) / fadeT + 1;
            } else {
                return 1;
            }
        }

        public void SetGlowDayNightSpriteAt(int x, int y, Sprite glow) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            GlowDayNight.SetTile(pos, TileOf(glow));
        }

        public void SetGlowSpriteAt(int x, int y, Sprite glow) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Glow.SetTile(pos, TileOf(glow));
        }
        public void SetGlowSpritesAt(int x, int y, Sprite[] sprites, float duration) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Glow.SetTile(pos, TileOf(sprites, duration, Color.white));
        }


        public void SetIndexSpriteAt(int x, int y, int index) {
            Vector3Int pos = new Vector3Int(x, y, 0);

            if (index <= 1) {
                Index.SetTile(pos, null);
            } else {
                int reminder = index;
                int kilos = 0;
                while (reminder >= 1000) {
                    reminder /= 1000;
                    kilos += 1;
                }
                Color indexColor;
                if (kilos == 0) {
                    indexColor = Color.white;
                } else if (kilos < 6) {
                    indexColor = Color.HSVToRGB((kilos + 1) / 6f, 0.75f, 1);
                } else {
                    indexColor = Color.gray;
                }
                Index.SetTile(pos, TileOf(Indexes[index], indexColor));
            }
        }

        public bool PlayParticleEffect { get; set; }
        /// <summary>
        /// too long the arguments
        /// </summary>
        public void SetFrontSpriteAt(int x, int y, Sprite sprite, Color color) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            A.Assert(sprite != null);
            Front.SetTile(pos, TileOf(sprite, color));
            if (PlayParticleEffect) {
                StartCoroutine(ScaleTileCoroutine(x, y, G.now));
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, x, y);
            }
        }
        public void SetFrontSpritesAt(int x, int y, Sprite[] sprites, float duration, Color color) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            A.Assert(sprites != null && sprites.Length > 0);
            Front.SetTile(pos, TileOf(sprites, sprites.Length / duration, color));
            if (PlayParticleEffect) {
                // StartCoroutine(ScaleTileCoroutine(x, y, G.now));
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, x, y);
            }
        }
        public void ClearFrontSpriteAt(int x, int y) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Front.SetTile(pos, null);
            if (PlayParticleEffect) {
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Destruct, x, y);
            }
        }

        public void ScaleTileAt(int x, int y) {
            StartCoroutine(ScaleTileCoroutine(x, y, G.now));
        }


        private long TileScaleDuration = Constants.Second / 4;
        private IEnumerator ScaleTileCoroutine(int x, int y, long start) {
            long now = G.now;
            while (now - start < TileScaleDuration) {
                float scale = (float)(now - start) / TileScaleDuration;
                ScaleTile(x, y, SmoothScale(scale));
                yield return null;
                now = G.now;
            }
            ScaleTile(x, y, 1);
            ScaleTileEnd(x, y);
        }
        private static float SmoothScale(float t) {
            const float c1 = 6f;
            const float c3 = c1 + 1;
            float tm = t - 1;
            return 1 + c3 * tm * tm * tm + c1 * tm * tm;
        }
        private void ScaleTile(int x, int y, float scale) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Front.SetTileFlags(pos, TileFlags.None);
            Front.SetTransformMatrix(pos, Matrix4x4.TRS(0.5f * (1 - scale) * Vector3.one, Quaternion.identity, Vector3.one * scale));
            Glow.SetTileFlags(pos, TileFlags.None);
            Glow.SetTransformMatrix(pos, Matrix4x4.TRS(0.5f * (1 - scale) * Vector3.one, Quaternion.identity, Vector3.one * scale));
            GlowDayNight.SetTileFlags(pos, TileFlags.None);
            GlowDayNight.SetTransformMatrix(pos, Matrix4x4.TRS(0.5f * (1 - scale) * Vector3.one, Quaternion.identity, Vector3.one * scale));
        }
        private void ScaleTileEnd(int x, int y) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Front.SetTransformMatrix(pos, Matrix4x4.identity);
            Glow.SetTransformMatrix(pos, Matrix4x4.identity);
            GlowDayNight.SetTransformMatrix(pos, Matrix4x4.identity);
        }




        public enum Dir
        {
            Up, Down, Left, Right
        }

        public void SetAnimSpriteAt(int x, int y, Sprite sprite, Color color, Sprite glow, Dir dir) {
            Tilemap tilemap;
            Tilemap tilemapGlow;
            switch (dir) {
                case Dir.Up:
                    tilemap = Up;
                    tilemapGlow = UpGlow;
                    break;
                case Dir.Down:
                    tilemap = Down;
                    tilemapGlow = DownGlow;
                    break;
                case Dir.Right:
                    tilemap = Right;
                    tilemapGlow = RightGlow;
                    break;
                case Dir.Left:
                    tilemap = Left;
                    tilemapGlow = LeftGlow;
                    break;
                default:
                    tilemap = null;
                    tilemapGlow = null;
                    break;
            };
            tilemap.SetTile(new Vector3Int(x, y, 0), TileOf(sprite, color));
            tilemapGlow.SetTile(new Vector3Int(x, y, 0), TileOf(glow, Color.white));
        }

        public void SetBackSpriteAt(int x, int y, Sprite sprite, Color color) {
            Back.SetTile(new Vector3Int(x, y, 0), TileOf(sprite, color));
        }

        public TileBase TileOf(Sprite sprite) {
            if (sprite == null) return null;
            SimpleTile.sprite = sprite;
            SimpleTile.color = Color.white;
            return SimpleTile;
        }
        public TileBase TileOf(Sprite sprite, Color color) {
            if (sprite == null) return null;
            SimpleTile.sprite = sprite;
            SimpleTile.color = color;
            return SimpleTile;
        }
        public TileBase TileOf(Sprite[] sprites, float speed, Color color) {
            if (sprites == null) return null;
            AnimatedTile.sprites = sprites;
            AnimatedTile.speed = speed;
            AnimatedTile.color = color;
            return AnimatedTile;
        }
        public TileBase TileOf(Sprite[] sprites, float speed) {
            if (sprites == null) return null;
            AnimatedTile.sprites = sprites;
            AnimatedTile.speed = speed;
            AnimatedTile.color = Color.white;
            return AnimatedTile;
        }


        public void EnterMap(MapDef mapDef) {

            Back.ClearAllTiles();
            Front.ClearAllTiles();

            Glow.ClearAllTiles();
            GlowDayNight.ClearAllTiles();

            Index.ClearAllTiles();
            Up.ClearAllTiles();
            Down.ClearAllTiles();
            Left.ClearAllTiles();
            Right.ClearAllTiles();
            UpGlow.ClearAllTiles();
            DownGlow.ClearAllTiles();
            LeftGlow.ClearAllTiles();
            RightGlow.ClearAllTiles();

            int width = mapDef.Width;
            int height = mapDef.Height;


            const float margin = 128;
            Background.localScale = new Vector3(width + margin, height + margin, 0) * 2;
            Background.localPosition = new Vector3(-margin, -margin);

            BackgroundSprite.sprite = mapDef.Theme.Background;
            BackgroundSprite.color = mapDef.Theme.BackgroundColor;
            BackgroundSprite.sharedMaterial = mapDef.Theme.BackgroundMaterial;
            BackgroundSprite.material = BackgroundSprite.material; // copy

            if (mapDef.Theme.Tileset == null || mapDef.Theme.Tileset.Length == 0) {

                if (mapDef.Theme.Ground == null) {
                    GroundSprite.enabled = false;
                } else {
                    GroundSprite.enabled = true;

                    Ground.localScale = new Vector3(width, height, 0);

                    GroundSprite.sprite = mapDef.Theme.Ground;
                    GroundSprite.color = mapDef.Theme.GroundColor;
                    GroundSprite.material = mapDef.Theme.GroundMaterial;
                    GroundSprite.material = GroundSprite.material; // copy
                }
            } else {
                GroundSprite.enabled = false;
            }
        }


        [SerializeField]
        private Transform Ground;
        [SerializeField]
        private SpriteRenderer GroundSprite;


        [SerializeField]
        private Transform Background;
        [SerializeField]
        private SpriteRenderer BackgroundSprite;



    }
}
