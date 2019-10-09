using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    public static bool isOnePlayerGame = true;

    public Text player1Text;
    public Text player2Text;
    public Text selectorText;

    void Start()
    {
        SetArrowPosition();

        StartCoroutine(C_StartBlinking(selectorText));  
    }

    void Update()
    {
        if ( Input.GetKeyUp(KeyCode.UpArrow) )
        {
            if ( !isOnePlayerGame )
            {
                isOnePlayerGame = true;
                SetArrowPosition();
            }
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (isOnePlayerGame)
            {
                isOnePlayerGame = false;
                SetArrowPosition();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Return))
        {
            GameManager.playerLives[0] = 3;
            GameManager.playerLives[1] = (isOnePlayerGame ? -1 : 3);

            SceneManager.LoadScene("Level");
        }
    }

    void SetArrowPosition()
    {
        Vector3 selectorPos = selectorText.transform.localPosition;
        float yPos = 0;

        if (isOnePlayerGame)
            yPos = player1Text.transform.localPosition.y;
        else
            yPos = player2Text.transform.localPosition.y;

        selectorText.transform.localPosition = new Vector3(selectorPos.x, yPos, selectorPos.z);
    }

    IEnumerator C_StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);

        blinkText.enabled = !blinkText.enabled;

        StartCoroutine(C_StartBlinking(blinkText));
    }
}
