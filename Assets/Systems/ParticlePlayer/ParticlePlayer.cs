
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace W
{
    /// <summary>
    /// just an object pool
    /// </summary>
    public class ParticlePlayer : MonoBehaviour
    {
        public static ParticlePlayer I => i;
        private static ParticlePlayer i;
        private void Awake() {
            A.Singleton(ref i, this);
            trans = transform;
        }

        private Transform trans;

        [SerializeField]
        private GameObject pefab;

        private List<GameObject> pool = new List<GameObject>();
        public GameObject GetFromPool() {
            GameObject go;
            if (pool.Count > 0) {
                go = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            } else {
                go = Instantiate(pefab, trans);
            }
            go.SetActive(true);
            return go;
        }
        public void ReturnToPool(GameObject go) {
            go.SetActive(false);
            pool.Add(go);
        }

        [Header("Defs")]
        [SerializeField]
        private ParticleDef construct;
        public ParticleDef Construct => construct;
        [SerializeField]
        private ParticleDef destruct;
        public ParticleDef Destruct => destruct;

        public void FrameAnimation(ParticleDef particle, int x, int y) {
            if (particle == null) return;
            IEnumerator coroutine = particle.FrameAnimation(x, y);
            if  (coroutine != null) {
               StartCoroutine(coroutine);
            }
        }
    }
}


