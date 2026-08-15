using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource sfxSource;

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

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && SaveGame.Instance.soundOn)
            sfxSource.PlayOneShot(clip);
    }
    
    public void PlaySFX(
        AudioClip clip,
        float volume
    )
    {
        if (clip == null ||
            SaveGame.Instance == null ||
            !SaveGame.Instance.soundOn)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }
}
