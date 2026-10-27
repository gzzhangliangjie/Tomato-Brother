using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effect
{
    public class Burst : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem burstParticle;
        private Action _releaseAction;

        // duration 随特效预制体变化，按值缓存避免每次爆炸都分配
        private static readonly Dictionary<float, WaitForSeconds> WaitCache = new();

        private void Awake()
        {
            burstParticle = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            StartCoroutine(RecycleBurst(burstParticle.main.duration));
        }

        public void Play()
        {
            burstParticle.Play();
        }
        
        public void SetDeactivateAction(Action releaseObj)
        {
            _releaseAction = releaseObj;
        }

        /// <summary>
        /// 在一定时间后进行回收
        /// </summary>
        /// <returns></returns>
        private IEnumerator RecycleBurst(float duration = 0.3f)
        {
            if (!WaitCache.TryGetValue(duration, out var wait))
            {
                wait = new WaitForSeconds(duration);
                WaitCache[duration] = wait;
            }
            yield return wait;
            _releaseAction.Invoke();
        }
    }
}