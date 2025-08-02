using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.ScriptableObject {
    [CreateAssetMenu(menuName = "Game/AI Config", fileName = "AI Config")]
    public class SO_AIConfig : UnityEngine.ScriptableObject {

        [Header("Roll Action Chance")] 
        public int chanceOfAttack;
        public int chanceOfExecutingCombo;
        public int chanceOfDodge;
        public int chanceOfParry;
        public int chanceOfCounter;
        public int chanceOfSuccessCombo;

    }
}