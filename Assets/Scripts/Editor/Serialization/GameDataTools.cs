using System.IO;
using System.Text.RegularExpressions;
using NFHGame;
using UnityEditor;
using UnityEngine;

namespace NFHGameEditor.Serialization {
    public static class GameDataTools {
        private const string k_UserSavePattern = @"^user_(-?\d{1,10})\.save$";
        private static readonly Regex k_UserSaveRegex = new Regex(k_UserSavePattern);

        [MenuItem("Tools/Delete All Data")]
        private static void DeleteAllData() {
            int option = EditorUtility.DisplayDialogComplex("Deleting all data", "Does you want to delete all data saved on this computer?", "Yes", "No", "Delete Only PlayerPrefs");
            switch (option) {
                case 0:
                    DeleteSavedData();
                    break;
                case 1:
                    break;
                case 2:
                    PlayerPrefs.DeleteAll();
                    break;
                default:
                    Debug.Log(option);
                    break;
            }
        }

        public static void DeleteSavedData() {
            foreach (var file in Directory.GetFiles(Application.persistentDataPath)) {
                string filename = Path.GetFileName(file);
                if (k_UserSaveRegex.IsMatch(filename) || filename == "game.data") {
                    File.Delete(file);
                }
            }
        }
    }
}