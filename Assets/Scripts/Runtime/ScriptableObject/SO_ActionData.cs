using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;

[CreateAssetMenu(menuName = "Action")]
public class SO_ActionData : ScriptableObject
{
    public string actionName;
    public bool isComboAction;
    public List<InputReference> inputsRequired;
    public List<SO_ActionData> counterActions; // Enemy actions that can counter this action
    public float inputBufferTime = 0.3f;
    public AttackHoldDuration holdDuration;
    public float damageAmount;
    public int cooldown;

    [Header("Feedback")] public AnimationClip animation;
    public AudioClip soundEffect;
}