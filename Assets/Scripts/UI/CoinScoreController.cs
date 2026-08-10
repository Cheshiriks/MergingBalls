using UnityEngine;
using TMPro;

public class CoinScoreController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text coinText;
    [SerializeField]
    private TMP_Text bestScoreText;
    void Start()
    {
        coinText.text = SaveGame.Instance.Coins.ToString();
        bestScoreText.text = SaveGame.Instance.MaxScore.ToString();
    }
    
}
