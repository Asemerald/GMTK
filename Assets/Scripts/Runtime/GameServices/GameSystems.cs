using System;
using System.Collections.Generic;
using Runtime.GameServices.Interfaces;

public class GameSystems : IDisposable
{
    private readonly Dictionary<Type, IGameSystem> systems = new();

    public void Register<T>(T system) where T : class, IGameSystem
    {
        systems[typeof(T)] = system ?? throw new ArgumentNullException(nameof(system));
    }

    public T Get<T>() where T : class, IGameSystem
    {
        systems.TryGetValue(typeof(T), out var system);
        return system as T;
    }

    public void Initialize()
    {
        foreach (var system in systems.Values)
        {
            system.Initialize();
        }
    }

    public void Tick()
    {
        foreach (var system in systems.Values)
        {
            system.Tick();
        }
    }

    public void Dispose()
    {
        foreach (var system in systems.Values)
        {
            system.Dispose();
        }
        systems.Clear();
    }
}