using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource _audioComponent;
    
    public static MusicManager Instance;

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
    
    private void Start()
    {
        _audioComponent = GetComponent<AudioSource>();
        
        if (SaveGame.Instance.musicOn)
        {
            _audioComponent.Play();
        }
    }

    public void Pause()
    {
        _audioComponent.Pause();
    }

    public void Play()
    {
        if (SaveGame.Instance.musicOn)
        {
            _audioComponent.Play();
        }
    }
}
