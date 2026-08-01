using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public sealed class FlyingRewardCoin : MonoBehaviour
{
    [Header("Полёт")]
    [SerializeField, Min(0.05f)]
    private float flightDuration = 0.65f;

    [SerializeField]
    private float arcHeight = 80f;

    [Header("Масштаб")]
    [SerializeField, Range(0.1f, 2f)]
    private float startScale = 0.7f;

    [SerializeField, Range(0.1f, 2f)]
    private float arrivalScale = 1.25f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();
    }

    public void Play(
        Vector3 startLocalPosition,
        Vector3 targetLocalPosition,
        float delay,
        Action onArrived
    )
    {
        StartCoroutine(
            Animate(
                startLocalPosition,
                targetLocalPosition,
                delay,
                onArrived
            )
        );
    }

    private IEnumerator Animate(
        Vector3 startPosition,
        Vector3 targetPosition,
        float delay,
        Action onArrived
    )
    {
        rectTransform.localPosition =
            startPosition;

        rectTransform.localScale =
            Vector3.one * startScale;

        canvasGroup.alpha = 1f;

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                delay
            );
        }

        Vector3 controlPoint =
            (startPosition + targetPosition) * 0.5f +
            Vector3.up * arcHeight;

        float elapsed = 0f;

        while (elapsed < flightDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / flightDuration
            );

            float smoothProgress =
                SmoothStep(progress);

            rectTransform.localPosition =
                CalculateBezierPoint(
                    startPosition,
                    controlPoint,
                    targetPosition,
                    smoothProgress
                );

            AnimateScaleAndAlpha(progress);

            yield return null;
        }

        rectTransform.localPosition =
            targetPosition;

        onArrived?.Invoke();

        Destroy(gameObject);
    }

    private void AnimateScaleAndAlpha(
        float progress
    )
    {
        const float pulseStart = 0.8f;

        if (progress < pulseStart)
        {
            float scaleProgress =
                progress / pulseStart;

            float scale = Mathf.Lerp(
                startScale,
                1f,
                SmoothStep(scaleProgress)
            );

            rectTransform.localScale =
                Vector3.one * scale;

            canvasGroup.alpha = 1f;

            return;
        }

        float arrivalProgress =
            (progress - pulseStart) /
            (1f - pulseStart);

        float pulseScale = Mathf.Lerp(
            1f,
            arrivalScale,
            Mathf.Sin(
                arrivalProgress * Mathf.PI
            )
        );

        rectTransform.localScale =
            Vector3.one *
            pulseScale *
            (1f - arrivalProgress);

        canvasGroup.alpha =
            1f - arrivalProgress;
    }

    private static Vector3 CalculateBezierPoint(
        Vector3 start,
        Vector3 control,
        Vector3 end,
        float progress
    )
    {
        float inverse =
            1f - progress;

        return
            inverse * inverse * start +
            2f * inverse * progress * control +
            progress * progress * end;
    }

    private static float SmoothStep(float value)
    {
        value = Mathf.Clamp01(value);

        return value * value *
               (3f - 2f * value);
    }
}
