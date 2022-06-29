using UnityEngine;
using static InputHelper;

/// <summary>
/// Controls all player movement and controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    // CONSTANTS
    // horizontal controls
    private const float GROUNDED_HORIZONTAL_FORCE = 30f;
    private const float AERIAL_HORIZONTAL_FORCE = 17f;
    // jumping
    private const float INIT_JUMP_SPEED = 5f;
    private const float JUMP_FORCE = 35f;
    private const float MAX_JUMP_TIME = 0.17f;
    // sliding
    private const float SLIDING_FORCE = 15f;
    private const float MAX_SLIDING_SPEED = 2f;
    // max speeds
    private const float MAX_HORIZONTAL_SPEED = 5f;
    private const float MAX_VERTICAL_SPEED = 5f;
    private const float MAX_FALLING_SPEED = 10f;
    // dashing
    private const float DASH_SPEED = 12f;
    private const float DASH_TIME = 0.2f;
    private const float DASH_COOLDOWN = 5.0f;

    // Managers
    private GameManager gameManager;

    // components
    private Rigidbody2D rb;
    private BoxCollider2D box;

    // Unity variables
    [SerializeField] private LayerMask platformMask;

    // facing variable
    private Side facing = Side.Right;
    private Side prevFacing = Side.Right;

    // jump variables
    private bool isJumping = false;
    private Side jumpSide = Side.None;
    private float jumpTimer = 0;

    // dash variables
    private bool isDashing = false;
    private OctoDirection dashDirection = OctoDirection.None;
    private float dashTimer = 0;
    private float dashCooldownTimer = 0;

    // physics variables
    private Vector2 forceSum;

    // Start is called before the first frame update
    void Start()
    {
        // create managers
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // component
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        // assign player position to appropriate spawn point
        transform.position = gameManager.GetSpawnPoint();
    }

    // Update is called once per frame
    void Update()
    {
        // reset force sum for each loop
        forceSum = new Vector2(0, 0);

        if (!isDashing) // only allows movement, jumping, sliding, speed capping, etc. while not dashinga
        {
            // HORIZONTAL CONTROLS ----------------------------------------------------------------

            // Apply forces
            if (InputHelper.GetRightOnly())
            {
                if (IsGrounded())
                {
                    facing = Side.Right;
                    forceSum += new Vector2(GROUNDED_HORIZONTAL_FORCE, 0);
                }
                else if (!isJumping || jumpSide != Side.Left) // prevents climbing up right walls
                {
                    facing = Side.Right;
                    forceSum += new Vector2(AERIAL_HORIZONTAL_FORCE, 0);
                }
            }
            else if (InputHelper.GetLeftOnly())
            {
                if (IsGrounded())
                {
                    facing = Side.Left;
                    forceSum += new Vector2(-GROUNDED_HORIZONTAL_FORCE, 0);
                }
                else if (!isJumping || jumpSide != Side.Right) // prevents climbing up left walls
                { 
                    facing = Side.Left;
                    forceSum += new Vector2(-AERIAL_HORIZONTAL_FORCE, 0);
                }
            }
            else if (IsGrounded()) // instantly stops player if grounded with no horizontal inputs
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            // JUMPING CONTROLS ------------------------------------------------------------------

            // grounded jump startup
            if (IsGrounded() && InputHelper.GetSpacePress())
            {
                // set initial jumping state
                isJumping = true;
                jumpTimer = 0;
                jumpSide = Side.None;

                // apply initial jump velocity
                rb.velocity = new Vector2(rb.velocity.x, INIT_JUMP_SPEED);
            }

            // right wall jump startup
            if (IsTouchingRightWall() && InputHelper.GetSpacePress() && !IsGrounded())
            {
                // set initial jumping state
                isJumping = true;
                jumpTimer = 0;
                jumpSide = Side.Left;
                facing = Side.Left;

                // apply initial jump velocity
                rb.velocity = new Vector2(-1 * INIT_JUMP_SPEED, INIT_JUMP_SPEED);
            }

            // left wall jump startup
            if (IsTouchingLeftWall() && InputHelper.GetSpacePress() && !IsGrounded())
            {
                // set initial jumping state
                isJumping = true;
                jumpTimer = 0;
                jumpSide = Side.Right;
                facing = Side.Right;

                // apply initial jump velocity
                rb.velocity = new Vector2(INIT_JUMP_SPEED, INIT_JUMP_SPEED);
            }

            // controls to apply jumping force and continue jump
            if (isJumping)
            {
                // increment jump timer
                jumpTimer += Time.deltaTime;

                // apply jump force
                switch (jumpSide)
                {
                    case Side.Left:
                        forceSum += new Vector2(-1 * JUMP_FORCE, JUMP_FORCE);
                        break;
                    case Side.Right:
                        forceSum += new Vector2(JUMP_FORCE, JUMP_FORCE);
                        break;
                    case Side.None:
                        forceSum += new Vector2(0, JUMP_FORCE);
                        break;
                }

                // end jumping on key release or timer end
                if (jumpTimer > MAX_JUMP_TIME || !InputHelper.GetUp())
                {
                    isJumping = false;
                }
            }

            // SLIDING CONTROLS -------------------------------------------------------------- 

            // apply sliding friction and cap sliding speed if sliding down a right wall
            if (IsTouchingRightWall() && rb.velocity.y < 0)
            {
                facing = Side.Right;
                forceSum += new Vector2(0, SLIDING_FORCE);
                if (rb.velocity.y < -1 * MAX_SLIDING_SPEED)
                    rb.velocity = new Vector2(0, -1 * MAX_SLIDING_SPEED);
            }

            // apply sliding friction and cap sliding speed if sliding down a Left wall
            if (IsTouchingLeftWall() && rb.velocity.y < 0)
            {
                facing = Side.Left;
                forceSum += new Vector2(0, SLIDING_FORCE);
                if (rb.velocity.y < -1 * MAX_SLIDING_SPEED)
                    rb.velocity = new Vector2(0, -1 * MAX_SLIDING_SPEED);
            }

            // MAX SPEED CAPS -----------------------------------------------------------------

            // check for max horizontal speed caps
            if (rb.velocity.x >= MAX_HORIZONTAL_SPEED)
                rb.velocity = new Vector2(MAX_HORIZONTAL_SPEED, rb.velocity.y);
            if (rb.velocity.x <= -1 * MAX_HORIZONTAL_SPEED)
                rb.velocity = new Vector2(-1 * MAX_HORIZONTAL_SPEED, rb.velocity.y);

            // check for max vertical speed caps
            if (rb.velocity.y > MAX_VERTICAL_SPEED)
                rb.velocity = new Vector2(rb.velocity.x, MAX_VERTICAL_SPEED);
            if (rb.velocity.y < -1 * MAX_FALLING_SPEED)
                rb.velocity = new Vector2(rb.velocity.x, -1 * MAX_FALLING_SPEED);
        }

        // DASH CONTROLS -----------------------------------------------------------------

        // dash startup
        if(InputHelper.GetShiftPress() && dashCooldownTimer <= 0)
        {
            isDashing = true;
            dashDirection = InputHelper.GetOctoDirectionHeld();
            dashTimer = 0;
            dashCooldownTimer = DASH_COOLDOWN;
        }

        // continues dash until dash terminates
        if(isDashing)
        {
            switch (dashDirection)
            {
                // set speed in appropriate direction (multiple by root 2 over 2 for diagonals)
                case InputHelper.OctoDirection.Up:
                    rb.velocity = new Vector2(0, DASH_SPEED);
                    break;
                case InputHelper.OctoDirection.UpRight:
                    rb.velocity = new Vector2(DASH_SPEED* Mathf.Sqrt(2)/2.0f, DASH_SPEED * Mathf.Sqrt(2) / 2.0f);
                    break;
                case InputHelper.OctoDirection.Right:
                    rb.velocity = new Vector2(DASH_SPEED, 0);
                    break;
                case InputHelper.OctoDirection.DownRight:
                    rb.velocity = new Vector2(DASH_SPEED * Mathf.Sqrt(2) / 2.0f, -1 * DASH_SPEED * Mathf.Sqrt(2) / 2.0f);
                    break;
                case InputHelper.OctoDirection.Down:
                    rb.velocity = new Vector2(0, -1 * DASH_SPEED);
                    break;
                case InputHelper.OctoDirection.DownLeft:
                    rb.velocity = new Vector2(-1 * DASH_SPEED * Mathf.Sqrt(2) / 2.0f, -1 * DASH_SPEED * Mathf.Sqrt(2) / 2.0f);
                    break;
                case InputHelper.OctoDirection.Left:
                    rb.velocity = new Vector2(-1 * DASH_SPEED, 0);
                    break;
                case InputHelper.OctoDirection.UpLeft:
                    rb.velocity = new Vector2(-1 * DASH_SPEED * Mathf.Sqrt(2) / 2.0f, DASH_SPEED * Mathf.Sqrt(2) / 2.0f);
                    break;
                case InputHelper.OctoDirection.None:
                    // defaults to dashing in the direction you are facing
                    if(facing == Side.Left)
                        rb.velocity = new Vector2(-1 * DASH_SPEED, 0);
                    else if(facing == Side.Right)
                        rb.velocity = new Vector2(DASH_SPEED, 0);
                    break;
            }

            // timer for ending dash
            dashTimer += Time.deltaTime;
            if(dashTimer > DASH_TIME)
            {
                isDashing = false;
            }
        }

        // reduce dash cooldown by time that passed
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        // Update orientation based on facing direction
        if (facing != prevFacing)
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        prevFacing = facing;
    }

    private void FixedUpdate()
    {
        // no physics calculations occur while dashing because the player is moving at a set velocity
        if(!isDashing)
        {
            // apply all applicable forces
            rb.AddForce(forceSum, ForceMode2D.Force);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // reload scene when contacting a hazard
        if (collision.CompareTag("Hazard"))
        {
            gameManager.HazardCollision();
        }

        // update save point and scene in save manager 
        if(collision.CompareTag("SpawnPoint"))
        {
            gameManager.SetSpawnPoint(collision.transform.position);
        }

        // load new scene when encountering a transition
        if(collision.CompareTag("Transition"))
        {
            if(collision.TryGetComponent(out TransitionData transitionData))
            {
                gameManager.TransitionScene(transitionData);
            }
            else
            {
                // ensures proper use of transition tag
                throw new System.Exception("Invalid use of 'Transition' tag");
            }
        }

        // restore health when encountering a restoration station
        if (collision.CompareTag("Restoration"))
        {
            gameManager.RestoreHealth();
        }
    }

    /// <summary>
    /// Returns true if the right side of the player is within a small distance of a right wall
    /// </summary>
    /// <returns></returns>
    private bool IsTouchingRightWall()
    {
        // send box cast below player
        const float horizontalCheckRange = 0.03f; // the amount that the boxcast sticks off the right side of the player box
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center + new Vector3(box.bounds.extents.x / 2.0f, 0), new Vector2(box.bounds.extents.x, box.bounds.size.y), 0f, Vector2.right, horizontalCheckRange, platformMask);

        // determine color for collider debug render
        Color rayColor;
        if (raycastHit.collider != null)
            rayColor = Color.green; // successful collision
        else
            rayColor = Color.red; // no collision

        // render collider box for debug mode
        Debug.DrawRay(box.bounds.center + new Vector3(box.bounds.extents.x, box.bounds.extents.y), Vector2.right * horizontalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(box.bounds.extents.x, -box.bounds.extents.y), Vector2.right * horizontalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(box.bounds.extents.x + horizontalCheckRange, box.bounds.extents.y), Vector2.down * box.bounds.size.y, rayColor);
        // return if boxcast hit a platform
        return raycastHit.collider != null;
    }

    /// <summary>
    /// Returns true if the left side of the player is within a small distance of a left wall
    /// </summary>
    /// <returns></returns>
    private bool IsTouchingLeftWall()
    {
        // send box cast below player
        const float horizontalCheckRange = 0.03f; // the amount that the boxcast sticks off the left side of the player box
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center + new Vector3(-box.bounds.extents.x/2.0f, 0), new Vector2(box.bounds.extents.x, box.bounds.size.y), 0f, Vector2.left, horizontalCheckRange, platformMask);

        // determine color for collider debug render
        Color rayColor;
        if (raycastHit.collider != null)
            rayColor = Color.green; // successful collision
        else
            rayColor = Color.red; // no collision
        // render collider box for debug mode
        Debug.DrawRay(box.bounds.center + new Vector3(-box.bounds.extents.x, box.bounds.extents.y), Vector2.left * horizontalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(-box.bounds.extents.x, -box.bounds.extents.y), Vector2.left * horizontalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(-box.bounds.extents.x - horizontalCheckRange, box.bounds.extents.y), Vector2.down * box.bounds.size.y, rayColor);

        // return if boxcast hit a platform
        return raycastHit.collider != null;
    }

    /// <summary>
    /// Returns true if the bottom of the player is within a small distance of ground
    /// </summary>
    /// <returns></returns>
    private bool IsGrounded()
    {
        // send box cast below player
        const float verticalCheckRange = 0.03f; // the amount that the boxcast sticks off the bottom of the player box
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center - new Vector3(0, box.bounds.size.y/4.0f), new Vector2(box.bounds.size.x, box.bounds.extents.y), 0f, Vector2.down, verticalCheckRange, platformMask);

        // determine color for collider debug render
        Color rayColor;
        if(raycastHit.collider != null)
            rayColor = Color.green; // successful collision   
        else
            rayColor = Color.red; // no collision
        // render collider box for debug mode
        Debug.DrawRay(box.bounds.center + new Vector3(-box.bounds.extents.x, -box.bounds.extents.y), Vector2.down * verticalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(box.bounds.extents.x, -box.bounds.extents.y), Vector2.down * verticalCheckRange, rayColor);
        Debug.DrawRay(box.bounds.center + new Vector3(-box.bounds.extents.x, -box.bounds.extents.y - verticalCheckRange), Vector2.right * box.bounds.size.x, rayColor);

        // return if boxcast hit a platform
        return raycastHit.collider != null;
    }

    private enum Side
    {
        Left, 
        Right, 
        None
    }
    
    /// <summary>
    /// Returns a float between 0 and 1. 0 indicates no cooldown remaining. 1 indicates full cooldown remaining
    /// </summary>
    public float GetDashCooldownPercentage()
    {
        if (dashCooldownTimer < 0)
            return 0;
        else
            return dashCooldownTimer / DASH_COOLDOWN;
    }
}
