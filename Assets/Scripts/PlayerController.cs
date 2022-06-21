using UnityEngine;
using static InputHelper;

/// <summary>
/// Controls all player movement and controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    // CONSTANTS
    // universal
    private const float GRAVITY_MULTIPLIER = 2f;
    // horizontal controls
    private const float GROUNDED_HORIZONTAL_FORCE = 2.5f;
    private const float AERIAL_HORIZONTAL_FORCE = 2.0f;
    // jumping
    private const float INIT_JUMP_SPEED = 5f;
    private const float JUMP_FORCE = 5f;
    private const float MAX_JUMP_TIME = 0.17f;
    // sliding
    private const float SLIDING_FORCE = 1.5f;
    private const float MAX_SLIDING_SPEED = 2f;
    // max speeds
    private const float MAX_HORIZONTAL_SPEED = 5f;
    private const float MAX_VERTICAL_SPEED = 5f;
    private const float MAX_FALLING_SPEED = 10f;
    // dashing
    private const float DASH_SPEED = 10f;
    private const float DASH_TIME = 0.15f;
    private const float DASH_COOLDOWN = 5.0f;

    // components
    private Rigidbody2D rb;
    private BoxCollider2D box;

    // Unity variables
    [SerializeField] private LayerMask platformMask;

    // jump variables
    private bool isJumping = false;
    private Side jumpSide = Side.None;
    private float jumpTimer = 0;

    // dash variables
    private bool isDashing = false;
    private OctoDirection dashDirection = OctoDirection.None;
    private float dashTimer = 0;
    private float dashCooldownTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        // component
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        // instantiation
        Physics2D.gravity *= GRAVITY_MULTIPLIER;
    }

    // Update is called once per frame
    void Update()
    {
        // HORIZONTAL CONTROLS ----------------------------------------------------------------

        // Apply forces
        if (InputHelper.GetRightOnly())
        {
            if (IsGrounded())
                rb.AddForce(new Vector2(GROUNDED_HORIZONTAL_FORCE, 0), ForceMode2D.Force);
            else if (!isJumping || jumpSide != Side.Left) // prevents climbing up right walls
                rb.AddForce(new Vector2(AERIAL_HORIZONTAL_FORCE, 0), ForceMode2D.Force);
        }
        else if (InputHelper.GetLeftOnly())
        {
            if (IsGrounded())
                rb.AddForce(new Vector2(-GROUNDED_HORIZONTAL_FORCE, 0), ForceMode2D.Force);
            else if (!isJumping || jumpSide != Side.Right) // prevents climbing up left walls
                rb.AddForce(new Vector2(-AERIAL_HORIZONTAL_FORCE, 0), ForceMode2D.Force);
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

            // apply initial jump velocity
            rb.velocity = new Vector2(INIT_JUMP_SPEED, INIT_JUMP_SPEED);
        }

        // controls to apply jumping force and continue jump
        if (isJumping)
        {
            // increment jump timer
            jumpTimer += Time.deltaTime;

            // apply jump force
            switch(jumpSide)
            {
                case Side.Left:
                    rb.AddForce(new Vector2(-1 * JUMP_FORCE, JUMP_FORCE), ForceMode2D.Force);
                    break;
                case Side.Right:
                    rb.AddForce(new Vector2(JUMP_FORCE, JUMP_FORCE), ForceMode2D.Force);
                    break;
                case Side.None:
                    rb.AddForce(new Vector2(0, JUMP_FORCE), ForceMode2D.Force);
                    break;
            }

            // end jumping on key release or timer end
            if (jumpTimer > MAX_JUMP_TIME || !InputHelper.GetUp())
            {
                isJumping = false;
            }
        }

        // SLIDING CONTROLS -------------------------------------------------------------- 

        // apply sliding friction and cap sliding speed if sliding down a wall
        if ((IsTouchingRightWall() || IsTouchingLeftWall()) && rb.velocity.y < 0)
        {
            rb.AddForce(new Vector2(0, -1 * SLIDING_FORCE), ForceMode2D.Force);
            if(rb.velocity.y < -1 * MAX_SLIDING_SPEED)
                rb.velocity = new Vector2(0, -1* MAX_SLIDING_SPEED);
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
                    rb.velocity = new Vector2(0, 0);
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
