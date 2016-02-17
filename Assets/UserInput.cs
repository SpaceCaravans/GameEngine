using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour {

    SpacecraftControl spacecraftControl;

	// Use this for initialization
	void Start () {
        spacecraftControl = GetComponent<SpacecraftControl>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float roll = Input.GetAxis("Horizontal");
        float pitch = Input.GetAxis("Vertical");
        bool airBrakes = Input.GetButton("Fire1");
        float throttle = Input.GetAxis("Throttle");
        float yaw = Input.GetAxis("Yaw");
        float stationaryVertical = Input.GetAxis("StationaryVertical");

        spacecraftControl.Move(roll, pitch, yaw, throttle, airBrakes, stationaryVertical);
	}
}
