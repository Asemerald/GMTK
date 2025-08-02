using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Input Condition", fileName = "Input Condition")]
public class SO_InputCondition : ScriptableObject
{
    public List<InputReference> inputsRequired;
    public float inputBufferTime = 0.3f;
}