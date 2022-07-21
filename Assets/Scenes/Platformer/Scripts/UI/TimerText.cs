using TMPro;
using UnityEngine;

public class TimerText : MonoBehaviour
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

    // Update is called once per frame
    void Update()
    {
        // retrieve current time
        time = gameManager.GetTime();

        // Minutes:Seconds
        text.SetText("Time: " + Mathf.Floor(time / 60) + ":" 
            + (time % 60 < 10 ? "0":"") + Mathf.Floor(time % 60));
    }
}
