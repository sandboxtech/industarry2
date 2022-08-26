
using UnityEngine;

namespace W
{
    public class Audio : MonoBehaviour
    {
        public static Audio I => i;
        private static Audio i;
        private void Awake() {
            A.Singleton(ref i, this);
        }

        [SerializeField]
        private AudioSource MusicSource;

        [SerializeField]
        private AudioSource SoundSource;


        [Header("Sounds")]

        [SerializeField]
        private AudioClip positiveSound;
        public AudioClip PositiveSound => positiveSound;
        [SerializeField]
        private AudioClip negativeSound;
        public AudioClip NegativeSound => negativeSound;


        [SerializeField]
        private AudioClip defaultSound;
        public AudioClip DefaultSound => defaultSound;

        [SerializeField]
        private AudioClip[] constructSound;
        public AudioClip[] ConstructSound => constructSound;

        [SerializeField]
        private AudioClip[] destructSound;
        public AudioClip[] DestructSound => destructSound;


        public AudioClip Clip {
            set {
                clips = null;
                clip = value;
            }
        }
        public AudioClip[] Clips {
            set {
                clip = null;
                clips = value;
            }
        }

        private AudioClip clip = null;
        private AudioClip[] clips = null;
        private void Update() {
            if (clip != null) {
                Randomize();
                SoundSource.PlayOneShot(clip);
                clip = null;
            }
            else if (clips != null) {
                Randomize();
                SoundSource.PlayOneShot(clips[RandomGenerator.I.Int % clips.Length]);
                clips = null;
            }
        }
        private void Randomize() {
            SoundSource.pitch = M.Lerp(15/16f, 1f, RandomGenerator.I.Float);
            SoundSource.volume = M.Lerp(14/16f, 1f, H.Hash(RandomGenerator.I.UInt));
        }
    }
}
