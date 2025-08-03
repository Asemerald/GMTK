using System.Collections.Generic;
using Runtime._Debug;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using Debug = UnityEngine.Debug;

/*
 * Ce script se charge de récupérer les actions et de les stocker dans une liste pour ensuite les jouer sur le tempo
 * Le script est utilisé par le joueur et l'IA
 */

namespace Runtime.GameServices {
    public class ActionHandlerService : IGameSystem {
        
        private readonly GameSystems _gameSystems;
        private BeatSyncService _beatSyncService;
        private ComboManagerService _comboManager;
        private ActionDatabase _actionDatabase;
        private FightResolverService _fightResolverService;
        public ActionDebugService _actionDebugService;
        
        internal Queue<(SO_ActionData, bool)> _actionQueue = new();
        
        public Queue<(SO_ActionData, bool)> ActionQueue => _actionQueue;
        
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
            _beatSyncService.OnQuarterBeat += PerformActionOnQuarterBeat;
        }

        public void Tick() {
            
        }

        public void RegisterActionOnBeat(SO_ActionData data, bool waitForNextBeat, bool inCombo = false) { //Register les actions dans une Queue
            _inCombo = inCombo;
            _actionQueue.Enqueue((data, data.CanExecuteOnHalfBeat));
            _waitForNextBeat = waitForNextBeat;
            
            if(_actionDebugService!=null)
                _actionDebugService.RegisterAction(data);
        }
        
        void PerformActionOnBeat() { //S'exécute sur chaque Temps
            CheckToExecuteAction(BeatFractionType.Full);
        }

        void PerformActionOnHalfBeat() { //S'exécute sur chaque Demi Temps
            CheckToExecuteAction(BeatFractionType.Half);
        }
        
        void PerformActionOnQuarterBeat() { //S'exécute sur chaque Quart Temps
            CheckToExecuteAction(BeatFractionType.FirstQuarter);
            
            if(_actionDebugService!=null)
                _actionDebugService.RegisterAction(null);
        }

        void CheckToExecuteAction(BeatFractionType fractionType) {
            if(_actionQueue.Count <= 0) return;
            
            if (_actionQueue.Peek().Item1.actionType is ActionType.Dodge) { //Pouvoir réaliser le dodge independent du temps
                ExecuteAction();
                return;
            }
            
            if (_inCombo && fractionType is not BeatFractionType.FirstQuarter) { //Execute le combo sur le temps plein et le demi temps
                ExecuteAction();
                return;
            }
            
            if (fractionType is BeatFractionType.FirstQuarter) { // Quart de temps
                if(_waitForNextBeat) return;
            }
            else if (fractionType is BeatFractionType.Half) { // Demi temps
                if(_waitForNextBeat) return;
                
                if (_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui s'execute sur un demi-temps
                    ExecuteAction();
            }
            else { //Temps plein
                if (!_actionQueue.Peek().Item2) //Check s'il s'agit d'une action qui execute sur un temps plein
                    ExecuteAction();
                
                _waitForNextBeat = false;
            }
        }

        void ExecuteAction() { //Se charge d'exécuter l'action
            var item = _actionQueue.Dequeue();
            var action = item.Item1;
            
            if(_isAI) _fightResolverService.GetAIAction(action);
            else _fightResolverService.GetPlayerAction(action);

            if (_actionDebugService != null) 
                _actionDebugService.MarkActionExecuted(action);

            if (!_inCombo) { //Évite d'enregistrer une action de combo dans la liste d'action précédent (on pourrait avoir des soucis de combo qui lance des combos)
                RegisterPreviousAction(action);
            }
            else { //Lorsqu'il est en combo, si la queue devient vide, alors il sort de l'état combo
                if (_actionQueue.Count <= 0) {
                    _inCombo = false;
                    _gameSystems.TriggerComboMode(false);
                }
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

                if(!_isAI)
                    _actionDatabase.UnlockPattern(comboAction);
                
                foreach (var action in comboAction.ComboActions) {
                    RegisterActionOnBeat(action, !action.CanExecuteOnHalfBeat, true);
                }
                _previousActions.Clear();
            }
            LaunchCombo(comboAction);
        }

        private void LaunchCombo(SO_ComboData comboAction)
        {
            if (comboAction == null)
                return;
            
            Debug.Log("ActionHandlerService::LaunchCombo - Combo launch");
                
            foreach (var action in comboAction.ComboActions) { 
                RegisterActionOnBeat(action, false, true);
            }
            
            _gameSystems.TriggerComboMode(true);
            
            _previousActions.Clear();
        }
    }
}