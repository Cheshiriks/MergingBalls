using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePopupController : MonoBehaviour
{
    [SerializeField] private TMP_Text userCoinsText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text errorText;
    
    [SerializeField] private Image animalIcon;
    
    [Header("Item icons")]
    [SerializeField] private Sprite parrotImg;
    [SerializeField] private Sprite mapImg;
    [SerializeField] private Sprite lightImg;
    [SerializeField] private Sprite boxImg;
    [SerializeField] private Sprite flagImg;
    
    [Header("Item controllers")]
    [SerializeField] private ParrotController parrotController;
    [SerializeField] private BoxController boxController;
    [SerializeField] private FlagController flagController;
    [SerializeField] private LightController lightController;
    [SerializeField] private MapController mapController;
    
    private int _price = 5000;
    private int _id = 0;

    public void Open(int animalId)
    {
        _id = animalId;
        
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        switch (animalId)
        {
            case 0:
                animalIcon.sprite = parrotImg;
                _price = 15000;
                break;
            case 1:
                animalIcon.sprite = mapImg;
                _price = 12000;
                break;
            case 2:
                animalIcon.sprite = lightImg;
                _price = 5000;
                break;
            case 3:
                animalIcon.sprite = boxImg;
                _price = 10000;
                break;
            case 4:
                animalIcon.sprite = flagImg;
                _price = 8000;
                break;
        }

        if (priceText != null)
            priceText.text = _price.ToString();

        gameObject.SetActive(true);
    }

    public void Buy()
    {
        
        if (SaveGame.Instance == null)
        {
            Debug.LogWarning("SaveGame.Instance is null. Cannot buy decoration.");
            return;
        }

        bool success = SaveGame.Instance.Coins >= _price;

        if (!success)
        {
            if (errorText != null)
                errorText.gameObject.SetActive(true);

            return;
        }

        int newCoins = SaveGame.Instance.MinusCoin(_price);
        BuyAnimal();

        userCoinsText.text = newCoins.ToString();
        //if (coinsCounterView != null)
        //    coinsCounterView.AnimateToValue(newCoins);

        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void BuyAnimal()
    {
        switch (_id)
        {
            case 0:
                SaveGame.Instance.IsBuyParrot = true;
                parrotController.Active();
                break;
            case 1:
                SaveGame.Instance.IsBuyMap = true;
                mapController.Active();
                break;
            case 2:
                SaveGame.Instance.IsBuyLight = true;
                lightController.Active();
                break;
            case 3:
                SaveGame.Instance.IsBuyBox = true;
                boxController.Active();
                break;
            case 4:
                SaveGame.Instance.IsBuyFlag = true;
                flagController.Active();
                break;
        }
    }
}
