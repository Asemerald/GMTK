using FMODUnity;
using UnityEngine;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(menuName = "Feedback/ActionFeedback")]
    public class SO_FeedbackData : UnityEngine.ScriptableObject
    {
        [Header("Feedback Settings")] public AnimationClip animationClip;
        public EventReference? soundEffect;
        public GameObject particlePrefab;
        public FeedbackSide side;
        public FeedbackTarget target;
        public HUEShiftValue hueShiftData;
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