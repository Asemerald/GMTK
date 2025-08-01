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

    internal Volume GlobalVolume;
    private ColorAdjustments _colorAdjustments;

    [Header("DEBUG")] [SerializeField] private SO_FeedbackData _feedbackDebugData;

    internal void Initialize(Volume globalVolume = null)
    {
        GlobalVolume = globalVolume ?? FindObjectOfType<Volume>();
        if (GlobalVolume == null)
        {
            Debug.LogError("Global Volume not found in the scene. Please ensure a Volume component is present.");
            return;
        }
        
        InitializeColorAdjustments();
    }

    private void InitializeColorAdjustments()
    {
        if (GlobalVolume.profile.TryGet(out _colorAdjustments))
        {
            Debug.Log("ColorAdjustments found in Global Volume profile.");
        }
        else
        {
            Debug.LogWarning("ColorAdjustments not found in Global Volume profile. Hue shift feedback will not work.");
            _colorAdjustments = null;
        }
    }

    public void PlayAnimation(FeedbackSide side, FeedbackTarget target, AnimationClip clip)
    {
        var animator = GetAnimator(target);
        if (animator == null || clip == null) return;

        animator.Play(clip.name);
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

    public void PlayHueShift(HUEShiftValue data)
    {
        if (_colorAdjustments == null)
            return;
        if (data == HUEShiftValue.None)
        {
            Debug.LogWarning("[FeedbackPlayer] Hue shift is set to None but I was still called, skipping.");
            return;
        }

        var targetValue = HuePresetToFloat(data);
        _colorAdjustments.hueShift.value = targetValue;
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

    private float HuePresetToFloat(HUEShiftValue hueShiftValue)
    {
        return hueShiftValue switch
        {
            HUEShiftValue.None => 0f,
            HUEShiftValue.Zero => 0f,
            /*HUEShiftValue.Medium => 90f,
            HUEShiftValue.High => 180f,*/
            _ => throw new ArgumentOutOfRangeException(nameof(hueShiftValue), hueShiftValue, null)
        };
    }

#if UNITY_EDITOR
    [ContextMenu("Test Tint Color")]
    private void TestTintColor()
    {
        Initialize();
        PlayHueShift(_feedbackDebugData.hueShiftData); // Juste un exemple pour tester
    }
#endif
}