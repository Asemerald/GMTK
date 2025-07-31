using UnityEngine;

[System.Serializable]
public class InputReference
{
    public string inputName;       // ex: "LongHoldX", "TapDown"
    public InputType inputType;    
    public float holdDuration = 0; 
}

public enum InputType
{
    Button,
    Axis,
    Directional,
    CustomTag 
}
