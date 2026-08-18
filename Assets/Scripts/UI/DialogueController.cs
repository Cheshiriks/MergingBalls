using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public sealed class DialogueLine
{
    [TextArea(2, 5)]
    public string textRus;

    [TextArea(2, 5)]
    public string textEng;
}

public sealed class DialogueController :
    MonoBehaviour,
    IPointerClickHandler
{
    [Header("Объекты")]
    [SerializeField]
    private RectTransform girl;

    [SerializeField]
    private GameObject dialog;

    [SerializeField]
    private TMP_Text dialogText;

    [Header("Реплики")]
    [SerializeField]
    private DialogueLine[] lines;

    [Header("Появление девушки")]
    [Tooltip(
        "На сколько UI-единиц девушка " +
        "изначально находится ниже своей позиции."
    )]
    [SerializeField, Min(0f)]
    private float startOffsetY = 900f;

    [SerializeField, Min(0.05f)]
    private float enterDuration = 0.7f;

    private Vector2 girlFinalPosition;

    private int currentLineIndex;

    private bool dialogueStarted;
    private bool canAdvance;
    private bool isFinished;

    private void Awake()
    {
        if (girl == null)
        {
            Debug.LogError(
                "В DialogueController не назначен Girl.",
                this
            );

            enabled = false;
            return;
        }

        if (dialog == null)
        {
            Debug.LogError(
                "В DialogueController не назначен Dialog.",
                this
            );

            enabled = false;
            return;
        }

        if (dialogText == null)
        {
            Debug.LogError(
                "В DialogueController не назначен DialogText.",
                this
            );

            enabled = false;
            return;
        }

        girlFinalPosition =
            girl.anchoredPosition;

        dialog.SetActive(false);
    }

    private IEnumerator Start()
    {
        /*
         * Защита от порядка Awake между объектами.
         */
        while (SaveGame.Instance == null)
        {
            yield return null;
        }

        if (SaveGame.Instance.IntroDialogueCompleted)
        {
            gameObject.SetActive(false);
            yield break;
        }

        StartCoroutine(
            PlayIntro()
        );
    }

    private IEnumerator PlayIntro()
    {
        dialogueStarted = false;
        canAdvance = false;
        isFinished = false;

        currentLineIndex = 0;

        dialog.SetActive(false);

        Vector2 startPosition =
            girlFinalPosition +
            Vector2.down * startOffsetY;

        girl.anchoredPosition =
            startPosition;

        /*
         * Девушка выезжает снизу.
         */
        float elapsed = 0f;

        while (elapsed < enterDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / enterDuration
            );

            float easedProgress =
                EaseOutCubic(progress);

            girl.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    girlFinalPosition,
                    easedProgress
                );

            yield return null;
        }

        girl.anchoredPosition =
            girlFinalPosition;

        /*
         * Теперь показываем облако с первой репликой.
         */
        dialog.SetActive(true);

        ShowCurrentLine();

        dialogueStarted = true;
        canAdvance = true;
    }

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        if (!dialogueStarted ||
            !canAdvance ||
            isFinished)
        {
            return;
        }

        NextLine();
    }

    private void NextLine()
    {
        if (lines == null ||
            lines.Length == 0)
        {
            FinishDialogue();
            return;
        }

        currentLineIndex++;

        /*
         * Все реплики закончились.
         */
        if (currentLineIndex >=
            lines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (lines == null ||
            lines.Length == 0)
        {
            dialogText.text = "";
            return;
        }

        if (currentLineIndex < 0 ||
            currentLineIndex >= lines.Length)
        {
            return;
        }

        DialogueLine line =
            lines[currentLineIndex];

        bool isRussian =
            SaveGame.Instance == null ||
            SaveGame.Instance.language == "ru";

        dialogText.text =
            isRussian
                ? line.textRus
                : line.textEng;
    }

    private void FinishDialogue()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        canAdvance = false;

        if (SaveGame.Instance != null)
        {
            SaveGame.Instance.CompleteIntroDialogue();
        }

        gameObject.SetActive(false);
    }

    private static float EaseOutCubic(
        float value
    )
    {
        value =
            Mathf.Clamp01(value);

        float inverse =
            1f - value;

        return
            1f -
            inverse * inverse * inverse;
    }
}
