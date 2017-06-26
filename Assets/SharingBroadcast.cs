using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class SharingBroadcast : MonoBehaviour {

	[SerializeField]
	private GameObject qrcodePlane;

	private UdpClient udpBroadcast;
	private static IPEndPoint udpEndPoint = new IPEndPoint (IPAddress.Broadcast, 3333);

	// Use this for initialization
	void Start () {
		udpBroadcast = new UdpClient ();
		udpBroadcast.EnableBroadcast = true;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 cameraPosition = qrcodePlane.transform.InverseTransformPoint(Camera.main.transform.position);
		Quaternion cameraRotation = Quaternion.Inverse(qrcodePlane.transform.rotation) * Camera.main.transform.rotation;

		byte[] udpData = new byte[sizeof(float) * 7];
		Array.Copy (BitConverter.GetBytes (cameraPosition.x), 0, udpData,  0, 4);
		Array.Copy (BitConverter.GetBytes (cameraPosition.y), 0, udpData,  4, 4);
		Array.Copy (BitConverter.GetBytes (cameraPosition.z), 0, udpData,  8, 4);
		Array.Copy (BitConverter.GetBytes (cameraRotation.x), 0, udpData, 12, 4);
		Array.Copy (BitConverter.GetBytes (cameraRotation.y), 0, udpData, 16, 4);
		Array.Copy (BitConverter.GetBytes (cameraRotation.z), 0, udpData, 20, 4);
		Array.Copy (BitConverter.GetBytes (cameraRotation.w), 0, udpData, 24, 4);
		udpBroadcast.BeginSend(udpData, udpData.Length, udpEndPoint, null, null);
	}
}
