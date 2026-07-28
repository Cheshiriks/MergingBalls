using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BallDefinition
{
    [SerializeField, Min(1)]
    private int value = 2;

    [SerializeField]
    private Sprite sprite;

    //[SerializeField]
    //private Color color = Color.white;

    [SerializeField, Min(0.1f)]
    private float scale = 1f;

    [SerializeField, Min(0.01f)]
    private float mass = 1f;

    [Header("Вероятность появления сверху")]

    [SerializeField, Range(0, 100)]
    private int spawnChancePercent;

    public int Value => value;
    public Sprite Sprite => sprite;
    //public Color Color => color;
    public float Scale => scale;
    public float Mass => mass;
    public int SpawnChancePercent => spawnChancePercent;
}

[CreateAssetMenu(
    fileName = "BallCatalog",
    menuName = "Merge Game/Ball Catalog"
)]
public sealed class BallCatalog : ScriptableObject
{
    public const int RequiredTotalChance = 100;

    [SerializeField]
    private List<BallDefinition> balls =
        new List<BallDefinition>();

    public int Count => balls.Count;

    public BallDefinition GetDefinition(int level)
    {
        if (level < 0 || level >= balls.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Уровень шара отсутствует в каталоге."
            );
        }

        BallDefinition definition = balls[level];

        if (definition == null)
        {
            throw new InvalidOperationException(
                $"Элемент каталога уровня {level} не заполнен."
            );
        }

        return definition;
    }

    public bool HasValidSpawnChances(
        out int totalChance
    )
    {
        totalChance = CalculateTotalSpawnChance();

        return totalChance == RequiredTotalChance;
    }

    public int GetRandomSpawnableLevel()
    {
        if (!HasValidSpawnChances(
                out int totalChance
            ))
        {
            Debug.LogError(
                "Невозможно выбрать случайный шар. " +
                $"Сумма вероятностей равна {totalChance}%, " +
                $"но должна быть {RequiredTotalChance}%.",
                this
            );

            return -1;
        }

        // Получаем целое число от 0 до 99.
        int randomValue =
            UnityEngine.Random.Range(0, 100);

        int accumulatedChance = 0;

        for (int level = 0;
             level < balls.Count;
             level++)
        {
            BallDefinition definition = balls[level];

            if (definition == null)
            {
                continue;
            }

            accumulatedChance +=
                definition.SpawnChancePercent;

            if (randomValue < accumulatedChance)
            {
                return level;
            }
        }

        Debug.LogError(
            "Не удалось выбрать уровень шара, " +
            "хотя сумма вероятностей равна 100%.",
            this
        );

        return -1;
    }

    private int CalculateTotalSpawnChance()
    {
        int totalChance = 0;

        for (int i = 0; i < balls.Count; i++)
        {
            BallDefinition definition = balls[i];

            if (definition != null)
            {
                totalChance +=
                    definition.SpawnChancePercent;
            }
        }

        return totalChance;
    }

    private void OnValidate()
    {
        if (balls == null || balls.Count == 0)
        {
            return;
        }

        int totalChance =
            CalculateTotalSpawnChance();

        if (totalChance != RequiredTotalChance)
        {
            Debug.LogWarning(
                $"В каталоге {name} сумма вероятностей " +
                $"равна {totalChance}%. " +
                $"Она должна быть равна " +
                $"{RequiredTotalChance}%.",
                this
            );
        }
    }
}