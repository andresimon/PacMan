using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map 
{
    public static readonly int mapdWidth = 28;
    public static readonly int mapHeight = 32;//36

    public GameObject[,] nodeObjects   = new GameObject[mapdWidth, mapHeight];
    public GameObject[,] pelletObjects = new GameObject[mapdWidth, mapHeight];
    public GameObject[] ghostObjects;

    public GameObject bonusItem;

    public int totalPellets = 0;
    
    public void Init()
    {
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));

        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;

            if (o.GetComponent<Node>() != null && o.tag != "GhostHome")
            {
                nodeObjects[(int)pos.x, (int)pos.y] = o;
            }
            else if (o.GetComponent<Pellet>() != null)
            {
                totalPellets++;
                pelletObjects[(int)pos.x, (int)pos.y] = o;
            }
        }

        ghostObjects = GameObject.FindGameObjectsWithTag("Ghost");
    }

    public Node GetNodeAtPosition(Vector2 pos)
    {
        GameObject tile = nodeObjects[(int)pos.x, (int)pos.y];
        if (tile != null)
            return tile.GetComponent<Node>();

        return null;
    }

    public GameObject GetPelletAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject obj = pelletObjects[tileX, tileY];
        if ( obj != null && obj.GetComponent<Pellet>() != null )
            return obj;

        return null;
    }

    public GameObject GetPortal(Vector2 pos)
    {
        GameObject tile = nodeObjects[(int)pos.x, (int)pos.y];

        if (tile != null)
        {
            if (tile.GetComponent<Teleporter>() != null)
            {
                GameObject destination = tile.GetComponent<Teleporter>().destination;
                return destination;
            }
        }

        return null;
    }

    float LengthFromNode(Vector2 previous, Vector2 target)
    {
        Vector2 vec = target - previous;
        return vec.sqrMagnitude;
    }

    public bool OverShotTarget(Vector2 previous, Vector2 current, Vector2 target)
    {
        float nodeToTarget = LengthFromNode(previous, target);
        float nodeToSelf = LengthFromNode(previous, current);

        return nodeToSelf > nodeToTarget;
    }

    public void ResetPelletsForPlayer(int playerNum)
    {
        foreach (GameObject o in pelletObjects)
        {
            if (o != null && o.GetComponent<Pellet>() != null)
            {
                if (playerNum == 1)
                    o.GetComponent<Pellet>().didConsumePlayerOne = false;
                else
                    o.GetComponent<Pellet>().didConsumePlayerTwo = false;
            }
        }
    }

    public void RedrawPellets()
    {
        foreach (GameObject o in pelletObjects)
        {
            if (o != null && o.GetComponent<Pellet>() != null)
            {
                if (GameManager.instance.isPlayerOneUp)
                    o.GetComponent<SpriteRenderer>().enabled = !o.GetComponent<Pellet>().didConsumePlayerOne;
                else
                    o.GetComponent<SpriteRenderer>().enabled = !o.GetComponent<Pellet>().didConsumePlayerTwo;
            }
        }
    }

}
