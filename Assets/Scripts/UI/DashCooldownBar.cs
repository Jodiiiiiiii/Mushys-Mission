using UnityEngine;
using UnityEngine.UI;

public class DashCooldownBar : MonoBehaviour
{
    // bars
    [SerializeField] private Image loadingBar;
    [SerializeField] private Image readyBar;
    
    // player controller
    private PlayerController playerController;

    // Start is called before the first frame update
    private void Start()
    {
        // instantiation
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        // display loading bar to show remaining cooldown
        loadingBar.rectTransform.sizeDelta = new Vector2(readyBar.rectTransform.rect.width * (1 - playerController.GetDashCooldownPercentage()), loadingBar.rectTransform.rect.height);

        // display ready bar on cooldown complete and remove ready bar when new cooldown starts
        if (playerController.GetDashCooldownPercentage() == 0)
            readyBar.gameObject.SetActive(true);
        else
            readyBar.gameObject.SetActive(false);
    }
}
