public class DebugInputService : IDebugSystem
{
    private readonly InputActions input;
    private readonly DebugUIState _debugUIState;

    public DebugInputService(DebugUIState debugUIState)
    {
        _debugUIState = debugUIState;
        input = new InputActions();
        input.Debug.Enable();
        input.Debug.ToggleBeatDebug.performed += _ => _debugUIState.Toggle("BeatSync");
        input.Debug.ToggleFPSDebug.performed += _ => _debugUIState.Toggle("FPS");
        input.Debug.ToggleTimelineDebug.performed += _ => _debugUIState.Toggle("Timeline");
    }

    public void Initialize()
    {
    }

    public void Tick()
    {
    }

    public void DrawDebugGUI()
    {
    }

    public void Dispose()
    {
        input.Dispose();
    }
}