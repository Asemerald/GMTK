using UnityEngine;

namespace Runtime.ScriptableObject
{
    [CreateAssetMenu(menuName = "Game/Feedback", fileName = "Feedback_")]
    public class SO_FeedbackData : UnityEngine.ScriptableObject
    {
        public FeedbackTarget target;
        public FeedbackSide side;
        public FeedbackType feedbackType;

        [Header("Animation")] public string animationTrigger;

        [Header("Particles")] public ParticleSystem particlePrefab;
    }

    public enum FeedbackTarget
    {
        Player,
        Enemy
    }

    public enum FeedbackSide
    {
        Left,
        Right
    }

    public enum FeedbackType
    {
        // A voir si on en a vraiment besoin parce qu sah jsp quoi en faire
        Punch,
        Block,
        Hit,
        Parry
    }
}