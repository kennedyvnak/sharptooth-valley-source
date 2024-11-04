using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using NFHGame.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame {
    public static class Helpers {
        public struct StringHelpers {
            public static bool StartsWith(string a, string b) {
                int aLen = a.Length;
                int bLen = b.Length;

                int ap = 0; int bp = 0;

                while (ap < aLen && bp < bLen && a[ap] == b[bp]) {
                    ap++;
                    bp++;
                }

                return (bp == bLen);
            }

            public static bool EndsWith(string a, string b) {
                int ap = a.Length - 1;
                int bp = b.Length - 1;

                while (ap >= 0 && bp >= 0 && a[ap] == b[bp]) {
                    ap--;
                    bp--;
                }

                return (bp < 0);
            }
        }

        public static void SetNavigation(this Selectable from, Selectable up = null, Selectable down = null, Selectable left = null, Selectable right = null) {
            var nav = from.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = up;
            nav.selectOnDown = down;
            nav.selectOnLeft = left;
            nav.selectOnRight = right;
            from.navigation = nav;
        }

        public static void SetUpNav(this Selectable from, Selectable up) {
            var nav = from.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = up;
            from.navigation = nav;
        }

        public static void SetDownNav(this Selectable from, Selectable down) {
            var nav = from.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = down;
            from.navigation = nav;
        }

        public static void SetLeftNav(this Selectable from, Selectable left) {
            var nav = from.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnLeft = left;
            from.navigation = nav;
        }

        public static void SetRightNav(this Selectable from, Selectable right) {
            var nav = from.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnRight = right;
            from.navigation = nav;
        }

        public const Ease CameraInEase = Ease.InSine;
        public const Ease CameraOutEase = Ease.OutSine;

        public static Camera mainCamera => CameraController.instance ? CameraController.instance.camera : null;
        public static CinemachineVirtualCamera vCam => CameraController.instance ? CameraController.instance.vCam : null;

        private static readonly Dictionary<float, WaitForSeconds> s_WaitForSeconds = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWaitForSeconds(float seconds) {
            if (s_WaitForSeconds.TryGetValue(seconds, out var waitForSeconds)) {
                return waitForSeconds;
            }

            waitForSeconds = new WaitForSeconds(seconds);
            s_WaitForSeconds[seconds] = waitForSeconds;
            return waitForSeconds;
        }

        public static IEnumerator DelayForFramesCoroutine(int frames, System.Action action) {
            for (int i = 0; i < frames; i++)
                yield return null;

            action?.Invoke();
        }

        public static void EnsureCoroutineStopped(this MonoBehaviour behaviour, ref Coroutine coroutine) {
            if (behaviour && coroutine != null) {
                behaviour.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public static void ToggleGroup(this CanvasGroup group, bool enabled) {
            group.alpha = enabled ? 1.0f : 0.0f;
            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }

        public static Tweener ToggleGroupAnimated(this CanvasGroup group, bool enabled, float animDuration) {
            if (!enabled) {
                group.interactable = false;
                group.blocksRaycasts = false;
                return DOVirtual.Float(group.alpha, 0.0f, animDuration, x => group.alpha = x).SetUpdate(true).SetTarget(group);
            } else {
                return DOVirtual.Float(group.alpha, 1.0f, animDuration, x => group.alpha = x).OnComplete(() => {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }).SetUpdate(true).SetTarget(group);
            }
        }

        public static Tweener ToggleScreen(this CanvasGroup group, bool enabled) {
            float fadeDuration = Screens.ScreenManager.instance.screenFadeDuration;
            if (!enabled) {
                group.interactable = true;
                group.blocksRaycasts = false;
                return DOVirtual.Float(group.alpha, 0.0f, fadeDuration, x => group.alpha = x).SetEase(Ease.OutSine).SetUpdate(true).SetTarget(group);
            } else {
                return DOVirtual.Float(group.alpha, 1.0f, fadeDuration, x => group.alpha = x).SetEase(Ease.InSine).OnComplete(() => {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }).SetUpdate(true).SetTarget(group);
            }
        }

        public static void DestroyChildren(this Transform transform) {
            int children = transform.childCount;
            for (int i = children - 1; i >= 0; i--) {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint) {
            Vector2 contentSize = scrollView.content.rect.size;
            Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
            Vector2 contentScale = scrollView.content.localScale;

            contentSize.Scale(contentScale);
            focusPoint.Scale(contentScale);

            Vector2 scrollPosition = scrollView.normalizedPosition;
            if (scrollView.horizontal && contentSize.x > viewportSize.x)
                scrollPosition.x = Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
            if (scrollView.vertical && contentSize.y > viewportSize.y)
                scrollPosition.y = Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));

            return scrollPosition;
        }

        public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, RectTransform item) {
            Vector2 itemCenterPoint = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

            Vector2 contentSizeOffset = scrollView.content.rect.size;
            contentSizeOffset.Scale(scrollView.content.pivot);

            return scrollView.CalculateFocusedScrollPosition(itemCenterPoint + contentSizeOffset);
        }

        public static void FocusAtPoint(this ScrollRect scrollView, Vector2 focusPoint) {
            scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(focusPoint);
        }

        public static void FocusOnItem(this ScrollRect scrollView, RectTransform item) {
            scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(item);
        }

        private static IEnumerator LerpToScrollPositionCoroutine(this ScrollRect scrollView, Vector2 targetNormalizedPos, float speed) {
            Vector2 initialNormalizedPos = scrollView.normalizedPosition;

            float t = 0f;
            while (t < 1f) {
                scrollView.normalizedPosition = Vector2.LerpUnclamped(initialNormalizedPos, targetNormalizedPos, 1f - (1f - t) * (1f - t));

                yield return null;
                t += speed * Time.unscaledDeltaTime;
            }

            scrollView.normalizedPosition = targetNormalizedPos;
        }

        public static IEnumerator FocusAtPointCoroutine(this ScrollRect scrollView, Vector2 focusPoint, float speed) {
            yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(focusPoint), speed);
        }

        public static IEnumerator FocusOnItemCoroutine(this ScrollRect scrollView, RectTransform item, float speed) {
            yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(item), speed);
        }

        public static void SetLeft(this RectTransform rt, float left) {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right) {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top) {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom) {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }
}