using UnityEngine;

[CreateAssetMenu(fileName = "BonusItemData", menuName = "Bonus Item", order = 52)]
public class BonusItemData : ScriptableObject
{
    public int level;
    public Sprite sprite;
    public int pointValue;
}

