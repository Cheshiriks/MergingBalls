using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BottomMenuSwitcher : MonoBehaviour
{
    public enum MenuType
    {
        Leader = 0,
        Home = 1,
        Shop = 2
    }

    [Serializable]
    public class Tab
    {
        [Header("Menu")]
        public GameObject menuRoot;

        [Header("Button")]
        public Button button;
        public RectTransform iconRect;
        public GameObject textObject;

        [NonSerialized]
        public CanvasGroup textCanvasGroup;

        [NonSerialized]
        public Coroutine animationCoroutine;
    }

    [Header("Tabs")]
    [SerializeField]
    private Tab leaderTab;

    [SerializeField]
    private Tab homeTab;

    [SerializeField]
    private Tab shopTab;

    [Header("Animation")]
    [SerializeField]
    private float activeIconSize = 80f;

    [SerializeField]
    private float inactiveIconSize = 66f;

    [SerializeField]
    private float activeIconPosY = 33f;

    [SerializeField]
    private float inactiveIconPosY = 20f;

    [SerializeField]
    private float animationDuration = 0.2f;

    [Header("Default")]
    [SerializeField]
    private MenuType defaultMenu = MenuType.Home;

    [Header("Leaderboard Parking")]

    [Tooltip(
        "Постоянно активный объект внутри Canvas, " +
        "где Leaderboard хранится, когда LeaderMenu закрыт."
    )]
    [SerializeField]
    private RectTransform leaderboardParking;

    [Tooltip(
        "Пустой объект внутри LeaderMenu, " +
        "куда Leaderboard переносится при открытии."
    )]
    [SerializeField]
    private RectTransform leaderboardAnchor;

    [Tooltip(
        "Наша обёртка над внешним префабом Leaderboard."
    )]
    [SerializeField]
    private RectTransform leaderboardCarrier;

    [Tooltip(
        "CanvasGroup на LeaderboardCarrier."
    )]
    [SerializeField]
    private CanvasGroup leaderboardCanvasGroup;

    private Tab[] tabs;

    private MenuType currentMenu;

    /*
     * false:
     * внешний Leaderboard ещё ни разу
     * не был активирован.
     *
     * true:
     * он был активирован один раз и
     * больше никогда не выключается.
     */
    private bool leaderboardWasOpened;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        tabs = new[]
        {
            leaderTab,
            homeTab,
            shopTab
        };

        InitializeTab(leaderTab);
        InitializeTab(homeTab);
        InitializeTab(shopTab);

        if (leaderTab.button != null)
        {
            leaderTab.button.onClick.AddListener(
                () => OpenMenu(MenuType.Leader)
            );
        }

        if (homeTab.button != null)
        {
            homeTab.button.onClick.AddListener(
                () => OpenMenu(MenuType.Home)
            );
        }

        if (shopTab.button != null)
        {
            shopTab.button.onClick.AddListener(
                () => OpenMenu(MenuType.Shop)
            );
        }

        InitializeLeaderboard();
    }

    private void Start()
    {
        SetStateImmediate(
            defaultMenu
        );
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void InitializeTab(Tab tab)
    {
        if (tab == null ||
            tab.textObject == null)
        {
            return;
        }

        tab.textCanvasGroup =
            tab.textObject.GetComponent<CanvasGroup>();

        if (tab.textCanvasGroup == null)
        {
            tab.textCanvasGroup =
                tab.textObject.AddComponent<CanvasGroup>();
        }
    }

    private void InitializeLeaderboard()
    {
        if (leaderboardCarrier == null)
        {
            Debug.LogError(
                "В BottomMenuSwitcher не назначен " +
                "Leaderboard Carrier.",
                this
            );

            return;
        }

        if (leaderboardParking == null)
        {
            Debug.LogError(
                "В BottomMenuSwitcher не назначен " +
                "Leaderboard Parking.",
                this
            );

            return;
        }

        if (leaderboardAnchor == null)
        {
            Debug.LogError(
                "В BottomMenuSwitcher не назначен " +
                "Leaderboard Anchor.",
                this
            );

            return;
        }

        if (leaderboardCanvasGroup == null)
        {
            leaderboardCanvasGroup =
                leaderboardCarrier
                    .GetComponent<CanvasGroup>();

            if (leaderboardCanvasGroup == null)
            {
                Debug.LogError(
                    "На LeaderboardCarrier отсутствует CanvasGroup.",
                    leaderboardCarrier
                );

                return;
            }
        }

        /*
         * В начале держим Leaderboard
         * в Parking.
         */
        leaderboardCarrier.SetParent(
            leaderboardParking,
            false
        );

        ResetLeaderboardTransform();

        HideLeaderboard();

        /*
         * До первого открытия LeaderMenu
         * внешний Leaderboard не должен работать.
         *
         * После первого SetActive(true)
         * мы больше НИКОГДА его не выключаем.
         */
        leaderboardCarrier.gameObject.SetActive(
            false
        );

        leaderboardWasOpened = false;
    }


    // =========================================================
    // MENU SWITCHING
    // =========================================================

    public void OpenMenu(
        MenuType menuType
    )
    {
        if (menuType == currentMenu)
        {
            return;
        }

        /*
         * Очень важный порядок:
         *
         * если мы УХОДИМ из LeaderMenu,
         * сначала вытаскиваем активный Leaderboard
         * в Parking.
         *
         * Только после этого ниже будет выполнен
         * LeaderMenu.SetActive(false).
         */
        if (currentMenu == MenuType.Leader)
        {
            ParkLeaderboard();
        }

        currentMenu = menuType;

        int activeIndex =
            (int)menuType;

        for (int i = 0;
             i < tabs.Length;
             i++)
        {
            bool isActive =
                i == activeIndex;

            Tab tab =
                tabs[i];

            if (tab.menuRoot != null)
            {
                tab.menuRoot.SetActive(
                    isActive
                );
            }

            StartTabAnimation(
                tab,
                isActive
            );
        }

        /*
         * LeaderMenu теперь уже активен.
         *
         * Поэтому только сейчас переносим
         * Leaderboard из Parking в Anchor.
         */
        if (menuType == MenuType.Leader)
        {
            DockLeaderboard();
        }
    }


    // =========================================================
    // IMMEDIATE STATE
    // =========================================================

    private void SetStateImmediate(
        MenuType menuType
    )
    {
        /*
         * На случай, если этот метод когда-нибудь
         * будет использоваться повторно во время игры.
         */
        if (currentMenu == MenuType.Leader &&
            leaderboardWasOpened)
        {
            ParkLeaderboard();
        }

        currentMenu = menuType;

        int activeIndex =
            (int)menuType;

        for (int i = 0;
             i < tabs.Length;
             i++)
        {
            bool isActive =
                i == activeIndex;

            ApplyTabStateImmediate(
                tabs[i],
                isActive
            );
        }

        /*
         * Это также позволяет поставить
         *
         * Default Menu = Leader
         *
         * и всё будет работать.
         */
        if (menuType == MenuType.Leader)
        {
            DockLeaderboard();
        }
    }

    private void ApplyTabStateImmediate(
        Tab tab,
        bool isActive
    )
    {
        if (tab == null)
        {
            return;
        }

        if (tab.menuRoot != null)
        {
            tab.menuRoot.SetActive(
                isActive
            );
        }

        if (tab.iconRect != null)
        {
            float targetSize =
                isActive
                    ? activeIconSize
                    : inactiveIconSize;

            float targetY =
                isActive
                    ? activeIconPosY
                    : inactiveIconPosY;

            tab.iconRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                targetSize
            );

            tab.iconRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                targetSize
            );

            Vector2 pos =
                tab.iconRect.anchoredPosition;

            pos.y =
                targetY;

            tab.iconRect.anchoredPosition =
                pos;
        }

        if (tab.textObject != null)
        {
            tab.textObject.SetActive(
                isActive
            );
        }

        if (tab.textCanvasGroup != null)
        {
            tab.textCanvasGroup.alpha =
                isActive
                    ? 1f
                    : 0f;

            tab.textCanvasGroup.interactable =
                false;

            tab.textCanvasGroup.blocksRaycasts =
                false;
        }
    }


    // =========================================================
    // LEADERBOARD DOCKING
    // =========================================================

    private void DockLeaderboard()
    {
        if (leaderboardCarrier == null ||
            leaderboardAnchor == null)
        {
            return;
        }

        /*
         * На этом этапе LeaderMenu уже Active.
         */
        leaderboardCarrier.SetParent(
            leaderboardAnchor,
            false
        );

        ResetLeaderboardTransform();

        /*
         * Самая важная часть.
         *
         * SetActive(true) внешнего Leaderboard
         * выполняется только ОДИН раз
         * за время жизни этой сцены.
         */
        if (!leaderboardWasOpened)
        {
            leaderboardWasOpened = true;

            leaderboardCarrier
                .gameObject
                .SetActive(true);
        }

        ShowLeaderboard();
    }

    private void ParkLeaderboard()
    {
        if (!leaderboardWasOpened ||
            leaderboardCarrier == null ||
            leaderboardParking == null)
        {
            return;
        }

        /*
         * Сначала визуально скрываем Leaderboard.
         */
        HideLeaderboard();

        /*
         * Потом переносим в объект,
         * который всегда остаётся активным.
         */
        leaderboardCarrier.SetParent(
            leaderboardParking,
            false
        );

        ResetLeaderboardTransform();

        /*
         * КРИТИЧЕСКИ ВАЖНО:
         *
         * здесь НЕЛЬЗЯ делать:
         *
         * leaderboardCarrier.gameObject.SetActive(false);
         *
         * Он должен оставаться активным.
         */
    }

    private void ShowLeaderboard()
    {
        if (leaderboardCanvasGroup == null)
        {
            return;
        }

        leaderboardCanvasGroup.alpha =
            1f;

        leaderboardCanvasGroup.interactable =
            true;

        leaderboardCanvasGroup.blocksRaycasts =
            true;
    }

    private void HideLeaderboard()
    {
        if (leaderboardCanvasGroup == null)
        {
            return;
        }

        leaderboardCanvasGroup.alpha =
            0f;

        leaderboardCanvasGroup.interactable =
            false;

        leaderboardCanvasGroup.blocksRaycasts =
            false;
    }

    private void ResetLeaderboardTransform()
    {
        if (leaderboardCarrier == null)
        {
            return;
        }

        /*
         * Положение самого LeaderboardAnchor
         * определяет, где будет Leaderboard.
         */
        leaderboardCarrier.anchoredPosition =
            Vector2.zero;

        leaderboardCarrier.localRotation =
            Quaternion.identity;

        leaderboardCarrier.localScale =
            Vector3.one;
    }


    // =========================================================
    // TAB ANIMATION
    // =========================================================

    private void StartTabAnimation(
        Tab tab,
        bool isActive
    )
    {
        if (tab == null)
        {
            return;
        }

        if (tab.animationCoroutine != null)
        {
            StopCoroutine(
                tab.animationCoroutine
            );
        }

        tab.animationCoroutine =
            StartCoroutine(
                AnimateTab(
                    tab,
                    isActive
                )
            );
    }

    private IEnumerator AnimateTab(
        Tab tab,
        bool isActive
    )
    {
        if (tab.iconRect == null)
        {
            tab.animationCoroutine = null;
            yield break;
        }

        float startWidth =
            tab.iconRect.rect.width;

        float startHeight =
            tab.iconRect.rect.height;

        float startY =
            tab.iconRect
                .anchoredPosition
                .y;

        float targetSize =
            isActive
                ? activeIconSize
                : inactiveIconSize;

        float targetY =
            isActive
                ? activeIconPosY
                : inactiveIconPosY;

        float startAlpha = 0f;

        float targetAlpha =
            isActive
                ? 1f
                : 0f;

        if (tab.textCanvasGroup != null)
        {
            if (isActive &&
                tab.textObject != null &&
                !tab.textObject.activeSelf)
            {
                tab.textObject.SetActive(
                    true
                );
            }

            startAlpha =
                tab.textCanvasGroup.alpha;
        }

        float time = 0f;

        while (time < animationDuration)
        {
            time +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time /
                    animationDuration
                );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            float width =
                Mathf.Lerp(
                    startWidth,
                    targetSize,
                    t
                );

            float height =
                Mathf.Lerp(
                    startHeight,
                    targetSize,
                    t
                );

            float posY =
                Mathf.Lerp(
                    startY,
                    targetY,
                    t
                );

            tab.iconRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width
            );

            tab.iconRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                height
            );

            Vector2 pos =
                tab.iconRect.anchoredPosition;

            pos.y =
                posY;

            tab.iconRect.anchoredPosition =
                pos;

            if (tab.textCanvasGroup != null)
            {
                tab.textCanvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        targetAlpha,
                        t
                    );
            }

            yield return null;
        }

        tab.iconRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            targetSize
        );

        tab.iconRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            targetSize
        );

        Vector2 finalPos =
            tab.iconRect.anchoredPosition;

        finalPos.y =
            targetY;

        tab.iconRect.anchoredPosition =
            finalPos;

        if (tab.textCanvasGroup != null)
        {
            tab.textCanvasGroup.alpha =
                targetAlpha;
        }

        if (!isActive &&
            tab.textObject != null)
        {
            tab.textObject.SetActive(
                false
            );
        }

        tab.animationCoroutine = null;
    }
}