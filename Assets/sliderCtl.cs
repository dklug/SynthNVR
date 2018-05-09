using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class sliderCtl : VRTK_InteractableObject {

	public Transform sliderBase;
	public interpolateBary controlSphere;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (IsGrabbed ()) {
            Vector3 clampedPos = transform.position;
            clampedPos.x = sliderBase.position.x;
            clampedPos.z = sliderBase.position.z;
            transform.position = clampedPos;
			controlSphere.updateControlFromSliders ();
		}
	}
}