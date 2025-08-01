using System.Collections;
using System.Collections.Generic;
using FMOD;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Runtime.GameServices {
    public class ActionHandlerService : IGameSystem {
        
        private readonly GameSystems _gameSystems;
        private BeatSyncService _beatSyncService;
        
        private Queue<(SO_ActionData, bool)> _actionQueue = new();
        
        private bool _waitForNextBeat = false;
        
        public ActionHandlerService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            _beatSyncService.OnBeat -= PerformActionOnBeat;
            _beatSyncService.OnHalfBeat -= PerformActionOnHalfBeat;
        }

        public void Initialize() {
            _beatSyncService = _gameSystems.Get<BeatSyncService>();

            _beatSyncService.OnBeat += PerformActionOnBeat;
            _beatSyncService.OnHalfBeat += PerformActionOnHalfBeat;
        }

        public void Tick() {
            
        }

        public void RegisterActionOnBeat(SO_ActionData data) { //Register les actions dans une Queue
            _actionQueue.Enqueue((data, data.CanExecuteOnHalfBeat));
        }
        
        void PerformActionOnBeat() { //S'exécute sur chaque Beat -- Va trier et vider la queue d'action et se charge d'exécuter les actions
            if (_actionQueue.Count <= 0) return;
            
            if(!_actionQueue.Peek().Item2)
                ExecuteAction();
        }

        void PerformActionOnHalfBeat() {
            if (_actionQueue.Count <= 0) return;
            
            if(_actionQueue.Peek().Item2)
                ExecuteAction(true);
        }

        void ExecuteAction(bool halfBeat = false) {
            _actionQueue.Dequeue();
           
            if(halfBeat)
                Debug.Log("Half Beat Execute");
            else
                Debug.Log("Beat Execute");
        }
    }
}