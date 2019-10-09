using UnityEngine;

public class Blinky : Ghost
{
    public override Vector2 GetGhostTargetTile()
    {
        //-  Blinky's target tile in Chase mode is defined as Pac-Man's current tile

        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));

        return targetTile;
    }

}
