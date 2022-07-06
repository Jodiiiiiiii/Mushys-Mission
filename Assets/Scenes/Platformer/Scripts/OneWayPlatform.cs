using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    // constants
    private const float ENABLED_SURFACE_ARC = 178;

    // components
    private PlatformEffector2D effector;

    // variables
    private InputHelper.OctoDirection direction;
    private bool isOverlappingPlayer;

    // Start is called before the first frame update
    void Start()
    {
        // components
        effector = GetComponent<PlatformEffector2D>();

        // variables
        isOverlappingPlayer = false;

        // starting configuration
        effector.surfaceArc = ENABLED_SURFACE_ARC;
    }

    // Update is called once per frame
    void Update()
    {
        // update current direction
        direction = InputHelper.GetOctoDirectionHeld();

        // disable one way collisions if holding down
        if(direction == InputHelper.OctoDirection.Down || direction == InputHelper.OctoDirection.DownLeft || direction == InputHelper.OctoDirection.DownRight)
        {
            if(effector.surfaceArc == ENABLED_SURFACE_ARC) // only disable if not already disabled
                effector.surfaceArc = 0; // removes collision arc
        }
        else
        {
            if(effector.surfaceArc == 0 && !isOverlappingPlayer) // only enable if not already enabled and if not overlapping the player (prevents resnapping player to platform)
                effector.surfaceArc = ENABLED_SURFACE_ARC; // reapplies collision arc
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isOverlappingPlayer = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isOverlappingPlayer = false;
        }
    }
}
