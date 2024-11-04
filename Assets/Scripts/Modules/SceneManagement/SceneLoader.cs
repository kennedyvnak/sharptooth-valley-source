using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NFHGame.Input;
using NFHGame.SceneManagement.SceneState;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NFHGame.SceneManagement {
    public class SceneLoader : SingletonPersistent<SceneLoader> {
        public delegate void SceneLoadingOperation(SceneLoadingHandler handler);

        public class SceneLoadingHandler {
            public readonly SceneReference sceneReference;
            public readonly string anchorID;

            public AsyncOperation operation { get; internal set; }
            public bool activated { get; internal set; }
            public Coroutine coroutine { get; internal set; }
            public bool inputStopped { get; private set; }

            public bool error { get; set; }

            public bool blackScreen { get; set; } = false;
            public bool saveGame { get; set; } = true;
            public bool charactersToLeftSide { get; set; } = false;

            public SceneLoadingHandler(SceneReference sceneReference, string anchorID) {
                this.sceneReference = sceneReference;
                this.anchorID = anchorID;
            }

            public void StopInput() {
                if (inputStopped || error) return;
                InputReader.instance.PushMap(InputReader.InputMap.None);
                GameManager.instance.playTimeCouting = false;
                inputStopped = true;
            }

            public void ResumeInput() {
                if (!inputStopped) return;
                InputReader.instance.PopMap(InputReader.InputMap.None);
                GameManager.instance.playTimeCouting = true;
                inputStopped = false;
            }
        }

        [SerializeField] private CanvasGroup m_LoadingScreenGroup;
        [SerializeField] private RectTransform m_CharactersTransform;
        [SerializeField] private Animations.SpriteArrayAnimator[] m_CharacterAnimators;
        [SerializeField] private Image m_SpammyImage;
        [SerializeField] private TMPro.TextMeshProUGUI m_LoadingText;
        [SerializeField] private float m_TweenDuration = 7.0f / 9.0f;
        [SerializeField] private float m_TotalScreenDuration = 3.0f;
        [SerializeField] private int m_TextEllipsisCount = 3;
        [SerializeField] private float m_TextEllipsisAnimDuration = 1.0f;

        [Header("Save Icon")]
        [SerializeField] private CanvasGroup m_SaveIcon;

        private bool _isLoadingScene = false;
        private SceneReference _currentSceneKey;
        private Coroutine _loadSceneCoroutine;
        private Tweener _ellipsisAnimationTweener;

        public SceneReference currentSceneKey => _currentSceneKey;
        public CanvasGroup saveIcon => m_SaveIcon;
        public bool isLoadingScene => _isLoadingScene;

        public event SceneLoadingOperation BeforeLoadScene;
        public event SceneLoadingOperation OnSceneActivate;
        public event SceneLoadingOperation OnSceneLoadFinish;

        private void Start() {
            _ellipsisAnimationTweener = DOVirtual.Int(0, m_TextEllipsisCount, m_TextEllipsisAnimDuration, SetLoadingTextEllipsisCount)
                .SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true).Pause();
        }

        public SceneLoadingHandler CreateHandler(SceneReference sceneReference, string anchorID) {
            return new SceneLoadingHandler(sceneReference, anchorID);
        }

        public void LoadScene(SceneLoadingHandler handler) {
            if (_isLoadingScene) {
                handler.error = true;
                GameLogger.loader.LogWarning("Trying to load a scene while another scene is loading.");
                return;
            }

            _currentSceneKey = handler.sceneReference;
            _loadSceneCoroutine = StartCoroutine(LoadSceneInternal(handler));
            handler.coroutine = _loadSceneCoroutine;
        }

        public SceneLoadingHandler LoadScene(SceneReference sceneReference, string anchorID) {
            var handler = CreateHandler(sceneReference, anchorID);
            LoadScene(handler);
            return handler;
        }

        public void RawLoadScene(SceneReference sceneReference) {
            SceneManager.LoadScene(sceneReference, LoadSceneMode.Single);
        }

        private IEnumerator LoadSceneInternal(SceneLoadingHandler handler) {
            GameLogger.loader.Log($"Start loading scene {handler.sceneReference.scenePath} with anchor {handler.anchorID}");
            bool showObjects = !handler.blackScreen;
            _isLoadingScene = true;
            if (handler.saveGame)
                Serialization.DataManager.instance.ClearSave();
            float startTime = Time.unscaledTime;

            BeforeLoadScene?.Invoke(handler);

            var op = SceneManager.LoadSceneAsync(handler.sceneReference);
            op.allowSceneActivation = false;
            handler.operation = op;

            m_SpammyImage.gameObject.SetActive(GameManager.instance.spammyInParty);

            {
                var scale = m_CharactersTransform.localScale;
                scale.x = handler.charactersToLeftSide ? -1 : 1;
                m_CharactersTransform.localScale = scale;
            }

            foreach (var characterAnim in m_CharacterAnimators) {
                characterAnim.enabled = showObjects;
                if (characterAnim.TryGetComponent<Image>(out var animImage))
                    animImage.enabled = showObjects;
            }

            m_LoadingText.enabled = showObjects;
            if (showObjects) _ellipsisAnimationTweener.Play();

            m_LoadingScreenGroup.blocksRaycasts = true;
            m_LoadingScreenGroup.interactable = true;
            var fadeInTween = DOVirtual.Float(0.0f, 1.0f, m_TweenDuration, value => m_LoadingScreenGroup.alpha = value).SetUpdate(true);

            yield return fadeInTween.WaitForCompletion();
            op.allowSceneActivation = true;
            yield return op;

            handler.activated = true;
            OnSceneActivate?.Invoke(handler);

            Resources.UnloadUnusedAssets();

            while (Time.unscaledTime - startTime < m_TotalScreenDuration) {
                yield return null;
            }

            List<SceneLoadAnchor> allAnchors = new List<SceneLoadAnchor>();
            SceneLoadAnchor anchor = null;

            foreach (var sceneAnchorObj in GameObject.FindGameObjectsWithTag("SceneAnchor")) {
                if (sceneAnchorObj.TryGetComponent<SceneLoadAnchor>(out var sceneAnchor)) {
                    allAnchors.Add(sceneAnchor);
                    if (sceneAnchor.anchorID.Equals(handler.anchorID)) {
                        anchor = sceneAnchor;
                        break;
                    }
                }
            }

            if (SceneStateController.instance) {
                SceneStateController.instance.BeforeAnchors(handler, allAnchors, ref anchor);
            }

            if (anchor) anchor.onLoad?.Invoke(handler);

            if (SceneStateController.instance) {
                SceneStateController.instance.StartControl(handler);
            }

            var fadeOutTween = DOVirtual.Float(1.0f, 0.0f, m_TweenDuration, value => m_LoadingScreenGroup.alpha = value).SetUpdate(true);
            yield return fadeOutTween.WaitForCompletion();
            m_LoadingScreenGroup.blocksRaycasts = false;
            m_LoadingScreenGroup.interactable = false;

            if (showObjects) {
                foreach (var characterAnim in m_CharacterAnimators) {
                    characterAnim.enabled = false;
                }
            }

            if (showObjects) _ellipsisAnimationTweener.Pause();

            _isLoadingScene = false;
            OnSceneLoadFinish?.Invoke(handler);
        }

        private void SetLoadingTextEllipsisCount(int value) {
            var charCount = m_LoadingText.textInfo.characterCount;
            m_LoadingText.maxVisibleCharacters = charCount - (m_TextEllipsisCount - value);
        }
    }
}