using UnityEngine;
using System.Collections.Generic;

namespace NFHGame.DialogueSystem.Portraits {
    public class Portrait {
        public Vector2 offsetPerPart;
        public Sprite body;
        public Sprite flippedBody;
        public float boxRightOffset;
        public Actors.DialogueActor.Actor actor;
        public bool facingRight;
        public Sprite[] parts;
        public int[] order;

        public Portrait(Vector2 offsetPerPart, Sprite body, Sprite flippedBody, float boxRightOffset, int length, bool facingRight, Actors.DialogueActor.Actor actor) {
            this.offsetPerPart = offsetPerPart;
            this.body = body;
            this.flippedBody = flippedBody;
            this.boxRightOffset = boxRightOffset;
            this.facingRight = facingRight;
            this.actor = actor;
            parts = new Sprite[length];
            order = new int[length];
        }
    }

    [CreateAssetMenu(menuName = "Scriptable/Dialogue/Portrait Collection")]
    public class PortraitCollection : ScriptableObject {
        [System.Serializable]
        public class PortraitPartCollection {
            public List<Sprite> parts;
            public int order;
        }

        [System.Serializable]
        public class PortraitPrefab {
            public int[] indexes;
        }

        public bool facingRight;
        public Vector2 offsetPerPart;
        public float boxRightOffset;
        public Sprite body;
        public Sprite flippedBody;
        public PortraitPartCollection[] partsCollection;
        public SerializedDictionary<string, PortraitPrefab> prefabs;

        private Portrait _portrait;

        public Portrait GetPortrait(string id, Actors.DialogueActor.Actor actor) {
            if (Helpers.StringHelpers.StartsWith(id, ":"))
                return GetFromIndex(id.Remove(0, 1), actor);
            else
                return GetFromName(id, actor);
        }

        public Portrait GetFromIndex(string rawIndex, Actors.DialogueActor.Actor actor) {
            try {
                var indexes = System.Array.ConvertAll(rawIndex.Split(','), i => int.Parse(i));
                return GetFromIndexes(indexes, actor);
            } catch (System.FormatException e) {
                GameLogger.dialogue.LogWarning(e.ToString());
                return GetFromName("default", actor);
            }
        }

        public Portrait GetFromName(string name, Actors.DialogueActor.Actor actor) {
            if (prefabs.TryGetValue(name, out var v)) {
                return GetFromIndexes(v.indexes, actor);
            } else {
                GameLogger.dialogue.LogWarning($"Can't find portrait '{name}'");
                return GetFromName("default", actor);
            }
        }

        public Portrait GetFromIndexes(int[] indexes, Actors.DialogueActor.Actor actor) {
            if (indexes.Length != partsCollection.Length) {
                GameLogger.dialogue.LogWarning("Indexes length doesn't match portrait parts length");
                return GetFromName("default", actor);
            }

            _portrait ??= new Portrait(offsetPerPart, body, flippedBody, boxRightOffset, indexes.Length, facingRight, actor);

            for (int i = 0; i < indexes.Length; i++) {
                int index = indexes[i];
                var collection = partsCollection[i];
                if (index < 0 || index >= collection.parts.Count) {
                    GameLogger.dialogue.LogError("Index invalid at PortraitCollection.GetFromIndexes()");
                    index = 0;
                }

                _portrait.parts[i] = collection.parts[index];
                _portrait.order[i] = collection.order;
            }

            return _portrait;
        }
    }
}