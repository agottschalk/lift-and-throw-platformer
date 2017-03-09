using UnityEngine;
using System.Collections;
using System.Text;

public class PlayerController : MonoBehaviour {

    #region Public Fields
    //set in inspector
    public float walkForce;
    public float jumpSpeed;
    public float walkMaxSpeed;

    public float inAirJumpForce;

    public int jumpLeewayFrames;
    public int maxHoldFrames;

    #endregion

    #region Private Fields

    private Rigidbody2D rb;
    private Collider2D footCollider;
    private Animator anim;
    private SpriteRenderer sr;

    //for determining whether or not to flip sprite
    private bool facingRight = true;

    //whether or not to perform jumping logic
    //private bool jump = false;

    //for ground checking
    private bool grounded = false;
    private int groundCheckRays = 3;

    //determines how fast sprite changes pose when in air
    public float jumpTransitionThreshold;

    //tracks last jump input
    private bool lastJumpInput = false;
    private float lastJumpTime = 0;

    private float jumpStartTime = 0;

    private float jumpLeeway;
    private float maxHoldTime;

    #endregion


    #region Hashes and ID's
    //animation states and parameters
    int idleHash = Animator.StringToHash("Idle");
    int walkHash = Animator.StringToHash("Walk");
    int speedHash = Animator.StringToHash("Speed");

    int terrainLayerMask;   //initialized in 'start'

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        footCollider = GetComponentsInChildren<Collider2D>()[0];

        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        terrainLayerMask = 1 << LayerMask.NameToLayer("Terrain");

        jumpLeeway = jumpLeewayFrames / 60f;
        maxHoldTime = maxHoldFrames / 60f;
    }


    #region Update cycle
    void Update()
    {
        if (grounded)
        {
            GroundedAnimation();
        }
        else
        {
            AirAnimation();
        }

    }

    void FixedUpdate()
    {
        //get input
        float horizontal = Input.GetAxisRaw("Horizontal");

        //move sprite
        Vector2 movement = new Vector2(horizontal * walkForce, 0);
        rb.AddForce(movement);

        //cap speed
        if(Mathf.Abs(rb.velocity.x) > walkMaxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * walkMaxSpeed, rb.velocity.y);
        }
        

        //flip sprite (if necessary)
        if ((facingRight && horizontal < 0)
            || !facingRight && horizontal > 0)
        {
            facingRight = !facingRight;

            Vector2 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }

        //apply jumping physics if necessary
        SetLastJump(Input.GetButton("Jump"));

        if (grounded && (Time.time - lastJumpTime < jumpLeeway))
        {
            //directly editing vel. gives more consistent jumps than using addForce
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            jumpStartTime = Time.fixedTime;
        }else if(!grounded &&
            lastJumpTime != 0 &&
            rb.velocity.y > 0 &&
            (Time.fixedTime - jumpStartTime) < maxHoldTime)
        {
            //keep rising
            rb.AddForce(Vector2.up * inAirJumpForce);
        }

        //because this methods anticipates where zangoose will be after the next physics update, 
        //it is called after applying inputs to avoid jumping while still in the air
        CheckGrounded();
    }



    #endregion

    #region Animation methods
    void AirAnimation()
    {
        if(rb.velocity.y > jumpTransitionThreshold)
        {
            //rising animation
            anim.Play("z_inair_up");
        }
        else if(rb.velocity.y < -jumpTransitionThreshold)
        {
            //falling animation
            anim.Play("z_inair_down");
        }
        else
        {
            //in between animation
            anim.Play("z_inair_level");
        }
    }

    void GroundedAnimation()
    {
        //choose walking or idle state
        if (rb.velocity.x == 0)
        {
            anim.Play("z_idle");
        }
        else
        {
            anim.Play("z_walk");
            //anim.SetTrigger(walkHash);
        }

        //set speed of walking animation
        anim.SetFloat(speedHash, (Mathf.Abs(rb.velocity.x) / walkMaxSpeed));
    }


    #endregion


    void SetLastJump(bool input)
    {
        //on frame jump key is pressed
        if(input && !lastJumpInput)
        {
            lastJumpTime = Time.fixedTime;
        }else if (!input)
        {
            //reset timer
            lastJumpTime = 0;
        }

        lastJumpInput = input;
    }


    bool CheckGrounded()
    {
        float distance;
        //if not moving up
        if (rb.velocity.y <= 0)
        {
            grounded = false;

            /*if the player is not moving down, ray cast will be 0.02 long, just over the distance between
            the player and to platform below if he is standing on one, otherwise it is the distance the
            player is expected to fall during the next physics update.*/
            distance = rb.velocity.y == 0 ? 0.02f : Mathf.Abs(rb.velocity.y) * Time.fixedDeltaTime;

            Vector2 start = new Vector2(footCollider.bounds.min.x * 1f, footCollider.bounds.min.y); //the 0.9s stop rays from hitting the sides of platforms when the player is right up against them
            Vector2 end = new Vector2(footCollider.bounds.max.x * 1f, footCollider.bounds.min.y);

            for (int i = 0; i < groundCheckRays; i++)
            {
                float lerpAmt = (float)(i / (groundCheckRays-1));
                Vector2 origin = Vector2.Lerp(start, end, lerpAmt);

                //use a raycast to see if a platform is below
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, terrainLayerMask);

                //grounded is true if ray hit something, otherwise keep checking
                if (hit.collider != null)
                {
                    grounded = true;
                    jumpStartTime = 0;
                    break;
                }
            }
        }
        else 
        {
            grounded = false;
        }

        return grounded;
    }
}
