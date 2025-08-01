using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using UnityEngine;

public class FeedbackService : IGameSystem
{
    private FeedbackPlayer _feedbackPlayer;
    private readonly GameSystems _gameSystems;


    public FeedbackService(GameSystems gameSystems)
    {
        _gameSystems = gameSystems ?? throw new System.ArgumentNullException(nameof(gameSystems));
    }

    public void Initialize()
    {
        _feedbackPlayer = GameObject.FindObjectOfType<FeedbackPlayer>();
        if (_feedbackPlayer == null)
            Debug.LogError("FeedbackReceiver not found in scene");
    }

    public void PlayActionFeedback(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.animationClip != null)
            _feedbackPlayer.PlayAnimation(feedback.side, feedback.target, feedback.animationClip);

        if (feedback.particlePrefab != null)
            _feedbackPlayer.PlayParticle(feedback.side, feedback.target, feedback.particlePrefab);

        if (feedback.soundEffect != null)
            _feedbackPlayer.PlaySound(feedback.soundEffect);

        if (feedback.colorTint.a > 0.01f) // éviter un Color.clear ou défaut
            _feedbackPlayer.TintColor(feedback.colorTint);
    }

    public void Tick()
    {
    }

    public void Dispose()
    {
    }
}