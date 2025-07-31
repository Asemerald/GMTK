using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Action")]
public class SO_CombatActionData : ScriptableObject 
{
    [Header("Core")]
    public string actionName;
    public List<SO_InputCondition> inputConditions;
    public CombatActionType type;
    
    [Header("Potential")]
    public float damageAmount;
    public float cooldown;
    public int level;
    
    [Header("Feedback")]
    public AnimationClip animation;
    public AudioClip soundEffect;
    
}
