using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    [Header("Игровые системы")]
    [SerializeField]
    private BallSpawner ballSpawner;

    [SerializeField]
    private Transform ballsParent;

    [Header("Интерфейс")]
    [SerializeField]
    private GameObject gameOverPanel;
    
    [SerializeField]
    private GameSessionManager gameSessionManager;

    private float _timeScaleBeforePause = 1f;
    
    public bool IsGameOver { get; private set; }

    public bool IsGameplayPaused { get; private set; }

    public bool CanUseGameplayInput =>
        !IsGameOver && !IsGameplayPaused;

    private void Awake()
    {
        IsGameOver = false;
        IsGameplayPaused = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (SaveGame.Instance == null)
        {
            Debug.LogError(
                "Не найден объект SaveGame.",
                this
            );

            return;
        }

        //SaveGame.Instance.NewGame();
    }
    
    private void OnDestroy()
    {
        if (IsGameplayPaused)
        {
            Time.timeScale =
                _timeScaleBeforePause > 0f
                    ? _timeScaleBeforePause
                    : 1f;
        }
    }

    public void SetGameplayPaused(bool isPaused)
    {
        if (IsGameOver ||
            IsGameplayPaused == isPaused)
        {
            return;
        }

        if (isPaused)
        {
            _timeScaleBeforePause =
                Time.timeScale > 0f
                    ? Time.timeScale
                    : 1f;

            IsGameplayPaused = true;
            Time.timeScale = 0f;
        }
        else
        {
            IsGameplayPaused = false;

            Time.timeScale =
                _timeScaleBeforePause > 0f
                    ? _timeScaleBeforePause
                    : 1f;
        }
    }

    public void LoseGame(Ball overflowBall)
    {
        if (IsGameOver)
        {
            return;
        }

        if (IsGameplayPaused)
        {
            SetGameplayPaused(false);
        }

        IsGameOver = true;

        // Сначала завершаем и удаляем сохранение партии.
        if (gameSessionManager != null)
        {
            gameSessionManager.ClearSession();
        }
        else
        {
            Debug.LogError(
                "В GameManager не назначен GameSessionManager.",
                this
            );
        }
        
        if (ballSpawner != null)
        {
            ballSpawner.StopGame();
        }

        FreezeAllBalls();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        string ballName = overflowBall != null
            ? overflowBall.name
            : "неизвестный шар";

        Debug.Log(
            $"Игра окончена. Переполнение вызвал {ballName}.",
            this
        );
    }

    private void FreezeAllBalls()
    {
        if (ballsParent == null)
        {
            return;
        }

        Ball[] balls =
            ballsParent.GetComponentsInChildren<Ball>(
                true
            );

        foreach (Ball ball in balls)
        {
            if (ball != null)
            {
                ball.FreezePhysics();
            }
        }
    }
    
    public void StartNewGame()
    {
        /*
         * Если игра была на паузе из-за SettingsMenu, возвращаем timeScale.
         */
        if (IsGameplayPaused)
        {
            SetGameplayPaused(false);
        }
        
        if (gameSessionManager != null)
        {
            gameSessionManager.ClearSession();
        }
        else
        {
            Debug.LogError(
                "В GameManager не назначен GameSessionManager.",
                this
            );

            return;
        }
        
        SceneManager.LoadScene(1);
    }
}
