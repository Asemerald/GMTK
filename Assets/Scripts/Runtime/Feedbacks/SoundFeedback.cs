using UnityEngine;

namespace Runtime.Feedbacks
{
    public class SoundFeedback : MonoBehaviour, IFeedbackHandler
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip blockSound;

        public void PlayFeedback(string feedbackType)
        {
            if (feedbackType == "Hit" && hitSound != null)
                audioSource.PlayOneShot(hitSound);
            else if (feedbackType == "Block" && blockSound != null) audioSource.PlayOneShot(blockSound);
        }
    }
}