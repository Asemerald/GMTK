using System;
using System.Collections.Generic;
using Runtime.GameServices.Interfaces;

namespace Runtime.GameServices
{
    public class GameSystems : IDisposable
    {
        private readonly List<IGameSystem> systems = new();

        public void Register(IGameSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            systems.Add(system);
        }

        public void Initialize()
        {
            foreach (var system in systems)
            {
                system.Initialize();
            }
        }

        public void Tick()
        {
            foreach (var system in systems)
            {
                system.Tick();
            }
        }

        public void Dispose()
        {
            foreach (var system in systems)
            {
                system.Dispose();
            }
            systems.Clear();
        }
    }
}