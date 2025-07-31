using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action")]
public class SO_ActionData : ScriptableObject 
{
    [Header("Core")]
    public string actionName;
    public List<SO_InputCondition> inputConditions;
    public ActionType type;
    
    [Header("Potential")]
    public float damageAmount;
    public float cooldown;
    public int level;
    
    [Header("Feedback")]
    public AnimationClip animation;
    public AudioClip soundEffect;
    
}
