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
        private ComboManagerService _comboManager;
        
        private Queue<(SO_ActionData, bool)> _actionQueue = new();
        
        private bool _waitForNextBeat = false;
        internal bool _inCombo = false;
        
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
            _comboManager = _gameSystems.Get<ComboManagerService>();

            _beatSyncService.OnBeat += PerformActionOnBeat;
            _beatSyncService.OnHalfBeat += PerformActionOnHalfBeat;
        }

        public void Tick() {
            
        }

        public void RegisterActionOnBeat(SO_ActionData data, bool waitForNextBeat, bool inCombo = false) { //Register les actions dans une Queue
            _inCombo = inCombo;
            _actionQueue.Enqueue((data, data.CanExecuteOnHalfBeat));
            _waitForNextBeat = waitForNextBeat;
        }
        
        void PerformActionOnBeat() { //S'exécute sur chaque Temps
            if (_actionQueue.Count <= 0) return;
            
            if(_inCombo) //Execute sans prendre en compte sur quel temps se joue l'action durant un combo
                ExecuteAction();
            
            if(!_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui s'execute sur un temps
                ExecuteAction();
        }

        void PerformActionOnHalfBeat() { //S'exécute sur chaque Demi Temps
            if (_actionQueue.Count <= 0 || _waitForNextBeat) return;
            
            if(_inCombo) //Execute sans prendre en compte sur quel temps se joue l'action durant un combo
                ExecuteAction(true);
            
            if(_actionQueue.Peek().Item2)//Check s'il s'agit d'une action qui s'execute sur un demi-temps
                ExecuteAction(true);
        }

        void ExecuteAction(bool halfBeat = false) { //Se charge d'exécuter l'action
            var item = _actionQueue.Dequeue();
            var action = item.Item1;

            if (!_inCombo) { //Évite d'enregistrer une action de combo dans la liste d'action précédent (on pourrait avoir des soucis de combo qui lance des combos)
                RegisterPreviousAction(item.Item1);
            }
            else {
                if (_actionQueue.Count <= 0) _inCombo = false;
            }
            
            if (halfBeat) { //Pour le moment c'est juste du debug pour voir quand est jouer une action
                Debug.Log("Half Beat Execute");
            }
            else {
                _waitForNextBeat = false; //Permet d'attendre un temps avant de jouer sur des demis temps
                Debug.Log("Beat Execute");
            }
        }

        void RegisterPreviousAction(SO_ActionData data) { //Enregistre les actions effectuées dans une limite de 2 - les combos ne nécessitant que 2 actions pour être déclenché
            _previousActions.Add(data);

            if (_previousActions.Count > 2)
                _previousActions.RemoveAt(0);

            //TODO clear la liste d'action après un temps sans actions, actuellement on peut faire une action puis attendre plusieurs temps pour en faire une autre et cela va déclencher un combo si ça remplit les conditions
            
            SO_ComboData comboAction = null;
            if(_previousActions.Count == 2) //Envoi au combo manager les deux dernier input
                comboAction = _comboManager.FindCombo(_previousActions[0], _previousActions[1]);

            if (comboAction) {
                _comboManager.LaunchCombo(comboAction); //Envoi le combo
                _previousActions.Clear();
            }
        }
    }
}