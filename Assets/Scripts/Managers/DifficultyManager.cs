using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public LevelDifficulty[] levelDifficulties;

    private LevelDifficulty current;

    Dictionary<int, LevelDifficulty> difficultyDict = new Dictionary<int, LevelDifficulty>();

    void Start()
    {
        for (int i = 0; i < levelDifficulties.Length; i++)
        {
            difficultyDict.Add(levelDifficulties[i].level, levelDifficulties[i]);
        }    
    }

    public LevelDifficulty GetDifficulty(int level)
    {
        LevelDifficulty result = null;

        difficultyDict.TryGetValue(level, out result);

        if (result == null)
            result = current;
        else
            current = result;

        return result;
    }
}
