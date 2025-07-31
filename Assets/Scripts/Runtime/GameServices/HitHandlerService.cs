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
    
    public HitHandlerService(GameSystems gameSystems) {
        _gameSystems = gameSystems;
    }
    
    public void Dispose() {
        _inputManager.OnActionPressed -= HandleInputPerformed;
        _inputManager.OnActionReleased -= HandleInputCanceled;
    }

    public void Initialize() {
        _beatSyncService = _gameSystems.Get<BeatSyncService>();
        _inputManager = _gameSystems.Get<InputManager>();
        
        _inputManager.OnActionPressed += HandleInputPerformed;
        _inputManager.OnActionReleased += HandleInputCanceled;
    }

    public void Tick() {
        
    }

    #region ActionPerformed
    
    void HandleInputPerformed(InputType inputType) {
        switch (inputType) {
            case InputType.Right:
                HandleRightInputPerformed();
                break;
            case InputType.Left:
                HandleLeftInputPerformed();
                break;
            default:
                Debug.LogError($"Unhandled Input Type: {inputType}");
                break;
        }
    }
    
    void HandleRightInputPerformed() {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None ");
            return;
        }

        Debug.Log("HitHandlerService::HandleRightInputPerformed - Fraction " + currentFraction);
        //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
       
    }
    
    void HandleLeftInputPerformed() {
        var currentFraction = GetBeatFraction();
        if (currentFraction == BeatFractionType.None) { //Check pour vérifier qu'il retourne bien une fraction existante
            Debug.LogError($"HitHandlerService::BeatFractionType - Return None ");
            return;
        }

        Debug.Log("HitHandlerService::HandleRightInputPerformed - Fraction " + currentFraction);
        //Fonction de tri pour savoir qu'elle action va être lancé en fonction du BeatFractionType
    }
    
    #endregion
    
    #region ActionCanceled

    void HandleInputCanceled(InputType inputType) {
        switch (inputType) {
            case InputType.Right:
                HandleRightInputCanceled();
                break;
            case InputType.Left:
                HandleLeftInputCanceled();
                break;
            default:
                Debug.LogError($"Unhandled Input Type: {inputType}");
                break;
        }
    }

    void HandleRightInputCanceled() {
        
    }

    void HandleLeftInputCanceled() {
        
    }

    #endregion
    
    
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

