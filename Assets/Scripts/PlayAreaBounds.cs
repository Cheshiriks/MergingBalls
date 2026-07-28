using UnityEngine;

public sealed class PlayAreaBounds : MonoBehaviour
{
    [Header("Боковые стены")]
    [SerializeField]
    private BoxCollider2D leftWall;

    [SerializeField]
    private BoxCollider2D rightWall;

    // Внутренняя сторона левой стены
    public float LeftX => leftWall.bounds.max.x;

    // Внутренняя сторона правой стены
    public float RightX => rightWall.bounds.min.x;

    public bool IsValid =>
        leftWall != null &&
        rightWall != null &&
        LeftX < RightX;

    private void OnValidate()
    {
        if (leftWall == null || rightWall == null)
        {
            return;
        }

        if (LeftX >= RightX)
        {
            Debug.LogError(
                "Левая и правая стены расположены некорректно.",
                this
            );
        }
    }
}
