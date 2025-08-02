using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;

[CreateAssetMenu(menuName = "Combo")]
public class SO_ComboData : ScriptableObject
{
    public SO_ActionData openingAction;
    public SO_ActionData confirmationAction;
    [SerializeField]
    private SO_ActionData[] comboActions = new SO_ActionData[9];
    public SO_ActionData[] ComboActions => comboActions;
    public SO_AIPattern UnlockPattern;
}
