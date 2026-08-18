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
    public bool IsShowAds => YG2.saves.isShowAds;
    
    public event Action<int> ScoreChanged;
    public event Action<int, int> CoinsChanged;
    
    public bool HasGameSession =>
        YG2.saves.gameSession != null &&
        YG2.saves.gameSession.hasSession;

    public GameSessionSave GameSession =>
        YG2.saves.gameSession;
    
    public bool IntroDialogueCompleted =>
        YG2.saves.introDialogueCompleted;
    
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
    
    public bool OffAds()
    {
        YG2.saves.isShowAds = false;
        YG2.SaveProgress();
        return YG2.saves.isShowAds;
    }
    
    public void SetFirstMenuSeen()
    {
        if (YG2.saves.hasSeenFirstMenu)
            return;

        YG2.saves.hasSeenFirstMenu = true;
        YG2.SaveProgress();
    }
    
    public bool HasSeenFirstMenu()
    {
        return YG2.saves.hasSeenFirstMenu;
    }
    
    public void CompleteIntroDialogue()
    {
        YG2.saves.introDialogueCompleted = true;

        YG2.SaveProgress();
    }
    
    public int AddScore(int scoreToAdd)
    {
        YG2.saves.score += scoreToAdd;

        /*if (YG2.saves.score > YG2.saves.bestScore)
        {
            YG2.saves.bestScore = YG2.saves.score;
            YG2.SetLeaderboard("MyMergingBalls", YG2.saves.bestScore);
        }*/
        
        //YG2.SaveProgress();
        
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
    
    public bool IsBuyParrot
    {
        get { return YG2.saves.isBuyParrot; }
        set
        {
            YG2.saves.isBuyParrot = value;
            YG2.SaveProgress();
        }
    }
    
    public bool IsBuyBox
    {
        get { return YG2.saves.isBuyBox; }
        set
        {
            YG2.saves.isBuyBox = value;
            YG2.SaveProgress();
        }
    }
    
    public bool IsBuyMap
    {
        get { return YG2.saves.isBuyMap; }
        set
        {
            YG2.saves.isBuyMap = value;
            YG2.SaveProgress();
        }
    }
    
    public bool IsBuyLight
    {
        get { return YG2.saves.isBuyLight; }
        set
        {
            YG2.saves.isBuyLight = value;
            YG2.SaveProgress();
        }
    }
    
    public bool IsBuyFlag
    {
        get { return YG2.saves.isBuyFlag; }
        set
        {
            YG2.saves.isBuyFlag = value;
            YG2.SaveProgress();
        }
    }
    
    public void SaveGameSession(
        int score,
        int completedQuests,
        SavedBallData[] balls
    )
    {
        if (YG2.saves.gameSession == null)
        {
            YG2.saves.gameSession =
                new GameSessionSave();
        }

        GameSessionSave session =
            YG2.saves.gameSession;

        session.hasSession = true;
        session.score = score;
        session.completedQuests =
            completedQuests;

        session.balls =
            balls ?? Array.Empty<SavedBallData>();

        if (YG2.saves.score > YG2.saves.bestScore)
        {
            YG2.saves.bestScore = YG2.saves.score;
            YG2.SetLeaderboard("MyMergingBalls", YG2.saves.bestScore);
        }
        
        YG2.SaveProgress();
    }
    
    public void ClearGameSession()
    {
        YG2.saves.gameSession =
            new GameSessionSave();
        
        if (YG2.saves.score >= YG2.saves.bestScore)
        {
            YG2.SetLeaderboard("MyMergingBalls", YG2.saves.bestScore);
        }

        YG2.SaveProgress();
    }
    
    public void RestoreScore(int value)
    {
        YG2.saves.score =
            Mathf.Max(0, value);

        ScoreChanged?.Invoke(
            YG2.saves.score
        );
    }
    
    public void SaveProgress()
    {
        YG2.SaveProgress();
    }
}
