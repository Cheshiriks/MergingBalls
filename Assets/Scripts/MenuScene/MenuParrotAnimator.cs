using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuParrotAnimator : MonoBehaviour
{
    [Header("Scene objects")]
    [SerializeField] private RectTransform birdRoot;

    [SerializeField] private Image parrotPerchImage;
    [SerializeField] private Image parrotBodyImage;
    [SerializeField] private Image parrotTailImage;
    [SerializeField] private Image parrotLeftWingImage;
    [SerializeField] private Image parrotRightWingImage;
    [SerializeField] private Image parrotHeadImage;

    [Header("Sprites")]
    [SerializeField] private Sprite perchSprite;
    [SerializeField] private Sprite bodySprite;
    [SerializeField] private Sprite tailSprite;
    [SerializeField] private Sprite leftWingSprite;
    [SerializeField] private Sprite rightWingSprite;
    [SerializeField] private Sprite headOpenEyesSprite;
    [SerializeField] private Sprite headClosedEyesSprite;

    [Header("Root Idle")]
    [SerializeField] private float rootBreathScale = 0.006f;
    [SerializeField] private float rootFloatY = 1.5f;
    [SerializeField] private float rootSpeed = 0.8f;

    [Header("Head Animation")]
    [SerializeField] private float headAngle = 3f;
    [SerializeField] private float headMoveX = 2f;
    [SerializeField] private float headMoveY = 2f;
    [SerializeField] private float headSpeed = 0.65f;

    [Header("Tail Animation")]
    [SerializeField] private float tailAngle = 6f;
    [SerializeField] private float tailSpeed = 1.5f;

    [Header("Wing Ruffle")]
    [SerializeField] private float wingAngle = 8f;
    [SerializeField] private float wingMoveY = 3f;
    [SerializeField] private float wingRuffleDuration = 0.25f;
    [SerializeField] private float wingMinDelay = 3f;
    [SerializeField] private float wingMaxDelay = 7f;

    [Header("Blink")]
    [SerializeField] private float blinkMinDelay = 2.5f;
    [SerializeField] private float blinkMaxDelay = 5f;
    [SerializeField] private float blinkDuration = 0.08f;
    [SerializeField] private float doubleBlinkChance = 0.25f;

    private RectTransform headRect;
    private RectTransform tailRect;
    private RectTransform leftWingRect;
    private RectTransform rightWingRect;

    private Vector2 birdRootStartPosition;
    private Vector2 headStartPosition;
    private Vector2 leftWingStartPosition;
    private Vector2 rightWingStartPosition;

    private Vector3 birdRootStartScale;

    private Quaternion headStartRotation;
    private Quaternion tailStartRotation;
    private Quaternion leftWingStartRotation;
    private Quaternion rightWingStartRotation;

    private Coroutine blinkCoroutine;
    private Coroutine wingCoroutine;

    private void Awake()
    {
        ApplySprites();
        CacheStartState();
    }

    private void OnEnable()
    {
        if (parrotHeadImage != null && headOpenEyesSprite != null)
            parrotHeadImage.sprite = headOpenEyesSprite;

        blinkCoroutine = StartCoroutine(BlinkRoutine());
        wingCoroutine = StartCoroutine(WingRuffleRoutine());
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (wingCoroutine != null)
            StopCoroutine(wingCoroutine);

        ResetToStartState();
    }

    private void Update()
    {
        float time = Time.time;

        AnimateBirdRoot(time);
        AnimateHead(time);
        AnimateTail(time);
    }

    private void ApplySprites()
    {
        if (parrotPerchImage != null && perchSprite != null)
            parrotPerchImage.sprite = perchSprite;

        if (parrotBodyImage != null && bodySprite != null)
            parrotBodyImage.sprite = bodySprite;

        if (parrotTailImage != null && tailSprite != null)
            parrotTailImage.sprite = tailSprite;

        if (parrotLeftWingImage != null && leftWingSprite != null)
            parrotLeftWingImage.sprite = leftWingSprite;

        if (parrotRightWingImage != null && rightWingSprite != null)
            parrotRightWingImage.sprite = rightWingSprite;

        if (parrotHeadImage != null && headOpenEyesSprite != null)
            parrotHeadImage.sprite = headOpenEyesSprite;
    }

    private void CacheStartState()
    {
        if (birdRoot != null)
        {
            birdRootStartPosition = birdRoot.anchoredPosition;
            birdRootStartScale = birdRoot.localScale;
        }

        if (parrotHeadImage != null)
        {
            headRect = parrotHeadImage.rectTransform;
            headStartPosition = headRect.anchoredPosition;
            headStartRotation = headRect.localRotation;
        }

        if (parrotTailImage != null)
        {
            tailRect = parrotTailImage.rectTransform;
            tailStartRotation = tailRect.localRotation;
        }

        if (parrotLeftWingImage != null)
        {
            leftWingRect = parrotLeftWingImage.rectTransform;
            leftWingStartPosition = leftWingRect.anchoredPosition;
            leftWingStartRotation = leftWingRect.localRotation;
        }

        if (parrotRightWingImage != null)
        {
            rightWingRect = parrotRightWingImage.rectTransform;
            rightWingStartPosition = rightWingRect.anchoredPosition;
            rightWingStartRotation = rightWingRect.localRotation;
        }
    }

    private void AnimateBirdRoot(float time)
    {
        if (birdRoot == null)
            return;

        float t = time * rootSpeed * Mathf.PI * 2f;

        float scale = 1f + Mathf.Sin(t) * rootBreathScale;
        float y = Mathf.Sin(t) * rootFloatY;

        birdRoot.localScale = birdRootStartScale * scale;
        birdRoot.anchoredPosition = birdRootStartPosition + new Vector2(0f, y);
    }

    private void AnimateHead(float time)
    {
        if (headRect == null)
            return;

        float t = time * headSpeed * Mathf.PI * 2f;

        float angle = Mathf.Sin(t) * headAngle;
        float x = Mathf.Sin(t * 0.7f) * headMoveX;
        float y = Mathf.Sin(t * 1.1f) * headMoveY;

        headRect.localRotation = headStartRotation * Quaternion.Euler(0f, 0f, angle);
        headRect.anchoredPosition = headStartPosition + new Vector2(x, y);
    }

    private void AnimateTail(float time)
    {
        if (tailRect == null)
            return;

        float angle = Mathf.Sin(time * tailSpeed * Mathf.PI * 2f) * tailAngle;

        tailRect.localRotation = tailStartRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkMinDelay, blinkMaxDelay));

            yield return BlinkOnce();

            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(0.12f);
                yield return BlinkOnce();
            }
        }
    }

    private IEnumerator BlinkOnce()
    {
        if (parrotHeadImage == null)
            yield break;

        if (headClosedEyesSprite != null)
            parrotHeadImage.sprite = headClosedEyesSprite;

        yield return new WaitForSeconds(blinkDuration);

        if (headOpenEyesSprite != null)
            parrotHeadImage.sprite = headOpenEyesSprite;
    }

    private IEnumerator WingRuffleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(wingMinDelay, wingMaxDelay));

            yield return AnimateWingsOutAndBack();

            if (Random.value < 0.35f)
            {
                yield return new WaitForSeconds(0.12f);
                yield return AnimateWingsOutAndBack();
            }
        }
    }

    private IEnumerator AnimateWingsOutAndBack()
    {
        float halfDuration = wingRuffleDuration * 0.5f;

        yield return AnimateWings(0f, 1f, halfDuration);
        yield return AnimateWings(1f, 0f, halfDuration);
    }

    private IEnumerator AnimateWings(float from, float to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / duration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            float value = Mathf.Lerp(from, to, smoothProgress);

            ApplyWingPose(value);

            yield return null;
        }

        ApplyWingPose(to);
    }

    private void ApplyWingPose(float value)
    {
        if (leftWingRect != null)
        {
            leftWingRect.localRotation =
                leftWingStartRotation * Quaternion.Euler(0f, 0f, wingAngle * value);

            leftWingRect.anchoredPosition =
                leftWingStartPosition + new Vector2(0f, wingMoveY * value);
        }

        if (rightWingRect != null)
        {
            rightWingRect.localRotation =
                rightWingStartRotation * Quaternion.Euler(0f, 0f, -wingAngle * value);

            rightWingRect.anchoredPosition =
                rightWingStartPosition + new Vector2(0f, wingMoveY * value);
        }
    }

    private void ResetToStartState()
    {
        if (birdRoot != null)
        {
            birdRoot.anchoredPosition = birdRootStartPosition;
            birdRoot.localScale = birdRootStartScale;
        }

        if (headRect != null)
        {
            headRect.anchoredPosition = headStartPosition;
            headRect.localRotation = headStartRotation;
        }

        if (tailRect != null)
            tailRect.localRotation = tailStartRotation;

        if (leftWingRect != null)
        {
            leftWingRect.anchoredPosition = leftWingStartPosition;
            leftWingRect.localRotation = leftWingStartRotation;
        }

        if (rightWingRect != null)
        {
            rightWingRect.anchoredPosition = rightWingStartPosition;
            rightWingRect.localRotation = rightWingStartRotation;
        }

        if (parrotHeadImage != null && headOpenEyesSprite != null)
            parrotHeadImage.sprite = headOpenEyesSprite;
    }
}
