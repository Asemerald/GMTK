using System;

public interface IDebugSystem : IDisposable
{
    void Initialize();
    void Tick();
}

public interface IDebugGUI
{
    void DrawDebugGUI();
}