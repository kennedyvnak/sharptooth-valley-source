using NFHGame.AudioManagement;
using UnityEngine;

namespace NFHGameTests {
    public class PlaySoundtrackTest : MonoBehaviour {
        public void SetSoundtrack(AudioMusicObject musicObject) {
            SoundtrackManager.instance.SetSoundtrack(musicObject);
        }

        public void StopSoundtrack() {
            SoundtrackManager.instance.StopSoundtrack();
        }
    }
}
