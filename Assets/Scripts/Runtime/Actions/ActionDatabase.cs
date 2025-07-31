using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class ActionDatabase : IGameSystem
{
    private Dictionary<InputReference, List<SO_ActionData>> actionDatas = new();
    public IReadOnlyDictionary<InputReference, List<SO_ActionData>> ActionDatas => actionDatas;

    readonly List<SO_ActionData> _actionDatas = new();
    
    public void Initialize()
    {
        LoadFromResources();
    }

    public void Tick() { }
    public void Dispose() { }

    private void LoadFromResources()
    {
        actionDatas.Clear();

        // Charge tous les ScriptableObjects dans Resources/Actions
        var allActions = Resources.LoadAll<SO_ActionData>("Actions");
        _actionDatas.AddRange(allActions);
        
        foreach (var data in allActions)
        {
            if (data.inputsRequired.Count == 0) {
                Debug.Log("ActionDatabase::LoadFromResources: No Inputs Required Detected in " + data.name);
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

        Debug.Log($"[ActionDatabase] Loaded {actionDatas.Count} actions from Resources.");
    }
}