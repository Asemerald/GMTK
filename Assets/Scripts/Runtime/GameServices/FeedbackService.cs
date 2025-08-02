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
    private BeatSyncService _beatSyncService;


    public FeedbackService(GameSystems gameSystems, FeedbackPlayer feedbackPlayer)
    {
        _gameSystems = gameSystems ?? throw new System.ArgumentNullException(nameof(gameSystems));

        _feedbackPlayer = feedbackPlayer ?? throw new System.ArgumentNullException(nameof(feedbackPlayer));
    }

    public void Initialize()
    {
        _gameConfig = _gameSystems.Get<GameConfigService>()?.GameConfig ??
                      throw new System.NullReferenceException("GameConfigService is not registered in GameSystems");

        _beatSyncService = _gameSystems.Get<BeatSyncService>() ??
                           throw new System.NullReferenceException("BeatSyncService is not registered in GameSystems");
        _feedbackPlayer.Initialize();

        _beatSyncService.OnBeat += () => FeedbackToDoEachBeat(_gameConfig.feedbackEachBeat);
        
        _beatSyncService.OnBar += () => FeedbackToDoEachBar(_gameConfig.feedbackEachBar);

    }

    public void PlayActionFeedback(SO_FeedbackData feedback, bool playerFeedback, ActionCallbackType callbackType)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService::PlayActionFeedback: Tried to play null feedback");
            return;
        }

        /*if (feedback.animationTriggerName != null)
            _feedbackPlayer.PlayAnimation(feedback.side, feedback.target, feedback.animationTriggerName);

        if (feedback.particlePrefab != null)
            _feedbackPlayer.PlayParticle(feedback.side, feedback.target, feedback.particlePrefab);

        if (feedback.soundEffect != null)
            _feedbackPlayer.PlaySound(feedback.soundEffect);

        if (feedback.hueShiftData != HUEShiftValue.None) // éviter un Color.clear ou défaut
            _feedbackPlayer.PlayHueShift(feedback.hueShiftData);

        if (feedback.enableLensDistortion)
            _feedbackPlayer.PlayDistortion(feedback);*/
    }

    private void FeedbackToDoEachBeat(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        /*if (feedback.hueShiftData != HUEShiftValue.None) // éviter un Color.clear ou défaut
            _feedbackPlayer.PlayHueShift(feedback.hueShiftData);

        if (feedback.enableLensDistortion)
            _feedbackPlayer.PlayDistortion(feedback);*/
    }

    private void FeedbackToDoEachBar(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        /*if (feedback.hueShiftData != HUEShiftValue.None) // éviter un Color.clear ou défaut
            _feedbackPlayer.PlayHueShift(feedback.hueShiftData);

        if (feedback.enableLensDistortion)
            _feedbackPlayer.PlayDistortion(feedback);*/
    }


    public void Tick()
    {
    }

    public void Dispose()
    {
    }
}