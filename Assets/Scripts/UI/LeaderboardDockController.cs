using UnityEngine;

public sealed class LeaderboardDockController : MonoBehaviour
{
    [Header("Меню")]
    [SerializeField]
    private GameObject leaderMenu;

    [Header("Контейнеры")]
    [SerializeField]
    private RectTransform parking;

    [SerializeField]
    private RectTransform leaderboardAnchor;

    [Header("Leaderboard")]
    [SerializeField]
    private RectTransform leaderboardCarrier;

    [SerializeField]
    private CanvasGroup leaderboardCanvasGroup;

    private bool wasOpened;

    private void Awake()
    {
        if (leaderMenu == null ||
            parking == null ||
            leaderboardAnchor == null ||
            leaderboardCarrier == null ||
            leaderboardCanvasGroup == null)
        {
            Debug.LogError(
                "LeaderboardDockController настроен не полностью.",
                this
            );

            enabled = false;
            return;
        }

        /*
         * Изначально держим внешний лидерборд
         * выключенным.
         *
         * Это единственный раз, когда мы будем
         * использовать SetActive для Carrier.
         */
        leaderboardCarrier.gameObject.SetActive(false);

        leaderMenu.SetActive(false);
    }

    public void OpenLeaderboard()
    {
        leaderMenu.SetActive(true);

        MoveTo(
            leaderboardAnchor
        );

        if (!wasOpened)
        {
            wasOpened = true;

            leaderboardCarrier
                .gameObject
                .SetActive(true);
        }

        ShowLeaderboard();
    }

    public void CloseLeaderboard()
    {
        /*
         * Сначала визуально скрываем.
         */
        HideLeaderboard();

        /*
         * Затем переносим в Parking,
         * который всегда активен.
         */
        MoveTo(
            parking
        );

        /*
         * И только теперь можно выключить меню.
         */
        leaderMenu.SetActive(false);
    }

    private void MoveTo(
        RectTransform parent
    )
    {
        leaderboardCarrier.SetParent(
            parent,
            false
        );

        leaderboardCarrier.anchoredPosition =
            Vector2.zero;

        leaderboardCarrier.localRotation =
            Quaternion.identity;

        leaderboardCarrier.localScale =
            Vector3.one;
    }

    private void ShowLeaderboard()
    {
        leaderboardCanvasGroup.alpha = 1f;
        leaderboardCanvasGroup.interactable = true;
        leaderboardCanvasGroup.blocksRaycasts = true;
    }

    private void HideLeaderboard()
    {
        leaderboardCanvasGroup.alpha = 0f;
        leaderboardCanvasGroup.interactable = false;
        leaderboardCanvasGroup.blocksRaycasts = false;
    }
}
