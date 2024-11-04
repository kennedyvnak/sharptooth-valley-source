using NFHGame.DialogueSystem.Actors;
using NFHGame.ScriptableSingletons;
using UnityEngine;

namespace NFHGame.DialogueSystem {
    public class DialogueDatabase : ScriptableSingleton<DialogueDatabase> {
        [SerializeField] private SerializedDictionary<string, DialogueActor> m_Actors;

        public bool TryGetActor(string actorTechName, out DialogueActor actor) => m_Actors.TryGetValue(actorTechName, out actor);
    }
}
