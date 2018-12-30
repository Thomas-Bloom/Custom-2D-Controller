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

        VerticalCollisions(ref velocity);

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

            Debug.DrawRay(rayCastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);

            // If raycast hits something
            if (hit) {
                velocity.y = (hit.distance - skinWidth) * dirY;
                rayLength = hit.distance;
            }
        }
    }

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

    struct RayCastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

}
