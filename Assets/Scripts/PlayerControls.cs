using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ensures that the component this script is attached to has a Controller2D script on it
// Generates the Controller script if one is not found
[RequireComponent(typeof (Controller2D))]
public class PlayerControls : MonoBehaviour {

    // Height of jump
    public float jumpHeight;
    // How long to take to highest point of jump
    public float timeToJumpHeight;

    public float moveSpeed;
    public float accelerationAir;
    public float accelerationGround;
    public float wallSlideSpeed;
    public float wallStickTime;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallJumpLarge;

    private float jumpVelocity;
    private float gravity;
    private Vector2 velocity;
    private float velocityXSmoothing;
    private bool wallSliding;
    private bool holdingWall;
    private float wallTimeUnstick;

    private Controller2D controller;

    private void Start() {
        controller = GetComponent<Controller2D>();

        // Equations
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpHeight, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpHeight;
    }

    private void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool holdingClimb = Input.GetButton("Climb");

        int wallDirX = (controller.collisionInfo.left) ? -1 : 1;

        // Movement
        float targetVelocity = input.x * moveSpeed * Time.deltaTime;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocityXSmoothing, controller.collisionInfo.below ? accelerationGround : accelerationAir);


        // Wall sliding
        wallSliding = false;

        if((controller.collisionInfo.left || controller.collisionInfo.right) && !controller.collisionInfo.below && velocity.y < 0) {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeed) {
                velocity.y = -wallSlideSpeed;
            }

            if(wallTimeUnstick > 0) {
                velocityXSmoothing = 0;

                velocity.x = 0;

                if(input.x != wallDirX && input.x != 0) {
                    wallTimeUnstick -= Time.deltaTime;
                }
                else {
                    wallTimeUnstick = wallStickTime;
                }
            }
            else {
                wallTimeUnstick = wallStickTime;
            }
        }


        if(controller.collisionInfo.above || controller.collisionInfo.below) {
            velocity.y = 0;
        }

       
        // Jumping
        // If jump button pressed while the player is standing on something
        if (Input.GetButtonDown("Jump")) {
            if (wallSliding) {
                // Moving in same direction as the wall
                if(wallDirX == input.x) {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if(input.x == 0) {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else {
                    velocity.x = -wallDirX * wallJumpLarge.x;
                    velocity.y = wallJumpLarge.y;
                }
            }
            if (controller.collisionInfo.below) {
                velocity.y = jumpVelocity;
            }
        }

        // Have gravity affect player's velocity every frame
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
