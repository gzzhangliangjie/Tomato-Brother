using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Effect
{
    public class HurtText : MonoBehaviour
    {
        private Action _releaseAction;

        // 每次受击都会弹出飘字，等待对象缓存为静态避免反复分配
        private static readonly WaitForSeconds HideInterval = new(0.25f);

        private void OnEnable()
        {
            StartCoroutine(HideText());
        }

        public void SetDeactivateAction(Action releaseObj)
        {
            _releaseAction = releaseObj;
        }

        private IEnumerator HideText()
        {
            transform.DOBlendableMoveBy(new Vector2(0, 2f), 0.1f);
            yield return HideInterval;
            transform.DOBlendableMoveBy(new Vector2(0, -0.5f), 0.1f);
            yield return HideInterval;
            
            _releaseAction.Invoke();
        }
    }
}