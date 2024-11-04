using System;
using DG.Tweening;
using UnityEngine;

namespace NFHGame.UI {
    public class FadeScreen : Singleton<FadeScreen> {
        public class FadeHandler {
            public readonly float duration;
            internal readonly Action<FadeHandler> fadeOut;

            public Action onFinishFadeIn;
            public Action onFinishFadeOut;

            public FadeHandler(float duration, Action<FadeHandler> fadeOut) {
                this.duration = duration;
                this.fadeOut = fadeOut;
            }

            public void FadeOut() {
                fadeOut?.Invoke(this);
            }
        }

        [SerializeField] private CanvasGroup m_Group;

        private Tweener _tweener;

        public FadeHandler FadeFor(float seconds) {
            FadeHandler handler = new FadeHandler(seconds, FadeOut);
            StartFade(handler);
            return handler;
        }

        private void FadeOut(FadeHandler handler) {
            EndFade(handler);
        }

        private void StartFade(FadeHandler handler) {
            if (_tweener.IsActive())
                _tweener.Kill();

            m_Group.blocksRaycasts = true;
            m_Group.interactable = true;
            _tweener = DOVirtual.Float(m_Group.alpha, 1.0f, handler.duration, x => m_Group.alpha = x).OnComplete(() => handler.onFinishFadeIn?.Invoke());
        }

        private void EndFade(FadeHandler handler) {
            if (_tweener.IsActive())
                _tweener.Kill();

            _tweener = DOVirtual.Float(m_Group.alpha, 0.0f, handler.duration, x => m_Group.alpha = x).OnComplete(() => {
                m_Group.blocksRaycasts = false;
                m_Group.interactable = false;
                handler.onFinishFadeOut?.Invoke();
            });
        }
    }
}