
using System.Collections;
using UnityEngine;

namespace W
{
    [CreateAssetMenu(fileName = "_ParticleDef_", menuName = "创建 ParticleDef 粒子效果定义", order = 2)]
    public class ParticleDef : ScriptableObject
    {

        [SerializeField]
        private float duration = 1;
        [SerializeField]
        private Sprite[] Frames;


        public IEnumerator FrameAnimation(int x, int y) {
            if (duration == 0) {
                return null;
            }

            if (Frames != null) {
                return _FrameAnimation(x, y);
            }

            return null;
        }

        private IEnumerator _FrameAnimation(int x, int y) {
            long start = G.now;
            long now = start;
            GameObject go = ParticlePlayer.I.GetFromPool();
            go.transform.localPosition = new Vector3(x, y, 0);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            long durationTicks = (long)(duration * Constants.Second);
            while (now - start < durationTicks) {

                float t = (float)(now - start) / durationTicks;

                int index = (int)(t * Frames.Length) % Frames.Length;
                sr.sprite = Frames[index];

                yield return null;
                now = G.now;
            }
            ParticlePlayer.I.ReturnToPool(go);
        }

        public static IEnumerator ScaleSpriteCoroutine(long start, long duration, Transform trans) {
            long now = G.now;
            GameObject go = ParticlePlayer.I.GetFromPool();

            float x = trans.localPosition.x;
            float y = trans.localPosition.y;

            while (now - start < duration) {

                float scale = (float)(now - start) / duration;
                scale = SmoothScale(scale);

                trans.localPosition = new Vector3(x, y, 0) + 0.5f * (1 - scale) * Vector3.one;
                trans.localScale = Vector3.one * scale;

                yield return null;
                now = G.now;
            }
            ParticlePlayer.I.ReturnToPool(go);
        }
        private static float SmoothScale(float t) {
            const float c1 = 10f;
            const float c3 = c1 + 1;
            float tm = t - 1;
            return 1 + c3 * tm * tm * tm + c1 * tm * tm;
        }
    }
}
