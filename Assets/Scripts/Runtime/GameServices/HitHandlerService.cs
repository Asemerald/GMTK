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
    
    private SO_ActionData currentActionData;
    
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
        
        _inputManager.OnActionPressed += HandleInputPerformed;
        _inputManager.OnActionReleased += HandleInputCanceled;
    }

    public void Tick() { //Ici faire la validation de action data base
        if (currentActionData != null && !_actionHandlerService._inCombo) { //Check si le joueur a une action data de sélectionner ou n'est pas en train d'effectuer un combo
            if (GetBeatFraction() == BeatFractionType.ThirdQuarter) { //Valide l'action - Register l'action dans ActionHandlerService qui s'occupe de jouer les actions sur le beat
                if(currentActionData.dodgeAction) _actionHandlerService.RegisterActionOnBeat(currentActionData, false);
                else _actionHandlerService.RegisterActionOnBeat(currentActionData, true);
                
                currentActionData = null;
            }
        }
    }

    #region ActionPerformed
    
    void HandleInputPerformed(InputType inputType) {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None");
            return;
        }
        
        if(GetBeatFraction() is BeatFractionType.ThirdQuarter && currentActionData != null) return; //Évite de pouvoir reset ou changer l'action en cours lorsqu'une action est déjà assigné et qu'on est dans le temps d'envoi de l'action

        if(inputType is InputType.DodgeLeft or InputType.DodgeRight) {
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
        else
        { //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
            foreach (var action in _actionDatabase.ActionDatas) {
                var breakLoop = false;
                
                if (action.Key.actionType == inputType) {
                    foreach (var hit in action.Value) {
                        if (hit.holdDuration == GetPossibleAttackOnBeat(currentFraction)) { //Ici enregistre une var l'action et sort de la loop
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

    void HandleDodgeInputPerformed(InputType inputType) {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None");
            return;
        }
        
        if(GetBeatFraction() is BeatFractionType.ThirdQuarter && currentActionData != null) return; //Évite de pouvoir reset ou changer l'action en cours lorsqu'une action est déjà assigné et qu'on est dans le temps d'envoi de l'action

        //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
        
        
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

    void HandleDodgeInputPerformed(InputType inputType) {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None");
            return;
        }
        
        if(GetBeatFraction() is BeatFractionType.ThirdQuarter && currentActionData != null) return; //Évite de pouvoir reset ou changer l'action en cours lorsqu'une action est déjà assigné et qu'on est dans le temps d'envoi de l'action

        //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
        foreach (var action in _actionDatabase.ActionDatas) {
            var breakLoop = false;
            
            if (action.Key.actionType == inputType) {
                
                foreach (var hit in action.Value) {
                    
                    if (hit.holdDuration == GetPossibleAttackOnBeat(currentFraction)) {
                        //Ici enregistre une var l'action et sort de la loop
                        currentActionData = hit;
                        breakLoop = true;
                        break;
                    }
                }
            
                if(breakLoop) 
                    break;
            }
        }
        else
        { //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
            foreach (var action in _actionDatabase.ActionDatas) {
                var breakLoop = false;
                
                if (action.Key.actionType == inputType) {
                    foreach (var hit in action.Value) {
                        if (hit.holdDuration == GetPossibleAttackOnBeat(currentFraction)) { //Ici enregistre une var l'action et sort de la loop
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
        if(inputType is InputType.Right or InputType.Left or InputType.Down or InputType.Up)
            HandleAttackInputCanceled();
    }

    void HandleAttackInputCanceled() {
        //Savoir quelle était le temps lorsque l'input a été press pour déterminer quel coup va être lancé ou non
        if(currentActionData == null) return;
        
        if(GetBeatFraction() == BeatFractionType.ThirdQuarter) return; //Si dans le 3/4 de temps l'action va être executé

        if (currentActionData.holdDuration == AttackHoldDuration.Full) { //Si l'action doit être pressé plus d'un demi temps et qu'on est en dehors du 3/4 de temps, alors on reset l'action
            currentActionData = null;
        }
    }
    

    #endregion

    AttackHoldDuration GetPossibleAttackOnBeat(BeatFractionType fractionType) {
        if (fractionType is BeatFractionType.None or BeatFractionType.Full or BeatFractionType.FirstQuarter)
            return AttackHoldDuration.Full;
        if (fractionType is BeatFractionType.Half or BeatFractionType.ThirdQuarter)
            return AttackHoldDuration.Half;
        
        return AttackHoldDuration.None;
    }
    
    
    //Fonction pour aller chercher la mesure
    BeatFractionType GetBeatFraction() {
        
        if (_beatSyncService.timelineInfo.currentHalfBeat == 0) {
            if(_beatSyncService.timelineInfo.currentQuarterBeat == 0)
                return BeatFractionType.Full;
            if (_beatSyncService.timelineInfo.currentQuarterBeat == 1)
                return BeatFractionType.FirstQuarter;
        }
        else if (_beatSyncService.timelineInfo.currentHalfBeat == 1) {
            if(_beatSyncService.timelineInfo.currentQuarterBeat == 0)
                return BeatFractionType.Half;
            if (_beatSyncService.timelineInfo.currentQuarterBeat == 1)
                return BeatFractionType.ThirdQuarter;
        }
        
        return BeatFractionType.None;
    }
}

