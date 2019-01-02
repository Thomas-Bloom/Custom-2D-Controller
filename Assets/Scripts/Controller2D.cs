using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RayCastController {
    public CollisionInfo collisionInfo;

    public override void Start() {
        base.Start();

        // Set the look direction to right at the start
        collisionInfo.direction = 1;
    }

    public void Move(Vector2 velocity, bool onPlatform = false) {
        UpdateRayCastOrigins();
        // Reset collision indicators
        collisionInfo.Reset();

        // Get the direction of the controller based upon their velocity
        // If velocity.x > 0 then the direction will be 1
        // If velocity.x < 0 then direction will be -1
        if(velocity.x != 0) {
            collisionInfo.direction = (int)Mathf.Sign(velocity.x);
        }

        // Constantly check for hoziontal collisions
        HorizontalCollisions(ref velocity);

        // Only check for vertical collisions if controller is moving up or down
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }

        // Move the controller
        transform.Translate(velocity);

        if (onPlatform) {
            collisionInfo.below = true;
        }
    }

    private void VerticalCollisions(ref Vector2 velocity) {
        // Up = 1   Down = -1
        float dirY = Mathf.Sign(velocity.y);
        // Raycast length
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 origin = (dirY == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
            origin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(origin, dirY * Vector2.up, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.up * dirY * rayLength, Color.red);

            // If raycast hits something
            if (hit) {

                if (hit.collider.tag.Equals("JumpThrough")) {
                    // Is player moving up...?
                    if(dirY == 1 || hit.distance == 0) {
                        continue;
                    }
                }

                // Constrain velocity so don't go through objects
                velocity.y = (hit.distance - skinWidth) * dirY;
                rayLength = hit.distance;

                // Set correct collisionInfo boolean to true
                // Right-hand side returns a boolean
                collisionInfo.above = (dirY == 1);
                collisionInfo.below = (dirY == -1);
            }
        }
    }

    private void HorizontalCollisions(ref Vector2 velocity) {
        // Right = 1   Left = -1
        float dirX = collisionInfo.direction;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if(Mathf.Abs(velocity.x) < skinWidth) {
            rayLength = skinWidth * 2;
        }

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 origin = (dirX == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight;
            origin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, dirX * Vector2.right, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.right * dirX * rayLength, Color.red);


            // If raycast hits something
            if (hit) {
                // Been crushed by the platform
                if(hit.distance == 0) {
                    continue;
                }

                velocity.x = (hit.distance - skinWidth) * dirX;
                rayLength = hit.distance;

                // Set correct collisionInfo boolean to true
                // Right-hand side returns a boolean
                collisionInfo.right = (dirX == 1);
                collisionInfo.left = (dirX == -1);
            }
        }
    }






    // Determine direction of collision (Above, below, right and left)
    public struct CollisionInfo {
        public bool above;
        public bool below;
        public bool right;
        public bool left;
        public int direction; // 1 = right  -1 = left

        public void Reset() {
            // Sets all collision direction indicators to false
            above = below = right = left = false;
        }
    }



}
