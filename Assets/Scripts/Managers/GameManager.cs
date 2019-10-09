using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Map map;
    [HideInInspector] public GameObject pacMan;
    [HideInInspector] public GameObject[] ghosts;

    private BonusItemManager biManager;

    private bool didStartDeath = false;
    private bool didStartConsumed = false;

    public static int[] playerLevel = { 1, 1 };

    public static int[] playerScore = { 0, 0 };

    public static int[] playerLives = { 3, 3 };

    public int[] pelletsConsumed = { 0, 0 };

    public bool isPlayerOneUp = true;

    //public GameObject bonusItem;

    [HideInInspector] public int ghostConsumedRunningScore;

    public bool shouldBlink = false;
    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;

    public Sprite mazeBlue;
    public Sprite mazeWhite;

    public Image[] levelImages;
    private bool didIncrementLevel = false;

    [Header("Canvas")]
    public Text playerText;
    public Text readyText;
    public Text highScoreText;
    public Text playerOneUpText;
    public Text playerTwoUpText;
    public Text playerOneScoreText;
    public Text playerTwoScoreText;

    public Image playerLives2;
    public Image playerLives3;

    public Text consumedGhostScoreText;

    [Header("Sounds")]
    public AudioClip scaredSound;
    public AudioClip backgroundSound;
    public AudioClip consumedGhostSound;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        map = new Map();
        map.Init();

        biManager = GetComponent<BonusItemManager>();

        pacMan = GameObject.Find("PacMan");

        ghosts = map.ghostObjects;

        if ( playerLevel[(isPlayerOneUp ? 0 : 1)] == 1 )
            AudioManager.instance.Play();

        StartGame();
    }

    void Update()
    {
        UpdateUI();

        CheckPelletsConsumed();

        CheckShouldBlink();

        BonusItems();
    }

    public void StartGame()
    {
        playerTwoUpText.enabled = !Menu.isOnePlayerGame;
        playerTwoScoreText.enabled = !Menu.isOnePlayerGame;

        if ( !Menu.isOnePlayerGame )
        {
            if (isPlayerOneUp)
                playerText.text = "PLAYER 1";
            else
                playerText.text = "PLAYER 2";
        }

        if (isPlayerOneUp)
            StartCoroutine(C_StartBlinking(playerOneUpText));
        else
            StartCoroutine(C_StartBlinking(playerTwoUpText));

        //- Hide all ghosts and PacMan
        foreach (GameObject o in ghosts)
        {
            o.GetComponent<SpriteRenderer>().enabled = false;
            o.GetComponent<Ghost>().canMove = false;
        }

        pacMan.GetComponent<SpriteRenderer>().enabled = false;
        pacMan.GetComponent<PacMan>().canMove = false;

        StartCoroutine(C_ShowObjectsAfter(2.25f));
    }

    public void StartConsumedBonusItem(GameObject bonusItem)
    {
        Vector2 pos = bonusItem.transform.position;
        Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

        // Re-using same float text of ghosts
        consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
        consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

        consumedGhostScoreText.text = bonusItem.GetComponent<BonusItem>().data.pointValue.ToString();

        consumedGhostScoreText.enabled = true;

        Destroy(bonusItem.gameObject);

        StartCoroutine(C_ConsumeBonusItem(0.75f));
    }

    IEnumerator C_ConsumeBonusItem(float delay)
    {
        yield return new WaitForSeconds(delay);

        consumedGhostScoreText.enabled = false;
    }

    IEnumerator C_StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);

        blinkText.enabled = !blinkText.enabled;

        StartCoroutine(C_StartBlinking(blinkText));
    }

    IEnumerator C_ShowObjectsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        //- Shows all ghosts and PacMan
        foreach (GameObject o in ghosts)
            o.GetComponent<SpriteRenderer>().enabled = true;

        pacMan.GetComponent<SpriteRenderer>().enabled = true;

        playerText.enabled = false;

        StartCoroutine(C_StartGameAfter(2));
    }

    IEnumerator C_StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject o in ghosts)
            o.GetComponent<Ghost>().canMove = true;

        pacMan.GetComponent<PacMan>().canMove = true;

        readyText.enabled = false;

        AudioManager.instance.Play(backgroundSound);
    }

    void UpdateUI()
    {
        int currentLevel = playerLevel[((isPlayerOneUp ? 0 : 1))];

        playerOneScoreText.text = playerScore[0].ToString();
        playerTwoScoreText.text = playerScore[1].ToString();

        playerLives3.enabled = (playerLives[(isPlayerOneUp ? 0 : 1)] - 3 >= 0);
        playerLives2.enabled = (playerLives[(isPlayerOneUp ? 0 : 1)] - 2 >= 0);

        for (int i = 0; i < levelImages.Length; i++)
        {
            Image li = levelImages[i];
            li.enabled = false;
        }

        for (int i = 1; i < levelImages.Length + 1; i++)
        {
            if ( currentLevel >= i )
            {
                Image li = levelImages[i-1];
                li.enabled = true;
            }
        }
    }

    void CheckPelletsConsumed()
    {
        if (isPlayerOneUp) //- Player 1 is playing
        {
            if (pelletsConsumed[0] == map.totalPellets)
            {
                PlayerWin(1);
            }
        }
        else //- Player 2 is playing
        {
            if (pelletsConsumed[1] == map.totalPellets)
            {
                PlayerWin(2);
            }
        }
    }

    void PlayerWin(int playerNum)
    {
        if (!didIncrementLevel)
        {
            didIncrementLevel = true;
            playerLevel[playerNum - 1]++;
        }

        StartCoroutine(C_ProcessWin(2));
    }

    IEnumerator C_ProcessWin(float delay)
    {
        pacMan.GetComponent<PacMan>().canMove = false;
        pacMan.GetComponent<Animator>().enabled = false;

        AudioManager.instance.Stop();

        foreach (GameObject o in ghosts)
        {
            o.GetComponent<Ghost>().canMove = false;
            o.GetComponent<Animator>().enabled = false;
        }

        yield return new WaitForSeconds(delay);

        StartCoroutine(C_BlinkMap(2));
    }

    IEnumerator C_BlinkMap(float delay)
    {
        pacMan.GetComponent<SpriteRenderer>().enabled = false;

        foreach (GameObject o in ghosts)
            o.GetComponent<SpriteRenderer>().enabled = false;

        //- Blink map
        shouldBlink = true;

        yield return new WaitForSeconds(delay);

        shouldBlink = false;

        //- Restart the game at the next level
        StartNextLevel();
    }

    private void StartNextLevel()
    {
        StopAllCoroutines();

        map.ResetPelletsForPlayer((isPlayerOneUp ? 1 : 2));

        int playerIndex = (isPlayerOneUp ? 0 : 1);

        pelletsConsumed[playerIndex] = 0;

        biManager.didSpawnBonusItem1[playerIndex] = false;
        biManager.didSpawnBonusItem2[playerIndex] = false;

        GameObject.Find("Maze").GetComponent<SpriteRenderer>().sprite = mazeBlue;

        didIncrementLevel = false;

        StartCoroutine(C_StartNextLevel(1));
    }

    IEnumerator C_StartNextLevel(float delay)
    {
        playerText.enabled = true;
        readyText.enabled = true;

        if (isPlayerOneUp)
            StartCoroutine(C_StartBlinking(playerOneUpText));
        else
            StartCoroutine(C_StartBlinking(playerTwoUpText));

        map.RedrawPellets();

        yield return new WaitForSeconds(delay);

        StartCoroutine(C_ProcessRestartShowObjects(1));
    }

    IEnumerator C_ProcessConsumedAfter(float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        consumedGhostScoreText.enabled = false;

        pacMan.GetComponent<SpriteRenderer>().enabled = true;

        consumedGhost.GetComponent<SpriteRenderer>().enabled = true;

        foreach (GameObject o in ghosts)
        {
            o.GetComponent<Ghost>().canMove = true;
        }

        pacMan.GetComponent<PacMan>().canMove = true;

        AudioManager.instance.Play(); //background music

        didStartConsumed = false;

    }

    private void CheckShouldBlink()
    {
        if (shouldBlink)
        {
            if (blinkIntervalTimer < blinkIntervalTime)
            {
                blinkIntervalTimer += Time.deltaTime;
            }
            else
            {
                blinkIntervalTimer = 0;

                if (GameObject.Find("Maze").GetComponent<SpriteRenderer>().sprite == mazeBlue)
                {
                    GameObject.Find("Maze").GetComponent<SpriteRenderer>().sprite = mazeWhite;
                }
                else
                {
                    GameObject.Find("Maze").GetComponent<SpriteRenderer>().sprite = mazeBlue;
                }
            }
        }
    }

    public void StartConsumed(Ghost consumedGhost)
    {
        if (!didStartConsumed)
        {
            didStartConsumed = true;

            foreach (GameObject o in ghosts)
                o.GetComponent<Ghost>().canMove = false;

            pacMan.GetComponent<PacMan>().canMove = false;
            pacMan.GetComponent<SpriteRenderer>().enabled = false;

            consumedGhost.GetComponent<SpriteRenderer>().enabled = false;

            AudioManager.instance.Stop();

            Vector2 pos = consumedGhost.transform.position;
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostScoreText.text = ghostConsumedRunningScore.ToString();

            consumedGhostScoreText.enabled = true;

            AudioManager.instance.Play(consumedGhostSound);

            StartCoroutine(C_ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            StopAllCoroutines();

            if (Menu.isOnePlayerGame)
            {
                playerOneUpText.enabled = true;
            }
            else
            {
                playerOneUpText.enabled = true;
                playerTwoUpText.enabled = true;
            }

            if ( map.bonusItem != null )
                Destroy(map.bonusItem.gameObject);

            didStartDeath = true;

            foreach (GameObject o in ghosts)
                o.GetComponent<Ghost>().canMove = false;

            pacMan.transform.GetComponent<PacMan>().canMove = false;
            pacMan.GetComponent<Animator>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(C_ProcessDeathAter(2));
        }
    }

    IEnumerator C_ProcessDeathAter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject o in ghosts)
        {
            o.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(C_ProcessDeathAnimation(1.9f));
    }

    IEnumerator C_ProcessDeathAnimation(float delay)
    {
        pacMan.transform.localScale = new Vector3(1, 1, 1); // one
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.GetComponent<Animator>().runtimeAnimatorController = pacMan.GetComponent<PacMan>().deathAnimation;
        pacMan.GetComponent<Animator>().enabled = true;

        AudioManager.instance.Play(pacMan.GetComponent<PacMan>().deathSound);

        yield return new WaitForSeconds(delay);

        StartCoroutine(C_ProcessRestart(1));
    }

    void NewGame()
    {
        playerLevel[0] = playerLevel[1] = 1;

        playerScore[0] = playerScore[1] = 0;

        playerLives[0] = 3;
        playerLives[1] = (Menu.isOnePlayerGame ? -1 : 3);
    }

    IEnumerator C_ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        NewGame();

        SceneManager.LoadScene("Menu");
    }

    IEnumerator C_ProcessRestart(float delay)
    {
        playerLives[(isPlayerOneUp ? 0 : 1)] -= 1;

        if (playerLives[0] <= 0 && playerLives[1] <= 0)
        {
            playerText.enabled = true;

            readyText.text = "GAME OVER";
            readyText.color = Color.red;
            readyText.enabled = true;

            pacMan.GetComponent<SpriteRenderer>().enabled = false;

            AudioManager.instance.Stop();

            StartCoroutine(C_ProcessGameOver(2));
        }
        else
        {
            if (playerLives[0] == 0)
            {
                playerText.text = "PLAYER 1";

                readyText.text = "GAME OVER";
                readyText.color = Color.red;

                readyText.enabled = true;
                playerText.enabled = true;

                playerLives[0] -= 1;

                pacMan.GetComponent<SpriteRenderer>().enabled = false;

                AudioManager.instance.Stop();

                yield return new WaitForSeconds(delay);

            }
            else if (playerLives[1] == 0)
            {
                playerText.text = "PLAYER 2";

                readyText.text = "GAME OVER";
                readyText.color = Color.red;

                readyText.enabled = true;
                playerText.enabled = true;

                playerLives[1] -= 1;

                pacMan.GetComponent<SpriteRenderer>().enabled = false;

                AudioManager.instance.Stop();

                yield return new WaitForSeconds(delay);
            }

            pacMan.GetComponent<SpriteRenderer>().enabled = false;

            AudioManager.instance.Stop();

            if (!Menu.isOnePlayerGame)
                     isPlayerOneUp = !isPlayerOneUp;

                if (isPlayerOneUp)
                StartCoroutine(C_StartBlinking(playerOneUpText));
            else
                StartCoroutine(C_StartBlinking(playerTwoUpText));

            if (!Menu.isOnePlayerGame)
            {
                if (isPlayerOneUp)
                    playerText.text = "PLAYER 1";
                else
                    playerText.text = "PLAYER 2";
            }

            readyText.text = "READY!";
            readyText.color = new Color(240f / 255f, 207f / 255f, 101f / 255f);

            playerText.enabled = true;
            readyText.enabled = true;

            map.RedrawPellets();

            yield return new WaitForSeconds(delay);

            StartCoroutine(C_ProcessRestartShowObjects(1));
        }
    }

    IEnumerator C_ProcessRestartShowObjects(float delay)
    {
        playerText.enabled = false;

        //- Shows all ghosts and PacMan
        foreach (GameObject o in ghosts)
        {
            o.GetComponent<SpriteRenderer>().enabled = true;
            o.GetComponent<Animator>().enabled = true;
            o.GetComponent<Ghost>().MoveToStartingPosition();
        }

        pacMan.GetComponent<Animator>().enabled = false;
        pacMan.GetComponent<PacMan>().MoveToStartingPosition();
        pacMan.GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(delay);

        Restart();
    }

    public void Restart()
    {
        pacMan.GetComponent<PacMan>().SetDifficultyForLevel(playerLevel[(isPlayerOneUp ? 0 : 1)]);

        foreach (GameObject o in ghosts)
            o.GetComponent<Ghost>().SetDifficultyForLevel(playerLevel[(isPlayerOneUp ? 0 : 1)]);

        readyText.enabled = false;

        pacMan.transform.GetComponent<PacMan>().Restart();

        foreach (GameObject o in ghosts)
            o.GetComponent<Ghost>().Restart();

        AudioManager.instance.Play(backgroundSound);

        didStartDeath = false;
    }

    public void IncrementScore(int amount)
    {
        int playerIndex = (GameManager.instance.isPlayerOneUp ? 0 : 1);

        playerScore[playerIndex] += amount;
    }

    void BonusItems()
    {
        biManager.SpawnBonusItemForPlayer();
    }
}
