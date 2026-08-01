using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    CreateBallLevel,
    ReachCombo,
    MergeCount,
    EarnScore,
    CreateSeveralBallsOfLevel
}

public sealed class QuestDefinition
{
    public QuestType Type { get; }
    public int Parameter { get; }
    public int TargetProgress { get; }

    public QuestDefinition(
        QuestType type,
        int parameter,
        int targetProgress
    )
    {
        Type = type;
        Parameter = parameter;
        TargetProgress = targetProgress;
    }

    public string GetTitle()
    {
        return Type switch
        {
            QuestType.CreateBallLevel =>
                $"Создайте монету {Parameter}",

            QuestType.ReachCombo =>
                $"Сделайте комбо x{Parameter}",

            QuestType.MergeCount =>
                $"Выполните {Parameter} слияний: ",

            QuestType.EarnScore =>
                $"Наберите {Parameter} очков",

            QuestType.CreateSeveralBallsOfLevel =>
                $"Создайте три монеты {Parameter}: ",

            _ => "Неизвестная цель"
        };
    }
}

public sealed class QuestManager : MonoBehaviour
{
    private readonly List<QuestDefinition> questPool =
        new List<QuestDefinition>();

    private QuestDefinition currentQuest;

    private int currentQuestIndex = -1;
    private int currentProgress;

    public QuestDefinition CurrentQuest =>
        currentQuest;

    public int CurrentProgress =>
        currentProgress;

    /// <summary>
    /// Количество выполненных миссий
    /// в пределах текущей партии.
    /// При перезагрузке сцены сбрасывается.
    /// </summary>
    public int CompletedMissionsCount
    {
        get;
        private set;
    }

    public string CurrentDisplayText =>
        BuildDisplayText();

    public event Action<string> QuestTextChanged;

    public event Action<QuestDefinition> QuestCompleted;
    
    public event Action<int> CompletedMissionsCountChanged;
    
    [Header("Награда")]
    [SerializeField, Min(0)]
    private int questRewardCoins = 50;
    
    [SerializeField]
    private MenuPresent menuPresent;
    private QuestDefinition pendingCompletedQuest;
    private bool pendingRewardClaimed;

    public bool IsTransitioning { get; private set; }

    private int completedQuestIndex = -1;
    private bool nextQuestPrepared;
    
    [Header("Игровые системы")]
    [SerializeField]
    private GameManager gameManager;
    
    private void Awake()
    {
        BuildQuestPool();

        CompletedMissionsCount = 0;
        currentQuestIndex = -1;
        currentProgress = 0;
    }

    private void Start()
    {
        SelectNextQuest(
            excludedQuestIndex: -1
        );
    }

    private void BuildQuestPool()
    {
        questPool.Clear();

        // Создайте шар 5, 6 или 7.
        AddQuest(
            QuestType.CreateBallLevel,
            parameter: 5,
            targetProgress: 1
        );

        AddQuest(
            QuestType.CreateBallLevel,
            parameter: 6,
            targetProgress: 1
        );

        AddQuest(
            QuestType.CreateBallLevel,
            parameter: 7,
            targetProgress: 1
        );

        // Сделайте комбо x2, x3 или x4.
        AddQuest(
            QuestType.ReachCombo,
            parameter: 2,
            targetProgress: 1
        );

        AddQuest(
            QuestType.ReachCombo,
            parameter: 3,
            targetProgress: 1
        );

        AddQuest(
            QuestType.ReachCombo,
            parameter: 4,
            targetProgress: 1
        );

        // Выполните 10, 15 или 20 слияний.
        AddQuest(
            QuestType.MergeCount,
            parameter: 10,
            targetProgress: 10
        );

        AddQuest(
            QuestType.MergeCount,
            parameter: 15,
            targetProgress: 15
        );

        AddQuest(
            QuestType.MergeCount,
            parameter: 20,
            targetProgress: 20
        );

        // Наберите 100, 150 или 200 очков.
        AddQuest(
            QuestType.EarnScore,
            parameter: 100,
            targetProgress: 100
        );

        AddQuest(
            QuestType.EarnScore,
            parameter: 150,
            targetProgress: 150
        );

        AddQuest(
            QuestType.EarnScore,
            parameter: 200,
            targetProgress: 200
        );

        // Создайте три шара уровня 3, 4 или 5.
        AddQuest(
            QuestType.CreateSeveralBallsOfLevel,
            parameter: 3,
            targetProgress: 3
        );

        AddQuest(
            QuestType.CreateSeveralBallsOfLevel,
            parameter: 4,
            targetProgress: 3
        );

        AddQuest(
            QuestType.CreateSeveralBallsOfLevel,
            parameter: 5,
            targetProgress: 3
        );

        if (questPool.Count != 15)
        {
            Debug.LogError(
                $"Должно быть 15 миссий, " +
                $"но сейчас создано {questPool.Count}.",
                this
            );
        }
    }

    private void AddQuest(
        QuestType type,
        int parameter,
        int targetProgress
    )
    {
        questPool.Add(
            new QuestDefinition(
                type,
                parameter,
                targetProgress
            )
        );
    }

    /// <summary>
    /// Вызывается ровно один раз после
    /// каждого успешного слияния.
    /// </summary>
    public void ReportSuccessfulMerge(
        Ball createdBall,
        int currentCombo,
        int earnedScore
    )
    {
        if (IsTransitioning)
        {
            return;
        }
        
        if (currentQuest == null ||
            createdBall == null)
        {
            return;
        }

        /*
         * В Ball уровень начинается с нуля:
         *
         * Level 0 = игровой уровень 1
         * Level 1 = игровой уровень 2
         * Level 2 = игровой уровень 3
         */
        int createdLevelNumber =
            createdBall.Level + 1;

        switch (currentQuest.Type)
        {
            case QuestType.CreateBallLevel:
                ReportCreatedBall(
                    createdLevelNumber
                );
                break;

            case QuestType.ReachCombo:
                ReportCombo(
                    currentCombo
                );
                break;

            case QuestType.MergeCount:
                currentProgress++;
                break;

            case QuestType.EarnScore:
                /*
                 * Считаем очки, заработанные после
                 * появления текущей миссии.
                 */
                currentProgress +=
                    Mathf.Max(0, earnedScore);
                break;

            case QuestType.CreateSeveralBallsOfLevel:
                ReportSeveralBalls(
                    createdLevelNumber
                );
                break;
        }

        currentProgress = Mathf.Clamp(
            currentProgress,
            0,
            currentQuest.TargetProgress
        );

        NotifyTextChanged();

        if (currentProgress >=
            currentQuest.TargetProgress)
        {
            CompleteCurrentQuest();
            return;
        }
    }

    private void ReportCreatedBall(
        int createdLevelNumber
    )
    {
        if (createdLevelNumber ==
            currentQuest.Parameter)
        {
            currentProgress = 1;
        }
    }

    private void ReportCombo(
        int currentCombo
    )
    {
        /*
         * Комбо x4 выполняет также задания
         * на x2 и x3.
         */
        if (currentCombo >=
            currentQuest.Parameter)
        {
            currentProgress = 1;
        }
    }

    private void ReportSeveralBalls(
        int createdLevelNumber
    )
    {
        if (createdLevelNumber ==
            currentQuest.Parameter)
        {
            currentProgress++;
        }
    }

    private void CompleteCurrentQuest()
    {
        if (IsTransitioning ||
            currentQuest == null)
        {
            return;
        }

        IsTransitioning = true;
        nextQuestPrepared = false;
        pendingRewardClaimed = false;
        
        if (gameManager != null)
        {
            gameManager.SetGameplayPaused(true);
        }

        QuestDefinition completedQuest =
            currentQuest;

        pendingCompletedQuest =
            completedQuest;

        completedQuestIndex =
            currentQuestIndex;

        CompletedMissionsCount++;

        CompletedMissionsCountChanged?.Invoke(
            CompletedMissionsCount
        );

        if (menuPresent != null)
        {
            menuPresent.Show(
                questRewardCoins,
                ClaimCompletedQuestReward
            );
        }
        else
        {
            Debug.LogError(
                "Не назначен MenuPresent в QuestManager. " +
                "Награда будет выдана без окна подарка.",
                this
            );

            ClaimCompletedQuestReward();
        }

        /*
         * Здесь больше не выдаём награду
         * и не вызываем QuestCompleted.
         *
         * QuestCompleted будет вызван только после
         * нажатия кнопки Получить.
         */
    }
    
    private void ClaimCompletedQuestReward()
    {
        if (!IsTransitioning ||
            pendingCompletedQuest == null ||
            pendingRewardClaimed)
        {
            return;
        }

        pendingRewardClaimed = true;
        
        if (gameManager != null)
        {
            gameManager.SetGameplayPaused(false);
        }

        if (SaveGame.Instance != null)
        {
            SaveGame.Instance.PlusCoin(
                questRewardCoins
            );
        }
        else
        {
            Debug.LogError(
                "Не найден SaveGame. Награда за миссию не начислена.",
                this
            );
        }

        QuestDefinition completedQuest =
            pendingCompletedQuest;

        pendingCompletedQuest = null;

        QuestCompleted?.Invoke(
            completedQuest
        );

        /*
         * Здесь НЕ сбрасываем IsTransitioning.
         *
         * Его должен сбросить старый механизм анимации
         * через FinishQuestTransition().
         */
    }
    
    public void ShowNextQuestDuringTransition()
    {
        if (!IsTransitioning ||
            nextQuestPrepared)
        {
            return;
        }

        nextQuestPrepared = true;

        SelectNextQuest(
            completedQuestIndex
        );

        /*
         * SelectNextQuest обнулит прогресс
         * и вызовет QuestTextChanged,
         * но ReportSuccessfulMerge всё ещё
         * заблокирован через IsTransitioning.
         */
    }

    public void FinishQuestTransition()
    {
        if (!IsTransitioning)
        {
            return;
        }

        IsTransitioning = false;
        nextQuestPrepared = false;
        completedQuestIndex = -1;
    }

    private void SelectNextQuest(
        int excludedQuestIndex
    )
    {
        if (questPool.Count == 0)
        {
            Debug.LogError(
                "Список миссий пуст.",
                this
            );

            currentQuest = null;
            currentQuestIndex = -1;
            currentProgress = 0;

            NotifyTextChanged();
            return;
        }

        currentQuestIndex =
            GetRandomQuestIndexExcept(
                excludedQuestIndex
            );

        currentQuest =
            questPool[currentQuestIndex];

        currentProgress = 0;

        NotifyTextChanged();
    }

    private int GetRandomQuestIndexExcept(
        int excludedQuestIndex
    )
    {
        if (questPool.Count == 1)
        {
            return 0;
        }

        /*
         * Для первой миссии ничего
         * исключать не нужно.
         */
        if (excludedQuestIndex < 0 ||
            excludedQuestIndex >= questPool.Count)
        {
            return UnityEngine.Random.Range(
                0,
                questPool.Count
            );
        }

        /*
         * Выбираем число в диапазоне,
         * который на один элемент меньше.
         */
        int randomIndex =
            UnityEngine.Random.Range(
                0,
                questPool.Count - 1
            );

        /*
         * Пропускаем запрещённый индекс.
         *
         * Например, исключён индекс 4:
         *
         * 0 1 2 3 [4] 5 6...
         *
         * Все случайные индексы от 4 и выше
         * сдвигаются на одну позицию вправо.
         */
        if (randomIndex >=
            excludedQuestIndex)
        {
            randomIndex++;
        }

        return randomIndex;
    }

    private void NotifyTextChanged()
    {
        QuestTextChanged?.Invoke(
            BuildDisplayText()
        );
    }

    private string BuildDisplayText()
    {
        if (currentQuest == null)
        {
            return "Нет цели";
        }

        string title =
            currentQuest.GetTitle();

        switch (currentQuest.Type)
        {
            /*
             * Для этих миссий промежуточного
             * прогресса фактически нет.
             */
            case QuestType.CreateBallLevel:
            case QuestType.ReachCombo:
                return title;

            /*
             * Для остальных показываем прогресс
             * на второй строке.
             */
            case QuestType.MergeCount:
            case QuestType.EarnScore:
            case QuestType.CreateSeveralBallsOfLevel:
                return
                    $"{title}\n" +
                    $"{currentProgress}/" +
                    $"{currentQuest.TargetProgress}";

            default:
                return title;
        }
    }
}
