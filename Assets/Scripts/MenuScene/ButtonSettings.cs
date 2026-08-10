using UnityEngine;

public class ButtonSettings : MonoBehaviour
{
    
    [SerializeField] private GameObject settingMenu;

    public void Open()
    {
        settingMenu.SetActive(true);
    }
    
    public void Close()
    {
        settingMenu.SetActive(false);
    }
}
