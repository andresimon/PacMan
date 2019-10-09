using UnityEngine;

[CreateAssetMenu(fileName = "LevelDifficulty", menuName = "Level Difficulty", order = 52)]
public class LevelDifficulty : ScriptableObject
{
    public new string name;
    public int level;
    [Space(10)]

    public int pacManSpeed = 0;
    [Space(10)]

    public int[] scatterModeTimes = { 7, 7, 5, 5 };
    public int[] chaseModeTimes = { 20, 20, 20 };

    [Space(10)]
    public int frightenedDuration = 10;
    public int startingBlinkingAt = 7;

    [Space(10)]
    public int pinkReleaseTime = 5; 
    public int blueReleaseTime = 14; 
    public int orangeReleaseTime = 21;

    [Header("Ghost Speed")]
    public float speedIncrement = 0f;
    public float consumedSpeedIncrement = 0f;
}
