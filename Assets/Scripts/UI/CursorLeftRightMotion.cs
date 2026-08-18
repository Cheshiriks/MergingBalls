using UnityEngine;

public class CursorLeftRightMotion : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    [Header("Motion")]
    [SerializeField] private float moveDistance = 40f;
    [SerializeField] private float moveSpeed = 4f;

    private Vector2 startPos;

    private void Awake()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        startPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float offsetX = Mathf.Sin(Time.unscaledTime * moveSpeed) * moveDistance;

        rectTransform.anchoredPosition =
            startPos + Vector2.right * offsetX;
    }
}
