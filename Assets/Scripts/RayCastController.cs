using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ensures that the component this script is attached to has a BoxCollider2D script on it
// Generates the BoxCollider2D if one is not found
[RequireComponent(typeof(BoxCollider2D))]
public class RayCastController : MonoBehaviour {
    public const float skinWidth = 0.015f;

    public LayerMask collisionMask;
    public RayCastOrigins rayCastOrigins;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;
    [HideInInspector]
    public Bounds bounds;
    [HideInInspector]
    public BoxCollider2D collider;

    public virtual void Awake() {
        collider = GetComponent<BoxCollider2D>();
    }

    public virtual void Start() {
        CalculateRaySpacing();
    }

    public void AssignBounds() {
        // Bounding box of the player
        bounds = collider.bounds;
        // Makes it so it doesn't take up the entire of the player, there is a gap around it
        bounds.Expand(skinWidth * -2);
    }

    // Determines where each ray is shot from (each corner of the controller's bounding box)
    public void UpdateRayCastOrigins() {
        AssignBounds();

        // Assign raycastorigins to the corners of the bounding box
        rayCastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        rayCastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        rayCastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        rayCastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public void CalculateRaySpacing() {
        AssignBounds();
        // Make sure at least two rays are being fired
        // Not worrying about a max number
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calculate spacing
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RayCastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
