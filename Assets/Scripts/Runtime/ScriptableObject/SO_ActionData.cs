using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Action")]
public class SO_ActionData : ScriptableObject
{
    [Header("General")]
    public string actionName;
    public ActionType actionType;
    public List<InputReference> inputsRequired;
    public float inputBufferTime = 0.3f;
    public List<SO_ActionData> counterActions;
    public AttackHoldDuration holdDuration;
    public bool CanExecuteOnHalfBeat = false;
    public bool dodgeAction = false;
    
    [Header("Stats")]
    public float damageAmount;
    public int cooldown;

    [Header("Feedback")] 
    public SO_FeedbackData feedbackDataSuccess;
    public SO_FeedbackData feedbackDataFail;
}