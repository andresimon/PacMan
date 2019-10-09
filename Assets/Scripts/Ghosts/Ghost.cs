using UnityEngine;

public abstract class Ghost : MonoBehaviour
{
    public enum GhostType
    {
        Red,
        Pink,
        Blue,
        Orange
    }

    public GhostType ghostType = GhostType.Red;

    public int releaseTime = 0;
    private float ghostReleaseTimer = 0;

    public bool canMove = true;

    public Node startingPosition;
    public Node homeNode;
    public Node ghostHouse;
    public Node houseEntrance;
    public bool isInGhostHouse = false;

    public enum Mode
    {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }

    Mode currentMode, previousMode;
    private int[] scatterModeTimes = { 7, 7, 5, 5 };
    private int[] chaseModeTimes = { 20, 20, 20 };
    int modeChangeIteration = 1;
    float modeChangeTimer = 0;

    private float levelSpeedIncrement = 0f;

    [Header("Animations")]
    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostWhite;
    public RuntimeAnimatorController ghostScared;

    [Header("Mode Behaviors")]
    public ModeBehavior chaseBehavior;
    public ModeBehavior scatterBehavior;
    public ModeBehavior frightenedBehavior;
    public ModeBehavior consumedBehavior;
    ModeBehavior currentModeBehavior;

    [Header("Frightened Mode")]
    public int frightenedDuration = 10;
    public int startingBlinkingAt = 7;
    private float blinkTimer = 0;
    private float frightenedModeTimer = 0;
    private bool frightenedIsWhite = false;

    protected GameObject pacMan;

    private Node currentNode, targetNode, previousNode;
    [HideInInspector] public Vector2 direction;
    private Vector2 nextDirection;
   
    private DifficultyManager diffManager;

    public Node GetCurrentNode() { return currentNode; }

    void Start()
    {
        pacMan = GameManager.instance.pacMan;

        diffManager = GameObject.FindObjectOfType<DifficultyManager>();

        ChangeMode(Mode.Scatter);

        SetInitialNodes();

        SetDifficultyForLevel(GameManager.playerLevel[(GameManager.instance.isPlayerOneUp ? 0 : 1)]);
    }

    void Update()
    {
        if (!canMove) return;

        UpdateMode();

        Move();

        ReleaseGhosts();

        CheckCollision();

        CheckIsInGhostHouse();
    }

    void ChangeMode(Mode m)
    {
        if (currentMode != m)
        {
            previousMode = currentMode;
            currentMode = m;

            switch (m)
            {
                case Mode.Chase:
                    currentModeBehavior = chaseBehavior;
                    break;
                case Mode.Scatter:
                    currentModeBehavior = scatterBehavior;
                    break;
                case Mode.Frightened:
                    currentModeBehavior = frightenedBehavior;
                    break;
                case Mode.Consumed:
                    currentModeBehavior = consumedBehavior;
                    break;
                default:
                    break;
            }
        }

        UpdateAnimatorController();
    }

    void SetInitialNodes()
    {
        currentNode = startingPosition;

        if (isInGhostHouse)
        {
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }

        previousNode = currentNode;

        UpdateAnimatorController();
    }

    public void SetDifficultyForLevel(int level)
    {
        LevelDifficulty diff = diffManager.GetDifficulty(level);

        if ( diff != null )
        {
            scatterModeTimes = diff.scatterModeTimes;
            chaseModeTimes   = diff.chaseModeTimes;

            frightenedDuration = diff.frightenedDuration;
            startingBlinkingAt = diff.startingBlinkingAt;

            switch (ghostType)
            {
                case GhostType.Red:
                    break;
                case GhostType.Pink:
                    releaseTime = diff.pinkReleaseTime;
                    break;
                case GhostType.Blue:
                    releaseTime = diff.blueReleaseTime;
                    break;
                case GhostType.Orange:
                    releaseTime = diff.orangeReleaseTime;
                    break;
                default:
                    break;
            }

            if (currentMode == Mode.Consumed)
                levelSpeedIncrement = diff.consumedSpeedIncrement;
            else
                levelSpeedIncrement = diff.speedIncrement;
        }
    }

    void UpdateAnimatorController()
    {
        currentModeBehavior.UpdateAnimatorController(gameObject);
    }

    void UpdateMode()
    {
        if (currentMode != Mode.Frightened)
        {
            modeChangeTimer += Time.deltaTime;

            if (modeChangeIteration < 4)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimes[modeChangeIteration - 1])
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }

                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimes[modeChangeIteration - 1])
                {
                    modeChangeIteration++;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 4)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimes[modeChangeIteration - 1])
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        }
        else if (currentMode == Mode.Frightened)
        {
            UpdateFrightenedMode();
        }
    }

    void UpdateFrightenedMode()
    {
        frightenedModeTimer += Time.deltaTime;

        if (frightenedModeTimer >= frightenedDuration)
        {
            AudioManager.instance.Play(GameManager.instance.backgroundSound);

            frightenedModeTimer = 0;

            ChangeMode(previousMode);
        }

        if (frightenedModeTimer >= startingBlinkingAt)
        {
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= 0.1f)
            {
                blinkTimer = 0;

                if (frightenedIsWhite)
                {
                    transform.GetComponent<Animator>().runtimeAnimatorController = ghostScared;
                    frightenedIsWhite = false;
                }
                else
                {
                    transform.GetComponent<Animator>().runtimeAnimatorController = ghostWhite;
                    frightenedIsWhite = true;
                }
            }
        }
    }

    Node ChooseNextNode()
    {
        return currentModeBehavior.ChooseNextNode(gameObject);
    }

    void Move()
    {
        if (targetNode != currentNode && targetNode != null && !isInGhostHouse)
        {
            if (OverShotTarget())
            {
                currentNode = targetNode;

                transform.localPosition = currentNode.transform.position;

                GameObject destinationTeleporter = GameManager.instance.map.GetPortal(currentNode.transform.position);
                if (destinationTeleporter != null)
                {
                    transform.localPosition = destinationTeleporter.transform.position;
                    currentNode = destinationTeleporter.GetComponent<Node>();
                }

                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;

                UpdateAnimatorController();
            }
            else
            {
                float finalSpeed = levelSpeedIncrement + currentModeBehavior.speed;
                transform.localPosition += (Vector3)(direction * finalSpeed) * Time.deltaTime;
            }
        }
    }

    void ReleaseGhosts()
    {
        ghostReleaseTimer += Time.deltaTime;

        if (ghostReleaseTimer > releaseTime)
        {
            if (ghostType != GhostType.Red && isInGhostHouse)
                isInGhostHouse = false;
        }
    }

    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (ghostRect.Overlaps(pacManRect))
        {
            if (currentMode == Mode.Frightened)
            {
                Consumed();
            }
            else
            {
                if (currentMode != Mode.Consumed)
                    GameManager.instance.StartDeath();
            }
        }
    }

    void CheckIsInGhostHouse()
    {
        if (currentMode != Mode.Consumed) return;

        Node node = GameManager.instance.map.GetNodeAtPosition(transform.position);
        if ( node != null && node.isGhostHouse )
        {
            currentNode = node;

            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];

            previousNode = currentNode;

            ChangeMode(Mode.Chase);

            UpdateAnimatorController();
        }
    }

    public abstract Vector2 GetGhostTargetTile();

    public void MoveToStartingPosition()
    {
        if (ghostType != GhostType.Red)
            isInGhostHouse = true;

        transform.position = startingPosition.transform.position;

        if (isInGhostHouse)
            direction = Vector2.up;
        else
            direction = Vector2.left;

        UpdateAnimatorController();
    }

    public void Restart()
    {
        canMove = true;

        ChangeMode(Mode.Scatter);

        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;

        SetInitialNodes();
    }

    public void StartFrightenedMode()
    {
        if (currentMode != Mode.Consumed)
        {
            GameManager.instance.ghostConsumedRunningScore = 200;

            frightenedModeTimer = 0;

            AudioManager.instance.Play(GameManager.instance.scaredSound);

            ChangeMode(Mode.Frightened);
        }
    }

    void Consumed()
    {
        GameManager.instance.IncrementScore(GameManager.instance.ghostConsumedRunningScore);

        ChangeMode(Mode.Consumed);

        GameManager.instance.StartConsumed(this.GetComponent<Ghost>());

        GameManager.instance.ghostConsumedRunningScore = GameManager.instance.ghostConsumedRunningScore * 2;
    }

    bool OverShotTarget()
    {
        return GameManager.instance.map.OverShotTarget(previousNode.transform.position, transform.position, targetNode.transform.position);
    }

    public float GetDistance(Vector2 posA, Vector2 posB)
    {
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

}
