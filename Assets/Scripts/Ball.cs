using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class Ball : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D ballRigidbody;
    private CircleCollider2D ballCollider;

    private MergeResolver mergeResolver;
    private bool isInitialized;

    public BallCatalog Catalog { get; private set; }
    public int Level { get; private set; }
    public int Value { get; private set; }

    public long SpawnOrder { get; private set; }
    public bool IsMerging { get; private set; }
    public bool IsReleased { get; private set; }

    public Vector2 LinearVelocity =>
        ballRigidbody != null
            ? ballRigidbody.linearVelocity
            : Vector2.zero;

    public float AngularVelocity =>
        ballRigidbody != null
            ? ballRigidbody.angularVelocity
            : 0f;

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponent<Rigidbody2D>();
        }

        if (ballCollider == null)
        {
            ballCollider = GetComponent<CircleCollider2D>();
        }
    }

    public bool Initialize(
        BallCatalog catalog,
        int level,
        long spawnOrder,
        MergeResolver resolver
    )
    {
        CacheComponents();

        if (catalog == null)
        {
            Debug.LogError(
                "Шару не передан BallCatalog.",
                this
            );

            return false;
        }

        if (resolver == null)
        {
            Debug.LogError(
                "Шару не передан MergeResolver.",
                this
            );

            return false;
        }

        if (spriteRenderer == null ||
            ballRigidbody == null ||
            ballCollider == null)
        {
            Debug.LogError(
                "На префабе Ball отсутствуют обязательные компоненты.",
                this
            );

            return false;
        }

        BallDefinition definition =
            catalog.GetDefinition(level);

        if (definition == null)
        {
            Debug.LogError(
                $"В каталоге отсутствует уровень {level}.",
                catalog
            );

            return false;
        }

        if (definition.Sprite == null)
        {
            Debug.LogError(
                $"Для шара уровня {level} не назначен Sprite.",
                catalog
            );

            return false;
        }

        Catalog = catalog;
        Level = level;
        Value = definition.Value;
        SpawnOrder = spawnOrder;

        mergeResolver = resolver;

        IsMerging = false;
        IsReleased = false;
        isInitialized = true;

        spriteRenderer.sprite = definition.Sprite;

        float scale = definition.Scale;

        transform.localScale = new Vector3(
            scale,
            scale,
            1f
        );

        ballRigidbody.mass = definition.Mass;

        ballCollider.enabled = true;

        gameObject.name =
            $"Ball_{definition.Value}";

        return true;
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (!isInitialized || IsMerging)
        {
            return;
        }

        Ball otherBall =
            collision.collider.GetComponentInParent<Ball>();

        if (otherBall == null ||
            otherBall == this ||
            otherBall.IsMerging)
        {
            return;
        }

        mergeResolver.TryMerge(
            this,
            otherBall
        );
    }

    public void LockForMerge()
    {
        if (IsMerging)
        {
            return;
        }

        IsMerging = true;

        // Немедленно прекращаем новые столкновения.
        ballCollider.enabled = false;
        ballRigidbody.simulated = false;
    }
    
    public void MarkReleased()
    {
        if (!isInitialized || IsMerging)
        {
            return;
        }

        IsReleased = true;
    }

    public void FreezePhysics()
    {
        CacheComponents();

        if (ballRigidbody == null)
        {
            return;
        }

        ballRigidbody.linearVelocity = Vector2.zero;
        ballRigidbody.angularVelocity = 0f;
        ballRigidbody.simulated = false;
    }
}
