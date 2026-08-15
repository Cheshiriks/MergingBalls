using System;

[Serializable]
public class GameSessionSave
{
    public bool hasSession = false;

    public int score = 0;
    public int completedQuests = 0;

    public SavedBallData[] balls =
        Array.Empty<SavedBallData>();
}

[Serializable]
public class SavedBallData
{
    public int level;

    public float positionX;
    public float positionY;
}
