using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;

[CreateAssetMenu(menuName = "Combo")]
public class SO_ComboData : ScriptableObject
{
    public SO_ActionData confirmationAction;
    public SO_ActionData openingAction;
    public List<SO_ActionData> comboActions;

    [Tooltip("time : 0.25 = 1er quart temps, time : 2 = 2eme mesure ; value 0 = comboActions[0], value 3 = comboActions[3]")]
    public AnimationCurve comboTimeline;  
}

