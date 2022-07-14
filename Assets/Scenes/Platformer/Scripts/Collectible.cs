using UnityEngine.SceneManagement;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    // constants
    private const float COLLECTION_WAIT_TIME = 5.0f;

    // game manager
    private GameManager gameManager;

    // components
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;

    // variables
    public bool isCollected { private set; get; }
    [SerializeField] private int index;
    private float collectionTimer;

    // Start is called before the first frame update
    void Start()
    {
        // game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // components
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // set collectible as active or not based on whether it has already been collected
        isCollected = gameManager.GetCollectibleState(index);
        if(isCollected)
        {
            Destroy(gameObject); // already collected, so just destroy it
        }
        collectionTimer = 0;

        // set listener for transition event so that claimed collectibles can be saved properly upon transition
        gameManager.transitionEvent.AddListener(SaveCollectionOnTransition);
    }

    private void Update()
    {
        if(isCollected)
        {
            // increments collection timer 
            if(collectionTimer < COLLECTION_WAIT_TIME)
            {
                collectionTimer += Time.deltaTime;
            }
            else
            {
                // updates game manager save state once collection timer has been met
                gameManager.SetCollectibleState(index, isCollected);

                // game object is done being useful
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// saves new collected state if scene is unloaded at transition but before the collection timer has finished
    /// </summary>
    /// <param name="scene"></param>
    private void SaveCollectionOnTransition()
    {
        if(isCollected)
        {
            // updates game manager save state
            gameManager.SetCollectibleState(index, isCollected);
        }
    }

    /// <summary>
    /// Called by the player when colliding with this trigger
    /// </summary>
    public void PlayerCollect()
    {
        isCollected = true;

        // disable collider and renderer
        circleCollider.enabled = false;
        spriteRenderer.enabled = false; // start 5 second collecting animation for gem here instead of disabling sprite renderer yet
    }

}