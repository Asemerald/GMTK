using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

namespace Runtime.GameServices {
    public class AIService : IGameSystem{

        private GameSystems _gameSystems;
        private ActionHandlerService _actionHandlerService;
        private ActionDatabase _actionDatabase;
        
        private List<SO_ActionData> _actionList;
        
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
    }
    
}