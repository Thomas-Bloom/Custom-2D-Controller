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

    private float jumpVelocity;
    private float gravity;
    private Vector2 velocity;
    private float velocityXSmoothing;

    private Controller2D controller;

    private void Start() {
        controller = GetComponent<Controller2D>();

        // Equations
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpHeight, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpHeight;
    }

    private void Update() {
        if(controller.collisionInfo.above || controller.collisionInfo.below) {
            velocity.y = 0;
        }

        // Movement
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetVelocity = input.x * moveSpeed * Time.deltaTime;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocity, ref velocityXSmoothing, controller.collisionInfo.below ? accelerationGround : accelerationAir);

        // Jumping
        // If jump button pressed while the player is standing on something
        if (Input.GetButtonDown("Jump") && controller.collisionInfo.below) {
            velocity.y = jumpVelocity;
        }

        // Have gravity affect player's velocity every frame
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
