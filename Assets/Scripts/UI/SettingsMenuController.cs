using UnityEngine;

public sealed class SettingsMenuController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private GameObject settingsMenu;

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
}
