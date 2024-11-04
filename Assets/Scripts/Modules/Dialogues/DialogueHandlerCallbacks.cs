using Articy.Unity;
using NFHGame.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueHandlerCallbacks {
    [SerializeField] private UnityEvent m_OnDialogueStartDraw;
    [SerializeField] private UnityEvent m_OnDialogueFinishDraw;
    [SerializeField] private UnityEvent m_OnDialogueShowBranches;
    [SerializeField] private UnityEvent<Branch> m_OnDialogueSelectBranch;
    [SerializeField] private UnityEvent<string> m_OnDialogueProcessGameTrigger;
    [SerializeField] private UnityEvent m_OnDialogueFinished;

    public UnityEvent onDialogueStartDraw { get => m_OnDialogueStartDraw; set => m_OnDialogueStartDraw = value; }
    public UnityEvent onDialogueFinishDraw { get => m_OnDialogueFinishDraw; set => m_OnDialogueFinishDraw = value; }
    public UnityEvent onDialogueShowBranches { get => m_OnDialogueShowBranches; set => m_OnDialogueShowBranches = value; }
    public UnityEvent<Branch> onDialogueSelectBranch { get => m_OnDialogueSelectBranch; set => m_OnDialogueSelectBranch = value; }
    public UnityEvent<string> onDialogueProcessGameTrigger { get => m_OnDialogueProcessGameTrigger; set => m_OnDialogueProcessGameTrigger = value; }
    public UnityEvent onDialogueFinished { get => m_OnDialogueFinished; set => m_OnDialogueFinished = value; }

    public void Connect(DialogueHandler handler) {
        Disconnect(handler);
        if (m_OnDialogueStartDraw != null) handler.onDialogueStartDraw += m_OnDialogueStartDraw.Invoke;
        if (m_OnDialogueFinishDraw != null) handler.onDialogueFinishDraw += m_OnDialogueFinishDraw.Invoke;
        if (m_OnDialogueShowBranches != null) handler.onDialogueShowBranches += m_OnDialogueShowBranches.Invoke;
        if (m_OnDialogueSelectBranch != null) handler.onDialogueSelectBranch += m_OnDialogueSelectBranch.Invoke;
        if (m_OnDialogueProcessGameTrigger != null) handler.onDialogueProcessGameTrigger += m_OnDialogueProcessGameTrigger.Invoke;
        if (m_OnDialogueFinished != null) handler.onDialogueFinished += m_OnDialogueFinished.Invoke;
    }

    private void Disconnect(DialogueHandler handler) {
        if (m_OnDialogueStartDraw != null) handler.onDialogueStartDraw -= m_OnDialogueStartDraw.Invoke;
        if (m_OnDialogueFinishDraw != null) handler.onDialogueFinishDraw -= m_OnDialogueFinishDraw.Invoke;
        if (m_OnDialogueShowBranches != null) handler.onDialogueShowBranches -= m_OnDialogueShowBranches.Invoke;
        if (m_OnDialogueSelectBranch != null) handler.onDialogueSelectBranch -= m_OnDialogueSelectBranch.Invoke;
        if (m_OnDialogueProcessGameTrigger != null) handler.onDialogueProcessGameTrigger -= m_OnDialogueProcessGameTrigger.Invoke;
        if (m_OnDialogueFinished != null) handler.onDialogueFinished -= m_OnDialogueFinished.Invoke;
    }
}