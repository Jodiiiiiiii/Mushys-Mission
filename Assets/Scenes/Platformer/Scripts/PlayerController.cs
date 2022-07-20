using System.Collections;
using UnityEngine;
using static InputHelper;

/// <summary>
/// Controls all player movement and controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    // CONSTANTS
    // horizontal controls
    private const float GROUNDED_HORIZONTAL_FORCE = 45f;
    private const float AERIAL_HORIZONTAL_FORCE = 23f;
    private const float GROUNDED_FRICTION_FORCE = 70f;
    private const float GROUNDED_STOPPING_THRESHOLD = 1f;
    // jumping
    private const float INIT_JUMP_SPEED = 5f;
    private const float MAX_JUMP_TIME = 0.2f;
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
    // dirt particles
    private const float PARTICLE_TRAIL_DELAY = 0.2f;
    // animation
    private const float SQUASH_DELAY = 0.2f;

    // Unity variables
    private GameManager gameManager;
    private CameraController camController;
    [SerializeField] ParticleSystem dirtEffect;

    // components
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private AudioSource audioSource;
    private Animator animator;

    // layer masks
    [SerializeField] private LayerMask standardPlatformMask; // does not include one-way platforms
    [SerializeField] private LayerMask allPlatformsMask;

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

    // particles
    private bool spawnDirtBottom;
    private bool spawnDirtLeft;
    private bool spawnDirtRight;
    private float trailDelayTimer;

    // audio
    [SerializeField] private AudioClip jumpAudio1;
    [SerializeField] private AudioClip jumpAudio2;
    [SerializeField] private AudioClip jumpAudio3;
    [SerializeField] private AudioClip jumpAudio4;
    [SerializeField] private AudioClip dashAudio1;
    [SerializeField] private AudioClip dashAudio2;
    [SerializeField] private AudioClip collisionAudio1;
    [SerializeField] private AudioClip collisionAudio2;
    [SerializeField] private AudioClip collisionAudio3;
    [SerializeField] private AudioClip collisionAudio4;
    [SerializeField] private AudioClip collectibleAudio;

    // animation
    private float squashDelayTimer;

    // Start is called before the first frame update
    void Start()
    {
        // Unity variables
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        camController = GameObject.Find("Cam Follow").GetComponent<CameraController>();

        // component
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        // assign player position to appropriate spawn point
        transform.position = gameManager.GetSpawnPoint();

        // variables
        spawnDirtBottom = true;
        spawnDirtLeft = true;
        spawnDirtRight = true;
        trailDelayTimer = PARTICLE_TRAIL_DELAY;
        squashDelayTimer = 0;
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
                if (IsGrounded() && Mathf.Abs(rb.velocity.y) <= 0.001) // prevents grounded controls while within one-way platform
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
                if (IsGrounded() && Mathf.Abs(rb.velocity.y) <= 0.001) // prevents grounded controls while within one-way platform
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
            // apply friction to stop player sliding
            else if (IsGrounded() && Mathf.Abs(rb.velocity.y) <= 0.001) // prevents grounded friction while within one-way platform
            {
                if(rb.velocity.x < -1 * GROUNDED_STOPPING_THRESHOLD)
                {
                    forceSum += new Vector2(GROUNDED_FRICTION_FORCE, 0);
                }
                else if(rb.velocity.x > GROUNDED_STOPPING_THRESHOLD)
                {
                    forceSum -= new Vector2(GROUNDED_FRICTION_FORCE, 0);
                }
                else // stops player completely if at a very small speed (grounded stopping threshold)
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }

            // JUMPING CONTROLS ------------------------------------------------------------------
            
            if(!isJumping)
            {
                
                // grounded jump startup
                if (IsGrounded() && InputHelper.GetSpacePress() && Mathf.Abs(rb.velocity.y) <= 0.001) // prevents grounded jump while within one-way platform (possible edge case exactly at the peak of vertical movement)
                {
                    // set initial jumping state
                    isJumping = true;
                    jumpTimer = 0;
                    jumpSide = Side.None;
                    // sound effect
                    PlayJumpSound();
                }

                // right wall jump startup
                else if (IsTouchingRightWall() && InputHelper.GetSpacePress())
                {
                    // set initial jumping state
                    isJumping = true;
                    jumpTimer = 0;
                    jumpSide = Side.Left;
                    facing = Side.Left;
                    // sound effect
                    PlayJumpSound();
                }

                // left wall jump startup
                else if (IsTouchingLeftWall() && InputHelper.GetSpacePress())
                {
                    // set initial jumping state
                    isJumping = true;
                    jumpTimer = 0;
                    jumpSide = Side.Right;
                    facing = Side.Right;
                    // sound effect
                    PlayJumpSound();
                }
            }
            else // controls to apply jumping force and continue jump
            {
                // increment jump timer
                jumpTimer += Time.deltaTime;

                // apply jump velocity
                switch (jumpSide)
                {
                    case Side.Left:
                        rb.velocity = new Vector2(-1 * INIT_JUMP_SPEED, Mathf.Sqrt(2)/2 * INIT_JUMP_SPEED);
                        break;
                    case Side.Right:
                        rb.velocity = new Vector2(INIT_JUMP_SPEED, Mathf.Sqrt(2)/2 * INIT_JUMP_SPEED);
                        break;
                    case Side.None:
                        rb.velocity = new Vector2(rb.velocity.x, INIT_JUMP_SPEED);
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
            if (IsTouchingRightWall() && rb.velocity.y < 0 && InputHelper.GetRightOnly())
            {
                facing = Side.Right;
                forceSum += new Vector2(0, SLIDING_FORCE);
                if (rb.velocity.y < -1 * MAX_SLIDING_SPEED)
                    rb.velocity = new Vector2(rb.velocity.x, -1 * MAX_SLIDING_SPEED);
            }

            // apply sliding friction and cap sliding speed if sliding down a Left wall
            if (IsTouchingLeftWall() && rb.velocity.y < 0 && InputHelper.GetLeftOnly())
            {
                facing = Side.Left;
                forceSum += new Vector2(0, SLIDING_FORCE);
                if (rb.velocity.y < -1 * MAX_SLIDING_SPEED)
                    rb.velocity = new Vector2(rb.velocity.x, -1 * MAX_SLIDING_SPEED);
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
            isJumping = false; // ensure jumping does not resume after dash

            isDashing = true;
            dashDirection = InputHelper.GetOctoDirectionHeld();
            dashTimer = 0;
            dashCooldownTimer = DASH_COOLDOWN;

            camController.PulseCamera(); // trigger shake animation

            // sound effect
            PlayDashSound();
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

        // DIRT PARTICLE EFFECTS

        // grounded effects
        if(IsGrounded() && Mathf.Abs(rb.velocity.y) <= 0.001)
        {
            // grounded impact
            if(spawnDirtBottom)
            {
                Instantiate(dirtEffect, new Vector3(transform.position.x ,transform.position.y - box.bounds.extents.y), transform.rotation);
                PlayCollisionSound();
                spawnDirtBottom = false;
            }

            // moving trail
            if((InputHelper.GetRightOnly() || InputHelper.GetLeftOnly()) && !IsTouchingLeftWall() && !IsTouchingRightWall())
            {
                if (trailDelayTimer < 0)
                {
                    Instantiate(dirtEffect, new Vector3(transform.position.x, transform.position.y - box.bounds.extents.y), transform.rotation);
                    PlayCollisionSound();
                    trailDelayTimer = PARTICLE_TRAIL_DELAY;
                }
                else
                {
                    trailDelayTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            spawnDirtBottom = true;
        }

        // left wall impact
        if(IsTouchingLeftWall())
        {
            if (spawnDirtLeft)
            {
                Instantiate(dirtEffect, new Vector3(transform.position.x - box.bounds.extents.x, transform.position.y), transform.rotation);
                PlayCollisionSound();
                spawnDirtLeft = false;
            }
        }
        else
        {
            spawnDirtLeft = true;
        }

        // right wall impact
        if (IsTouchingRightWall())
        {
            if (spawnDirtRight)
            {
                Instantiate(dirtEffect, new Vector3(transform.position.x + box.bounds.extents.x, transform.position.y), transform.rotation);
                PlayCollisionSound();
                spawnDirtRight = false;
            }
        }
        else
        {
            spawnDirtRight = true;
        }

        // ANIMATIONS ------------------------------------------------------------------------------

        // sliding
        if(((IsTouchingLeftWall() && InputHelper.GetLeftOnly()) || (IsTouchingRightWall() && InputHelper.GetRightOnly())) && rb.velocity.y < 0 && !IsGrounded())
        {
            SetSlideAnim(true);
        }
        else
        {
            SetSlideAnim(false);
        }

        // running
        if(IsGrounded() && !InputHelper.GetLeftOnly() && !InputHelper.GetRightOnly() && Mathf.Abs(rb.velocity.y) <= 0.001 && Mathf.Abs(rb.velocity.x) <= 0.001)
        {
            squashDelayTimer += Time.deltaTime;
            if (squashDelayTimer >= SQUASH_DELAY)
                SetRunAnim(false);
        }
        else
        {
            SetRunAnim(true);
            squashDelayTimer = 0;
        }
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
            DeathAnim();
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

        // collect collectible
        if(collision.CompareTag("Collectible"))
        {
            if (collision.TryGetComponent(out Collectible collectibleScript))
            {
                collectibleScript.PlayerCollect();
                PlayCollectibleSound();
            }
            else
            {
                // ensures proper use of transition tag
                throw new System.Exception("Invalid use of 'Collectible' tag");
            }
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
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center + new Vector3(box.bounds.extents.x / 2.0f, 0), new Vector2(box.bounds.extents.x, box.bounds.size.y), 0f, Vector2.right, horizontalCheckRange, standardPlatformMask);

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
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center + new Vector3(-box.bounds.extents.x/2.0f, 0), new Vector2(box.bounds.extents.x, box.bounds.size.y), 0f, Vector2.left, horizontalCheckRange, standardPlatformMask);

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
        RaycastHit2D raycastHit = Physics2D.BoxCast(box.bounds.center - new Vector3(0, box.bounds.size.y/4.0f), new Vector2(box.bounds.size.x, box.bounds.extents.y), 0f, Vector2.down, verticalCheckRange, allPlatformsMask);

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

    private void PlayJumpSound()
    {
        switch (Random.Range(1, 5)) // random int from 1 to 4
        {
            case 1:
                audioSource.PlayOneShot(jumpAudio1);
                break;
            case 2:
                audioSource.PlayOneShot(jumpAudio2);
                break;
            case 3:
                audioSource.PlayOneShot(jumpAudio3);
                break;
            case 4:
                audioSource.PlayOneShot(jumpAudio4);
                break;
        }
    }

    private void PlayDashSound()
    {
        switch (Random.Range(1, 3)) // random int, either 1 or 2
        {
            case 1:
                audioSource.PlayOneShot(dashAudio1, 0.5f);
                break;
            case 2:
                audioSource.PlayOneShot(dashAudio2, 0.5f);
                break;
        }
    }

    private void PlayCollisionSound()
    {
        switch (Random.Range(1, 5)) // random int from 1 to 4
        {
            case 1:
                audioSource.PlayOneShot(collisionAudio1);
                break;
            case 2:
                audioSource.PlayOneShot(collisionAudio2);
                break;
            case 3:
                audioSource.PlayOneShot(collisionAudio3);
                break;
            case 4:
                audioSource.PlayOneShot(collisionAudio4);
                break;
        }
    }

    private void PlayCollectibleSound()
    {
        audioSource.PlayOneShot(collectibleAudio, 0.5f);
    }

    private void SetRunAnim(bool state)
    {
        animator.SetBool("walking", state);
    }

    private void SetSlideAnim(bool state)
    {
        animator.SetBool("sliding", state);
    }

    private void DeathAnim()
    {
        animator.SetTrigger("death");
    }
}
