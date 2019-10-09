using UnityEngine;

public class Node : MonoBehaviour
{
    // Nodes work like waypoints where PackMan/Ghosts are allowed to change direction
    public Node[] neighbors;

    [HideInInspector] public Vector2[] validDirections;

    public bool isGhostHouseEntrance = false;
    public bool isGhostHouse = false;

    void Start()
    {
        validDirections = new Vector2[neighbors.Length];

        for (int i = 0; i < neighbors.Length; i++)
        {
            Node neighbor = neighbors[i];
            Vector2 tempVector = neighbor.transform.localPosition - transform.localPosition;

            validDirections[i] = tempVector.normalized;
        }
    }

}
