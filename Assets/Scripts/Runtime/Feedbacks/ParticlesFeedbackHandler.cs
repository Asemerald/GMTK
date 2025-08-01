using UnityEngine;

public class ParticleFeedbackHandler : MonoBehaviour, IFeedbackHandler
{
    [SerializeField] private ParticleSystem hitVFX;

    public void PlayFeedback(string feedbackType)
    {
        if (feedbackType == "Hit") hitVFX.Play();
    }
}