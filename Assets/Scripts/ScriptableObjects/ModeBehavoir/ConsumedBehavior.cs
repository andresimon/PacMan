using UnityEngine;

[CreateAssetMenu(fileName = "ConsumedBehavior", menuName = "Mode Behaviors/Consumed", order = 51)]
public class ConsumedBehavior : ModeBehavior
{
    [Header("Sprites")]
    public Sprite eyesUp;
    public Sprite eyesDown;
    public Sprite eyesLeft;
    public Sprite eyesRight;

    public override void UpdateAnimatorController(GameObject ghost)
    {
        Vector2 direction = ghost.GetComponent<Ghost>().direction;

        ghost.GetComponent<Animator>().runtimeAnimatorController = null;

        if (direction == Vector2.up)
            ghost.GetComponent<SpriteRenderer>().sprite = eyesUp;
        else if (direction == Vector2.down)
            ghost.GetComponent<SpriteRenderer>().sprite = eyesDown;
        else if (direction == Vector2.left)
            ghost.GetComponent<SpriteRenderer>().sprite = eyesLeft;
        else if (direction == Vector2.right)
            ghost.GetComponent<SpriteRenderer>().sprite = eyesRight;
    }

    public override Node ChooseNextNode(GameObject ghost)
    {
        Vector2 targetTile = Vector2.zero;

        Node currentNode = ghost.GetComponent<Ghost>().GetCurrentNode();

        if ( currentNode == ghost.GetComponent<Ghost>().houseEntrance)
            targetTile = ghost.GetComponent<Ghost>().ghostHouse.transform.position;
        else
            targetTile = ghost.GetComponent<Ghost>().houseEntrance.transform.position;

        Node nodeToMove = null;

        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

      //  N//entNode = ghost.GetComponent<Ghost>().GetCurrentNode();
        Vector2 direction = ghost.GetComponent<Ghost>().direction;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if (currentNode.validDirections[i] != direction * -1)
            {
                foundNodes[nodeCounter] = currentNode.neighbors[i];
                foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                nodeCounter++;
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