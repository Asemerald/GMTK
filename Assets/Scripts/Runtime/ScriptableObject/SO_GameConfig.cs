using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
    public class SO_GameConfig : UnityEngine.ScriptableObject
    {
        [Header("Prefabs")] public GameObject playerPrefab;
        public GameObject enemyPrefab;
        public GameObject hitEffectPrefab;
        public GameObject blockEffectPrefab;

        [Header("Feedbacks")] public SO_FeedbackData feedbackEachBeat;
        public SO_FeedbackData feedbackEachBar;
        public SO_FeedbackData blockRightPunchFeedback;
        public SO_FeedbackData blockLeftPunchFeedback;
        public SO_FeedbackData blockLeftHookFeedback;
        public SO_FeedbackData blockRightHookFeedback;

        [Header("Audio")] public EventReference gameMusic;
        public float musicVolume = 1f;
        public int defaultBPM = 120;

        [Header("Gameplay")] public int beatsPerBar = 4;
        public float maxRoundDuration = 120f; // en secondes
        public float inputTimingTolerance = 0.1f; // secondes de marge
        public int maxHealth = 100;
        public int maxStructure = 100;
        public int timeBeforeStructureRegen = 5; 

        [Header("Debug")] public bool enableDebug = false;
    }
}