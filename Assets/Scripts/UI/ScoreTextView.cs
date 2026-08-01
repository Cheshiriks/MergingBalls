using System.Collections;
using TMPro;
using UnityEngine;

public sealed class ScoreTextView : MonoBehaviour
{
    [Header("Текст")]
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text maxScoreText;

    [SerializeField]
    private string prefix = "";

    [Header("Анимация счётчика")]

    [Tooltip("Время, за которое UI дойдёт до нового значения.")]
    [SerializeField, Min(0.01f)]
    private float countingDuration = 1f;

    [Header("Анимация увеличения текста")]

    [SerializeField]
    private RectTransform animatedTransform;

    [SerializeField, Range(1f, 1.5f)]
    private float punchScale = 1.12f;

    [SerializeField, Min(0.01f)]
    private float punchDuration = 0.14f;

    private SaveGame saveGame;

    private Coroutine countingCoroutine;
    private Coroutine punchCoroutine;

    private int displayedScore;
    private int targetScore;

    private Vector3 initialScale = Vector3.one;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }

        if (animatedTransform == null &&
            scoreText != null)
        {
            animatedTransform =
                scoreText.rectTransform;
        }

        if (animatedTransform != null)
        {
            initialScale =
                animatedTransform.localScale;
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        SynchronizeImmediately();
    }

    private void Start()
    {
        maxScoreText.text = SaveGame.Instance.MaxScore.ToString();
        
        if (saveGame == null)
        {
            TrySubscribe();
            SynchronizeImmediately();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopAnimations();
    }

    private void TrySubscribe()
    {
        if (saveGame != null)
        {
            return;
        }

        saveGame = SaveGame.Instance;

        if (saveGame == null)
        {
            return;
        }

        saveGame.ScoreChanged +=
            HandleScoreChanged;
    }

    private void Unsubscribe()
    {
        if (saveGame == null)
        {
            return;
        }

        saveGame.ScoreChanged -=
            HandleScoreChanged;

        saveGame = null;
    }

    private void SynchronizeImmediately()
    {
        int currentScore =
            saveGame != null
                ? saveGame.Score
                : 0;

        displayedScore = currentScore;
        targetScore = currentScore;

        SetScoreText(displayedScore);
    }

    private void HandleScoreChanged(int newScore)
    {
        targetScore = Mathf.Max(0, newScore);

        if (targetScore <= displayedScore)
        {
            StopCounting();

            displayedScore = targetScore;
            SetScoreText(displayedScore);

            return;
        }

        StopCounting();

        countingCoroutine = StartCoroutine(
            CountToTarget(
                displayedScore,
                targetScore
            )
        );

        RestartPunchAnimation();
    }

    private IEnumerator CountToTarget(
        int startScore,
        int endScore
    )
    {
        int pointsToAdd =
            endScore - startScore;

        if (pointsToAdd <= 0)
        {
            displayedScore = endScore;
            SetScoreText(displayedScore);

            countingCoroutine = null;
            yield break;
        }

        float animationStartTime =
            Time.unscaledTime;

        for (int step = 1;
             step <= pointsToAdd;
             step++)
        {
            float normalizedStep =
                (float)step / pointsToAdd;

            float scheduledTime =
                animationStartTime +
                countingDuration *
                normalizedStep;

            while (Time.unscaledTime <
                   scheduledTime)
            {
                yield return null;
            }

            displayedScore =
                startScore + step;

            SetScoreText(displayedScore);
        }

        displayedScore = endScore;
        targetScore = endScore;

        SetScoreText(displayedScore);

        countingCoroutine = null;
    }

    private void RestartPunchAnimation()
    {
        if (animatedTransform == null)
        {
            return;
        }

        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }

        animatedTransform.localScale =
            initialScale;

        punchCoroutine =
            StartCoroutine(AnimatePunch());
    }

    private IEnumerator AnimatePunch()
    {
        float elapsed = 0f;
        const float growPart = 0.35f;

        while (elapsed < punchDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / punchDuration
            );

            float currentScale;

            if (progress < growPart)
            {
                float growProgress =
                    progress / growPart;

                currentScale = Mathf.Lerp(
                    1f,
                    punchScale,
                    SmoothStep(growProgress)
                );
            }
            else
            {
                float returnProgress =
                    (progress - growPart) /
                    (1f - growPart);

                currentScale = Mathf.Lerp(
                    punchScale,
                    1f,
                    SmoothStep(returnProgress)
                );
            }

            animatedTransform.localScale =
                initialScale * currentScale;

            yield return null;
        }

        animatedTransform.localScale =
            initialScale;

        punchCoroutine = null;
    }

    private static float SmoothStep(float value)
    {
        value = Mathf.Clamp01(value);

        return value * value *
               (3f - 2f * value);
    }

    private void SetScoreText(int score)
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text =
            $"{prefix}{score}";
    }

    private void StopCounting()
    {
        if (countingCoroutine == null)
        {
            return;
        }

        StopCoroutine(countingCoroutine);
        countingCoroutine = null;
    }

    private void StopAnimations()
    {
        StopCounting();

        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }

        if (animatedTransform != null)
        {
            animatedTransform.localScale =
                initialScale;
        }
    }
}