using System;
using System.Collections;
using System.Numerics;
using FMODUnity;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class FeedbackPlayer : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Animator playerAnimator;
    [SerializeField] internal Animator enemyAnimator;
    [SerializeField] public ArmColliderLogic leftPlayerArmColliderLogic;
    [SerializeField] public ArmColliderLogic rightPlayerArmColliderLogic;
    [SerializeField] public ArmColliderLogic leftEnemyArmColliderLogic;
    [SerializeField] public ArmColliderLogic rightEnemyArmColliderLogic;

    [SerializeField] private ParticleSystem leftImpactParticles;
    [SerializeField] private ParticleSystem rightImpactParticles;

    [Header("Ring")] [SerializeField] private bool enableRingColorSwitch = false;
    [SerializeField] private MeshRenderer ringMesh;
    [SerializeField] private Color ringColor = Color.white;
    [SerializeField] private Color ringColor2 = Color.black;

    private Volume globalVolume;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    private Material ringMat;

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
        
        // Create an instance of the ring material and each bar, switch its color if enabled

        if (enableRingColorSwitch)
        {
            ringMat = new Material(ringMesh.material); 
            ringMesh.material = ringMat;
        }
        
    }

    public void FeedbackEachBar()
    {
        if (enableRingColorSwitch)
        {
            // Change ringmat color to the other depending on the current color
            if (ringMat.color == ringColor)
            {
                ringMat.color = ringColor2;
            }
            else
            {
                ringMat.color = ringColor;
            }
        }
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


    public void PlayAnimation(FeedbackSide side, FeedbackTarget feedbackTarget, string animationTriggerName)
    {
        var animator = GetAnimator(feedbackTarget);
        if (animator == null || string.IsNullOrEmpty(animationTriggerName))
            return;

        animator.SetTrigger(animationTriggerName);

        // Sélection du bon ArmColliderLogic selon la main et la cible
        ArmColliderLogic colliderLogic = side switch
        {
            FeedbackSide.Left when feedbackTarget == FeedbackTarget.Player => leftPlayerArmColliderLogic,
            FeedbackSide.Right when feedbackTarget == FeedbackTarget.Player => rightPlayerArmColliderLogic,
            FeedbackSide.Left when feedbackTarget == FeedbackTarget.Enemy => leftEnemyArmColliderLogic,
            FeedbackSide.Right when feedbackTarget == FeedbackTarget.Enemy => rightEnemyArmColliderLogic,
            FeedbackSide.Center when feedbackTarget == FeedbackTarget.Player => leftPlayerArmColliderLogic, // Center peut être Left pour le player
            FeedbackSide.Center when feedbackTarget == FeedbackTarget.Enemy => leftEnemyArmColliderLogic,
            _ => null
        };

        if (colliderLogic == null)
        {
            Debug.LogWarning($"[FeedbackPlayer] Aucun collider trouvé pour {side} {feedbackTarget}");
            return;
        }

        // Handlers pour stop animation après un seul hit
        Action<Vector3> armHitHandler = null;
        Action<Vector3> headHitHandler = null;

        armHitHandler = (Vector3 hitPos) =>
        {
            StopAnimation(feedbackTarget);
            colliderLogic.OnArmHit -= armHitHandler;
            colliderLogic.OnHeadHit -= headHitHandler;
        };

        headHitHandler = (Vector3 hitPos) =>
        {
            StopAnimation(feedbackTarget);
            colliderLogic.OnArmHit -= armHitHandler;
            colliderLogic.OnHeadHit -= headHitHandler;
        };

        colliderLogic.OnArmHit += armHitHandler;
        colliderLogic.OnHeadHit += headHitHandler;
    }




    public void PlayParticle(FeedbackSide side, FeedbackTarget target, GameObject particlePrefab)
    {
        ArmColliderLogic colliderLogic = side switch
        {
            FeedbackSide.Left when target == FeedbackTarget.Player => leftPlayerArmColliderLogic,
            FeedbackSide.Right when target == FeedbackTarget.Player => rightPlayerArmColliderLogic,
            FeedbackSide.Left when target == FeedbackTarget.Enemy => leftEnemyArmColliderLogic,
            FeedbackSide.Right when target == FeedbackTarget.Enemy => rightEnemyArmColliderLogic,
            _ => null
        };

        if (colliderLogic == null)
        {
            Debug.LogWarning($"[FeedbackPlayer] Aucun collider trouvé pour {side} {target}");
            return;
        }

        // On garde la référence du handler pour pouvoir l’unsub après appel
        Action<Vector3> armHitHandler = null;
        Action<Vector3> headHitHandler = null;
        armHitHandler = (Vector3 hitPos) =>
        {
            // Jouer la particule
            PlayParticleAtPosition(particlePrefab, hitPos);

            // Désabonnement pour que ça ne se joue qu'une fois
            colliderLogic.OnArmHit -= armHitHandler;
            colliderLogic.OnHeadHit -= headHitHandler;
        };
        
        headHitHandler = (Vector3 hitPos) =>
        {
            // Jouer la particule
            PlayParticleAtPosition(particlePrefab, hitPos);

            // Désabonnement pour que ça ne se joue qu'une fois
            colliderLogic.OnHeadHit -= headHitHandler;
            colliderLogic.OnArmHit += armHitHandler;
        };
        
        // On s’abonne aux événements de collision
        colliderLogic.OnHeadHit += headHitHandler;
        colliderLogic.OnArmHit += armHitHandler;
    }

    private void PlayParticleAtPosition(GameObject particlePrefab, Vector3 position)
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning("[FeedbackPlayer] Aucun prefab de particule fourni.");
            return;
        }

        GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
        if (particle.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
            Destroy(particle, ps.main.duration);
        }
        else
        {
            Destroy(particle, 2f);
        }
    }



    public void PlaySound(EventReference clip)
    {
        if (clip.IsNull)
        {
            Debug.LogWarning("[FeedbackPlayer] Aucun clip audio fourni.");
            return;
        }
        
        RuntimeManager.PlayOneShot(clip);
    }

    public void PlayHueShift()
    {
        if (colorAdjustments == null)
            return;
        
        // Chose a random value between -180 and 180 and apply it to the hue shift
        float targetValue = UnityEngine.Random.Range(-180f, 180f);
        
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
            PlayAnimation(feedbackSide, feedbackTarget, feedbackData.animationSuccessTriggerName);
        }

        // Play sound effect if specified
        if (!feedbackData.successSoundEffect.IsNull)
        {
            PlaySound(feedbackData.successSoundEffect);
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