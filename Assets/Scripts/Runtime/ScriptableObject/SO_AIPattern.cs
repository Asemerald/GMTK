using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ScriptableObject {
    [CreateAssetMenu]
    public class SO_AIPattern : UnityEngine.ScriptableObject {
        public SO_ComboData pattern;
        public bool isUnlock;
    }
}