using UnityEngine;

public class MusicOnOff : MonoBehaviour
{
    [SerializeField] private GameObject musicOn;
    [SerializeField] private GameObject musicOff;

    void Start()
    {
        if (SaveGame.Instance.musicOn)
        {
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
        else
        {
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
    }

    public void MusicOn()
    {
        if (!SaveGame.Instance.musicOn)
        {
            SaveGame.Instance.musicOn = true;
        
            musicOn.SetActive(true);
            musicOff.SetActive(false);
            
            MusicManager.Instance.Play();
        }
    }
    
    public void MusicOff()
    {
        if (SaveGame.Instance.musicOn)
        {
            SaveGame.Instance.musicOn = false;
        
            musicOn.SetActive(false);
            musicOff.SetActive(true);
            
            MusicManager.Instance.Pause();
        }
    }
}
