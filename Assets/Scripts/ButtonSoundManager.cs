using UnityEngine;

public class ButtonSoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip collapseClip;
    
    public static ButtonSoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Play()
    {
        AudioManager.Instance.PlaySFX(collapseClip);
    }
}
