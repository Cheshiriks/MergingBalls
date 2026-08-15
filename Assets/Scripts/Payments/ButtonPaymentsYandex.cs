using UnityEngine;
using YG;

public class ButtonPaymentsYandex : MonoBehaviour
{
    public void BuyOffAds()
    {
        YG2.BuyPayments("merging_ads");
    }
    
    public void Buy3000Coins()
    {
        YG2.BuyPayments("coins_3000");
    }
}
