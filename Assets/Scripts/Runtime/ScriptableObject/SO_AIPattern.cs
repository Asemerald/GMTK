using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ScriptableObject {
    [CreateAssetMenu(menuName = "Actions/AI Pattern", fileName = "AI Pattern")]
    public class SO_AIPattern : UnityEngine.ScriptableObject {
        public SO_ComboData pattern;
        public bool isUnlock;
    }
}