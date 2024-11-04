using Articy.SharptoothValley.GlobalVariables;
using NFHGame.ArticyImpl.Variables;
using System.Collections;
using UnityEngine;
using static NFHGame.ArticyImpl.ArticyManager;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class TrustFill : GameTrigger {

        [SerializeField] private int m_EndTrust;
        [SerializeField] private float m_FillDuration;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            var trustPoints = ArticyVariables.globalVariables.trustPoints;
            var dinnerPoints = trustPoints.dinnerPoints;
            if (dinnerPoints >= m_EndTrust) {
                handler.onReturnToDialogue.Invoke();
                return false;
            }
            
            StartCoroutine(TrustFillCoroutine(handler, trustPoints));
            return true;
        }

        private IEnumerator TrustFillCoroutine(GameTriggerProcessor.GameTriggerHandler handler, trustPoints trustPoints) {
            var scope = new ToggleRollbackScope(false);
            while (trustPoints.dinnerPoints < m_EndTrust) {
                trustPoints.dinnerPoints++;
                yield return Helpers.GetWaitForSeconds(m_FillDuration);
            }
            scope.Dispose();
            handler.onReturnToDialogue.Invoke();
        }
    }
}
