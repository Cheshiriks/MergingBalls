using UnityEngine;
using YG;
using TMPro;

public class MyPurchase : MonoBehaviour
{
    [SerializeField] private PurchaseButtonController purchaseButtonController;
    [SerializeField] private TextMeshProUGUI textCoins;
    
    private void OnEnable()
    {
        Debug.Log("Start Purchase controller!!!");
        
        YG2.onPurchaseSuccess += SuccessPurchased;
        YG2.onPurchaseFailed += FailedPurchased;

        YG2.ConsumePurchases();
    }

    private void OnDisable()
    {
        YG2.onPurchaseSuccess -= SuccessPurchased;
        YG2.onPurchaseFailed -= FailedPurchased;
    }
    
    private void SuccessPurchased(string id)
    {
        // Ваш код для обработки покупки, например:
        if (id == "merging_ads")
        {
            SaveGame.Instance.OffAds();
            
            purchaseButtonController.ChangePurchaseButton();
            Debug.Log("merging_ads !!!");
        }
        
        else if (id == "coins_3000")
        {
            int newCoins = SaveGame.Instance.PlusCoin(3000);
            textCoins.text = newCoins.ToString();
                
            Debug.Log("coins_3000 !!!");
        }
        
        else
            Debug.Log("Unknown purchase id - " + id);
    }

    private void FailedPurchased(string id)
    {
        // Покупка не была совершена
        Debug.Log("Error purchase - " + id);
    }
}
