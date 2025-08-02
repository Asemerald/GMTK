using FMODUnity;
using UnityEngine;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(menuName = "Feedback/ActionFeedback")]
    public class SO_FeedbackData : UnityEngine.ScriptableObject
    {
        [Header("Animation Settings")] public string startAnimationName;
        public string animationSuccessTriggerName;
        public string animationFailTriggerName;
        
        [Header("Audio Settings")]
        public EventReference? soundEffect;
        
        [Header("Move Settings")]
        public FeedbackSide side;

        [Header("Lens Distortion Success Settings")] public bool successEnableLensDistortion = false;
        [Range(-1f, 1f)] public float successTargetIntensity = 0f;

        [Tooltip("Le temps pour arriver a la l'intensité que t'as choisis jpense t pas debile")]
        public float successTimeToTargetIntensity = 0.5f;

        [Tooltip("Le temps que ça reste a l'intensité que t'as choisis jpense t pas debile aussi")]
        public float successTimeAtTargetIntensity = 0.5f;

        [Tooltip("Le temps pour revenir a l'intensité de base jpense t pas debile tjr tu connais en amont")]
        public float successTimeToBaseIntensity = 0.5f;
        
        [Header("Lens Distortion parry Settings")]
        public bool parryEnableLensDistortion = false;
        [Range(-1f, 1f)] public float parryTargetIntensity = 0f;
        [Tooltip("Le temps pour arriver a la l'intensité que t'as choisis jpense t pas debile")]
        public float parryTimeToTargetIntensity = 0.5f;
        [Tooltip("Le temps que ça reste a l'intensité que t'as choisis jpense t pas debile aussi")]
        public float parryTimeAtTargetIntensity = 0.5f;
        [Tooltip("Le temps pour revenir a l'intensité de base jpense t pas debile tjr tu connais en amont")]
        public float parryTimeToBaseIntensity = 0.5f;
        
        [Header("Lens Distortion fail Settings")]
        public bool failEnableLensDistortion = false;
        [Range(-1f, 1f)] public float failTargetIntensity = 0f;
        [Tooltip("Le temps pour arriver a la l'intensité que t'as choisis jpense t pas debile")]
        public float failTimeToTargetIntensity = 0.5f;
        [Tooltip("Le temps que ça reste a l'intensité que t'as choisis jpense t pas debile aussi")]
        public float failTimeAtTargetIntensity = 0.5f;
        [Tooltip("Le temps pour revenir a l'intensité de base jpense t pas debile tjr tu connais en amont")]
        public float failTimeToBaseIntensity = 0.5f;
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
    
    public enum ActionCallbackType
    {
        OnStart,
        OnSuccess,
        OnBlock,
        OnFail
    }
}