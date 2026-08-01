using UnityEngine;
using System;
using YG;

public class SaveGame : MonoBehaviour
{
    public static SaveGame Instance;
    
    // глобальные переменные не для сохранения
    public bool soundOn = true;
    public bool musicOn = true;
    public string language = "ru";
    
    public int Coins => YG2.saves.coins;
    public int Score => YG2.saves.score;
    public int MaxScore => YG2.saves.bestScore;
    
    public event Action<int> ScoreChanged;
    public event Action<int, int> CoinsChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            Instance = this;
            
            language = YG2.envir.language;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public int AddScore(int scoreToAdd)
    {
        YG2.saves.score += scoreToAdd;

        if (YG2.saves.score > YG2.saves.bestScore)
        {
            YG2.saves.bestScore = YG2.saves.score;
        }
        
        YG2.SaveProgress();
        //YG2.SetLeaderboard("MyMergingBalls", YG2.saves.bestScore);
        
        ScoreChanged?.Invoke(YG2.saves.score);
        
        return YG2.saves.score;
    }
    
    public int PlusCoin(int addCoins)
    {
        int previousCoins =
            YG2.saves.coins;

        YG2.saves.coins +=
            addCoins;

        CoinsChanged?.Invoke(
            previousCoins,
            YG2.saves.coins
        );
        
        YG2.SaveProgress();
        return YG2.saves.coins;
    }
    
    public int MinusCoin(int minusCoins)
    {
        if (minusCoins <= 0 || YG2.saves.coins < minusCoins)
        {
            return Coins;
        }
        
        int previousCoins =
            YG2.saves.coins;

        YG2.saves.coins -=
            minusCoins;

        CoinsChanged?.Invoke(
            previousCoins,
            YG2.saves.coins
        );
            
        YG2.SaveProgress();
        return YG2.saves.coins;
    }
    
    public int NewGame()
    {
        YG2.saves.score = 0;
        //YG2.SaveProgress();

        ScoreChanged?.Invoke(YG2.saves.score);

        return YG2.saves.score;
    }
}
