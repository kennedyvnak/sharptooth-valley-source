using Cinemachine;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Graphics;
using NFHGame.RangedValues;
using System.Collections;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class RockFall : GameTrigger {
        public const string RockFallScreenShakeTweenID = "RockFallScreenShake";
        [SerializeField] private CinemachineImpulseSource m_DefaultSource;
        [SerializeField] private float m_RockFallDuration = 3.0f;

        [SerializeField, RangedValue(-100.0f, 100.0f)] private RangedFloat m_SpawnRangeY;
        [SerializeField] private float m_BastheetDeadzone;
        [SerializeField] private float m_SpawnRangeX;

        [SerializeField] private AudioObject m_QuakeSound;
        [SerializeField] private TimerRNG m_RockTimer;

        private Coroutine _rockCoroutine;
        private PooledAudioHandler _quakeSourceHandler;
        private bool _inRockFall;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            if (_inRockFall) {
                handler.onReturnToDialogue?.Invoke();
                return false;
            }

            StartRockFall();
            DOVirtual.DelayedCall(m_RockFallDuration, () => {
                CameraController.instance.camera.transform.rotation = Quaternion.identity;
                handler.onReturnToDialogue?.Invoke();
            });

            return true;
        }

        public void StartRockFall() {
            if (_inRockFall) return;

            _inRockFall = true;
            _rockCoroutine = StartCoroutine(RockFallCoroutine());
            m_DefaultSource.GenerateImpulse();
            _quakeSourceHandler = AudioPool.instance.PlaySound(m_QuakeSound);

            DOVirtual.DelayedCall(m_DefaultSource.m_ImpulseDefinition.m_ImpulseDuration, () => {
                m_DefaultSource.GenerateImpulse();
            }).SetLoops(-1, LoopType.Restart).SetId(RockFallScreenShakeTweenID).OnKill(StopRockFall);
        }

        public void StopRockFall() {
            this.EnsureCoroutineStopped(ref _rockCoroutine);
            _quakeSourceHandler.source.DOFade(0.0f, 3.0f).SetEase(Helpers.CameraOutEase).OnComplete(() => _quakeSourceHandler.source.Stop());
            _inRockFall = false;
        }

        private IEnumerator RockFallCoroutine() {
            m_RockTimer.execute = SpawnRock;
            m_RockTimer.Reset();

            while (true) {
                m_RockTimer.Step(Time.deltaTime);
                yield return null;
            }
        }

        private void SpawnRock() {
            var size = RockProvider.instance.GetRandomSize();
            var rock = RockProvider.instance.SpawnRock(RockProvider.instance.GetRandomRock(size), size, GetRandomPosition(), true, transform);
            rock.fakeBroke = true;
        }

        private Vector3 GetRandomPosition() {
            var bastheetPosX = GameCharactersManager.instance.bastheet.transform.position.x;
            bool left = Random.value > 0.5f;
            var posX = bastheetPosX + (left ? -Random.value * m_SpawnRangeX - m_BastheetDeadzone : Random.value * m_SpawnRangeX + m_BastheetDeadzone);
            return new Vector3(posX, m_SpawnRangeY.RandomRange(), 0.0f);
        }
    }
}
