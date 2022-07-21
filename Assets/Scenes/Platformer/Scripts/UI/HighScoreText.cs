using TMPro;
using UnityEngine;

public class HighScoreText : MonoBehaviour
{
    // game manager
    GameManager gameManager;

    // components
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        // game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // components
        text = GetComponent<TextMeshProUGUI>();

        // set text to proper high score
        text.SetText("High Score: " + gameManager.GetHighScore());
    }
}
