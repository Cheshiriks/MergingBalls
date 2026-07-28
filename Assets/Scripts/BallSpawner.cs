using System.Collections;
using UnityEngine;

public sealed class BallSpawner : MonoBehaviour
{
    private const int FirstBallLevel = 0;

    [Header("Основные ссылки")]
    [SerializeField]
    private Ball ballPrefab;

    [SerializeField]
    private BallCatalog catalog;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private Transform ballsParent;

    [SerializeField]
    private PlayAreaBounds playAreaBounds;

    [SerializeField]
    private MergeResolver mergeResolver;

    [Header("Создание следующего шара")]
    [SerializeField, Min(0f)]
    private float spawnDelay = 0.5f;

    private DropBallController currentController;
    private Coroutine spawnCoroutine;

    private bool _isFirstBall = true;
    private long _nextSpawnOrder = 1;
    private bool _isStopped = false;

    private void Start()
    {
        _isStopped = false;
        
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        SpawnTopBall();
    }

    private bool ValidateReferences()
    {
        if (ballPrefab == null)
        {
            Debug.LogError(
                "Не назначен Ball Prefab.",
                this
            );

            return false;
        }

        if (catalog == null)
        {
            Debug.LogError(
                "Не назначен Ball Catalog.",
                this
            );

            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "Не назначен Spawn Point.",
                this
            );

            return false;
        }

        if (ballsParent == null)
        {
            Debug.LogError(
                "Не назначен Balls Parent.",
                this
            );

            return false;
        }

        if (playAreaBounds == null ||
            !playAreaBounds.IsValid)
        {
            Debug.LogError(
                "Некорректный PlayAreaBounds.",
                this
            );

            return false;
        }

        if (mergeResolver == null)
        {
            Debug.LogError(
                "Не назначен MergeResolver.",
                this
            );

            return false;
        }

        if (catalog.Count == 0)
        {
            Debug.LogError(
                "Каталог шаров пуст.",
                catalog
            );

            return false;
        }

        if (!catalog.HasValidSpawnChances(
                out int totalChance
            ))
        {
            Debug.LogError(
                $"Сумма вероятностей равна {totalChance}%, " +
                "но должна быть равна 100%.",
                catalog
            );

            return false;
        }

        return true;
    }

    private void SpawnTopBall()
    {
        if (_isStopped || currentController != null)
        {
            return;
        }

        int level = _isFirstBall
            ? FirstBallLevel
            : catalog.GetRandomSpawnableLevel();

        if (!CanCreateLevel(level))
        {
            Debug.LogError(
                $"Невозможно создать шар уровня {level}.",
                this
            );

            return;
        }

        long spawnOrder = _nextSpawnOrder++;

        Ball newBall = CreateBall(
            level,
            spawnPoint.position,
            Quaternion.identity,
            spawnOrder
        );

        if (newBall == null)
        {
            return;
        }

        if (!newBall.TryGetComponent(
                out DropBallController controller
            ))
        {
            Debug.LogError(
                "На префабе отсутствует DropBallController.",
                newBall
            );

            Destroy(newBall.gameObject);
            return;
        }

        if (!controller.BeginAiming(
                playAreaBounds
            ))
        {
            Destroy(newBall.gameObject);
            return;
        }

        controller.Dropped +=
            HandleBallDropped;

        currentController = controller;
        _isFirstBall = false;
    }

    private Ball CreateBall(
        int level,
        Vector2 position,
        Quaternion rotation,
        long spawnOrder
    )
    {
        Ball newBall = Instantiate(
            ballPrefab,
            position,
            rotation,
            ballsParent
        );

        if (!newBall.Initialize(
                catalog,
                level,
                spawnOrder,
                mergeResolver
            ))
        {
            Destroy(newBall.gameObject);
            return null;
        }

        return newBall;
    }

    public Ball SpawnMergedBall(
        int level,
        Vector2 position,
        float rotationZ,
        long inheritedSpawnOrder,
        Vector2 initialVelocity,
        float initialAngularVelocity
    )
    {
        if (_isStopped || !CanCreateLevel(level))
        {
            return null;
        }

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationZ
            );

        Ball resultBall = CreateBall(
            level,
            position,
            rotation,
            inheritedSpawnOrder
        );

        if (resultBall == null)
        {
            return null;
        }
        
        resultBall.MarkReleased();

        if (resultBall.TryGetComponent(
                out DropBallController controller
            ))
        {
            controller.DisableControl();
        }

        Rigidbody2D resultRigidbody =
            resultBall.GetComponent<Rigidbody2D>();

        resultRigidbody.simulated = true;
        resultRigidbody.linearVelocity =
            initialVelocity;

        resultRigidbody.angularVelocity =
            initialAngularVelocity;

        return resultBall;
    }

    public bool CanCreateLevel(int level)
    {
        return catalog != null &&
               level >= 0 &&
               level < catalog.Count;
    }

    private void HandleBallDropped(
        DropBallController droppedBall
    )
    {
        if (_isStopped || droppedBall != currentController)
        {
            return;
        }

        droppedBall.Dropped -=
            HandleBallDropped;

        currentController = null;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine =
            StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(
            spawnDelay
        );

        spawnCoroutine = null;

        if (!_isStopped)
        {
            SpawnTopBall();
        }
    }
    
    public void StopGame()
    {
        if (_isStopped)
        {
            return;
        }

        _isStopped = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (currentController != null)
        {
            currentController.Dropped -=
                HandleBallDropped;

            currentController.DisableControl();
            currentController = null;
        }
    }
}
