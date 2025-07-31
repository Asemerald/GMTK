using System;
using System.Collections.Generic;
using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using UnityEngine;

public class ActionDatabase : IGameSystem
{
    //public static ActionDatabase Instance { get; private set; }

    [SerializeField] SO_ActionData[] actionDatasToSet; 

    private Dictionary<SO_ActionData, List<InputReference>> actionDatas = new  Dictionary<SO_ActionData, List<InputReference>>();
    public IReadOnlyDictionary<SO_ActionData, List<InputReference>> ActionDatas => actionDatas;
    
    public void Dispose()
    {
        //throw new NotImplementedException();
    }

    public void Initialize()
    {
        //SetActionDatas();
    }

    public void Tick()
    {
        //throw new NotImplementedException();
    }
    

    private void SetActionDatas()
    {
        actionDatas.Clear();

        foreach (SO_ActionData data in actionDatasToSet)
        {
            if (data == null || actionDatas.ContainsKey(data))
                continue;

            var inputRefs = new List<InputReference>();

            if (data.inputConditions != null)
            {
                foreach (SO_InputCondition inputCondition in data.inputConditions)
                {
                    if (inputCondition == null || inputCondition.inputsRequired == null) continue;

                    foreach (InputReference inputRef in inputCondition.inputsRequired)
                    {
                        if (inputRef != null)
                            inputRefs.Add(inputRef);
                    }
                }
            }
            else
            {
                Debug.LogWarning(data.actionName+" has no input conditions");
            }

            actionDatas.Add(data, inputRefs);
        }
    }
}
