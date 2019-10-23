using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Controller2D target;
    public Vector2 focusSize;

    public float verticalOffset;

    private Focus focus;

    private void Start() {
        focus = new Focus(target.collider.bounds, focusSize);
    }

    private void LateUpdate() {
        focus.Update(target.collider.bounds);

        Vector2 focusPos = focus.centre + Vector2.up * verticalOffset;

        transform.position = (Vector3)focusPos + Vector3.forward * -10;
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(focus.centre, focusSize);
    }

    struct Focus {
        public Vector2 centre;
        public Vector2 velocity;

        private float leftSide, rightSide, topSide, bottomSide;

        public Focus(Bounds targetBounds, Vector2 size) {
            leftSide = targetBounds.center.x - (size.x / 2);
            rightSide = targetBounds.center.x + (size.x / 2);
            topSide = targetBounds.center.y + size.y;
            bottomSide = targetBounds.min.y;

            velocity = Vector2.zero;

            centre = new Vector2((leftSide + rightSide) / 2, (topSide + bottomSide) / 2);
        }

        public void Update(Bounds targetBounds) {
            float shiftX = 0;

            if (targetBounds.min.x < leftSide) {
                shiftX = targetBounds.min.x - leftSide;
            }
            else if(targetBounds.max.x > rightSide) {
                shiftX = targetBounds.max.x - rightSide;
            }
            leftSide += shiftX;
            rightSide += shiftX;

            float shiftY = 0;

            if (targetBounds.min.y < bottomSide) {
                shiftY = targetBounds.min.y - bottomSide;
            }
            else if (targetBounds.max.y > topSide) {
                shiftY = targetBounds.max.y - topSide;
            }
            bottomSide += shiftY;
            topSide += shiftY;

            // Update centre position
            centre = new Vector2((leftSide + rightSide) / 2, (topSide + bottomSide) / 2);
            velocity = new Vector2(shiftX, shiftY);

        }
    }
}
