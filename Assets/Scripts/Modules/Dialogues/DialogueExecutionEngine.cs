using System.Collections.Generic;
using Articy.SharptoothValley;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using NFHGame.AchievementsManagement;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem.Actors;
using NFHGame.DialogueSystem.Portraits;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem {
    public class DialogueExecutionEngine {
        public readonly struct BranchComparer : IComparer<Branch> {
            public int Compare(Branch b0, Branch b1) {
                if (b0.Target is IObjectWithPosition pos0 && b1.Target is IObjectWithPosition pos1) {
                    return pos0.Position.y.CompareTo(pos1.Position.y);
                }
                return 0;
            }
        }

        public delegate void UpdateActorSpeech(DialogueActor actor, bool thinking, string name, string speech, Portrait portrait, bool rightSide);
        public delegate void UpdateNarrator(string speech);
        public delegate void UpdateRobotic(DialogueActor actor, string speech);
        public delegate void UpdateJournal(string speech, bool isMark);
        public delegate void UpdateCrystal(string speech, bool hurt);
        public delegate void UpdateDualCharacters(DialogueActor actorA, string nameA, Portrait portraitA, DialogueActor actorB, string nameB, Portrait portraitB, string speech);
        public delegate void UpdateTripleCharacters(DialogueActor actorA, string nameA, Portrait portraitA, DialogueActor actorB, string nameB, Portrait portraitB, DialogueActor actorC, string nameC, Portrait portraitC, string speech);
        public delegate void CreateBranch(Branch branch, string branchText, bool wasSelectedBefore, bool locked, bool selected);

        public readonly DialogueManager dialogueManager;
        public readonly ArticyFlowPlayer flowPlayer;
        public readonly RectTransform branchButtonScrollRectTransform;
        public readonly ScrollRect branchButtonsScrollRect;

        public bool running { get; private set; }

        public DialogueHandler currentHandler { get; private set; }
        public IList<Branch> currentBranches { get; private set; }
        public List<ulong> passedByBranchesID => DataManager.instance.gameData.passedByBranchesID;
        public List<ulong> lockedBranchesID => DataManager.instance.gameData.lockedBranchesID;
        public IFlowObject currentObject { get; private set; }
        public IFlowObject lastObject { get; private set; }
        public bool isBranching { get; private set; }
        public int enqueuedDialogue { get; private set; }

        public bool canStep { get; private set; }

        public int inputSelectedBranch { get; set; } = -1;
        public float lastBranchingTime { get; private set; }

        public Queue<ArticyRef> dialoguesQueue { get; private set; }
        public ArticyRef forcedDialogue { get; private set; }

        public HashSet<DialogueActor.Actor> articySetPortraitsSides { get; private set; }
        public Dictionary<DialogueActor.Actor, bool> overridePortraitsSide { get; private set; }

        public event UpdateActorSpeech OnUpdateActorSpeech;
        public event UpdateNarrator OnUpdateNarrator;
        public event UpdateCrystal OnUpdateCrystal;
        public event UpdateRobotic OnUpdateRobotic;
        public event UpdateJournal OnUpdateJournal;
        public event UpdateDualCharacters OnUpdateDualCharacters;
        public event UpdateTripleCharacters OnUpdateTripleCharacters;

        public event System.Action OnBeforeBranching;
        public event CreateBranch OnCreateBranch;
        public event System.Action<Branch> OnSelectBranch;
        public event System.Action OnCreateBranching;
        public event System.Action OnFinish;
        public event System.Action<string> OnUnlockAchievement;

        public event System.Action<string> OnFindGameTrigger;
        public event System.Action<GameTriggers.GameTriggerProcessor.GameTriggerHandler> OnProcessGameTrigger;

        public event System.Action<bool> OnCanStepChanged;

        public DialogueExecutionEngine(DialogueManager dialogueManager, ArticyFlowPlayer flowPlayer, RectTransform branchButtonScrollRectTransform, ScrollRect branchButtonScrollRect) {
            this.dialogueManager = dialogueManager;
            this.flowPlayer = flowPlayer;
            this.branchButtonScrollRectTransform = branchButtonScrollRectTransform;
            this.branchButtonsScrollRect = branchButtonScrollRect;

            dialoguesQueue = new Queue<ArticyRef>();
            articySetPortraitsSides = new HashSet<DialogueActor.Actor>();
            overridePortraitsSide = new Dictionary<DialogueActor.Actor, bool>();
            flowPlayer.BranchSorting = new BranchComparer();
        }

        public bool ClearCache() {
            if (running) return false;

            SetCanStep(false);
            currentBranches = null;
            currentObject = null;
            lastObject = null;
            isBranching = false;
            enqueuedDialogue = 0;
            canStep = false;
            inputSelectedBranch = 0;
            lastBranchingTime = 0.0f;

            return true;
        }

        public void Setup(ArticyRef aRef, DialogueHandler handler) {
            currentHandler = handler;

            ClearCache();

            running = true;
            flowPlayer.StartOn = aRef.GetObject();
            GameLogger.dialogue.Log($"Setup dialogue {aRef.id}.", LogLevel.FunctionScope);
        }

        internal void OnFlowPlayerPaused(IFlowObject aObject) {
            if (!running) return;

            currentObject = aObject;
            isBranching = false;
        }

        internal void OnBranchesUpdated(IList<Branch> aBranches) {
            if (!running) return;

            if (currentObject == null || (lastObject != null && currentObject == lastObject)) {
                Finish();
                return;
            }

            lastObject = currentObject;
            currentBranches = aBranches;
            BeforeBranching();

            if (forcedDialogue != null) {
                var chacedDialogue = forcedDialogue;
                forcedDialogue = null;
                flowPlayer.StartOn = chacedDialogue.GetObject();
                return;
            }

            if (currentObject is DialogueFragment frag) {
                if (frag.Speaker != null && !string.IsNullOrEmpty(frag.Text) && DialogueDatabase.instance.TryGetActor(frag.Speaker.TechnicalName, out var actor)) {
                    PlayBox(currentObject, actor);
                    return;
                }
                GameLogger.dialogue.LogWarning($"Found invalid Dialogue Fragment ({(currentObject as ArticyObject).TechnicalName}). Skipping it");
            }

            if (ProcessAsDialogue(currentObject)) return;

            if (currentObject is IHub && aBranches.Count > 1) {
                SetupBranching();
                return;
            }

            Step();
        }

        private bool ProcessAsDialogue(IFlowObject aObject) {
            if (aObject is GameTrigger gameTrigger) {
                var triggerID = gameTrigger.Template.GameTrigger.TriggerCode;
                OnFindGameTrigger?.Invoke(triggerID);
                currentHandler?.onDialogueProcessGameTrigger?.Invoke(triggerID);
                if (GameTriggers.GameTriggerProcessor.instance) {
                    var triggerHandler = GameTriggers.GameTriggerProcessor.instance.CreateHandler(gameTrigger);
                    var trigger = GameTriggers.GameTriggerProcessor.instance.GetTrigger(triggerID);
                    if (trigger != null) {
                        OnProcessGameTrigger?.Invoke(triggerHandler);
                        trigger.Process(triggerHandler, triggerID);
                        GameLogger.dialogue.Log($"Process Game Trigger '{triggerID}'", LogLevel.Verbose);
                        return true;
                    }
                }
            } else if (aObject is DualCharacters dualCharacters) {
                var dualCharactersTemplate = dualCharacters.Template.DualCharacters;
                var actorATechName = dualCharactersTemplate.ActorA.TechnicalName;
                var actorBTechName = dualCharactersTemplate.ActorB.TechnicalName;
                var expressionA = dualCharactersTemplate.ExpressionA;
                var expressionB = dualCharactersTemplate.ExpressionB;

                if (DialogueDatabase.instance.TryGetActor(actorATechName, out var actorA) && DialogueDatabase.instance.TryGetActor(actorBTechName, out var actorB)) {
                    OnUpdateDualCharacters?.Invoke(actorA, actorA.GetName(), ExtractPortrait(expressionA, actorA), actorB, actorB.GetName(), ExtractPortrait(expressionB, actorB), dualCharacters.ExtractText());
                    return true;
                }
            } else if (aObject is TripleCharacters tripleCharacters) {
                var tripleCharactersTemplate = tripleCharacters.Template.TripleCharacters;
                var actorATechName = tripleCharactersTemplate.ActorA.TechnicalName;
                var actorBTechName = tripleCharactersTemplate.ActorB.TechnicalName;
                var actorCTechName = tripleCharactersTemplate.ActorC.TechnicalName;
                var expressionA = tripleCharactersTemplate.ExpressionA;
                var expressionB = tripleCharactersTemplate.ExpressionB;
                var expressionC = tripleCharactersTemplate.ExpressionC;

                if (DialogueDatabase.instance.TryGetActor(actorATechName, out var actorA) && DialogueDatabase.instance.TryGetActor(actorBTechName, out var actorB) && DialogueDatabase.instance.TryGetActor(actorCTechName, out var actorC)) {
                    OnUpdateTripleCharacters?.Invoke(actorA, actorA.GetName(), ExtractPortrait(expressionA, actorA), actorB, actorB.GetName(), ExtractPortrait(expressionB, actorB), actorC, actorC.GetName(), ExtractPortrait(expressionC, actorC), tripleCharacters.ExtractText());
                    return true;
                }
            }

            return false;
        }

        public void OnFinishAnimation() {
            currentHandler?.onDialogueFinishDraw?.Invoke();
            SetCanStep(true);
        }

        public void InputStep() {
            if (!running) return;

            if (isBranching && Time.time - lastBranchingTime >= dialogueManager.branchingInputDelay) {
                SelectBranch(currentBranches[inputSelectedBranch]);
            } else if (canStep) {
                Step();
            }
        }

        public void Step() {
            if (!running) return;

            if (currentBranches == null || currentBranches.Count == 0) {
                if (dialoguesQueue.Count > 0)
                    flowPlayer.StartOn = dialoguesQueue.Dequeue().GetObject();
                else
                    Finish();
            } else if (currentBranches.Count > 1) {
                SetupBranching();
            } else {
                flowPlayer.Play();
            }
        }

        private void BeforeBranching() {
            SetCanStep(false);
            OnBeforeBranching?.Invoke();
        }

        private void SetupBranching() {
            SetCanStep(false);
            if (currentBranches.Count > 1)
                CreateBranching();
            else
                Finish();
        }

        private void CreateBranching() {
            isBranching = true;
            inputSelectedBranch = 0;

            bool selectedBranch = false;
            for (int i = 0; i < currentBranches.Count; i++) {
                Branch branch = currentBranches[i];
                if (!branch.IsValid) continue;
                var branchTargetObj = branch.Target as ArticyObject;
                if (branchTargetObj == null || branch.Target is not IDialogueFragment) continue;
                string branchLabel;
                if (branch.Target is IObjectWithMenuText objectWithMenuText && !string.IsNullOrWhiteSpace(objectWithMenuText.MenuText)) {
                    branchLabel = objectWithMenuText.MenuText;
                } else if (branch.Target is IObjectWithText objectWithText) {
                    branchLabel = objectWithText.Text;
                } else {
                    branchLabel = "[[Invalid]]";
                }

                bool locked = false;
                if (branch.Target is DialogueLocker target) {
                    locked = lockedBranchesID.Contains(branchTargetObj.Id);
                }

                bool wasSelectedBefore = passedByBranchesID.Contains(branchTargetObj.Id);
                OnCreateBranch?.Invoke(branch, branchLabel, wasSelectedBefore, locked, !selectedBranch && !locked);
                if (!selectedBranch && !locked) {
                    selectedBranch = true;
                    inputSelectedBranch = i;
                }
            }

            lastBranchingTime = Time.time;

            OnCreateBranching?.Invoke();
            currentHandler?.onDialogueShowBranches?.Invoke();
        }

        public void Navigate(int direction) {
            int currentBranchCount = currentBranches.Count;
            inputSelectedBranch = (inputSelectedBranch - direction) % currentBranchCount;
            if (inputSelectedBranch < 0) inputSelectedBranch = currentBranchCount - 1;

            while (lockedBranchesID.Contains((currentBranches[inputSelectedBranch].Target as ArticyObject).Id)) {
                inputSelectedBranch = (inputSelectedBranch - direction) % currentBranchCount;
                if (inputSelectedBranch < 0) inputSelectedBranch = currentBranchCount - 1;
            }
        }

        public void SelectBranch(Branch branch) {
            if (Time.time - lastBranchingTime < dialogueManager.branchingInputDelay) return;

            if (branch.Target is ArticyObject aObj) {
                if (!passedByBranchesID.Contains(aObj.Id) && aObj is not IgnoreBranchReaded)
                    passedByBranchesID.Add(aObj.Id);
                if (branch.Target is DialogueLocker && !lockedBranchesID.Contains(aObj.Id))
                    lockedBranchesID.Add(aObj.Id);
            }

            OnSelectBranch?.Invoke(branch);
            currentHandler?.onDialogueSelectBranch?.Invoke(branch);
            flowPlayer.Play(branch);
        }

        public void Finish() {
            if (!running) return;

            if (dialoguesQueue.Count == 0) {
                FinishFlow();
            } else {
                enqueuedDialogue = 2;
                flowPlayer.StartOn = dialoguesQueue.Dequeue().GetObject();
            }
        }

        public void FinishFlow() {
            flowPlayer.StartOn = null;

            running = false;

            foreach (var pSide in articySetPortraitsSides)
                overridePortraitsSide.Remove(pSide);
            articySetPortraitsSides.Clear();

            OnFinish?.Invoke();
            currentHandler?.onDialogueFinished?.Invoke();
            GameLogger.dialogue.Log("Finish dialogue flow", LogLevel.FunctionScope);
        }

        public void EnqueueDialogue(ArticyRef dialogue) {
            dialoguesQueue.Enqueue(dialogue);
        }

        public void ForceSetDialogue(ArticyRef dialogue) {
            forcedDialogue = dialogue;
        }

        public void SetCanStep(bool canStep) {
            this.canStep = canStep;
            OnCanStepChanged?.Invoke(canStep);
        }

        internal void UnlockAchievement(string achievementKey) {
            AchievementsManager.instance.UnlockAchievement(achievementKey);
            OnUnlockAchievement?.Invoke(achievementKey);
        }

        internal void SetPortraitSide(string character, bool leftSide) {
            if (!DialogueDatabase.instance.TryGetActor(character, out var actor)) return;
            overridePortraitsSide[actor.actor] = leftSide;
            articySetPortraitsSides.Add(actor.actor);
        }

        private void PlayBox(IFlowObject aObject, DialogueActor actor) {
            switch (actor.actor) {
                case DialogueActor.Actor.Narrator:
                    OnUpdateNarrator?.Invoke(aObject.ExtractText());
                    return;
                case DialogueActor.Actor.Crystal:
                    var crystalHurt = (aObject as IObjectWithStageDirections).StageDirections == "hurt";
                    OnUpdateCrystal?.Invoke(aObject.ExtractText(), crystalHurt);
                    return;
                case DialogueActor.Actor.LynnJournal:
                    var lynnIsMark = (aObject as IObjectWithStageDirections).StageDirections == "mark";
                    OnUpdateJournal?.Invoke(aObject.ExtractText(), lynnIsMark);
                    return;
                case DialogueActor.Actor.Arken:
                case DialogueActor.Actor.Dragon:
                    OnUpdateRobotic?.Invoke(actor, aObject.ExtractText());
                    return;
            }

            bool npc = actor.actor != DialogueActor.Actor.Bastheet && actor.actor != DialogueActor.Actor.Thinking;
            bool thinking = actor.actor == DialogueActor.Actor.Thinking;

            string speech = aObject.ExtractText();
            string id = "default";
            if (!actor.hasDefaultPortrait) {
                if (aObject is not IObjectWithStageDirections stageDirections) {
                    GameLogger.dialogue.LogWarning($"Object {(aObject as ArticyObject).TechnicalName} isn't a IObjectWithStageDirections");
                } else {
                    if (string.IsNullOrWhiteSpace(stageDirections.StageDirections)) {
                        GameLogger.dialogue.LogWarning($"Setting default portrait for object {(aObject as ArticyObject).TechnicalName} with null IObjectWithStageDirections");
                    } else {
                        id = stageDirections.StageDirections;
                    }
                }
            }
            Portrait portrait = ExtractPortrait(id, actor);

            var hasOverrideSide = overridePortraitsSide.TryGetValue(actor.actor, out var overridePortraitSide);
            OnUpdateActorSpeech?.Invoke(actor, thinking, actor.GetName(), speech, portrait, hasOverrideSide ? overridePortraitSide : npc);
        }

        private Portrait ExtractPortrait(string id, DialogueActor actor) {
            if (actor.hasDefaultPortrait) {
                return actor.GetDefaultPortrait();
            }
            return actor.portraitCollection.GetPortrait(id, actor.actor);
        }
    }
}