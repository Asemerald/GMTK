using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;

[CreateAssetMenu(menuName = "Combo")]
public class SO_ComboData : ScriptableObject
{
    public SO_ActionData confirmationAction;
    public SO_ActionData openingAction;
    public List<SO_ActionData> comboActions;
    
    public SO_AIPattern UnlockPattern;
}
