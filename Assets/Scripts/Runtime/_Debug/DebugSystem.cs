#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugSystem
{
    private List<IDebugSystem> _debugSystems = new();

    public void Register(IDebugSystem debugSystem)
    {
        _debugSystems.Add(debugSystem);
        debugSystem.Initialize();
    }

    public void Tick()
    {
        foreach (var debugSystem in _debugSystems)
            debugSystem.Tick();
    }

    public void DrawDebugGUI()
    {
        foreach (var system in _debugSystems)
            if (system is IDebugGUI gui)
                gui.DrawDebugGUI();
    }

    public void Dispose()
    {
        foreach (var debugSystem in _debugSystems)
            debugSystem.Dispose();
    }
}
#endif