using System.Collections.Generic;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class ActionDatabase : IGameSystem
{
    private Dictionary<SO_ActionData, List<InputReference>> actionDatas = new();
    public IReadOnlyDictionary<SO_ActionData, List<InputReference>> ActionDatas => actionDatas;

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

        foreach (var data in allActions)
        {
            if (data == null || actionDatas.ContainsKey(data))
                continue;

            var inputRefs = new List<InputReference>();

            if (data.inputConditions != null)
            {
                foreach (var inputCondition in data.inputConditions)
                {
                    if (inputCondition == null || inputCondition.inputsRequired == null) continue;

                    foreach (var inputRef in inputCondition.inputsRequired)
                    {
                        if (inputRef != null)
                            inputRefs.Add(inputRef);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"{data.actionName} has no input conditions");
            }

            actionDatas.Add(data, inputRefs);
        }

        Debug.Log($"[ActionDatabase] Loaded {actionDatas.Count} actions from Resources.");
    }
}