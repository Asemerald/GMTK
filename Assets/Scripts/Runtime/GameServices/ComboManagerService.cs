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

    public void GetCombo()
    {
        /*foreach (SO_ComboData comboData in _actionDatabase.ComboDatas)
        {
            //_actionDatabase.ComboDatas.
        }*/
    }
}
