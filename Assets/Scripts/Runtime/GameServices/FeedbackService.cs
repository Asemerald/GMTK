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
    private StructureService _structureService;
    
    bool debug;

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
        
        _structureService = _gameSystems.Get<StructureService>() ??
                            throw new System.NullReferenceException("StructureService is not registered in GameSystems");
        
        _feedbackPlayer.Initialize();
        
        
        _beatSyncService.OnBar += _feedbackPlayer.FeedbackEachBar;
    }

    public void PlayActionFeedback(SO_FeedbackData feedback, FeedbackTarget feedbackTarget, ActionCallbackType callbackType)
    {
        if (feedback == null)
        {
           FeedbackToDoEachBeat(_gameConfig.feedbackEachBeat);
           if(debug) 
               Debug.LogWarning("FeedbackService: PlayFeedback NULL by " + feedbackTarget);
        }
        else
        {
            if(debug) 
                Debug.Log("FeedbackService: PlayFeedback "+ feedback.name + " by " + feedbackTarget);
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
                if(debug)
                    Debug.LogWarning($"FeedbackService::PlayActionFeedback: Unknown callback type {callbackType}");
                break;
        }
    
    }
    
    private void FeedbackToDoOnStart(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            if(debug)
                Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if (feedback.startAnimationName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedback.side, feedbackTarget, feedback.startAnimationName);
        }
        
        
    }
    
    private void FeedbackToDoOnSuccess(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            if(debug)
                Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if(debug)
            Debug.LogWarning("FeedbackService: Feedback Success");
        
        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedback.side, feedbackTarget, feedback.animationSuccessTriggerName);
        }

        if (!feedback.successSoundEffect.IsNull)
        {
            _feedbackPlayer.PlaySound(feedback.successSoundEffect);
        }
        
        if (feedback.successEnableLensDistortion)
        {
            _feedbackPlayer.PlayDistortion(feedback, ActionCallbackType.OnSuccess);
        }
        
        if (_gameConfig.hitEffectPrefab != null)
        {
            _feedbackPlayer.PlayParticle(feedback.side, feedbackTarget, _gameConfig.hitEffectPrefab);
        }
        
        _feedbackPlayer.PlayHueShift();
    }
    
    private void FeedbackToDoOnBlock(SO_FeedbackData feedback, FeedbackTarget feedbackTarget)
    {
        if (feedback == null)
        {
            if(debug)
                Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }
        
        if(debug)
            Debug.LogWarning("FeedbackService: Block Feedback");

        if (feedback.animationSuccessTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedback.side, feedbackTarget, feedback.animationSuccessTriggerName);
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
            if(debug)
                Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }

        if(debug)
            Debug.LogWarning("FeedbackService: Failed Feedback");
        if (feedback.animationFailTriggerName != string.Empty)
        {
            _feedbackPlayer.PlayAnimation(feedback.side, feedbackTarget, feedback.animationFailTriggerName);
        }
        
        if (!feedback.successSoundEffect.IsNull)
        {
            _feedbackPlayer.PlaySound(feedback.successSoundEffect);
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
    public void PlayBlockFeedback(PunchType punchType, FeedbackSide punchSide,FeedbackTarget target)
    {
        switch (punchType, punchSide)
        {
            case (PunchType.Punch, FeedbackSide.Left):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockLeftPunchFeedback, target, FeedbackSide.Left, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Punch, FeedbackSide.Right):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockRightPunchFeedback,target, FeedbackSide.Right, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Hook, FeedbackSide.Left):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockLeftHookFeedback,  target, FeedbackSide.Left, _gameConfig.blockEffectPrefab);
                break;
            case (PunchType.Hook, FeedbackSide.Right):
                _feedbackPlayer.PlayBlockFeedback(_gameConfig.blockRightHookFeedback, target, FeedbackSide.Right, _gameConfig.blockEffectPrefab);
                break;
            default:
                if(debug)
                    Debug.LogWarning($"FeedbackService: Unknown punch type {punchType} or side {punchSide}");
                break;
        }
    }

    private void FeedbackToDoEachBeat(SO_FeedbackData feedback)
    {
        if (feedback == null)
        {
            if(debug)
                Debug.LogWarning("FeedbackService: Tried to play null feedback");
            return;
        }
        
        _feedbackPlayer.PlayAnimation(feedback.side, FeedbackTarget.Player, feedback.animationSuccessTriggerName);
        _feedbackPlayer.PlayAnimation(feedback.side, FeedbackTarget.Enemy, feedback.animationSuccessTriggerName);

       
    }

    public void FeedbackDeath(bool player) {
        _feedbackPlayer.PlayAnimation(FeedbackSide.Center, player ? FeedbackTarget.Player : FeedbackTarget.Enemy, "P_Break");
    }

    public void EndScreen(bool player) {
        _feedbackPlayer.DisplayWinCanvas(player);
    }

    public void Tick()
    {
        _feedbackPlayer.ChangeEnemyHpAmount(_structureService.EnemyHP);
        _feedbackPlayer.ChangePlayerHpAmount(_structureService.PlayerHP);
    }
    

    public void Dispose()
    {
    }
    
    public void SetDebug(bool state)
    {
        debug = state;
    }
}