using System.Collections.Generic;
using UnityEngine;

public class BonusItemManager : MonoBehaviour
{
    public BonusItemData[] bonusItems;

    private BonusItemData current;

    Dictionary<int, BonusItemData> bonusItemDict = new Dictionary<int, BonusItemData>();

    [HideInInspector] public bool[] didSpawnBonusItem1 = { false, false };
    [HideInInspector] public bool[] didSpawnBonusItem2 = { false, false };

    void Start()
    {
        for (int i = 0; i < bonusItems.Length; i++)
        {
            bonusItemDict.Add(bonusItems[i].level, bonusItems[i]);
        }    
    }

    public BonusItemData GetBonusItem(int level)
    {
        BonusItemData result = null;

        bonusItemDict.TryGetValue(level, out result);

        if (result == null)
            result = current;
        else
            current = result;

        return result;
    }

    public void SpawnBonusItemForPlayer()
    {
        int playerIndex = (GameManager.instance.isPlayerOneUp ? 0 : 1);
        int pelletsConsumed = GameManager.instance.pelletsConsumed[playerIndex];
        int level = GameManager.playerLevel[playerIndex];

        if (pelletsConsumed >= 70 && pelletsConsumed <= 170 && !didSpawnBonusItem1[playerIndex])
        {
            didSpawnBonusItem1[playerIndex] = true;
            SpawnBonusItem(level);
        }
        else if (pelletsConsumed > 170 && !didSpawnBonusItem2[playerIndex])
        {
            didSpawnBonusItem2[playerIndex] = true;
            SpawnBonusItem(level);
        }
    }

    void SpawnBonusItem(int level)
    {
        BonusItemData biData = GetBonusItem(level);
        if (biData == null) return;

        GameObject bonusItem = Resources.Load("Prefabs/BonusItem", typeof(GameObject)) as GameObject;

        if ( bonusItem != null )
        {
            bonusItem.GetComponent<BonusItem>().data = biData;
            bonusItem.GetComponent<SpriteRenderer>().sprite = bonusItem.GetComponent <BonusItem>().data.sprite;

            GameObject bi = Instantiate(bonusItem);

            GameManager.instance.map.bonusItem = bi;
        }
    }

}
