using System.Collections;
using TMPro;
using UnityEngine;

public sealed class QuestTextView : MonoBehaviour
{
    [Header("Менеджер")]
    [SerializeField]
    private QuestManager questManager;

    [Header("Текущая миссия")]
    [SerializeField]
    private TMP_Text questText;

    [SerializeField]
    private RectTransform questTextTransform;

    [Header("Выполненные миссии")]
    [SerializeField]
    private TMP_Text completedMissionsText;

    [SerializeField]
    private string completedMissionsPrefix = "";

    [Header("Анимация миссии")]
    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color completedColor =
        new Color(0.25f, 1f, 0.25f, 1f);

    [SerializeField, Min(0.05f)]
    private float hideDuration = 0.35f;

    [SerializeField, Min(0.05f)]
    private float showDuration = 0.3f;

    [Header("Летящие монетки")]
    [SerializeField]
    private FlyingRewardCoin flyingCoinPrefab;

    [Tooltip(
        "Полноэкранный UI-слой внутри Canvas."
    )]
    [SerializeField]
    private RectTransform rewardFxLayer;

    [Tooltip(
        "Обычно сюда назначается RectTransform текста миссии."
    )]
    [SerializeField]
    private RectTransform rewardStartPoint;

    [Tooltip(
        "Сюда назначается UI-иконка монеты."
    )]
    [SerializeField]
    private RectTransform rewardTargetPoint;

    [SerializeField, Min(1)]
    private int flyingCoinsCount = 3;

    [SerializeField, Min(0f)]
    private float delayBetweenCoins = 0.12f;

    [Header("Пульсация иконки монеты")]
    [SerializeField]
    private RectTransform coinIconTransform;

    [SerializeField, Range(1f, 1.5f)]
    private float coinIconPulseScale = 1.15f;

    [SerializeField, Min(0.05f)]
    private float coinIconPulseDuration = 0.18f;

    private Vector3 questTextInitialScale;
    private Vector3 coinIconInitialScale;

    private Coroutine transitionCoroutine;
    private Coroutine coinIconPulseCoroutine;

    private int activeFlyingCoins;

    private void Awake()
    {
        if (questTextTransform == null &&
            questText != null)
        {
            questTextTransform =
                questText.rectTransform;
        }

        if (rewardStartPoint == null)
        {
            rewardStartPoint =
                questTextTransform;
        }

        if (questTextTransform != null)
        {
            questTextInitialScale =
                questTextTransform.localScale;
        }

        if (coinIconTransform != null)
        {
            coinIconInitialScale =
                coinIconTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (questManager == null)
        {
            Debug.LogError(
                "В QuestTextView не назначен QuestManager.",
                this
            );

            return;
        }

        questManager.QuestTextChanged +=
            HandleQuestTextChanged;

        questManager.CompletedMissionsCountChanged +=
            HandleCompletedMissionsCountChanged;

        questManager.QuestCompleted +=
            HandleQuestCompleted;

        RefreshAll();
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnDisable()
    {
        if (questManager == null)
        {
            return;
        }

        questManager.QuestTextChanged -=
            HandleQuestTextChanged;

        questManager.CompletedMissionsCountChanged -=
            HandleCompletedMissionsCountChanged;

        questManager.QuestCompleted -=
            HandleQuestCompleted;
    }

    private void RefreshAll()
    {
        if (questManager == null)
        {
            return;
        }

        SetQuestText(
            questManager.CurrentDisplayText
        );

        SetCompletedMissionsText(
            questManager.CompletedMissionsCount
        );
    }

    private void HandleQuestTextChanged(
        string newText
    )
    {
        SetQuestText(newText);
    }

    private void HandleCompletedMissionsCountChanged(
        int completedMissionsCount
    )
    {
        SetCompletedMissionsText(
            completedMissionsCount
        );
    }

    private void HandleQuestCompleted(
        QuestDefinition completedQuest
    )
    {
        if (transitionCoroutine != null)
        {
            return;
        }

        transitionCoroutine =
            StartCoroutine(
                PlayQuestTransition()
            );
    }

    private IEnumerator PlayQuestTransition()
    {
        if (questText == null ||
            questTextTransform == null)
        {
            questManager.ShowNextQuestDuringTransition();
            questManager.FinishQuestTransition();

            transitionCoroutine = null;
            yield break;
        }

        questText.color =
            completedColor;

        StartCoroutine(
            SpawnRewardCoins()
        );

        yield return AnimateScale(
            questTextTransform,
            questTextInitialScale,
            Vector3.zero,
            hideDuration
        );

        /*
         * Выбирается новая миссия.
         * QuestTextChanged автоматически заменит текст.
         * При этом прогресс всё ещё заблокирован.
         */
        questManager.ShowNextQuestDuringTransition();

        questText.color =
            normalColor;

        questTextTransform.localScale =
            Vector3.zero;

        yield return AnimateScale(
            questTextTransform,
            Vector3.zero,
            questTextInitialScale,
            showDuration
        );

        /*
         * Ждём прилёта всех трёх визуальных монеток.
         */
        while (activeFlyingCoins > 0)
        {
            yield return null;
        }

        questManager.FinishQuestTransition();

        transitionCoroutine = null;
    }

    private IEnumerator SpawnRewardCoins()
    {
        if (flyingCoinPrefab == null ||
            rewardFxLayer == null ||
            rewardStartPoint == null ||
            rewardTargetPoint == null)
        {
            yield break;
        }

        Vector3 startLocalPosition =
            rewardFxLayer.InverseTransformPoint(
                rewardStartPoint.position
            );

        Vector3 targetLocalPosition =
            rewardFxLayer.InverseTransformPoint(
                rewardTargetPoint.position
            );

        for (int i = 0;
             i < flyingCoinsCount;
             i++)
        {
            FlyingRewardCoin flyingCoin =
                Instantiate(
                    flyingCoinPrefab,
                    rewardFxLayer
                );

            activeFlyingCoins++;

            flyingCoin.Play(
                startLocalPosition,
                targetLocalPosition,
                0f,
                HandleFlyingCoinArrived
            );

            if (i <
                flyingCoinsCount - 1)
            {
                yield return
                    new WaitForSecondsRealtime(
                        delayBetweenCoins
                    );
            }
        }
    }

    private void HandleFlyingCoinArrived()
    {
        activeFlyingCoins =
            Mathf.Max(
                0,
                activeFlyingCoins - 1
            );

        RestartCoinIconPulse();
    }

    private void RestartCoinIconPulse()
    {
        if (coinIconTransform == null)
        {
            return;
        }

        if (coinIconPulseCoroutine != null)
        {
            StopCoroutine(
                coinIconPulseCoroutine
            );
        }

        coinIconTransform.localScale =
            coinIconInitialScale;

        coinIconPulseCoroutine =
            StartCoroutine(
                AnimateCoinIconPulse()
            );
    }

    private IEnumerator AnimateCoinIconPulse()
    {
        float elapsed = 0f;

        while (elapsed <
               coinIconPulseDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed /
                coinIconPulseDuration
            );

            float scaleMultiplier =
                1f +
                Mathf.Sin(progress * Mathf.PI) *
                (coinIconPulseScale - 1f);

            coinIconTransform.localScale =
                coinIconInitialScale *
                scaleMultiplier;

            yield return null;
        }

        coinIconTransform.localScale =
            coinIconInitialScale;

        coinIconPulseCoroutine = null;
    }

    private static IEnumerator AnimateScale(
        RectTransform target,
        Vector3 startScale,
        Vector3 endScale,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            float smoothProgress =
                progress * progress *
                (3f - 2f * progress);

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    smoothProgress
                );

            yield return null;
        }

        target.localScale =
            endScale;
    }

    private void SetQuestText(string value)
    {
        if (questText != null)
        {
            questText.text = value;
        }
    }

    private void SetCompletedMissionsText(
        int value
    )
    {
        if (completedMissionsText != null)
        {
            completedMissionsText.text =
                $"{completedMissionsPrefix}{value}";
        }
    }
}