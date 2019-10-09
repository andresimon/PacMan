using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour
{
    private Node startingPosition;
    private Node currentNode, previousNode, targetNode;

    public float moveSpeed = 6.0f;
    public bool canMove = true;

    public Sprite idleSprite;

    Vector2 direction = Vector2.zero;
    Vector2 nextDirection;
    [HideInInspector] public Vector2 orientation;

    private AudioSource audioSource;

    [Header("Animations")]
    public RuntimeAnimatorController chompAnimation;
    public RuntimeAnimatorController deathAnimation;

    private bool playedChomp1 = false;

    [Header("Sounds")]
    public AudioClip chomp1;
    public AudioClip chomp2;
    public AudioClip deathSound;

    private DifficultyManager diffManager;

    void Start()
    {
        audioSource = transform.GetComponent<AudioSource>();

        diffManager = GameObject.FindObjectOfType<DifficultyManager>();

        Node node = GameManager.instance.map.GetNodeAtPosition(transform.localPosition);

        startingPosition = node;

        if ( node != null )
            currentNode = node;

        direction = Vector2.left;
        orientation = Vector2.left;

        ChangePosition(direction);

        UpdateRotation();

        SetDifficultyForLevel(GameManager.playerLevel[(GameManager.instance.isPlayerOneUp ? 0 : 1)]);
    }

    public void SetDifficultyForLevel(int level)
    {
        LevelDifficulty diff = diffManager.GetDifficulty(level);

        if (diff != null)
        {
            moveSpeed = diff.pacManSpeed;
        }
    }

    public void MoveToStartingPosition()
    {
        transform.position = startingPosition.transform.position;

        transform.GetComponent<SpriteRenderer>().sprite = idleSprite;

        direction = Vector2.left;
        orientation = direction;

        UpdateRotation();
    }

    public void Restart()
    {
        canMove = true;
        currentNode = startingPosition;
        nextDirection = Vector2.left;

        GetComponent<Animator>().runtimeAnimatorController = chompAnimation;
        GetComponent<Animator>().enabled = true;

        ChangePosition(direction);
    }

    void Update()
    {
        if (!canMove) return;

        CheckInput();

        Move();

        UpdateRotation();

        UpdateAnimationState();

        ConsumePellet();
    }

    void PlayChompSound()
    {
        if ( playedChomp1 )
        {
            audioSource.PlayOneShot(chomp2);
            playedChomp1 = false;
        }
        else
        {
            audioSource.PlayOneShot(chomp1);
            playedChomp1 = true;
        }
    }

    void CheckInput()
    {
        if ( Input.GetKeyDown(KeyCode.LeftArrow) )
        {
            ChangePosition(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangePosition(Vector2.right);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangePosition(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(Vector2.down);
        }
    }

    void ChangePosition(Vector2 dir)
    {
        if ( dir != direction )
            nextDirection = dir;

        if ( currentNode != null )
        {
            Node nodeToMove = CanMove(dir);

            if ( nodeToMove != null )
            {
                direction = dir;
                targetNode = nodeToMove;
                previousNode = currentNode;
                currentNode = null;
            }
        }
    }

    void Move ()
    {
        if ( targetNode != currentNode && targetNode != null )
        {
            if ( nextDirection == direction * -1)
            {
                direction *= -1;

                Node tempNode = targetNode;
                targetNode = previousNode;
                previousNode = tempNode;
            }

            if ( OverShotTarget() )
            {     
                currentNode = targetNode;

                transform.localPosition = currentNode.transform.position;

                GameObject destinationTeleporter = GameManager.instance.map.GetPortal(currentNode.transform.position);
                if ( destinationTeleporter != null )
                {
                    transform.localPosition = destinationTeleporter.transform.position;
                    currentNode = destinationTeleporter.GetComponent<Node>();
                }

                Node nodeToMove = CanMove(nextDirection);

                if ( nodeToMove != null )
                    direction = nextDirection;

                if ( nodeToMove == null )
                    nodeToMove = CanMove(direction);

                if (nodeToMove != null)
                {
                    targetNode = nodeToMove;
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    direction = Vector2.zero;
                }
            }
            else
            {
                transform.localPosition += (Vector3)(direction * moveSpeed) * Time.deltaTime;
            }
        }
    }

    void UpdateAnimationState()
    {
        if ( direction == Vector2.zero )
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = idleSprite;
        }
        else
        {
            GetComponent<Animator>().enabled = true;
        }
    }

    void UpdateRotation()
    {
        if (direction == Vector2.left)
        {
           transform.localScale = new Vector3(-1, 1, 1);
           transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (direction == Vector2.right)
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (direction == Vector2.up)
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else if (direction == Vector2.down)
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
        }

        orientation = direction;
    }

    void MoveToNode(Vector2 dir)
    {
        Node nodeToMove = CanMove(dir);
        if (nodeToMove != null)
        {
            transform.localPosition = nodeToMove.transform.position;
            currentNode = nodeToMove;
        }
    }

    void ConsumePellet()
    {
        GameObject o = GameManager.instance.map.GetPelletAtPosition(transform.position);
        if (o != null)
        {
            bool didConsumed = false;

            Pellet pellet = o.GetComponent<Pellet>();

            if (GameManager.instance.isPlayerOneUp)
            {
                if (!pellet.didConsumePlayerOne)
                {
                    GameManager.instance.pelletsConsumed[0] += 1;
                    pellet.didConsumePlayerOne = true;

                    didConsumed = true;
                }
            }
            else
            {
                if (!pellet.didConsumePlayerTwo)
                {
                    GameManager.instance.pelletsConsumed[1] += 1;
                    pellet.didConsumePlayerTwo = true;

                    didConsumed = true;
                }
            }

            if ( didConsumed )
            {
                o.GetComponent<SpriteRenderer>().enabled = false;

                PlayChompSound();

                if (pellet.isSuperPellet)
                {
                    GameManager.instance.IncrementScore(50);

                    GameObject[] ghosts = GameManager.instance.map.ghostObjects;
                    foreach (GameObject go in ghosts)
                    {
                        go.GetComponent<Ghost>().StartFrightenedMode();
                    }
                }
                else
                {
                    GameManager.instance.IncrementScore(10);
                }
            }
        }
    }

    public void ConsumeBonusItem(GameObject bonusItem)
    {
        int playerIndex = (GameManager.instance.isPlayerOneUp ? 0 : 1);

        GameManager.instance.IncrementScore(bonusItem.GetComponent<BonusItem>().data.pointValue);

        GameManager.instance.StartConsumedBonusItem(bonusItem);
    }

    Node CanMove (Vector2 dir)
    {
        Node nodeToMove = null;

        for (int i = 0; i < currentNode.validDirections.Length; i++)
        {
            if ( currentNode.validDirections[i] == dir && !currentNode.neighbors[i].isGhostHouse)
            {
                nodeToMove = currentNode.neighbors[i];
                break;
            }
        }
        return nodeToMove;
    }

    bool OverShotTarget()
    {
        return GameManager.instance.map.OverShotTarget(previousNode.transform.position, transform.position, targetNode.transform.position);
    }
}
