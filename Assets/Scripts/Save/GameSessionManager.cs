using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameSessionManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    private BallSpawner ballSpawner;

    [SerializeField]
    private Transform ballsParent;

    [SerializeField]
    private QuestManager questManager;

    [Header("Автосохранение")]
    [SerializeField, Min(0.5f)]
    private float saveInterval = 2f;

    private Coroutine autosaveCoroutine;

    private bool isInitialized;
    private bool sessionEnded;

    private void Start()
    {
        InitializeSession();

        autosaveCoroutine =
            StartCoroutine(
                AutosaveLoop()
            );
    }

    private void InitializeSession()
    {
        if (SaveGame.Instance == null)
        {
            Debug.LogError(
                "GameSessionManager: не найден SaveGame.",
                this
            );

            return;
        }

        if (ballSpawner == null)
        {
            Debug.LogError(
                "GameSessionManager: не назначен BallSpawner.",
                this
            );

            return;
        }

        if (questManager == null)
        {
            Debug.LogError(
                "GameSessionManager: не назначен QuestManager.",
                this
            );

            return;
        }

        if (SaveGame.Instance.HasGameSession)
        {
            RestoreSession();
        }
        else
        {
            StartNewSession();
        }

        /*
         * Только после восстановления игрового поля
         * создаём управляемый верхний шар.
         */
        ballSpawner.StartSpawning();

        isInitialized = true;
    }

    private void StartNewSession()
    {
        SaveGame.Instance.NewGame();

        questManager.RestoreCompletedMissionsCount(
            0
        );
    }

    private void RestoreSession()
    {
        GameSessionSave session =
            SaveGame.Instance.GameSession;

        if (session == null)
        {
            StartNewSession();
            return;
        }

        /*
         * 1. Восстанавливаем очки.
         */
        SaveGame.Instance.RestoreScore(
            session.score
        );

        /*
         * 2. Восстанавливаем количество
         * выполненных заданий.
         */
        questManager.RestoreCompletedMissionsCount(
            session.completedQuests
        );

        /*
         * 3. Восстанавливаем поле.
         */
        RestoreBalls(
            session.balls
        );
    }

    private void RestoreBalls(
        SavedBallData[] savedBalls
    )
    {
        if (savedBalls == null)
        {
            return;
        }

        foreach (SavedBallData savedBall
                 in savedBalls)
        {
            if (savedBall == null)
            {
                continue;
            }

            Vector2 position =
                new Vector2(
                    savedBall.positionX,
                    savedBall.positionY
                );

            ballSpawner.SpawnRestoredBall(
                savedBall.level,
                position
            );
        }
    }

    private IEnumerator AutosaveLoop()
    {
        while (!sessionEnded)
        {
            /*
             * Используем realtime, чтобы сохранение
             * происходило даже при открытых настройках,
             * где Time.timeScale == 0.
             */
            yield return
                new WaitForSecondsRealtime(
                    saveInterval
                );

            SaveCurrentSession();
        }
    }

    public void SaveCurrentSession()
    {
        if (!isInitialized ||
            sessionEnded ||
            SaveGame.Instance == null)
        {
            return;
        }

        SavedBallData[] balls =
            CaptureBalls();

        SaveGame.Instance.SaveGameSession(
            SaveGame.Instance.Score,
            questManager.CompletedMissionsCount,
            balls
        );
    }

    private SavedBallData[] CaptureBalls()
    {
        if (ballsParent == null)
        {
            return new SavedBallData[0];
        }

        Ball[] balls =
            ballsParent.GetComponentsInChildren<Ball>(
                false
            );

        List<SavedBallData> result =
            new List<SavedBallData>();

        foreach (Ball ball in balls)
        {
            if (ball == null)
            {
                continue;
            }

            /*
             * Верхний шар в режиме прицеливания
             * не сохраняем.
             */
            if (!ball.IsReleased)
            {
                continue;
            }

            /*
             * Шары, находящиеся прямо сейчас
             * в процессе удаления при merge,
             * тоже не сохраняем.
             */
            if (ball.IsMerging)
            {
                continue;
            }

            Vector3 position =
                ball.transform.position;

            result.Add(
                new SavedBallData
                {
                    level =
                        ball.Level,

                    positionX =
                        position.x,

                    positionY =
                        position.y
                }
            );
        }

        return result.ToArray();
    }

    public void ClearSession()
    {
        if (sessionEnded)
        {
            return;
        }

        // С этого момента никакое сохранение
        // текущей партии больше невозможно.
        sessionEnded = true;

        if (autosaveCoroutine != null)
        {
            StopCoroutine(autosaveCoroutine);
            autosaveCoroutine = null;
        }

        if (SaveGame.Instance != null)
        {
            SaveGame.Instance.ClearGameSession();
        }
    }

    private void OnApplicationPause(
        bool pauseStatus
    )
    {
        if (pauseStatus)
        {
            SaveCurrentSession();
        }
    }

    private void OnApplicationFocus(
        bool hasFocus
    )
    {
        if (!hasFocus)
        {
            SaveCurrentSession();
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentSession();
    }
}
