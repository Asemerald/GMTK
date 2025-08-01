using FMODUnity;
using UnityEngine;
using Runtime.Enums;
using Runtime.ScriptableObject;

public class FeedbackPlayer : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Animator playerAnimator;

    [SerializeField] private Animator enemyAnimator;

    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private ParticleSystem leftImpactParticles;
    [SerializeField] private ParticleSystem rightImpactParticles;

    [SerializeField] private Renderer postProcessHueTarget;

    public void PlayAnimation(FeedbackSide side, FeedbackTarget target, AnimationClip clip)
    {
        var animator = GetAnimator(target);
        if (animator == null || clip == null) return;

        animator.Play(clip.name);
    }

    public void PlayParticle(FeedbackSide side, FeedbackTarget target, GameObject particlePrefab)
    {
        // If you use prefab instantiation
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

    public void TintColor(Color color)
    {
        if (postProcessHueTarget != null)
            // Exemple : on modifie une propriété de material qui pilote un shader post-process custom
            postProcessHueTarget.material.SetColor("_HueTint", color);
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
        // Juste un exemple générique — à adapter selon ta scène
        var baseTransform = target == FeedbackTarget.Player ? playerAnimator.transform : enemyAnimator.transform;
        var offset = side == FeedbackSide.Left ? Vector3.left : Vector3.right;
        return baseTransform.position + offset * 0.5f;
    }
}