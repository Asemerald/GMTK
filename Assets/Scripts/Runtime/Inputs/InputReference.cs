using UnityEngine;
using Runtime.Enums;

[System.Serializable]
public class InputReference
{
    public string inputName;       // ex: "LongHoldX", "TapDown"
    public InputType inputType;    
    public float holdDuration = 0; 
}

