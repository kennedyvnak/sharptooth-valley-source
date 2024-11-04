using NFHGame.RangedValues;

namespace NFHGame {
    [System.Serializable]
    public class TimerRNG {
        [RangedValue(0.1f, 100.0f)] public RangedFloat rate;

        [System.NonSerialized] public float targetRate;
        [System.NonSerialized] public float currentRate;
        [System.NonSerialized] public bool paused;
        [System.NonSerialized] public System.Action execute;

        public void Step(float delta) {
            if (paused) return;

            currentRate += delta;
            if (currentRate >= targetRate) {
                execute();
                Reset();
            }
        }

        public void Reset() {
            currentRate = 0.0f;
            targetRate = rate.RandomRange();
        }
    }
}