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

    public void Initialize()
    {
        _actionDatabase = _gameSystems.Get<ActionDatabase>();
    }
    public void Tick() {}
    public void Dispose() {}

    void LaunchCombo(SO_ComboData comboToLaunch)
    {
        
    }

    void CancelCombo(SO_ComboData comboToCancel)
    {
        
    }

    public SO_ComboData FindCombo(SO_ActionData openingCombo, SO_ActionData confirmationCombo)
    {
        if (openingCombo != null && confirmationCombo != null && _actionDatabase != null)
        {
            Debug.LogError("No combo found null ref");
            return null;
        }

        if (_actionDatabase.ComboDatas.TryGetValue(openingCombo, out var combos))
        {
            foreach (var combo in combos)
            {
                if (combo.confirmationAction == confirmationCombo)
                {
                    return combo;
                }
            }
        }
        return null;
    }
}
