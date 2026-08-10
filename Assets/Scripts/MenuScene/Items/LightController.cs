using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private GameObject item;
    [SerializeField] private GameObject button;

    void Start()
    {
        RefreshState();
    }

    private void OnEnable()
    {
        RefreshState();
    }

    public void Active()
    {
        item.SetActive(true);
        button.SetActive(false);
    }
    
    private void RefreshState()
    {
        if (SaveGame.Instance == null)
        {
            return;
        }
        
        bool isBought = SaveGame.Instance.IsBuyLight;
        
        item.SetActive(isBought);
        button.SetActive(!isBought);
    }
}
