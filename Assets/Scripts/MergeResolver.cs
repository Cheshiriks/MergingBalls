using UnityEngine;

public sealed class MergeResolver : MonoBehaviour
{
    [SerializeField]
    private BallSpawner ballSpawner;
    
    [Header("Комбо")]
    [SerializeField]
    private ComboManager comboManager;
    
    [Header("Задания")]
    [SerializeField]
    private QuestManager questManager;

    public void TryMerge(
        Ball firstBall,
        Ball secondBall
    )
    {
        if (firstBall == null ||
            secondBall == null ||
            firstBall == secondBall)
        {
            return;
        }

        // Защита от повторного слияния.
        if (firstBall.IsMerging ||
            secondBall.IsMerging)
        {
            return;
        }

        // Разные уровни не объединяются.
        if (firstBall.Level != secondBall.Level)
        {
            return;
        }

        int nextLevel =
            firstBall.Level + 1;

        // Максимальный уровень больше не сливается.
        if (!ballSpawner.CanCreateLevel(nextLevel))
        {
            return;
        }

        SelectTargetAndSource(
            firstBall,
            secondBall,
            out Ball targetBall,
            out Ball sourceBall
        );

        Vector2 resultPosition =
            targetBall.transform.position;

        float resultRotation =
            targetBall.transform.eulerAngles.z;

        // Результат получает усреднённую скорость.
        Vector2 resultVelocity =
            (
                targetBall.LinearVelocity +
                sourceBall.LinearVelocity
            ) * 0.5f;

        float resultAngularVelocity =
            (
                targetBall.AngularVelocity +
                sourceBall.AngularVelocity
            ) * 0.5f;

        // Результат считается продолжением старого шара.
        long resultSpawnOrder =
            targetBall.SpawnOrder;

        /*
         * Критически важный участок.
         *
         * Сначала блокируем оба шара, и только затем
         * создаём результат и удаляем исходные объекты.
         */
        targetBall.LockForMerge();
        sourceBall.LockForMerge();

        Ball resultBall =
            ballSpawner.SpawnMergedBall(
                nextLevel,
                resultPosition,
                resultRotation,
                resultSpawnOrder,
                resultVelocity,
                resultAngularVelocity
            );

        if (resultBall == null)
        {
            Debug.LogError(
                "Не удалось создать результат слияния.",
                this
            );
        }
        
        int currentCombo = 1;
        
        if (comboManager != null)
        {
            currentCombo = comboManager.RegisterMerge(
                resultBall.transform.position
            );
        }
        
        int earnedScore =
            resultBall.Value;
        
        if (SaveGame.Instance == null)
        {
            Debug.LogError(
                "Не найден объект SaveGame.",
                this
            );
        }
        else
        {
            SaveGame.Instance.AddScore(
                earnedScore
            );
        }
        
        if (questManager != null)
        {
            questManager.ReportSuccessfulMerge(
                resultBall,
                currentCombo,
                earnedScore
            );
        }
        else
        {
            Debug.LogWarning(
                "В MergeResolver не назначен QuestManager.",
                this
            );
        }

        Destroy(targetBall.gameObject);
        Destroy(sourceBall.gameObject);
    }

    private static void SelectTargetAndSource(
        Ball firstBall,
        Ball secondBall,
        out Ball targetBall,
        out Ball sourceBall
    )
    {
        if (firstBall.SpawnOrder <
            secondBall.SpawnOrder)
        {
            targetBall = firstBall;
            sourceBall = secondBall;
            return;
        }

        if (secondBall.SpawnOrder <
            firstBall.SpawnOrder)
        {
            targetBall = secondBall;
            sourceBall = firstBall;
            return;
        }

        // Запасной вариант на случай одинаковых SpawnOrder.
        if (firstBall.GetEntityId() <
            secondBall.GetEntityId())
        {
            targetBall = firstBall;
            sourceBall = secondBall;
        }
        else
        {
            targetBall = secondBall;
            sourceBall = firstBall;
        }
    }
}
