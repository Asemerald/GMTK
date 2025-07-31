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
        //TODO to add 
        //input.Debug.ToggleFpsDebug.performed += _ => _debugUIState.Toggle("FPS");
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