namespace Runtime._Debug
{
    public class FPSDebugService : IDebugSystem, IDebugGUI
    {
        private float _fps;
        private float _deltaTime;
        private const float UpdateInterval = 0.5f; // Update every 0.5 seconds

        private readonly DebugUIState _debugUIState;

        public FPSDebugService(DebugUIState debugUIState)
        {
            _debugUIState = debugUIState;
            _fps = 0f;
            _deltaTime = 0f;
        }

        public void Initialize()
        {
        }

        public void Tick()
        {
            // Update the delta time for FPS calculation
            _deltaTime += (UnityEngine.Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fps = 1.0f / _deltaTime;

            if (_deltaTime < UpdateInterval) return;
            _deltaTime = 0f; // Reset delta time after updating FPS
        }

        public void DrawDebugGUI()
        {
            if (!_debugUIState.IsVisible("FPS")) return;

            UnityEngine.GUILayout.BeginVertical("box");
            UnityEngine.GUILayout.Label($"[FPS] Current FPS: {_fps:F0}");
            UnityEngine.GUILayout.EndVertical();
        }

        public void Dispose()
        {
        }
    }
}