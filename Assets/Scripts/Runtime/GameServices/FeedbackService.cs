using Runtime.GameServices;
using Runtime.GameServices.Interfaces;
using Runtime.ScriptableObject;
using Unity.VisualScripting;
using UnityEngine;

public class FeedbackService : IGameSystem
{
    private FeedbackPlayer _feedbackPlayer;
    private readonly GameSystems _gameSystems;
    private SO_GameConfig _gameConfig;


    public FeedbackService(GameSystems gameSystems)
    {
        _gameSystems = gameSystems ?? throw new System.ArgumentNullException(nameof(gameSystems));
    }

    public void Initialize()
    {
        _gameConfig = _gameSystems.Get<GameConfigService>()?.GameConfig ??
                      throw new System.NullReferenceException("GameConfigService is not registered in GameSystems");

        // Spawn Global Volume if it exists in the config
        if (_gameConfig.volumePrefab != null)
        {
            var globalVolume = Object.Instantiate(_gameConfig.volumePrefab);
            if (globalVolume == null) Debug.LogError("[FeedbackService] Failed to instantiate volumePrefab");
        }


        // Instantiate the FeedbackPlayer Gameobject 
        _feedbackPlayer = Object.Instantiate(_gameConfig.FeedbackPlayerPrefab);

        if (_feedbackPlayer == null)
        {
            Debug.LogError("[FeedbackService] Failed to instantiate FeedbackPlayerPrefab");
            return;
        }

        _feedbackPlayer.Initialize();
    }

    public void PlayActionFeedback(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.animationTriggerName != null)
            _feedbackPlayer.PlayAnimation(feedback.side, feedback.target, feedback.animationTriggerName);

        if (feedback.particlePrefab != null)
            _feedbackPlayer.PlayParticle(feedback.side, feedback.target, feedback.particlePrefab);

        if (feedback.soundEffect != null)
            _feedbackPlayer.PlaySound(feedback.soundEffect);

        if (feedback.hueShiftData != HUEShiftValue.None) // éviter un Color.clear ou défaut
            _feedbackPlayer.PlayHueShift(feedback.hueShiftData);

        if (feedback.enableLensDistortion)
            _feedbackPlayer.PlayDistortion(feedback);
    }

    public void Tick()
    {
    }

    public void Dispose()
    {
    }
}