using System;
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using NFHGame.DialogueSystem.Actors;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class DialogueBranchBox : DialogueBox {
        [SerializeField] private VerticalLayoutGroup m_Layout;
        [SerializeField] private PortraitDisplay m_Portrait;
        [SerializeField] private DialogueActor m_BastheetActor;
        [SerializeField] private BranchButton m_BranchButtonPrefab;
        [SerializeField] private RectTransform m_BranchButtonsParent;
        [SerializeField] private float m_BranchButtonScrollRectMinSize, m_BranchButtonScrollRectMaxSize;
        [SerializeField] private float m_SelectBranchAnimSpeed;
        [SerializeField] private float m_MinButtonWidth;
        [SerializeField] private float m_MaxLayoutWidth;
        [SerializeField] private float m_ScrollRectBorder;

        public BranchButton branchButtonPrefab => m_BranchButtonPrefab;
        public RectTransform branchButtonsParent => m_BranchButtonsParent;

        private Coroutine _focusOnBranchCoroutine;

        public void CreateBranch(BranchButton button, Branch branch, string branchLabel, bool wasSelectedBefore, bool locked, bool selected, Action<Branch> selectBranch) {
            button.AssignBranch(branch, branchLabel, wasSelectedBefore, locked, selected, selectBranch);
        }

        public void CreateButtons(List<BranchButton> activeButtons, RectTransform branchButtonScrollRectTransform) {
            branchButtonsParent.sizeDelta = new Vector2(m_MaxLayoutWidth, branchButtonsParent.sizeDelta.y);
            m_Layout.SetLayoutVertical();
            m_Layout.SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Layout.transform as RectTransform);

            float buttonsHeight = 12.0f + (activeButtons.Count - 1) * 4.0f;
            float buttonWidthMin = m_MinButtonWidth;
            foreach (var button in activeButtons) {
                var buttonSize = button.UpdateSize();
                buttonsHeight += buttonSize.y;
                if (buttonWidthMin < buttonSize.x) {
                    buttonWidthMin = buttonSize.x;
                }
            }

            branchButtonsParent.sizeDelta = new Vector2(buttonWidthMin + m_Layout.padding.horizontal, buttonsHeight);
            float height = Mathf.Clamp(buttonsHeight, m_BranchButtonScrollRectMinSize, m_BranchButtonScrollRectMaxSize);
            branchButtonScrollRectTransform.sizeDelta = new Vector2(branchButtonsParent.sizeDelta.x + m_ScrollRectBorder, height);

            m_Layout.SetLayoutVertical();
            m_Layout.SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Layout.transform as RectTransform);

            foreach (var button in activeButtons) {
                button.UpdateBaseLine();
            }
        }

        public void SelectBranch(int idx, ScrollRect branchButtonsScrollRect) {
            this.EnsureCoroutineStopped(ref _focusOnBranchCoroutine);
            RectTransform child = branchButtonsParent.GetChild(idx) as RectTransform;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(child.gameObject);
            _focusOnBranchCoroutine = StartCoroutine(branchButtonsScrollRect.FocusOnItemCoroutine(child, m_SelectBranchAnimSpeed));

            SetPoratrait(child.GetComponent<BranchButton>());
        }

        public void BranchMouseEnter(BranchButton button) => SetPoratrait(button);

        public override void ClearCache() {
            this.EnsureCoroutineStopped(ref _focusOnBranchCoroutine);
        }

        private void SetPoratrait(BranchButton button) {
            if (button.branch.Target is IObjectWithStageDirections target)
                m_Portrait.SetPortrait(m_BastheetActor.portraitCollection.GetPortrait(target.StageDirections, m_BastheetActor.actor), 1);
            else
                m_Portrait.SetPortrait(m_BastheetActor.portraitCollection.GetPortrait("default", m_BastheetActor.actor), 1);
        }
    }
}
