#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

public class DebugSystemInitializer : MonoBehaviour
{
    internal DebugSystem DebugSystem;

    private void OnGUI()
    {
        DebugSystem?.DrawDebugGUI();
    }

    private void Update()
    {
        DebugSystem?.Tick();
    }

    private void OnDestroy()
    {
        DebugSystem?.Dispose();
    }
}
#endif