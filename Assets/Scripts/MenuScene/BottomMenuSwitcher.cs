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

        [NonSerialized] public CanvasGroup textCanvasGroup;
        [NonSerialized] public Coroutine animationCoroutine;
    }

    [Header("Tabs")]
    [SerializeField] private Tab leaderTab;
    [SerializeField] private Tab homeTab;
    [SerializeField] private Tab shopTab;

    [Header("Animation")]
    [SerializeField] private float activeIconSize = 80f;
    [SerializeField] private float inactiveIconSize = 66f;
    [SerializeField] private float activeIconPosY = 33f;
    [SerializeField] private float inactiveIconPosY = 20f;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Default")]
    [SerializeField] private MenuType defaultMenu = MenuType.Home;

    private Tab[] tabs;
    private MenuType currentMenu;

    private void Awake()
    {
        tabs = new[] { leaderTab, homeTab, shopTab };

        InitializeTab(leaderTab);
        InitializeTab(homeTab);
        InitializeTab(shopTab);

        leaderTab.button.onClick.AddListener(() => OpenMenu(MenuType.Leader));
        homeTab.button.onClick.AddListener(() => OpenMenu(MenuType.Home));
        shopTab.button.onClick.AddListener(() => OpenMenu(MenuType.Shop));
    }

    private void Start()
    {
        SetStateImmediate(defaultMenu);
    }

    private void InitializeTab(Tab tab)
    {
        if (tab.textObject != null)
        {
            tab.textCanvasGroup = tab.textObject.GetComponent<CanvasGroup>();

            if (tab.textCanvasGroup == null)
                tab.textCanvasGroup = tab.textObject.AddComponent<CanvasGroup>();
        }
    }

    public void OpenMenu(MenuType menuType)
    {
        if (menuType == currentMenu)
            return;
        
        PrepareHomeMenuBeforeSwitch(menuType);

        currentMenu = menuType;
        int activeIndex = (int)menuType;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = i == activeIndex;

            if (tabs[i].menuRoot != null)
                tabs[i].menuRoot.SetActive(isActive);

            StartTabAnimation(tabs[i], isActive);
        }
    }

    private void SetStateImmediate(MenuType menuType)
    {
        PrepareHomeMenuBeforeSwitch(menuType);
        
        currentMenu = menuType;
        int activeIndex = (int)menuType;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = i == activeIndex;
            ApplyTabStateImmediate(tabs[i], isActive);
        }
    }

    private void ApplyTabStateImmediate(Tab tab, bool isActive)
    {
        if (tab.menuRoot != null)
            tab.menuRoot.SetActive(isActive);

        if (tab.iconRect != null)
        {
            float targetSize = isActive ? activeIconSize : inactiveIconSize;
            float targetY = isActive ? activeIconPosY : inactiveIconPosY;

            tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize);
            tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize);

            Vector2 pos = tab.iconRect.anchoredPosition;
            pos.y = targetY;
            tab.iconRect.anchoredPosition = pos;
        }

        if (tab.textObject != null)
            tab.textObject.SetActive(isActive);

        if (tab.textCanvasGroup != null)
        {
            tab.textCanvasGroup.alpha = isActive ? 1f : 0f;
            tab.textCanvasGroup.interactable = false;
            tab.textCanvasGroup.blocksRaycasts = false;
        }
    }

    private void StartTabAnimation(Tab tab, bool isActive)
    {
        if (tab.animationCoroutine != null)
            StopCoroutine(tab.animationCoroutine);

        tab.animationCoroutine = StartCoroutine(AnimateTab(tab, isActive));
    }

    private IEnumerator AnimateTab(Tab tab, bool isActive)
    {
        if (tab.iconRect == null)
            yield break;

        float startWidth = tab.iconRect.rect.width;
        float startHeight = tab.iconRect.rect.height;
        float startY = tab.iconRect.anchoredPosition.y;

        float targetSize = isActive ? activeIconSize : inactiveIconSize;
        float targetY = isActive ? activeIconPosY : inactiveIconPosY;

        float startAlpha = 0f;
        float targetAlpha = isActive ? 1f : 0f;

        if (tab.textCanvasGroup != null)
        {
            if (isActive && tab.textObject != null && !tab.textObject.activeSelf)
                tab.textObject.SetActive(true);

            startAlpha = tab.textCanvasGroup.alpha;
        }

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            // Более мягкая кривая
            t = Mathf.SmoothStep(0f, 1f, t);

            float size = Mathf.Lerp(startWidth, targetSize, t);
            float posY = Mathf.Lerp(startY, targetY, t);

            tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(startHeight, targetSize, t));

            Vector2 pos = tab.iconRect.anchoredPosition;
            pos.y = posY;
            tab.iconRect.anchoredPosition = pos;

            if (tab.textCanvasGroup != null)
                tab.textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize);
        tab.iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize);

        Vector2 finalPos = tab.iconRect.anchoredPosition;
        finalPos.y = targetY;
        tab.iconRect.anchoredPosition = finalPos;

        if (tab.textCanvasGroup != null)
            tab.textCanvasGroup.alpha = targetAlpha;

        if (!isActive && tab.textObject != null)
            tab.textObject.SetActive(false);

        tab.animationCoroutine = null;
    }
    
    private void PrepareHomeMenuBeforeSwitch(MenuType targetMenu)
    {
        // Если переходим в HomeMenu, переносить Leaderboard
        // в паркинг не нужно.
        if (targetMenu == MenuType.Home)
            return;

        // Проверяем фактическое состояние объекта,
        // а не только currentMenu.
        if (homeTab.menuRoot == null ||
            !homeTab.menuRoot.activeSelf)
        {
            return;
        }

        // Сначала перемещаем Leaderboard в активный Parking,
        // и только после этого выключаем HomeMenu.
        //if (homeMenuController != null)
        //    homeMenuController.PrepareForMenuHide();
    }
}