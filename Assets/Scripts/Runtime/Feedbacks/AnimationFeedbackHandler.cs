using UnityEngine;

public class AnimationFeedbackHandler : MonoBehaviour, IFeedbackHandler
{
    [SerializeField] private Animator animator;

    public void PlayFeedback(string feedbackType)
    {
        animator.SetTrigger(feedbackType); // e.g. "PunchLeft", "Block"
    }
}