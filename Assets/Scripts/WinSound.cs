using UnityEngine;

public class WinSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip openWinMenu;

    private void OnEnable()
    {
        if (openWinMenu != null)
            AudioManager.Instance.PlaySFX(openWinMenu);
    }
}
