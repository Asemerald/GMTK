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
        GUILayout.Label($"[BeatSync] Current Half Beat: {_beatSync.timelineInfo.currentHalfBeat}");
        GUILayout.Label($"[BeatSync] Current Quarter Beat: {_beatSync.timelineInfo.currentQuarterBeat}");
        GUILayout.Label($"[BeatSync] Current Bar: {_beatSync.timelineInfo.currentBar}");
        GUILayout.Label($"[BeatSync] Last Marker: {(string)_beatSync.timelineInfo.lastMarker}");
        GUILayout.EndVertical();
    }

    public void Dispose()
    {
    }
}