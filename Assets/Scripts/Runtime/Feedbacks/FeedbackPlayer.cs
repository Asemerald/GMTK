using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FeedbackPlayer : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Animator playerAnimator;

    [SerializeField] internal Animator enemyAnimator;

    [SerializeField] private ParticleSystem leftImpactParticles;
    [SerializeField] private ParticleSystem rightImpactParticles;

    private Volume globalVolume;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;

    [Header("DEBUG")] [SerializeField] private SO_FeedbackData _feedbackDebugData;
    [SerializeField] private Volume debugGlobalVolume;

    internal void Initialize(Volume globalVolume = null)
    {
        this.globalVolume = globalVolume ?? FindFirstObjectByType<Volume>();
        if (this.globalVolume == null)
        {
            Debug.LogError("Global Volume not found in the scene. Please ensure a Volume component is present.");
            return;
        }

        InitializeColorAdjustments();
        InitializeLensDistortion();
    }

    private void InitializeColorAdjustments()
    {
        if (globalVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.Log("ColorAdjustments found in Global Volume profile.");
        }
        else
        {
            Debug.LogWarning("ColorAdjustments not found in Global Volume profile. Hue shift feedback will not work.");
            colorAdjustments = null;
        }
    }

    private void InitializeLensDistortion()
    {
        if (globalVolume.profile.TryGet(out lensDistortion))
        {
            Debug.Log("LensDistortion found in Global Volume profile.");
        }
        else
        {
            Debug.LogWarning("LensDistortion not found in Global Volume profile. Distortion feedback will not work.");
            lensDistortion = null;
        }
    }


    public void PlayAnimation(FeedbackTarget feedbackTarget, string animationTriggerName)
    {
        var animator = GetAnimator(feedbackTarget);
        if (animator == null || animationTriggerName == null) return;

        animator.SetTrigger(animationTriggerName);
    }


    public void PlayParticle(FeedbackSide side, FeedbackTarget target, GameObject particlePrefab)
    {
        if (particlePrefab != null)
        {
            var spawnPosition = GetImpactPosition(side, target);
            Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Or if you're using pooled particles or preplaced ones
            var particles = GetPreplacedParticleSystem(side);
            if (particles != null) particles.Play();
        }
    }

    public void PlaySound(EventReference? clip)
    {
        Debug.Log($"Playing sound: {clip}");
        //TODO PlayOneShot avec FMOD
    }

    public void PlayHueShift()
    {
        if (colorAdjustments == null)
            return;
        
        var targetValue = 0f; // Default value if no hue shift is set
        
        colorAdjustments.hueShift.value = targetValue;
    }

    public void PlayDistortion(SO_FeedbackData feedbackData, ActionCallbackType callbackType)
    {
        if (lensDistortion == null)
        {
            Debug.LogWarning(
                "[FeedbackPlayer] LensDistortion not found in Global Volume profile. Distortion feedback will not work.");
            return;
        }
        
        switch (callbackType)
        {
            case ActionCallbackType.OnSuccess:
                StartCoroutine(ApplyLensDistortion(
                    feedbackData.successTargetIntensity,
                    feedbackData.successTimeToTargetIntensity,
                    feedbackData.successTimeAtTargetIntensity,
                    feedbackData.successTimeToBaseIntensity));
                break;
            case ActionCallbackType.OnBlock:
                StartCoroutine(ApplyLensDistortion(
                    feedbackData.parryTargetIntensity,
                    feedbackData.parryTimeToTargetIntensity,
                    feedbackData.parryTimeAtTargetIntensity,
                    feedbackData.parryTimeToBaseIntensity));
                break;
            case ActionCallbackType.OnFail:
                StartCoroutine(ApplyLensDistortion(
                    feedbackData.failTargetIntensity,
                    feedbackData.failTimeToTargetIntensity,
                    feedbackData.failTimeAtTargetIntensity,
                    feedbackData.failTimeToBaseIntensity));
                break;
            default:
                Debug.LogWarning("FeedbackPlayer: Unknown callback type for lens distortion.");
                break;
        }
        
        
    }

    private IEnumerator ApplyLensDistortion(float targetIntensity, float duration, float TimeAtTarget, float timeToZero)
    {
        // Ensure the lens distortion effect is enabled, then go to the target intensity in duration seconds, then back to zero in timeToZero seconds.
        if (lensDistortion == null)
        {
            Debug.LogWarning(
                "[FeedbackPlayer] LensDistortion not found in Global Volume profile. Distortion feedback will not work.");
            yield break;
        }

        //lensDistortion.intensity.Override(targetIntensity);
        var elapsedTime = 0f;
        var initialIntensity = lensDistortion.intensity.value;

        // Increase intensity to target
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            lensDistortion.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, t);
            yield return null;
        }

        // Wait for a moment at target intensity
        yield return new WaitForSeconds(TimeAtTarget);
        // Decrease intensity back to zero
        elapsedTime = 0f;
        while (elapsedTime < timeToZero)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / timeToZero);
            lensDistortion.intensity.value = Mathf.Lerp(targetIntensity, 0f, t);
            yield return null;
        }

        // Ensure intensity is set to zero at the end
        lensDistortion.intensity.value = 0f;
    }
    
    private void StopAnimation(FeedbackTarget feedbackTarget)
    {
        // Stop the animation by making it go to the idle state
        var animator = GetAnimator(feedbackTarget);
        if (animator == null) return;
        animator.SetTrigger("P_Idle"); // HARDCODER mais hassoul
        
    }

    public void PlayBlockFeedback(SO_FeedbackData feedbackData, FeedbackTarget feedbackTarget, FeedbackSide feedbackSide, GameObject blockParticlePrefab)
    {
        if (feedbackData == null)
        {
            Debug.LogWarning("FeedbackPlayer: Tried to play null feedback");
            return;
        }

        // Play animation if specified
        if (!string.IsNullOrEmpty(feedbackData.startAnimationName))
        {
            PlayAnimation(feedbackTarget, feedbackData.animationSuccessTriggerName);
        }

        // Play sound effect if specified
        if (!feedbackData.soundEffect.IsNull)
        {
            PlaySound(feedbackData.soundEffect);
        }

        // Play particles
        PlayParticle(feedbackSide, feedbackTarget, blockParticlePrefab);

        // Apply lens distortion effects based on the feedback type
        if (feedbackData.successEnableLensDistortion)
        {
            StartCoroutine(ApplyLensDistortion(
                feedbackData.successTargetIntensity,
                feedbackData.successTimeToTargetIntensity,
                feedbackData.successTimeAtTargetIntensity,
                feedbackData.successTimeToBaseIntensity));
        }
        else if (feedbackData.parryEnableLensDistortion)
        {
            StartCoroutine(ApplyLensDistortion(
                feedbackData.parryTargetIntensity,
                feedbackData.parryTimeToTargetIntensity,
                feedbackData.parryTimeAtTargetIntensity,
                feedbackData.parryTimeToBaseIntensity));
        }
        else if (feedbackData.failEnableLensDistortion)
        {
            StartCoroutine(ApplyLensDistortion(
                feedbackData.failTargetIntensity,
                feedbackData.failTimeToTargetIntensity,
                feedbackData.failTimeAtTargetIntensity,
                feedbackData.failTimeToBaseIntensity));
        }
    }


    private Animator GetAnimator(FeedbackTarget target)
    {
        return target switch
        {
            FeedbackTarget.Player => playerAnimator,
            FeedbackTarget.Enemy => enemyAnimator,
            _ => null
        };
    }

    private ParticleSystem GetPreplacedParticleSystem(FeedbackSide side)
    {
        return side switch
        {
            FeedbackSide.Left => leftImpactParticles,
            FeedbackSide.Right => rightImpactParticles,
            _ => null
        };
    }

    private Vector3 GetImpactPosition(FeedbackSide side, FeedbackTarget target)
    {
        var baseTransform = target == FeedbackTarget.Player ? playerAnimator.transform : enemyAnimator.transform;
        var offset = side == FeedbackSide.Left ? Vector3.left : Vector3.right;
        return baseTransform.position + offset * 0.5f;
    }

    #region Debug Methods

#if UNITY_EDITOR
    [ContextMenu("Test Tint Color")]
    private void TestTintColor()
    {
        //PlayHueShift(_feedbackDebugData.hueShiftData); // Juste un exemple pour tester
    }
    
#endif

    #endregion
}