using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

	[SerializeField]
	private GameObject qrcodePlane;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = qrcodePlane.transform.TransformPoint (new Vector3(0, 0.05f, 0.4f));
		transform.rotation = qrcodePlane.transform.rotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
	}
}
