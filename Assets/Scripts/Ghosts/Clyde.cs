using UnityEngine;

public class Clyde : Ghost
{
    public override Vector2 GetGhostTargetTile()
    {
        // Calcule the distance from Pac-Man
        // If the distance is greater than 8 tiles, targeting is the same as Blinky (so target Pac-Man)
        // If the distance is less than 8 tiles, then target is their home node, so same as Scatter Mode

        Vector2 pacManPosition = pacMan.transform.localPosition;

        float distance = GetDistance(transform.localPosition, pacManPosition);

        Vector2 targetTile = Vector2.zero;

        if (distance > 8)
        {
            targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        }
        else
        {
            targetTile = homeNode.transform.position;
        }

        return targetTile;
    }

}
