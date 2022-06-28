using UnityEngine;

public class HealthIndicator : MonoBehaviour
{
    // Unity variables
    [SerializeField] private GameObject[] indicators;

    // managers
    private GameManager gameManager;

    // variables
    private int prevHealth;

    // Start is called before the first frame update
    void Start()
    {
        // managers
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // variables
        prevHealth = gameManager.GetHealth();

        // sets initial state of health
        UpdateHealthIndicator();
    }

    private void Update()
    {
        // updates health indicator if health changes without reloading scene (e.g. restoration station)
        if (prevHealth != gameManager.GetHealth())
            UpdateHealthIndicator();

        // set previous state of health at the end
        prevHealth = gameManager.GetHealth();
    }

    /// <summary>
    /// sets health indicator to display current amount of health (0 through 9)
    /// </summary>
    private void UpdateHealthIndicator()
    {
        switch (gameManager.GetHealth())
        {
            case 9:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(true);
                indicators[5].SetActive(true);
                indicators[6].SetActive(true);
                indicators[7].SetActive(true);
                indicators[8].SetActive(true);
                break;
            case 8:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(true);
                indicators[5].SetActive(true);
                indicators[6].SetActive(true);
                indicators[7].SetActive(true);
                indicators[8].SetActive(false);
                break;
            case 7:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(true);
                indicators[5].SetActive(true);
                indicators[6].SetActive(true);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 6:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(true);
                indicators[5].SetActive(true);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 5:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(true);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 4:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(true);
                indicators[4].SetActive(false);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 3:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(true);
                indicators[3].SetActive(false);
                indicators[4].SetActive(false);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 2:
                indicators[0].SetActive(true);
                indicators[1].SetActive(true);
                indicators[2].SetActive(false);
                indicators[3].SetActive(false);
                indicators[4].SetActive(false);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 1:
                indicators[0].SetActive(true);
                indicators[1].SetActive(false);
                indicators[2].SetActive(false);
                indicators[3].SetActive(false);
                indicators[4].SetActive(false);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            case 0:
                indicators[0].SetActive(false);
                indicators[1].SetActive(false);
                indicators[2].SetActive(false);
                indicators[3].SetActive(false);
                indicators[4].SetActive(false);
                indicators[5].SetActive(false);
                indicators[6].SetActive(false);
                indicators[7].SetActive(false);
                indicators[8].SetActive(false);
                break;
            default:
                throw new System.Exception("invalid health amount");
        }
    }

}
