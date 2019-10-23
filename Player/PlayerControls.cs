using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Ensures that the component this script is attached to has a Controller2D script on it
// Generates the Controller script if one is not found
[RequireComponent(typeof (Controller2D))]
public class PlayerControls : MonoBehaviour {
    [Header("Movement")]
    // Horizontal move speed
    public float moveSpeed;

    [HideInInspector]
    public bool canRun;
    [HideInInspector]
    public float runSpeedMultiplier;

    /* Affects movement on ground and in air
     * Movement on ground could be quicker 
     * Movement in air could be slower
    */
    public float accelerationAir;
    public float accelerationGround;

    // Affects constant downward force upon the player
    private float gravity;
    // Current velocity in any direction stored as a Vector (x, y)
    private Vector2 velocity;
    // Smoothes movement when stopping -> Means player won't suddenly stop in place
    private float velocityXSmoothing;

    [Header("Jumping")]
    // Max height of jump
    public float maxJumpHeight;
    // Min height of jump
    public float minJumpheight;
    // How long to take to highest point of jump
    public float timeToJumpHeight;

    // These represent how high the different types of jumps will go by taking into account the gravity modifier and timeToJumpHeight
    private float maxJumpVelocity;
    private float minJumpVelocity;

    [Header("Wall Jumping")]
    // WALL CLIMB
    [HideInInspector]
    // Decides if the player is allowed to jump on/off walls
    public bool canWallJump;
    // Hidden unless canWallJump is true
    [HideInInspector]
    // Climbing directly up a wall
    public Vector2 wallJumpClimb;
    [HideInInspector]
    // Jumping off without pressing a key
    public Vector2 wallJumpOff;
    [HideInInspector]
    // Jumping off while pressing opposite direction key
    public Vector2 wallJumpLarge;

    [Header ("Wall Handling")]
    // How fast the player slides down surfaces
    public float wallSlideSpeed;
    // Time the player is stuck to the wall (If they are pressing opposite direction key they won't move off wall)
    public float wallStickTime;

    // Represents if player is sliding down a wall
    private bool wallSliding;
    // Represents if player is holding onto a wall
    private bool holdingWall;
    // Time that player has been stuck to a wall
    private float wallTimeUnstick;

    [Header("Dash Ability")]
    [HideInInspector]
    public bool canDash;
    public float dashDistance;

    [Header("Other")]
    private Controller2D controller;


    // At the start of execution
    private void Start() {
        // Assign controller component to variable
        controller = GetComponent<Controller2D>();

        // EQUATIONS
        
        // Work out gravity depending on how high and time for jumping
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpHeight, 2);

        // Work out velocity of different jump heights
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpHeight;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpheight);
    }

    // Called once per frame
    private void Update() {
        // If player can't wall jump then just make the wall jump variables not store any values
        if (!canWallJump) {
            wallJumpClimb = new Vector2();
            wallJumpOff = new Vector2();
            wallJumpLarge = new Vector2();
        }

        // Check for input and assign values to input vector
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Check for button held for climbing
        bool holdingClimb = Input.GetButton("Climb");
        bool holdingRun = Input.GetAxisRaw("Run") > 0;

        // Returns number based on which direction collision is occuring
        int wallDirX = (controller.collisionInfo.left) ? -1 : 1;

        // MOVEMENT
        float targetVelocity;

        if (holdingRun && canRun) {
            targetVelocity = input.x * moveSpeed * runSpeedMultiplier * Time.deltaTime;
        }
        else {
            targetVelocity = input.x * moveSpeed * Time.deltaTime;
        }
        // Assigns the desired velocity that the player wants to eventually move at
        

        // Slowly build up velocity till player hits targetVelocity
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocityXSmoothing, controller.collisionInfo.below ? accelerationGround : accelerationAir);

        // WALL SLIDING
        wallSliding = false;

        // Check to see if player is currently on a wall
        if((controller.collisionInfo.left || controller.collisionInfo.right) && !controller.collisionInfo.below && velocity.y < 0) {
            wallSliding = true;

            // Limit slide speed to the wall slide speed
            if (velocity.y < -wallSlideSpeed) {
                velocity.y = -wallSlideSpeed;
            }

            // If timer is still going while on wall...
            if(wallTimeUnstick > 0) {
                velocityXSmoothing = 0;

                // Freeze player's x velocity so they don't come off the wall
                velocity.x = 0;

                // If they are on the wall...
                if(input.x != wallDirX && input.x != 0) {
                    // Reduce the timer
                    wallTimeUnstick -= Time.deltaTime;
                }
                else {
                    // Reset the timer -> Player has come off the wall
                    wallTimeUnstick = wallStickTime;
                }
            }
            else {
                // Reset the timer -> Player has come off the wall
                wallTimeUnstick = wallStickTime;
            }
        }

        if(controller.collisionInfo.above || controller.collisionInfo.below) {
            velocity.y = 0;
        }

        // JUMPING

        //If jump button pressed
        if (Input.GetButtonDown("Jump")) {
            if (wallSliding) {
                // Moving in same direction as the wall -> Climb up wall
                if(wallDirX == input.x) {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                // No input -> Small jump off the wall
                else if(input.x == 0) {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                // Input in opposite direction to wall -> Large jump off wall
                else {
                    velocity.x = -wallDirX * wallJumpLarge.x;
                    velocity.y = wallJumpLarge.y;
                }
            }
            if (controller.collisionInfo.below) {
                velocity.y = maxJumpVelocity;
            }
        }

        // If jump button is released
        if (Input.GetButtonUp("Jump")) {
            // Perform a smaller jump
            if(velocity.y > minJumpVelocity) {
                velocity.y = minJumpVelocity;
            }
        }
        // Have gravity affect player's velocity every frame
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void FlipSprite() {
        Vector3 theScale = transform.localScale;

        theScale *= 1;

        transform.localScale = theScale;
    }
}

// Editor script to show information based upon if boolean is true

[CustomEditor(typeof (PlayerControls))]
public class PlayerControlsEditor : Editor {
    public override void OnInspectorGUI() {
        // For all other non-HideInInspector fields
        DrawDefaultInspector();

        PlayerControls playerControls = (PlayerControls)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ability Enablers", EditorStyles.boldLabel);

        // RUN ENABLER
        #region RunEnable
        playerControls.canRun = EditorGUILayout.Toggle("Can run", playerControls.canRun);

        if (playerControls.canRun) {
            playerControls.runSpeedMultiplier = EditorGUILayout.FloatField("Run Speed Multiplier", playerControls.runSpeedMultiplier);
        }
        #endregion

        // WALL JUMP ENABLER
        #region WallJumpEnable

        // Draw checkbox for bool
        playerControls.canWallJump = EditorGUILayout.Toggle("Can Wall Jump", playerControls.canWallJump);

        // If the boolean is true then show the other values
        if (playerControls.canWallJump) {
            playerControls.wallJumpClimb = EditorGUILayout.Vector2Field("Wall Jump Climb", playerControls.wallJumpClimb);
            playerControls.wallJumpOff = EditorGUILayout.Vector2Field("Wall Jump Off", playerControls.wallJumpOff);
            playerControls.wallJumpLarge = EditorGUILayout.Vector2Field("Wall Jump Large", playerControls.wallJumpLarge);
        }
        #endregion

    }
}
 