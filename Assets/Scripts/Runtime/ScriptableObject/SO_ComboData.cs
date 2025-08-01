using System.Collections.Generic;
using UnityEngine;
using Runtime.Enums;

[CreateAssetMenu(menuName = "Combo")]
public class SO_ComboData : ScriptableObject
{
    public SO_ActionData confirmationAction;
    public SO_ActionData openingAction;
    [SerializeField]
    private SO_ActionData[] comboActions = new SO_ActionData[16];
    public SO_ActionData[] ComboActions => comboActions;
}
