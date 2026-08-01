using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class ComboPopup : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField, Min(0.05f)]
    private float duration = 0.8f;

    [SerializeField, Min(0f)]
    private float riseDistance = 0.8f;

    [Header("Масштаб")]
    [SerializeField, Range(0.1f, 1f)]
    private float startScale = 0.65f;

    [SerializeField, Range(1f, 2f)]
    private float peakScale = 1.2f;

    [Tooltip(
        "В какой части анимации текст достигнет " +
        "максимального размера."
    )]
    [SerializeField, Range(0.05f, 0.9f)]
    private float peakTime = 0.2f;

    [Header("Затухание")]
    [Tooltip(
        "До этой части анимации текст остаётся " +
        "полностью непрозрачным."
    )]
    [SerializeField, Range(0f, 0.9f)]
    private float fadeStart = 0.35f;

    private TMP_Text comboText;
    private Vector3 initialScale;

    private void Awake()
    {
        comboText = GetComponent<TMP_Text>();
        initialScale = transform.localScale;
    }

    public void Play(int combo)
    {
        if (combo < 2)
        {
            Destroy(gameObject);
            return;
        }

        comboText.text = $"x{combo}";
        comboText.alpha = 1f;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 startPosition = transform.position;

        Vector3 endPosition =
            startPosition + Vector3.up * riseDistance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float smoothProgress =
                SmoothStep(progress);

            // Поднимаем текст вверх.
            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                smoothProgress
            );

            AnimateScale(progress);
            AnimateAlpha(progress);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void AnimateScale(float progress)
    {
        float scaleMultiplier;

        if (progress < peakTime)
        {
            float growProgress =
                progress / peakTime;

            scaleMultiplier = Mathf.Lerp(
                startScale,
                peakScale,
                SmoothStep(growProgress)
            );
        }
        else
        {
            float returnProgress =
                (progress - peakTime) /
                (1f - peakTime);

            scaleMultiplier = Mathf.Lerp(
                peakScale,
                1f,
                SmoothStep(returnProgress)
            );
        }

        transform.localScale =
            initialScale * scaleMultiplier;
    }

    private void AnimateAlpha(float progress)
    {
        if (progress <= fadeStart)
        {
            comboText.alpha = 1f;
            return;
        }

        float fadeProgress =
            (progress - fadeStart) /
            (1f - fadeStart);

        comboText.alpha =
            1f - Mathf.Clamp01(fadeProgress);
    }

    private static float SmoothStep(float value)
    {
        value = Mathf.Clamp01(value);

        return value * value *
               (3f - 2f * value);
    }
}
