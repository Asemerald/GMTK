using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

namespace Runtime.GameServices {
    public class AIService : IGameSystem{

        private GameSystems _gameSystems;
        private ActionHandlerService _actionHandlerService;
        private ActionDatabase _actionDatabase;
        
        private List<SO_ActionData> _actionList;

        bool doingPattern = false;
        
        public AIService(GameSystems gameSystems) {
            _gameSystems = gameSystems;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() {
            _actionHandlerService = new ActionHandlerService(_gameSystems, true);
            _actionDatabase = _gameSystems.Get<ActionDatabase>();

            _actionHandlerService.Initialize();
            _actionList = SetActionDataList();
        }

        public void Tick() {
            _actionHandlerService.Tick();
            
            if(_actionHandlerService._actionQueue.Count <= 0 && !_actionHandlerService._inCombo)
                CallAction();
        }

        void CallAction() {
            Debug.Log("AIService::CallAction");
            
            var random = Random.Range(0,101); //Random pour déterminer le % de chance de déclencher un combo

            if (random > 69) //30% de chance de faire un combo
                DoPatternAction();
            else
                _actionHandlerService.RegisterActionOnBeat(GetActionData(), true);
        }

        List<SO_ActionData> SetActionDataList() {

            var localList = new List<SO_ActionData>();
            
            foreach (var action in _actionDatabase.ActionDatas) {
                foreach (var a in action.Value) {
                    localList.Add(a);
                }
            }
            
            return localList;
        }
        
        SO_ActionData GetActionData() {
            var randomIndex = Random.Range(0, _actionList.Count);
            
            return _actionList[randomIndex];
        }

        void DoPatternAction() {
            var randomIndex = Random.Range(0, _actionDatabase._aiPatterns.Count);

            if (!_actionDatabase._aiPatterns[randomIndex].isUnlock) return;
            
            var open = _actionDatabase._aiPatterns[randomIndex].pattern.openingAction;
            var close = _actionDatabase._aiPatterns[randomIndex].pattern.confirmationAction;
                
            _actionHandlerService.RegisterActionOnBeat(open, true);
            _actionHandlerService.RegisterActionOnBeat(close, true);
        }
    }
}