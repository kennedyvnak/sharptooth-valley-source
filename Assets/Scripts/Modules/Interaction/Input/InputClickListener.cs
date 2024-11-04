namespace NFHGame.Interaction.Input {
    [System.Flags]
    public enum InputClickProcessResult {
        None = 0,
        ShouldIgnore = 1 << 0,
        Interacted = 1 << 1,
    }

    public interface IInputClickListener {
        InputClickProcessResult Process(InputClickController controller);
    }
}