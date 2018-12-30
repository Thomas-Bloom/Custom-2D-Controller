using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ensures that the component this script is attached to has a Controller2D script on it
// Generates the Controller script if one is not found
[RequireComponent(typeof (Controller2D))]
public class PlayerControls : MonoBehaviour {

    public float moveSpeed;
    public float gravity = -20f;

    private Vector2 velocity;

    private Controller2D controller;

    private void Start() {
        controller = GetComponent<Controller2D>();
    }

    private void Update() {
        if(controller.collisionInfo.above || controller.collisionInfo.below) {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        velocity.x = input.x * moveSpeed * Time.deltaTime;

        // Have gravity affect player's velocity every frame
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
