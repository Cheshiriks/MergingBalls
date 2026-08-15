using UnityEngine;
using TMPro;
using YG;

public class PurchaseButtonController : MonoBehaviour
{
    
    [Header("Purchase")]
    [SerializeField] private GameObject purchaseAds;
    
    [Header("Coast Text")]
    [SerializeField] private TextMeshProUGUI purchaseAdsText;
    [SerializeField] private TextMeshProUGUI purchase3000Coins;
    
    void Start()
    {
        ChangePurchaseButton();
        InitButtonText();
    }

    public void ChangePurchaseButton()
    {
        if (!SaveGame.Instance.IsShowAds)
        {
            purchaseAds.SetActive(false);
        }
    }

    private void InitButtonText()
    {
        purchaseAdsText.text = YG2.PurchaseByID("merging_ads").price;
        purchase3000Coins.text = YG2.PurchaseByID("coins_3000").price;
    }
}
