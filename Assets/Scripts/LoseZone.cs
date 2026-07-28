using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class LoseZone : MonoBehaviour
{
    private sealed class TrackedBall
    {
        public Ball Ball;
        public float StableTime;

        public TrackedBall(Ball ball)
        {
            Ball = ball;
            StableTime = 0f;
        }
    }

    [Header("Основные ссылки")]
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private PlayAreaBounds playAreaBounds;

    [SerializeField]
    private Transform dottedLine;

    [Tooltip("Обычно сюда назначается SpawnPoint.")]
    [SerializeField]
    private Transform topPoint;

    [Header("Размер зоны")]
    [SerializeField, Min(0f)]
    private float topPadding = 0.5f;

    [Header("Условия поражения")]
    [SerializeField, Min(0.1f)]
    private float requiredStayTime = 1.5f;

    [Tooltip(
        "Пока шар движется быстрее этого значения, " +
        "таймер поражения не увеличивается."
    )]
    [SerializeField, Min(0f)]
    private float maximumCountingSpeed = 0.4f;

    private BoxCollider2D zoneCollider;

    private readonly List<TrackedBall> trackedBalls =
        new List<TrackedBall>();

    private void Awake()
    {
        zoneCollider =
            GetComponent<BoxCollider2D>();

        zoneCollider.isTrigger = true;

        FitColliderToPlayArea();

        if (gameManager == null)
        {
            Debug.LogError(
                "В LoseZone не назначен GameManager.",
                this
            );

            enabled = false;
        }
    }

    private void Reset()
    {
        BoxCollider2D collider =
            GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        zoneCollider =
            GetComponent<BoxCollider2D>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }

        FitColliderToPlayArea();
    }

    private void Update()
    {
        if (gameManager == null ||
            gameManager.IsGameOver)
        {
            trackedBalls.Clear();
            return;
        }

        float maximumSpeedSquared =
            maximumCountingSpeed *
            maximumCountingSpeed;

        for (int i = trackedBalls.Count - 1;
             i >= 0;
             i--)
        {
            TrackedBall tracked =
                trackedBalls[i];

            Ball ball = tracked.Ball;

            if (ball == null ||
                ball.IsMerging)
            {
                trackedBalls.RemoveAt(i);
                continue;
            }

            // Верхний управляемый шар не учитываем.
            if (!ball.IsReleased)
            {
                tracked.StableTime = 0f;
                continue;
            }

            // Быстро падающий или подпрыгивающий шар
            // пока не считается переполнением.
            if (ball.LinearVelocity.sqrMagnitude >
                maximumSpeedSquared)
            {
                tracked.StableTime = 0f;
                continue;
            }

            tracked.StableTime +=
                Time.deltaTime;

            if (tracked.StableTime >=
                requiredStayTime)
            {
                gameManager.LoseGame(ball);
                trackedBalls.Clear();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        RegisterBall(other);
    }

    private void OnTriggerStay2D(
        Collider2D other
    )
    {
        /*
         * Эта дополнительная регистрация полезна,
         * когда Rigidbody2D включается уже внутри зоны.
         */
        RegisterBall(other);
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        Ball ball =
            other.GetComponentInParent<Ball>();

        if (ball == null)
        {
            return;
        }

        RemoveBall(ball);
    }

    private void RegisterBall(
        Collider2D other
    )
    {
        if (gameManager == null ||
            gameManager.IsGameOver)
        {
            return;
        }

        Ball ball =
            other.GetComponentInParent<Ball>();

        if (ball == null)
        {
            return;
        }

        for (int i = 0;
             i < trackedBalls.Count;
             i++)
        {
            if (trackedBalls[i].Ball == ball)
            {
                return;
            }
        }

        trackedBalls.Add(
            new TrackedBall(ball)
        );
    }

    private void RemoveBall(Ball ball)
    {
        for (int i = trackedBalls.Count - 1;
             i >= 0;
             i--)
        {
            if (trackedBalls[i].Ball == ball)
            {
                trackedBalls.RemoveAt(i);
            }
        }
    }

    [ContextMenu("Подогнать зону под игровое поле")]
    private void FitColliderToPlayArea()
    {
        if (zoneCollider == null)
        {
            zoneCollider =
                GetComponent<BoxCollider2D>();
        }

        if (zoneCollider == null ||
            playAreaBounds == null ||
            !playAreaBounds.IsValid ||
            dottedLine == null ||
            topPoint == null)
        {
            return;
        }

        float bottomY =
            dottedLine.position.y;

        float topY =
            topPoint.position.y +
            topPadding;

        if (topY <= bottomY)
        {
            return;
        }

        float leftX =
            playAreaBounds.LeftX;

        float rightX =
            playAreaBounds.RightX;

        float worldWidth =
            rightX - leftX;

        float worldHeight =
            topY - bottomY;

        Vector3 worldCenter =
            new Vector3(
                (leftX + rightX) * 0.5f,
                (bottomY + topY) * 0.5f,
                transform.position.z
            );

        transform.position =
            worldCenter;

        float scaleX =
            Mathf.Abs(transform.lossyScale.x);

        float scaleY =
            Mathf.Abs(transform.lossyScale.y);

        if (scaleX < 0.0001f ||
            scaleY < 0.0001f)
        {
            return;
        }

        zoneCollider.offset =
            Vector2.zero;

        zoneCollider.size =
            new Vector2(
                worldWidth / scaleX,
                worldHeight / scaleY
            );
    }
}
