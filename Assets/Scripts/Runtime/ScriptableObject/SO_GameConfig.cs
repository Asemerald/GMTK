using FMODUnity;
using UnityEngine;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
    public class SO_GameConfig : UnityEngine.ScriptableObject
    {
        [Header("Audio")] public EventReference gameMusic;
        public float musicVolume = 1f;
        public int defaultBPM = 120;

        [Header("Gameplay")] public int beatsPerBar = 4;
        public float maxRoundDuration = 120f; // en secondes
        public float inputTimingTolerance = 0.1f; // secondes de marge

        [Header("Debug")] public bool enableDebug = false;
    }
}