using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessFeedbackHandler : MonoBehaviour, IFeedbackHandler
{
    [SerializeField] private Volume volume;
    private ColorAdjustments colorAdjust;

    private void Awake()
    {
        volume.profile.TryGet(out colorAdjust);
    }

    public void PlayFeedback(string feedbackType)
    {
        if (feedbackType == "Block") StartCoroutine(FlashHue());
    }

    private IEnumerator FlashHue()
    {
        colorAdjust.hueShift.value = 180;
        yield return new WaitForSeconds(0.2f);
        colorAdjust.hueShift.value = 0;
    }
}