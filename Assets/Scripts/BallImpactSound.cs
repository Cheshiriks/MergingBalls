using UnityEngine;

using UnityEngine;

[RequireComponent(typeof(Ball))]
public sealed class BallImpactSound : MonoBehaviour
{
    [Header("Звуки")]
    [SerializeField]
    private AudioClip coinHitSound;

    [SerializeField]
    private AudioClip floorHitSound;

    [Header("Сила удара")]
    [Tooltip(
        "Столкновения слабее этой скорости " +
        "вообще не воспроизводят звук."
    )]
    [SerializeField, Min(0f)]
    private float minimumImpactSpeed = 0.8f;

    [Tooltip(
        "При этой скорости звук уже играет " +
        "на максимальной громкости."
    )]
    [SerializeField, Min(0.1f)]
    private float fullVolumeImpactSpeed = 6f;

    [Header("Громкость")]
    [SerializeField, Range(0f, 1f)]
    private float minimumVolume = 0.15f;

    [SerializeField, Range(0f, 1f)]
    private float maximumVolume = 0.8f;

    [Header("Защита от дребезга")]
    [SerializeField, Min(0f)]
    private float soundCooldown = 0.08f;
    
    private Ball ball;

    private float lastSoundTime =
        float.NegativeInfinity;

    private void Awake()
    {
        ball = GetComponent<Ball>();
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        float impactSpeed =
            collision.relativeVelocity.magnitude;

        if (impactSpeed <
            minimumImpactSpeed)
        {
            return;
        }

        if (Time.unscaledTime -
            lastSoundTime <
            soundCooldown)
        {
            return;
        }

        Ball otherBall =
            collision.collider
                .GetComponentInParent<Ball>();
        
        if (otherBall != null)
        {
            HandleBallCollision(
                otherBall,
                impactSpeed
            );

            return;
        }

        if (collision.collider.CompareTag(
                "Floor"
            ))
        {
            PlayImpactSound(
                floorHitSound,
                impactSpeed
            );
        }
    }

    private void HandleBallCollision(
        Ball otherBall,
        float impactSpeed
    )
    {
        if (otherBall == null ||
            otherBall == ball)
        {
            return;
        }

        /*
         * OnCollisionEnter2D получат обе монеты.
         *
         * Поэтому звук разрешаем только одному
         * объекту из пары.
         */
        if (GetEntityId() >
            otherBall.GetEntityId())
        {
            return;
        }

        PlayImpactSound(
            coinHitSound,
            impactSpeed
        );
    }

    private void PlayImpactSound(
        AudioClip clip,
        float impactSpeed
    )
    {
        if (clip == null)
        {
            return;
        }

        float normalizedImpact =
            Mathf.InverseLerp(
                minimumImpactSpeed,
                fullVolumeImpactSpeed,
                impactSpeed
            );

        float volume =
            Mathf.Lerp(
                minimumVolume,
                maximumVolume,
                normalizedImpact
            );

        lastSoundTime =
            Time.unscaledTime;

        AudioManager.Instance.PlaySFX(
            clip,
            volume
        );
    }
}
