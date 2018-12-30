using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ensures that the component this script is attached to has a Controller2D script on it
// Generates the Controller script if one is not found
[RequireComponent(typeof (Controller2D))]
public class PlayerControls : MonoBehaviour {

    private Controller2D controller;

    private void Start() {
        controller = GetComponent<Controller2D>();
    }
}
