using System;
using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

namespace Runtime.GameServices
{
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
            try
            {
                foreach (var system in systems.Values) system.Initialize();
                Debug.Log($"[GameSystems] Initialized {systems.Count} systems.");
            }
            catch (Exception e)
            {
                // Debug wich system failed to initialize
                Debug.LogError($"[GameSystems] Initialization failed: {e.Message}");
                foreach (var system in systems)
                    if (system.Value == null)
                        Debug.LogError($"[GameSystems] System {system.Key.Name} is null.");
                    else
                        Debug.Log($"[GameSystems] System {system.Key.Name} initialized successfully.");

                throw;
            }
        }

        public void Tick()
        {
            foreach (var system in systems.Values) system.Tick();
        }

        public void Dispose()
        {
            foreach (var system in systems.Values) system.Dispose();
            systems.Clear();
        }

        public void TriggerComboMode(bool inCombo) {
            Get<HitHandlerService>()._inComboInputMode = inCombo;
            Get<ActionHandlerService>()._inCombo = inCombo;
            Get<AIService>()._inComboInputMode = inCombo;

            if (!inCombo) {
                Get<AIService>().EmptyQueue();
            }
        }
    }
}