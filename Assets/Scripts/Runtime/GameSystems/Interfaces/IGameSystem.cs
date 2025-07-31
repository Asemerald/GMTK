using System;

public interface IGameSystem : IDisposable
{
    /// <summary>
    /// Method to call to initialize the system
    /// </summary>
    void Initialize();
    
    /// <summary>
    ///  Method to call every frame to update the system
    /// </summary>
    void Tick();
}