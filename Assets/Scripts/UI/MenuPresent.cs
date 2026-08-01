using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPresent : MonoBehaviour
{
    [SerializeField] private Button getButton;
    [SerializeField] private TMP_Text rewardText;

    private Action onClaim;
    private bool claimed;

    private void Awake()
    {
        getButton.onClick.AddListener(HandleGetClicked);
    }

    public void Show(int rewardAmount, Action onClaimCallback)
    {
        claimed = false;
        onClaim = onClaimCallback;

        rewardText.text = "+" + rewardAmount;

        getButton.interactable = true;
        gameObject.SetActive(true);
    }

    private void HandleGetClicked()
    {
        if (claimed)
            return;

        claimed = true;
        getButton.interactable = false;

        gameObject.SetActive(false);

        onClaim?.Invoke();
        onClaim = null;
    }
}
