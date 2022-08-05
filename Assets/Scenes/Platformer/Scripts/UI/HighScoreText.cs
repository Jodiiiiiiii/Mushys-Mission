using TMPro;
using UnityEngine;

public class HighScoreText : MonoBehaviour
{
    // game manager
    GameManager gameManager;

    // components
    private TextMeshProUGUI text;

    // variables
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        // game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // components
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // variables
        time = gameManager.GetBestTime();

        if(gameManager.GetHighScore() != -1)
        {
            // set text to proper high score (without time display)
            text.SetText("High Score: " + gameManager.GetHighScore() + "/25 (" 
                + Mathf.Floor(time / 60) + ":"
                + (time % 60 < 10 ? "0" : "") + Mathf.Floor(time % 60) + ")");
        }
        
    }
}
