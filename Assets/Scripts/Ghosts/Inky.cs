using UnityEngine;

public class Inky : Ghost
{
    public override Vector2 GetGhostTargetTile()
    {
        // Select the position 2 tiles in front of Pac-Man
        // Draw vector from Blinky to that position
        // Double the length of that vector

        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);
        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);

        Vector2 targetTile = pacManTile + (2 * pacManOrientation);

        // Temp vector for Blinky position
        Vector2 tempBlinkyPosition = GameObject.Find("Blinky").transform.position;
        int blinkyPostionX = Mathf.RoundToInt(tempBlinkyPosition.x);
        int blinkyPostionY = Mathf.RoundToInt(tempBlinkyPosition.y);
        tempBlinkyPosition = new Vector2(blinkyPostionX, blinkyPostionY);

        float distance = GetDistance(tempBlinkyPosition, targetTile) * 2;

        targetTile = new Vector2(tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);

        return targetTile;
    }

}
