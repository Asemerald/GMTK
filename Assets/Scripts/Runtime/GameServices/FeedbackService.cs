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
        
        

    }

    public void PlayActionFeedback(SO_FeedbackData feedback, FeedbackTarget feedbackTarget, ActionCallbackType callbackType)
    {
        if (feedback == null)
        {
           FeedbackToDoEachBeat(_gameConfig.feedbackEachBeat);
        }
        
        Debug.LogWarning("FeedbackService: PlayFeedback");
        
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

        Debug.LogWarning("FeedbackService: Feedback Success");
        
        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationSuccessTriggerName);
        }

        if (!feedback.soundEffect.IsNull)
        {
            _feedbackPlayer.PlaySound(feedback.soundEffect);
        }
        
        if (feedback.successEnableLensDistortion)
        {
            _feedbackPlayer.PlayDistortion(feedback, ActionCallbackType.OnSuccess);
        }
        
        if (_gameConfig.hitEffectPrefab != null)
        {
            _feedbackPlayer.PlayParticle(feedback.side, feedbackTarget, _gameConfig.hitEffectPrefab);
        }
    }
    
    private void FeedbackToDoOnBlock(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }
        
        Debug.LogWarning("FeedbackService: Block Feedback");

        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationSuccessTriggerName);
        }

        if (!feedback.blockSoundEffect.IsNull)
        {
            _feedbackPlayer.PlaySound(feedback.blockSoundEffect);
        }
        
        if (feedback.parryEnableLensDistortion)
        {
            _feedbackPlayer.PlayDistortion(feedback, ActionCallbackType.OnBlock);
        }
        
        if (_gameConfig.blockEffectPrefab != null)
        {
            _feedbackPlayer.PlayParticle(feedback.side, feedbackTarget, _gameConfig.blockEffectPrefab);
        }
    }
    
    private void FeedbackToDoOnFail(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        Debug.LogWarning("FeedbackService: Failed Feedback");
        if (feedback.animationFailTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedbackTarget, feedback.animationFailTriggerName);
        }
        
        if (!feedback.soundEffect.IsNull)
        {
            _feedbackPlayer.PlaySound(feedback.soundEffect);
        }
        
        if (feedback.failEnableLensDistortion)
        {
            _feedbackPlayer.PlayDistortion(feedback, ActionCallbackType.OnFail);
        }
    }

    /// <summary>
    /// Plays block feedback based on the punch type and side.
    /// </summary>
    /// <param name="punchType">The type of punch (Punch or Hook).</param>
    /// <param name="punchSide">The side of the punch (Left or Right).</param>
    public void PlayBlockFeedback(PunchType punchType, FeedbackSide punchSide)
    {
        switch (punchType, punchSide)
        {
            case (PunchType.Punch, FeedbackSide.Left):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockLeftPunchFeedback, FeedbackTarget.Player, FeedbackSide.Left, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Punch, FeedbackSide.Right):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockRightPunchFeedback, FeedbackTarget.Player, FeedbackSide.Right, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Hook, FeedbackSide.Left):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockLeftHookFeedback, FeedbackTarget.Player, FeedbackSide.Left, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Hook, FeedbackSide.Right):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockRightHookFeedback, FeedbackTarget.Player, FeedbackSide.Right, _gameConfig.blockEffectPrefab);
                break;
            default:
                Debug.LogWarning($"FeedbackService: Unknown punch type {punchType} or side {punchSide}");
                break;
        }
    }

    private void FeedbackToDoEachBeat(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            //Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }
        
        _feedbackPlayer.PlayAnimation(FeedbackTarget.Player, feedback.animationSuccessTriggerName);
        _feedbackPlayer.PlayAnimation(FeedbackTarget.Enemy, feedback.animationSuccessTriggerName);

       
    }

    private void FeedbackToDoEachBar(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            //Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }
        
    }


    public void Tick()
    {
    }

    public void Dispose()
    {
    }
}