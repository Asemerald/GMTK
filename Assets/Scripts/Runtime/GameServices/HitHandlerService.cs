using Runtime.Enums;
using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using Runtime.Inputs;
using UnityEngine;

public class HitHandlerService : IGameSystem
{
    private readonly GameSystems _gameSystems;
    private InputManager _inputManager;
    private BeatSyncService _beatSyncService;
    private ActionDatabase _actionDatabase;
    
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
        
        _inputManager.OnActionPressed += HandleInputPerformed;
        _inputManager.OnActionReleased += HandleInputCanceled;
    }

    public void Tick() {
        //Ici faire la validation de action data base
        if (currentActionData != null) {
            if (GetBeatFraction() == BeatFractionType.ThirdQuarter) { //Valide l'action
                //ajoute l'action pour être joué sur le prochain temps dans le BeatSyncService
                Debug.Log("HitHandlerService::Tick - Execute Action ");
                currentActionData = null;
            }
        }
    }

    #region ActionPerformed
    
    void HandleInputPerformed(InputType inputType) {
        if(inputType is InputType.Right or InputType.Left)
            HandleAttackInputPerformed(inputType);
    }
    
    void HandleAttackInputPerformed(InputType inputType) {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None ");
            return;
        }

        //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
        foreach (var action in _actionDatabase.ActionDatas) {
            var breakLoop = false;
            
            if (action.Key.actionType == inputType) {
                
                foreach (var hit in action.Value) {
                    
                    if (hit.holdDuration == GetByIntBeatFraction(currentFraction)) {
                        //Ici déclencher l'action
                        Debug.Log("HitHandlerService::HandleRightInputPerformed - Set Action " + hit.name);
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
    
    
    #endregion
    
    #region ActionCanceled

    void HandleInputCanceled(InputType inputType) {
        if(inputType is InputType.Right or InputType.Left)
            HandleAttackInputCanceled();
    }

    void HandleAttackInputCanceled() {
        //Savoir quelle était le temps lorsque l'input a été press pour déterminer quel coup va être lancé ou non
        if(currentActionData == null) return;
        
        if(GetBeatFraction() == BeatFractionType.ThirdQuarter) return; //Si dans le 3/4 de temps l'action va être executé

        if (currentActionData.holdDuration == 0) { //Si l'action doit être pressé plus d'un demi temps et qu'on est en dehors du 3/4 de temps, alors on reset l'action
            currentActionData = null;
            Debug.Log("HitHandlerService::HandleAttackInputCanceled - Attack Cancelled ");
        }
    }
    

    #endregion

    int GetByIntBeatFraction(BeatFractionType fractionType) {
        var value = fractionType switch {
            BeatFractionType.None => 0,
            BeatFractionType.FirstQuarter => 0,
            BeatFractionType.Half => 1,
            BeatFractionType.ThirdQuarter => 1,
            BeatFractionType.Full => 0,
        };
        
        return value;
    }
    
    //Fonction pour aller chercher la mesure
    BeatFractionType GetBeatFraction() {
        
        if (_beatSyncService.timelineInfo.currentHalfBeat == 0) {
            if(_beatSyncService.timelineInfo.currentQuarterBeat == 0)
                return BeatFractionType.Full;
            if (_beatSyncService.timelineInfo.currentQuarterBeat == 1)
                return BeatFractionType.FirstQuarter;
        }else if (_beatSyncService.timelineInfo.currentHalfBeat == 1) {
            if(_beatSyncService.timelineInfo.currentQuarterBeat == 0)
                return BeatFractionType.Half;
            if (_beatSyncService.timelineInfo.currentQuarterBeat == 1)
                return BeatFractionType.ThirdQuarter;
        }
        
        return BeatFractionType.None;
    }
}

