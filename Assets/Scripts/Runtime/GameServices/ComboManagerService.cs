using System.Collections.Generic;
using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class ComboManagerService : IGameSystem
{
    public ComboManagerService(GameSystems gameSystems) 
    {
        _gameSystems = gameSystems;
    }
    
    private SO_ComboData currentCombo;
    private GameSystems _gameSystems;
    private ActionDatabase _actionDatabase;
    private ActionHandlerService _actionHandlerService;

    public void Initialize() {
        _actionDatabase = _gameSystems.Get<ActionDatabase>();
        _actionHandlerService = _gameSystems.Get<ActionHandlerService>();
    }
    public void Tick() {}
    public void Dispose() {}

    public void LaunchCombo(SO_ComboData comboToLaunch) {
        Debug.Log("ComboManagerService::LaunchCombo - Combo launch");
        foreach (var action in comboToLaunch.comboActions) {
            _actionHandlerService.RegisterActionOnBeat(action, true, true);
        }
        
    }

    void CancelCombo(SO_ComboData comboToCancel) {
        
    }

    public SO_ComboData FindCombo(SO_ActionData openingCombo, SO_ActionData confirmationCombo) {
        
        if (_actionDatabase == null) {
            Debug.LogError("ComboManagerService::FindCombo - ActionDatabase not initialized");
            return null;
        }
        
        if (openingCombo == null || confirmationCombo == null) {
            Debug.LogError("No combo found null ref");
            return null;
        }

        if (_actionDatabase.ComboDatas.TryGetValue(openingCombo, out var combos)) {
            foreach (var combo in combos) {
                if (combo.confirmationAction == confirmationCombo) {
                    return combo;
                }
            }
        }
        return null;
    }
}
