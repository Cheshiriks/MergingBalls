using UnityEngine;

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

        SaveGame.Instance.NewGame();
    }

    public void SetGameplayPaused(
        bool isPaused
    )
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameplayPaused = isPaused;
    }

    public void LoseGame(Ball overflowBall)
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        IsGameplayPaused = false;

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
}
