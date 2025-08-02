using System.Collections.Generic;
using Runtime.Enums;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using UnityEngine;

/*
 * Script qui gère le fonctionnement de l'IA - De comment les actions sont choisi
 *
 */

namespace Runtime.GameServices {
    public class AIService : IGameSystem{

        private GameSystems _gameSystems;
        private ActionHandlerService _aiActionHandler;
        private ActionHandlerService _playerActionHandler;
        private ActionDatabase _actionDatabase;
        private BeatSyncService _beatSyncService;
        private FightResolverService _fightResolverService;
        
        private List<SO_ActionData> _actionList = new List<SO_ActionData>();
        internal List<SO_AIPattern> _unlockedPatterns = new List<SO_AIPattern>();
        
        private SO_AIConfig _aiConfig;
        
        private SO_ActionData _currentPlayerAction;
        bool canCounter = false; //Bool qui passe en true dès que le joueur a réaliser un contre - L'IA va roll un % de chance d'en réaliser un après une esquive réussi
        
        bool hasRolled = false;
        
        internal bool _inComboInputMode = false;
        
        public AIService(GameSystems gameSystems, SO_AIConfig config) {
            _gameSystems = gameSystems;
            _aiConfig = config;
        }
        
        public void Dispose() {
            
        }

        public void Initialize() {
            _aiActionHandler = new ActionHandlerService(_gameSystems, true);
            _actionDatabase = _gameSystems.Get<ActionDatabase>();
            _playerActionHandler = _gameSystems.Get<ActionHandlerService>();
            _beatSyncService = _gameSystems.Get<BeatSyncService>();
            _fightResolverService = _gameSystems.Get<FightResolverService>();

            _aiActionHandler.Initialize();
            _actionList = SetActionDataList();

            _beatSyncService.OnBeat += ResetRolled;
            _beatSyncService.OnHalfBeat += ResetRolled;
            _beatSyncService.OnQuarterBeat += ResetRolled;
        }

        public void Tick() {
            _aiActionHandler.Tick();

            if(hasRolled) return;
            
            //Check ici pour le mode combo
            if (_inComboInputMode) { //Ajouter une notion de timing au if et else
                if (_beatSyncService.GetBeatFraction() is BeatFractionType.FirstQuarter) { //Prend une action sur le premier quart de temps
                    hasRolled = true;
                    
                    _fightResolverService.StopAiTimer();
                    
                    if (_aiActionHandler._actionQueue.Count <= 0) {
                        if (RollAction(_aiConfig.chanceOfAttack)) { //si le roll réussi -> sort une attaque
                            _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Attack), false);
                        }
                        else if (RollAction(_aiConfig.chanceOfParry)) { //si le roll réussi -> sort une parade
                            _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Parry), false);
                        }
                        else if (RollAction(_aiConfig.chanceOfDodge)) {//si le roll réussi -> sort une esquive
                            _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Dodge), false);
                        }
                    }
                }
                
                return;
            }
            
            if(!canCounter && _playerActionHandler._previousActions.Count > 0) //Check si l'IA peut counter
                if(_playerActionHandler._previousActions[^1].actionType is ActionType.Counter) //Si la dernière action joueur est un counter alors l'IA débloque le counter
                    canCounter = true;
            
            if (_aiActionHandler._actionQueue.Count <= 0 && !_aiActionHandler._inCombo) { //Lorsque l'IA n'a aucune action de prise et n'effectue pas un combo → Prend l'action de roll une action
                if (_beatSyncService.GetBeatFraction() is BeatFractionType.FirstQuarter) { //Prend une action sur le premier quart de temps
                    CallAllPossibleAction();
                }
            }
        }

        void CallAllPossibleAction() {
            hasRolled = true;
            if (_playerActionHandler._actionQueue.Count <= 0) { //Si le joueur n'a aucune action en queue
                if (_aiActionHandler._previousActions.Count > 0) {//Check si l'IA a réalisé une esquive précédemment
                    if (_aiActionHandler._previousActions[^1].actionType is ActionType.Dodge && RollAction(_aiConfig.chanceOfCounter)) {
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Counter), false);
                    }        
                }
                else if (RollAction(_aiConfig.chanceOfAttack)) { //si le roll réussi -> sort une attaque
                    RolledAttack();
                }
                //sinon il ne fait rien
            }
            else { //Roll une action en fonction de l'action choisis par le joueur
                    
                var GetActionType = _playerActionHandler._actionQueue.Peek().Item1.actionType;
               
                if (_aiActionHandler._previousActions[^1].actionType is ActionType.Dodge && RollAction(_aiConfig.chanceOfCounter)) {
                    _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Counter), false);
                }  
                else if (GetActionType is ActionType.Attack) {
                    
                    if (RollAction(_aiConfig.chanceOfAttack)) { //si le roll réussi -> sort une attaque
                        RolledAttack();
                    }
                    else if (RollAction(_aiConfig.chanceOfParry)) { //si le roll réussi -> sort une parade
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Parry), false);
                    }
                    else if (RollAction(_aiConfig.chanceOfDodge)) {//si le roll réussi -> sort une esquive
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Dodge), false);
                    }
                    //Si aucun ne réussi alors l'IA reste passive
                }
                else if (GetActionType is ActionType.Combo) {
                    
                    if (RollAction(_aiConfig.chanceOfAttack)) { //si le roll réussi -> sort une attaque
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Attack), false);
                    }
                    else if (RollAction(_aiConfig.chanceOfParry)) { //si le roll réussi -> sort une parade
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Parry), false);
                    }
                    else if (RollAction(_aiConfig.chanceOfDodge)) {//si le roll réussi -> sort une esquive
                        _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Dodge), false);
                    }
                    //Si aucun ne réussi alors l'IA reste passive
                }
                else if (GetActionType is ActionType.Parry) {
                    
                    if (RollAction(_aiConfig.chanceOfAttack)) { //si le roll réussi -> sort une attaque
                        RolledAttack();
                    }
                    //Si aucun ne réussi alors l'IA reste passive
                }
                /*else if (GetActionType is ActionType.Dodge) {
                    
                }
                else if (GetActionType is ActionType.Empty) {
                    
                }*/
            }
            
        }

        
        void RolledAttack() {
            if (RollAction(_aiConfig.chanceOfExecutingCombo) && _unlockedPatterns.Count > 0) {
                var randomIndex = Random.Range(0, _unlockedPatterns.Count);
            
                var open = _unlockedPatterns[randomIndex].pattern.openingAction;
                var close = _unlockedPatterns[randomIndex].pattern.confirmationAction;
                
                _aiActionHandler.RegisterActionOnBeat(open, true);
                _aiActionHandler.RegisterActionOnBeat(close, true);
            }
            else
                _aiActionHandler.RegisterActionOnBeat(GetActionData(ActionType.Attack), true);
        }
        
        SO_ActionData GetActionData(ActionType actionType) { //Changer la fonction pour être similaire a celle de hithandler et tiré l'action par rapport au type
            
            if(actionType is ActionType.Dodge) { //Obtenir action de dodge
                foreach (var action in _actionList) {
                    if (action.actionType is not ActionType.Dodge) continue;

                    return action;
                }
            }
            else if(actionType is ActionType.Attack) { //Fonction de tri pour savoir quelle action va être lancé en fonction du BeatFractionType
                var possibleActions = new List<SO_ActionData>();
                var currentFraction = _beatSyncService.GetBeatFraction();
                
                foreach (var action in _actionList) {
                    if (action.actionType is not ActionType.Attack) continue;
                    
                    if(action.holdDuration == _beatSyncService.GetPossibleAttackOnBeat(currentFraction))
                        possibleActions.Add(action);
                }
                
                return possibleActions[Random.Range(0, possibleActions.Count)];
            }
            else if (actionType is ActionType.Parry) {
                foreach (var action in _actionList) {
                    if (action.actionType is not ActionType.Parry) continue;

                    return action;
                }
            }
            else if (actionType is ActionType.Combo) {
                var possibleActions = new List<SO_ActionData>();
                
                foreach (var action in _actionList) {
                    if (action.actionType is not ActionType.Combo) continue;
                    
                    possibleActions.Add(action);
                }
                
                return possibleActions[Random.Range(0, possibleActions.Count)];
            }
            else if (actionType is ActionType.Counter) {
                foreach (var action in _actionList) {
                    if (action.actionType is not ActionType.Counter) continue;

                    return action;
                }
            }

            return null;
        }
        
        bool RollAction(int successPercent) { //Fonction de roll
            var roll = Random.Range(0, 100);
            return roll <= successPercent;
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

        void ResetRolled() {
            hasRolled = false;
        }

        public void EmptyQueue() {
            if(_playerActionHandler._actionQueue.Count > 0)
                _playerActionHandler._actionQueue.Clear();
            if(_aiActionHandler._actionQueue.Count > 0)
                _aiActionHandler._actionQueue.Clear();
        }
    }
}