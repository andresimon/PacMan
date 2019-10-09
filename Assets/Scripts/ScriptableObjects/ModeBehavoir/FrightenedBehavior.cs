using UnityEngine;

[CreateAssetMenu(fileName = "FrightenedBehavior", menuName = "Mode Behaviors/Frightened", order = 51)]
public class FrightenedBehavior : ModeBehavior
{
    public override void UpdateAnimatorController(GameObject ghost)
    {
       ghost.GetComponent<Animator>().runtimeAnimatorController = ghost.GetComponent<Ghost>().ghostScared;
    }

    public override Node ChooseNextNode(GameObject ghost)
    {
        Vector2 targetTile = Vector2.zero;

        targetTile = GetRandomTile();

        Node nodeToMove = null;

        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        Node currentNode = ghost.GetComponent<Ghost>().GetCurrentNode();
        Vector2 direction = ghost.GetComponent<Ghost>().direction;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if (currentNode.validDirections[i] != direction * -1)
            {
                GameObject tile = GetTileAtPosition(currentNode.transform.position);

                if (tile.transform.GetComponent<Node>().isGhostHouseEntrance)
                {
                    // Found ghost house entrance, so don't allow to go inside
                    if (currentNode.validDirections[i] != Vector2.down)
                    {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                }
                else
                {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }
            }
        }

        if (foundNodes.Length == 1)
        {
            nodeToMove = foundNodes[0];
            direction = foundNodesDirection[0];
        }
        else if (foundNodes.Length > 1)
        {
            float leastDistance = 10000f;

            for (int i = 0; i < foundNodes.Length; i++)
            {
                if (foundNodesDirection[i] != Vector2.zero)
                {
                    float distance = ghost.GetComponent<Ghost>().GetDistance(foundNodes[i].transform.position, targetTile);

                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        nodeToMove = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }

        ghost.GetComponent<Ghost>().direction = direction;

        return nodeToMove;
    }

}
