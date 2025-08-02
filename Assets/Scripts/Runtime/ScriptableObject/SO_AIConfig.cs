using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.ScriptableObject {
    [CreateAssetMenu(menuName = "Runtime Scriptable Object/AI Config")]
    public class SO_AIConfig : UnityEngine.ScriptableObject {

        [Header("Roll Action Chance")] 
        public int attack;
        public int tryExecuteCombo;
        public int dodge;
        public int parry;
        public int counter;
        public int executeSuccessCombo;

    }
}