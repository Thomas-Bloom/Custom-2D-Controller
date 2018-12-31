using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : RayCastController {

    public Vector2 move;
    public LayerMask passengerMask;

    public Vector2[] wayPoints;

    private List<PassengerMovement> passengerMovement;
    private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start () {
        base.Start();
	}
	
	void Update () {
        UpdateRayCastOrigins();

        Vector2 velocity = move * Time.deltaTime;

        CalculatePassengerMovement(velocity);
        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
	}

    private void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in passengerMovement) {
            if (!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if(passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.onPlatform);
            }
        }
    }

    // Vector2 is the velocity of any controller2D that is being affected by platform
    private void CalculatePassengerMovement(Vector2 velocity) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float dirX = Mathf.Sign(velocity.x);
        float dirY = Mathf.Sign(velocity.y);

        // Vertical platform
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 origin = (dirY == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
                origin += Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(origin, dirY * Vector2.up, rayLength, passengerMask);

                // Found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);

                        float pushX = (dirY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * dirY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), dirY == 1, true));
                    }
                }
            }
        }
        // Horizontal platform
        if (velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 origin = (dirY == -1) ? origin = rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
                origin += Vector2.right * (verticalRaySpacing * i + velocity.x);

                RaycastHit2D hit = Physics2D.Raycast(origin, dirY * Vector2.up, rayLength, passengerMask);

                // Found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);

                        float pushX = velocity.x - (hit.distance - skinWidth) * dirX;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger on top of horizontal or downward moving platform
        if(dirY == -1 || velocity.y == 0 && velocity.x != 0) {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 origin = rayCastOrigins.topLeft;
                origin += Vector2.right * (verticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, passengerMask);

                // Found a passenger
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);

                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector2 velocity;
        public bool onPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform t, Vector2 v, bool on, bool before) {
            transform = t;
            velocity = v;
            onPlatform = on;
            moveBeforePlatform = before;
        }
    }

    private void OnDrawGizmos() {
        
    }
}
