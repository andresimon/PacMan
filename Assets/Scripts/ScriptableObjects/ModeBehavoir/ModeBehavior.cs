using UnityEngine;

public abstract class ModeBehavior : ScriptableObject, IBehavior
{
    public float speed;
    
    public abstract void UpdateAnimatorController(GameObject ghost);

    public abstract Node ChooseNextNode(GameObject ghost);

    public Vector2 GetRandomTile()
    {
        int x = Random.Range(0, Map.mapdWidth); 
        int y = Random.Range(0, Map.mapHeight); 

        return new Vector2(x, y);
    }

    public GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameManager.instance.map.nodeObjects[tileX, tileY];
        if (tile != null)
        {
            return tile;
        }
        return null;
    }

}
