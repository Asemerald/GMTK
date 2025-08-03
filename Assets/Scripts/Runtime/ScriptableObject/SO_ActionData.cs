using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Actions/Action Data", fileName = "Action Data")]
public class SO_ActionData : ScriptableObject
{
    [Header("General")]
    public string actionName;
    public ActionType actionType;
    public List<InputReference> inputsRequired;
    public List<SO_ActionData> counterActions;
    public AttackHoldDuration holdDuration;
    public bool CanExecuteOnHalfBeat = false;
    
    [Header("Stats")]
    public int damageAmount;
    public int cooldown;

    [Header("Feedback")] 
    public SO_FeedbackData feedbackData;
}