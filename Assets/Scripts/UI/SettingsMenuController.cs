using UnityEngine;
using System.Collections;

public sealed class SettingsMenuController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private GameObject settingsMenu;
    
    [Header("First menu")]
    [SerializeField] private GameObject firstMenu;
    
    // задержка перед закрытием 
    private float _firstMenuCloseDelay = 0.5f;
    private bool _isClosingFirstMenu;
    private Coroutine _closeFirstMenuCoroutine;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError(
                "В SettingsMenuController не назначен GameManager.",
                this
            );
        }

        if (settingsMenu == null)
        {
            Debug.LogError(
                "В SettingsMenuController не назначено меню настроек.",
                this
            );

            enabled = false;
            return;
        }
        
        if (firstMenu != null)
            firstMenu.SetActive(false);
        
        TryShowFirstMenu();

        IsOpen = false;
        settingsMenu.SetActive(false);
    }

    public void OpenSettings()
    {
        if (IsOpen ||
            gameManager == null ||
            gameManager.IsGameOver)
        {
            return;
        }

        gameManager.SetGameplayPaused(true);

        IsOpen = true;
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (!IsOpen)
        {
            return;
        }

        settingsMenu.SetActive(false);
        IsOpen = false;

        if (gameManager != null)
        {
            gameManager.SetGameplayPaused(false);
        }
    }
    
    private void TryShowFirstMenu()
    {
        if (firstMenu == null)
            return;

        firstMenu.SetActive(false);

        if (SaveGame.Instance == null)
        {
            Debug.LogWarning("SaveGame.Instance is null. FirstMenu cannot check save data.");
            return;
        }

        bool hasSeenFirstMenu = !SaveGame.Instance.HasSeenFirstMenu();

        firstMenu.SetActive(hasSeenFirstMenu);
    }
    
    public void CloseFirstMenu()
    {
        if (_isClosingFirstMenu)
            return;

        _closeFirstMenuCoroutine = StartCoroutine(CloseFirstMenuCoroutine());
    }

    private IEnumerator CloseFirstMenuCoroutine()
    {
        _isClosingFirstMenu = true;
        
        if (SaveGame.Instance != null)
            SaveGame.Instance.SetFirstMenuSeen();

        yield return new WaitForSecondsRealtime(_firstMenuCloseDelay);

        if (firstMenu != null)
            firstMenu.SetActive(false);

        _isClosingFirstMenu = false;
        _closeFirstMenuCoroutine = null;
    }
}
