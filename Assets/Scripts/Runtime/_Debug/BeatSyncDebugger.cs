using Runtime.GameServices;
using UnityEngine;

public class BeatSyncDebugService : IDebugSystem, IDebugGUI
{
    private readonly BeatSyncService _beatSync;
    private readonly DebugUIState _debugUIState;

    public BeatSyncDebugService(BeatSyncService beatSync, DebugUIState state)
    {
        _beatSync = beatSync;
        _debugUIState = state;
    }

    public void Initialize()
    {
    }

    public void Tick()
    {
    }

    public void DrawDebugGUI()
    {
        if (!_debugUIState.IsVisible("BeatSync")) return;

        GUILayout.BeginVertical("box");
        GUILayout.Label($"[BeatSync] Current Beat: {_beatSync.timelineInfo.currentBeat}");
        GUILayout.Label($"[BeatSync] Last Marker: {_beatSync.timelineInfo.lastMarker}");
        GUILayout.EndVertical();
    }

    public void Dispose()
    {
    }
}