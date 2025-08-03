using Runtime.Enums;
using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using Runtime.Inputs;
using UnityEngine;

/*
 * HitHandlerService s'occupe d'enregistrer les inputs du joueur pour les convertir en actions
 * Ce script est unique au joueur
 */

public class HitHandlerService : IGameSystem
{
    private readonly GameSystems _gameSystems;
    private InputManager _inputManager;
    private BeatSyncService _beatSyncService;
    private ActionDatabase _actionDatabase;
    private ActionHandlerService _actionHandlerService;
    private FightResolverService _fightResolverService;
    
    private SO_ActionData currentActionData;
    
    internal bool _inComboInputMode = false;
    
    public HitHandlerService(GameSystems gameSystems) {
        _gameSystems = gameSystems;
    }
    
    public void Dispose() {
        _inputManager.OnActionPressed -= HandleInputPerformed;
        _inputManager.OnActionReleased -= HandleInputCanceled;
    }

    public void Initialize() {
        _beatSyncService = _gameSystems.Get<BeatSyncService>();
        _actionDatabase = _gameSystems.Get<ActionDatabase>();
        _inputManager = _gameSystems.Get<InputManager>();
        _actionHandlerService = _gameSystems.Get<ActionHandlerService>();
        _fightResolverService = _gameSystems.Get<FightResolverService>();
        
        _inputManager.OnActionPressed += HandleInputPerformed;
        _inputManager.OnActionReleased += HandleInputCanceled;
    }

    public void Tick() { //Ici faire la validation d'action data base
        if (currentActionData != null && !_actionHandlerService._inCombo) { //Check si le joueur a une action data de sélectionner ou n'est pas en train d'effectuer un combo
            if (_beatSyncService.GetBeatFraction() == BeatFractionType.ThirdQuarter) { //Valide l'action - Register l'action dans ActionHandlerService qui s'occupe de jouer les actions sur le beat
                if (currentActionData.actionType is (ActionType.Dodge or ActionType.Parry) || _inComboInputMode) {
                    if (currentActionData.actionType is ActionType.Parry) {
                        _actionHandlerService.RegisterActionOnBeat(currentActionData, false);
                        _actionHandlerService.RegisterActionOnBeat(currentActionData, false);
                        currentActionData = null;
                        return;
                    }
                    _actionHandlerService.RegisterActionOnBeat(currentActionData, false);
                }
                else _actionHandlerService.RegisterActionOnBeat(currentActionData, true);
                
                currentActionData = null;
            }
        }
    }

    #region ActionPerformed
    
    void HandleInputPerformed(InputType inputType) {
        if (_inComboInputMode) { //Si en mode combo, alors lors d'un input le timer s'arrete et se valide
            _fightResolverService.StopPlayerTimer();
            
            if (_actionHandlerService._actionQueue.Count > 0) return; //Si le joueur a une action en queue cela veut dire qu'il est en combo et donc return
            //Sinon, il envoie une action
        }
        
        var currentFraction = _beatSyncService.GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None");
            return;
        }
        
        if(_beatSyncService.GetBeatFraction() is BeatFractionType.ThirdQuarter && currentActionData != null) return; //Évite de pouvoir reset ou changer l'action en cours lorsqu'une action est déjà assigné et qu'on est dans le temps d'envoi de l'action

        if(inputType is (InputType.DodgeLeft or InputType.DodgeRight or InputType.Parry)) {
            foreach (var action in _actionDatabase.ActionDatas) {
                var breakLoop = false;
            
                if (action.Key.actionType == inputType) {
                    if(action.Value.Count >= 1)
                        foreach (var hit in action.Value) {
                            currentActionData = hit;
                            breakLoop = true;
                            break;
                        
                        }
                }
            
                if(breakLoop) 
                    break;
            }
        }
        else if(inputType is InputType.Left or InputType.Right){
            if (_actionHandlerService._previousActions.Count > 0) {
                var performCounter = false;
                if (_actionHandlerService._previousActions[^1].actionType == ActionType.Dodge) {
                    foreach (var action in _actionDatabase.ActionDatas) {
                        var breakLoop = false;
                    
                        if (action.Key.actionType == inputType) {
                            foreach (var hit in action.Value) {
                                if (hit.actionType is ActionType.Counter) { //Ici enregistre une var l'action et sort de la loop
                                    currentActionData = hit;
                                    performCounter = true;
                                    breakLoop = true;
                                    break;
                                }
                            }
                        }
                    
                        if(breakLoop) 
                            break;
                    }
                }
                
                if(performCounter) return;
                
            }
            
            
            foreach (var action in _actionDatabase.ActionDatas) {
                var breakLoop = false;
                
                if (action.Key.actionType == inputType) {
                    foreach (var hit in action.Value) {
                        if (hit.holdDuration == _beatSyncService.GetPossibleAttackOnBeat(currentFraction)) { //Ici enregistre une var l'action et sort de la loop
                            currentActionData = hit;
                            breakLoop = true;
                            break;
                        }
                    }
                }
                
                if(breakLoop) 
                    break;
            }
        }
    }
    
    #endregion
    
    #region ActionCanceled

    void HandleInputCanceled(InputType inputType) {
        if(inputType is InputType.Right or InputType.Left)
            HandleAttackInputCanceled();
    }

    void HandleAttackInputCanceled() {
        //Savoir quelle était le temps lorsque l'input a été press pour déterminer quel coup va être lancé ou non
        if(currentActionData == null) return;
        
        if(_beatSyncService.GetBeatFraction() == BeatFractionType.ThirdQuarter) return; //Si dans le 3/4 de temps l'action va être executé

        if (currentActionData.holdDuration == AttackHoldDuration.Full) { //Si l'action doit être pressé plus d'un demi temps et qu'on est en dehors du 3/4 de temps, alors on reset l'action
            currentActionData = null;
        }
    }
    

    #endregion
    
}

