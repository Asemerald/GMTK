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

    public void PlayActionFeedback(SO_FeedbackData feedback, FeedbackTarget feedbackTarget, ActionCallbackType callbackType)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService::PlayActionFeedback: Tried to play null feedback");
            return;
        }
        
        switch (callbackType)
        {
            case ActionCallbackType.OnStart:
                FeedbackToDoOnStart(feedback, feedbackTarget);
                break;
            case ActionCallbackType.OnSuccess:
                FeedbackToDoOnSuccess(feedback, feedbackTarget);
                break;
            case ActionCallbackType.OnBlock:
                FeedbackToDoOnBlock(feedback, feedbackTarget);
                break;
            case ActionCallbackType.OnFail:
                FeedbackToDoOnFail(feedback, feedbackTarget);
                break;
            default:
                Debug.LogWarning($"FeedbackService::PlayActionFeedback: Unknown callback type {callbackType}");
                break;
        }
    
    }
    
    private void FeedbackToDoOnStart(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.startAnimationName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.startAnimationName);
        }
    }
    
    private void FeedbackToDoOnSuccess(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationSuccessTriggerName);
        }

        if (feedback.soundEffect != null)
        {
            _feedbackPlayer.PlaySound(feedback.soundEffect);
        }
    }
    
    private void FeedbackToDoOnBlock(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationSuccessTriggerName);
        }

        if (feedback.soundEffect != null)
        {
            _feedbackPlayer.PlaySound(feedback.soundEffect);
        }
    }
    
    private void FeedbackToDoOnFail(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.animationFailTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationFailTriggerName);
        }
        
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