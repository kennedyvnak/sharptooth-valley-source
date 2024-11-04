using System;
using System.Collections.Generic;
using NFHGame.Serialization.States;
using UnityEngine;

namespace NFHGame.Serialization {
    [System.Serializable]
    public class GameData : System.ICloneable {
        [System.Serializable]
        public struct ItemReadData {
            public string key;
            public int dialogue;
            public int readCount;
        }

        public ulong version = 0L;

        public int userId;
        public long lastSerialization;
        public double playTime;

        public bool haloActive;

        public SerializationState state;

        public List<string> gameKeys;

        public List<ItemReadData> readItems;

        public List<ulong> passedByBranchesID;
        public List<ulong> lockedBranchesID;
        public int readPassage;

        public int itemsSacrificesCount;
        public SceneManagement.SceneState.Level4e1StateController.PuzzleState lastPuzzleState;

        public List<ulong> articyVariablesDialoguesLock;
        public string articyNumberVariables;
        public string articyStringVariables;

        public DateTime serializationDate => DateTime.FromBinary(lastSerialization);

        public GameData Clone() {
            var json = JsonUtility.ToJson(this, false);
            return JsonUtility.FromJson<GameData>(json);
        }
        object System.ICloneable.Clone() => Clone();
    }
}