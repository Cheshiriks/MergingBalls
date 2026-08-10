using UnityEngine;

public class ParrotController : MonoBehaviour
{
    
    [SerializeField] private GameObject animal;
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
        animal.SetActive(true);
        button.SetActive(false);
    }

    private void RefreshState()
    {
        if (SaveGame.Instance == null)
        {
            return;
        }
        
        bool isBought = SaveGame.Instance.IsBuyParrot;
        
        animal.SetActive(isBought);
        button.SetActive(!isBought);
    }
}
