using UnityEngine;

[System.Serializable]
public class InputReference
{
    public string inputName;       // e.g. "LightAttack", "Dodge", "Down"
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
