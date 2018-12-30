using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ensures that the component this script is attached to has a BoxCollider2D script on it
// Generates the BoxCollider2D if one is not found

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    public LayerMask collisionMask;
    public CollisionInfo collisionInfo;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    private BoxCollider2D collider;
    private RayCastOrigins rayCastOrigins;
    private Bounds bounds;

    private const float skinWidth = 0.015f;


    private void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector2 velocity) {
        UpdateRayCastOrigins();
        // Reset collision indicators
        collisionInfo.Reset();

        if(velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }

        if(velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }

        transform.Translate(velocity);
    }

    private void VerticalCollisions(ref Vector2 velocity) {
        // Up = 1   Down = -1
        float dirY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 origin = (dirY == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
            origin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(origin, dirY * Vector2.up, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.up * dirY * rayLength, Color.red);

            // If raycast hits something
            if (hit) {
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
        float dirX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 origin = (dirX == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight;
            origin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, dirX * Vector2.right, rayLength, collisionMask);

            Debug.DrawRay(origin, Vector2.right * dirX * rayLength, Color.red);


            // If raycast hits something
            if (hit) {
                velocity.x = (hit.distance - skinWidth) * dirX;
                rayLength = hit.distance;

                // Set correct collisionInfo boolean to true
                // Right-hand side returns a boolean
                collisionInfo.right = (dirX == 1);
                collisionInfo.left = (dirX == -1);
            }
        }
    }

    // Determines where each ray is shot from (each corner of the controller's bounding box)
    private void UpdateRayCastOrigins() {
        AssignBounds();
        // Assign raycastorigins to the corners of the bounding box
        rayCastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        rayCastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        rayCastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        rayCastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    private void AssignBounds() {
        // Bounding box of the player
        bounds = collider.bounds;
        // Makes it so it doesn't take up the entire of the player, there is a gap around it
        bounds.Expand(skinWidth * -2);
    }

    private void CalculateRaySpacing() {
        AssignBounds();

        // Make sure at least two rays are being fired
        // Not worrying about a max number
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calculate spacing
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    // Determine direction of collision (Above, below, right and left)
    public struct CollisionInfo {
        public bool above;
        public bool below;
        public bool right;
        public bool left;

        public void Reset() {
            // Sets all collision direction indicators to false
            above = below = right = left = false;
        }
    }

    private struct RayCastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

}
