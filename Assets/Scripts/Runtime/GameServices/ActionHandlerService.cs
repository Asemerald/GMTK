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
        private ActionDatabase _actionDatabase;
        private FightResolverService _fightResolverService;
        
        internal Queue<(SO_ActionData, bool)> _actionQueue = new();
        
        private bool _waitForNextBeat = false;
        internal bool _inCombo = false;
        internal bool _isAI;
        
        internal List<SO_ActionData> _previousActions = new();
        
        public ActionHandlerService(GameSystems gameSystems, bool isAI = false) {
            _gameSystems = gameSystems;
            _isAI = isAI;
        }
        
        public void Dispose() {
            _beatSyncService.OnBeat -= PerformActionOnBeat;
            _beatSyncService.OnHalfBeat -= PerformActionOnHalfBeat;
        }

        public void Initialize() {
            _beatSyncService = _gameSystems.Get<BeatSyncService>();
            _comboManager = _gameSystems.Get<ComboManagerService>();
            _actionDatabase = _gameSystems.Get<ActionDatabase>();
            _fightResolverService = _gameSystems.Get<FightResolverService>();

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
            if (_inCombo) //Execute sans prendre en compte sur quel temps se joue l'action durant un combo
            {
                ExecuteAction();
                return;
            }

            if (!_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui s'execute sur un temps
            {
                ExecuteAction();
                return;
            }
        }

        void PerformActionOnHalfBeat() { //S'exécute sur chaque Demi Temps
            if (_actionQueue.Count <= 0 || _waitForNextBeat) return;

            if (_inCombo) //Execute sans prendre en compte sur quel temps se joue l'action durant un combo
            {
                ExecuteAction();
                return;
            }

            if (_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui s'execute sur un demi-temps
            {
                ExecuteAction();
                return;
            }
        }

        void ExecuteAction() { //Se charge d'exécuter l'action
            var item = _actionQueue.Dequeue();
            var action = item.Item1;

            if(_isAI)
                _fightResolverService.GetAIAction(item.Item1);
            else
                _fightResolverService.GetPlayerAction(item.Item1);
            
            if (!_inCombo) { //Évite d'enregistrer une action de combo dans la liste d'action précédent (on pourrait avoir des soucis de combo qui lance des combos)
                RegisterPreviousAction(item.Item1);
            }
            else {
                if (_actionQueue.Count <= 0) _inCombo = false;
            }
            
            if (action.CanExecuteOnHalfBeat) { //Pour le moment c'est juste du debug pour voir quand est jouer une action
                _waitForNextBeat = false;
                Debug.Log("AIService::PerformActionOnHalfBeat - " + (_isAI ? "AI" : "Player"));
            }
            else {
                _waitForNextBeat = true; //Permet d'attendre un temps avant de jouer sur des demis temps
                Debug.Log("AIService::PerformActionBeat - " + (_isAI ? "AI" : "Player"));
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
                //_comboManager.LaunchCombo(comboAction); //Envoi le combo
                Debug.Log("ComboManagerService::LaunchCombo - Combo launch");

                _actionDatabase.UnlockPattern(comboAction);
                
                foreach (var action in comboAction.comboActions) {
                    RegisterActionOnBeat(action, !action.CanExecuteOnHalfBeat, true);
                }
                _previousActions.Clear();
            }
        }
    }
}