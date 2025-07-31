using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;

[CreateAssetMenu(menuName = "Action")]
public class SO_ActionData : ScriptableObject
{
    [Header("Core")] public string actionName;
    public List<InputReference> inputsRequired;
    public List<SO_ActionData> counterActions; // Enemy actions that can counter this action
    public float inputBufferTime = 0.3f;

    [Header("Potential")] public float damageAmount;
    public float cooldown;

    [Header("Feedback")] public AnimationClip animation;
    public AudioClip soundEffect;
}