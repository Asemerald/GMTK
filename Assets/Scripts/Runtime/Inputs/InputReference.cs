using Runtime.Actions;
using UnityEngine;
using Runtime.Enums;

[System.Serializable]
public class InputReference
{
    public string inputName; // ex: "LongHoldX", "TapDown"
    public ActionType actionType;
    public int holdDuration;
}

