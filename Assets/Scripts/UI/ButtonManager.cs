using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void MenuScene()
    {
        SceneManager.LoadScene(0);
    }
    
    public void GameScene()
    {
        SceneManager.LoadScene(1);
    }
}
