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
        
        internal List<SO_ActionData> _previousActions = new();
        
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

        public void RegisterActionOnBeat(SO_ActionData data, bool waitForNextBeat) { //Register les actions dans une Queue
            _actionQueue.Enqueue((data, data.CanExecuteOnHalfBeat));
            _waitForNextBeat = waitForNextBeat;
        }
        
        void PerformActionOnBeat() { //S'exécute sur chaque Temps
            if (_actionQueue.Count <= 0) return;
            
            if(!_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui s'execute sur un temps
                ExecuteAction();
        }

        void PerformActionOnHalfBeat() { //S'exécute sur chaque Demi Temps
            if (_actionQueue.Count <= 0 || _waitForNextBeat) return;
            
            if(_actionQueue.Peek().Item2)//Check s'il s'agit d'une action qui s'execute sur un demi-temps
                ExecuteAction(true);
        }

        void ExecuteAction(bool halfBeat = false) { //Se charge d'exécuter l'action
            var action = _actionQueue.Dequeue();
            RegisterPreviousAction(action.Item1);
            
            if (halfBeat) {
                Debug.Log("Half Beat Execute");
            }
            else {
                _waitForNextBeat = false;
                Debug.Log("Beat Execute");
            }
        }

        void RegisterPreviousAction(SO_ActionData data) { //Enregistre les actions effectuées dans une limite de 2 - les combos ne nécessitant que 2 actions pour être déclenché
            _previousActions.Add(data);

            if (_previousActions.Count > 2) {
                _previousActions.RemoveAt(0);
            }
        }
    }
}