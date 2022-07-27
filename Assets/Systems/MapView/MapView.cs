
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
        private TilemapRenderer tr;
        private Material mat;
        [SerializeField]
        private Tilemap Front;
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
        private Tilemap Back;

        private void Start() {
            upTrans = Up.transform;
            downTrans = Down.transform;
            leftTrans = Left.transform;
            rightTrans = Right.transform;

            tr = Glow.GetComponent<TilemapRenderer>();
            mat = tr.material; // instantiated
            tr.material = mat; // yes
        }

        private const long deltaT = 2 * Constants.Second;
        private const float stayT = 0.25f;
        private const float fadeT = 0.0625f;

        private void Update() {
            float t = (float)(G.now % deltaT) / deltaT;
            t = SmoothT(t);

            float motion = 1 - t;
            upTrans.position = new Vector3(0, motion, 0);
            downTrans.position = new Vector3(0, -motion, 0);
            rightTrans.position = new Vector3(motion, 0, 0);
            leftTrans.position = new Vector3(-motion, 0, 0);

            float opacity = SmoothOpacity(t);
            Color color = new Color(1, 1, 1, opacity);
            Up.color = color;
            Down.color = color;
            Left.color = color;
            Right.color = color;

            UpdateLightAndGlow();
        }

        [SerializeField]
        private Gradient defaultDaylightGradient;
        [SerializeField]
        private AnimationCurve intensityCurve;
        [SerializeField]
        private AnimationCurve glowCurve;

        private const float DayDuration = 60f;
        private void UpdateLightAndGlow() {

            float day = (Time.time / DayDuration) % 1;

            light2D.intensity = M.Lerp(0.375f, 1f, intensityCurve.Evaluate(day));
            light2D.color = defaultDaylightGradient.Evaluate(day);

            float glow = glowCurve.Evaluate(day);
            mat.SetFloat("_GlowIntensity", glow * 3);
            Glow.color = new Color(1, 1, 1, glow);
        }

        public float Lightness {
            set {
                light2D.intensity = value;
            }
        }

        private float SmoothT(float t) {
            if (t < stayT) {
                return 0;
            } else if (t > 1 - stayT) {
                return 1;
            } else {
                float t2 = (t - stayT) / (1 - 2 * stayT);
                return (-Mathf.Cos(t2 * Mathf.PI) + 1) / 2;
            }
        }
        private float SmoothOpacity(float t) {
            if (t < fadeT) {
                return t;
            } else if (t > 1 - fadeT) {
                return (-t + 1 - fadeT) / fadeT + 1;
            } else {
                return 1;
            }
        }

        public void SetGlowSpriteAt(int x, int y, Sprite glow) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            Glow.SetTile(pos, TileOf(glow));
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
                StartCoroutine(ScaleTileCoroutine(Front, x, y, G.now));
                ParticlePlayer.I.FrameAnimation(ParticlePlayer.I.Construct, x, y);
            }
        }
        public void SetFrontSpritesAt(int x, int y, Sprite[] sprites, float duration, Color color) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            A.Assert(sprites != null && sprites.Length > 0);
            Front.SetTile(pos, TileOf(sprites, duration * sprites.Length, color));
            if (PlayParticleEffect) {
                StartCoroutine(ScaleTileCoroutine(Front, x, y, G.now));
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
            StartCoroutine(ScaleTileCoroutine(Front, x, y, G.now));
        }


        private long TileScaleDuration = Constants.Second / 4;
        private IEnumerator ScaleTileCoroutine(Tilemap tilemap, int x, int y, long start) {
            long now = G.now;
            while (now - start < TileScaleDuration) {
                float scale = (float)(now - start) / TileScaleDuration;
                ScaleTile(tilemap, x, y, SmoothScale(scale));
                yield return null;
                now = G.now;
            }
            ScaleTile(tilemap, x, y, 1);
            ScaleTileEnd(tilemap, x, y);
        }
        private static float SmoothScale(float t) {
            const float c1 = 6f;
            const float c3 = c1 + 1;
            float tm = t - 1;
            return 1 + c3 * tm * tm * tm + c1 * tm * tm;
        }
        private void ScaleTile(Tilemap tilemap, int x, int y, float scale) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            tilemap.SetTileFlags(pos, TileFlags.None);
            tilemap.SetTransformMatrix(pos, Matrix4x4.TRS(0.5f * (1 - scale) * Vector3.one, Quaternion.identity, Vector3.one * scale));
        }
        private void ScaleTileEnd(Tilemap tilemap, int x, int y) {
            Vector3Int pos = new Vector3Int(x, y, 0);
            tilemap.SetTransformMatrix(pos, Matrix4x4.identity);
        }




        public enum Dir
        {
            Up, Down, Left, Right
        }

        public void SetAnimSpriteAt(int x, int y, Sprite sprite, Color color, Dir dir) {
            Tilemap tilemap;
            switch (dir) {
                case Dir.Up:
                    tilemap = Up;
                    break;
                case Dir.Down:
                    tilemap = Down;
                    break;
                case Dir.Right:
                    tilemap = Right;
                    break;
                case Dir.Left:
                    tilemap = Left;
                    break;
                default:
                    tilemap = null;
                    break;
            };
            tilemap.SetTile(new Vector3Int(x, y, 0), TileOf(sprite, color));
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



        public void EnterMap(MapDef mapDef) {

            Back.ClearAllTiles();
            Front.ClearAllTiles();

            int width = mapDef.Width;
            int height = mapDef.Height;


            const float margin = 128;
            Background.localScale = new Vector3(width + margin, height + margin, 0) * 2;
            Background.localPosition = new Vector3(-margin, -margin);

            BackgroundSprite.sprite = mapDef.Theme.Background;
            BackgroundSprite.color = mapDef.Theme.BackgroundColor;
            BackgroundSprite.material = mapDef.Theme.BackgroundMaterial;


            if (mapDef.Theme.Tileset == null || mapDef.Theme.Tileset.Length == 0) {
                Ground.localScale = new Vector3(width, height, 0);

                GroundSprite.sprite = mapDef.Theme.Ground;
                GroundSprite.color = mapDef.Theme.GroundColor;
                GroundSprite.material = mapDef.Theme.GroundMaterial;
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
