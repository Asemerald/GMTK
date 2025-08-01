using FMODUnity;
using UnityEngine;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(menuName = "Feedback/ActionFeedback")]
    public class SO_FeedbackData : UnityEngine.ScriptableObject
    {
        [Header("Feedback Settings")] public string animationTriggerName;
        public EventReference? soundEffect;
        public GameObject particlePrefab;
        public FeedbackSide side;
        public FeedbackTarget target;
        public HUEShiftValue hueShiftData;

        [Header("Lens Distortion Settings")] public bool enableLensDistortion = false;
        [Range(-1f, 1f)] public float targetIntensity = 0f;

        [Tooltip("Le temps pour arriver a la l'intensité que t'as choisis jpense t pas debile")]
        public float timeToTargetIntensity = 0.5f;

        [Tooltip("Le temps que ça reste a l'intensité que t'as choisis jpense t pas debile aussi")]
        public float timeAtTargetIntensity = 0.5f;

        [Tooltip("Le temps pour revenir a l'intensité de base jpense t pas debile tjr tu connais en amont")]
        public float timeToBaseIntensity = 0.5f;
    }

    public enum FeedbackTarget
    {
        Player,
        Enemy
    }

    public enum FeedbackSide
    {
        Left,
        Right,
        Center
    }

    public enum HUEShiftValue
    {
        None,
        Zero
    }
}