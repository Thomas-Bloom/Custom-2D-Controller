using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : RayCastController {

    public float moveSpeed;
    public LayerMask passengerMask;

    public Vector2[] wayPoints;
    public bool isCyclic;
    public float waitTime;
    [Range(0, 2)]
    public float smoothAmount;

    private Vector2[] globalWayPoints;
    private int fromIndex;
    private float wayPointPercentage; // 0 - 1
    private float nextMoveTime;

    private List<PassengerMovement> passengerMovement;
    private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	public override void Start () {
        base.Start();

        globalWayPoints = new Vector2[wayPoints.Length];

        for (int i = 0; i < wayPoints.Length; i++) {
            globalWayPoints[i] = wayPoints[i] + (Vector2)transform.position;
        }
    }
	
	void Update () {
        UpdateRayCastOrigins();

        Vector2 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);
        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
	}

    private float SmoothMovement(float x) {
        float a = smoothAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
    }

    private Vector2 CalculatePlatformMovement() {
        if(Time.time < nextMoveTime) {
            return Vector2.zero;
        }

        fromIndex %= globalWayPoints.Length;

        int toIndex = (fromIndex + 1) % globalWayPoints.Length;

        float distanceBetween = Vector2.Distance(globalWayPoints[fromIndex], globalWayPoints[toIndex]);
        wayPointPercentage += Time.deltaTime * moveSpeed / distanceBetween;
        wayPointPercentage = Mathf.Clamp01(wayPointPercentage);

        float smoothedPercentage = SmoothMovement(wayPointPercentage);

        Vector2 newPos = Vector2.Lerp(globalWayPoints[fromIndex], globalWayPoints[toIndex], smoothedPercentage);

        // Reached next waypoint
        if(wayPointPercentage >= 1) {
            wayPointPercentage = 0;
            fromIndex++;

            if (!isCyclic) {
                if (fromIndex >= globalWayPoints.Length - 1) {
                    fromIndex = 0;
                    System.Array.Reverse(globalWayPoints);
                }
            }

            nextMoveTime = Time.time + waitTime;
        }
        return newPos - (Vector2)transform.position;
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
        if(wayPoints != null) {
            Gizmos.color = Color.red;
            float size = 0.25f;

            for(int i = 0; i < wayPoints.Length; i++) {
                Vector2 globalPointPos = (Application.isPlaying) ? globalWayPoints[i] : wayPoints[i] + (Vector2)transform.position;
                Gizmos.DrawLine(globalPointPos - Vector2.up * size, globalPointPos + Vector2.up * size);
                Gizmos.DrawLine(globalPointPos - Vector2.left * size, globalPointPos + Vector2.left * size);
            }
        }
    }
}
