using UnityEngine;

public sealed class ComboManager : MonoBehaviour
{
    [Header("Отображение")]
    [SerializeField]
    private ComboPopup popupPrefab;

    [SerializeField]
    private Transform effectsParent;

    [SerializeField]
    private Vector3 popupOffset =
        new Vector3(0f, 0.1f, 0f);

    [Header("Правила комбо")]
    [Tooltip(
        "Максимальное время между соседними " +
        "слияниями одной цепочки."
    )]
    [SerializeField, Min(0.05f)]
    private float comboWindow = 0.8f;

    private int currentCombo;
    private float lastMergeTime =
        float.NegativeInfinity;

    public int CurrentCombo => currentCombo;

    public int RegisterMerge(
        Vector3 worldPosition
    )
    {
        float currentTime = Time.time;

        bool continuesCombo =
            currentCombo > 0 &&
            currentTime - lastMergeTime <=
            comboWindow;

        if (continuesCombo)
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 1;
        }

        // Каждое новое слияние продлевает окно комбо.
        lastMergeTime = currentTime;

        // Для первого слияния x1 не показываем.
        if (currentCombo >= 2)
        {
            ShowPopup(
                currentCombo,
                worldPosition
            );
        }

        return currentCombo;
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        lastMergeTime =
            float.NegativeInfinity;
    }

    private void ShowPopup(
        int combo,
        Vector3 worldPosition
    )
    {
        if (popupPrefab == null)
        {
            Debug.LogError(
                "В ComboManager не назначен " +
                "ComboPopup Prefab.",
                this
            );

            return;
        }

        ComboPopup popup = Instantiate(
            popupPrefab,
            worldPosition + popupOffset,
            Quaternion.identity,
            effectsParent
        );

        popup.Play(combo);
    }
}
