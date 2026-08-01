using System.Collections;
using TMPro;
using UnityEngine;

public sealed class CoinTextView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text coinText;

    [Tooltip(
        "Время, за которое число визуально увеличится " +
        "от старого значения до нового."
    )]
    [SerializeField, Min(0.05f)]
    private float countingDuration = 0.8f;

    [SerializeField]
    private string prefix = "";

    private SaveGame saveGame;
    private Coroutine countingCoroutine;

    private int displayedCoins;
    private int targetCoins;

    private void Awake()
    {
        if (coinText == null)
        {
            coinText =
                GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        SynchronizeImmediately();
    }

    private void Start()
    {
        if (saveGame == null)
        {
            TrySubscribe();
            SynchronizeImmediately();
        }
    }

    private void OnDisable()
    {
        if (saveGame != null)
        {
            saveGame.CoinsChanged -=
                HandleCoinsChanged;

            saveGame = null;
        }

        if (countingCoroutine != null)
        {
            StopCoroutine(
                countingCoroutine
            );

            countingCoroutine = null;
        }
    }

    private void TrySubscribe()
    {
        if (saveGame != null)
        {
            return;
        }

        saveGame =
            SaveGame.Instance;

        if (saveGame != null)
        {
            saveGame.CoinsChanged +=
                HandleCoinsChanged;
        }
    }

    private void SynchronizeImmediately()
    {
        displayedCoins =
            saveGame != null
                ? saveGame.Coins
                : 0;

        targetCoins =
            displayedCoins;

        SetText(displayedCoins);
    }

    private void HandleCoinsChanged(
        int previousCoins,
        int newCoins
    )
    {
        targetCoins =
            Mathf.Max(0, newCoins);

        if (targetCoins <=
            displayedCoins)
        {
            StopCounting();

            displayedCoins =
                targetCoins;

            SetText(displayedCoins);
            return;
        }

        StopCounting();

        countingCoroutine =
            StartCoroutine(
                CountToTarget(
                    displayedCoins,
                    targetCoins
                )
            );
    }

    private IEnumerator CountToTarget(
        int startValue,
        int endValue
    )
    {
        int amount =
            endValue - startValue;

        if (amount <= 0)
        {
            countingCoroutine = null;
            yield break;
        }

        float animationStartTime =
            Time.unscaledTime;

        for (int step = 1;
             step <= amount;
             step++)
        {
            float scheduledTime =
                animationStartTime +
                countingDuration *
                ((float)step / amount);

            while (Time.unscaledTime <
                   scheduledTime)
            {
                yield return null;
            }

            displayedCoins =
                startValue + step;

            SetText(displayedCoins);
        }

        displayedCoins =
            endValue;

        SetText(displayedCoins);

        countingCoroutine = null;
    }

    private void StopCounting()
    {
        if (countingCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            countingCoroutine
        );

        countingCoroutine = null;
    }

    private void SetText(int value)
    {
        if (coinText != null)
        {
            coinText.text =
                $"{prefix}{value}";
        }
    }
}
