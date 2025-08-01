using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using UnityEngine;

public class ActionDatabase : IGameSystem
{
    private Dictionary<InputReference, List<SO_ActionData>> actionDatas = new();
    public IReadOnlyDictionary<InputReference, List<SO_ActionData>> ActionDatas => actionDatas;

    readonly List<SO_ActionData> _actionDatas = new();
    
    
    private Dictionary<SO_ActionData, List<SO_ComboData>> comboDatas = new();
    public IReadOnlyDictionary<SO_ActionData,List<SO_ComboData>> ComboDatas => comboDatas;

    readonly List<SO_ComboData> _comboDatas = new();
    internal List<SO_AIPattern> _aiPatterns = new List<SO_AIPattern>();
    
    public void Initialize()
    {
        LoadFromResources();
    }

    public void Tick() { }
    public void Dispose() { }

    private void LoadFromResources()
    {
        actionDatas.Clear();
        comboDatas.Clear();

        // Charge tous les ScriptableObjects dans Resources/Actions
        var allActions = Resources.LoadAll<SO_ActionData>("Actions");
        _actionDatas.AddRange(allActions);
        
        var allCombos = Resources.LoadAll<SO_ComboData>("Combos");
        _comboDatas.AddRange(allCombos);
        
        var allPattern = Resources.LoadAll<SO_AIPattern>("Pattern");
        _aiPatterns.AddRange(allPattern);
        
        foreach (var data in allActions)
        {
            if (data.inputsRequired.Count == 0|| data.isComboAction) {
                Debug.LogWarning("ActionDatabase::LoadFromResources: No Inputs Required Detected in " + data.name);
                continue;
            }

            if (data.inputsRequired.Count > 1) {
                foreach (var input in data.inputsRequired) {
                    if(!actionDatas.ContainsKey(input))
                        actionDatas.Add(input, new List<SO_ActionData>());
                    
                    actionDatas[input].Add(data);
                }
            }
            else {
                if(!actionDatas.ContainsKey(data.inputsRequired[0]))
                    actionDatas.Add(data.inputsRequired[0], new List<SO_ActionData>());
                    
                actionDatas[data.inputsRequired[0]].Add(data);
            }
        }

        //Debug.Log($"[ActionDatabase] Loaded {actionDatas.Count} actions from Resources.");
        
        foreach (var combo in allCombos)
        {
            if (combo.openingAction == null || combo.confirmationAction == null)
            {
                Debug.LogWarning("ActionDatabase::LoadFromResources: No OP Action or Conf Action Detected in COMBO " + combo.name);
                continue;
            }
            
            if (!comboDatas.TryGetValue(combo.openingAction, out var list))
            {
                list = new List<SO_ComboData>();
                comboDatas[combo.openingAction] = list;
            }
            
            bool duplicateFound = list.Exists(existing => existing.confirmationAction == combo.confirmationAction);

            if (duplicateFound)
            {
                Debug.LogError($"[ActionDatabase] Duplicate combo with same opening ({combo.openingAction.name}) and confirmation ({combo.confirmationAction.name}) found in '{combo.name}'. This is not allowed.");
                continue;
            }

            list.Add(combo);
        }

        Debug.Log($"[ActionDatabase] Loaded {actionDatas.Count} actions and {comboDatas.Count} combos from Resources.");

    }
    
    
    public void UnlockPattern(SO_ComboData comboToUnlock) {
        foreach (var pattern in _aiPatterns) {
            if (pattern.pattern == comboToUnlock) {
                pattern.isUnlock = true;
                break;
            }
        }
    }
}