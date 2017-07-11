using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class SharingReceive : MonoBehaviour {

	[SerializeField]
	private GameObject qrcodePlane;

	[SerializeField]
	private GameObject avatarPrefab;

	private Hashtable devices = new Hashtable();
	private Dictionary<string, GameObject> avatars = new Dictionary<string, GameObject> ();

	// Use this for initialization
	void Start () {
		UdpClient udpReceive = new UdpClient (new IPEndPoint (IPAddress.Any, 3333));
		udpReceive.BeginReceive (ReceiveCallback, udpReceive);
	}
	
	// Update is called once per frame
	void Update () {
		lock (devices.SyncRoot) {
			foreach (DictionaryEntry e in devices) {
				string address = (string)e.Key;
				object[] values = (object[])e.Value;
				Vector3 position = (Vector3)values [0];
				Quaternion rotation = (Quaternion)values [1];

				GameObject avatar;
				if (!avatars.TryGetValue (address, out avatar)) {
					avatar = Instantiate (avatarPrefab);
					avatars [address] = avatar;
				}
				avatar.transform.position = qrcodePlane.transform.TransformPoint (position);
				avatar.transform.rotation = qrcodePlane.transform.rotation * rotation;
			}
		}
	}

	private void ReceiveCallback(IAsyncResult ar) {
		UdpClient udpReceive = (UdpClient) ar.AsyncState;
		IPEndPoint remoteEP = null;
		byte[] udpData = udpReceive.EndReceive (ar, ref remoteEP);

		Vector3 cameraPosition = new Vector3 (
			BitConverter.ToSingle (udpData, 0),
			BitConverter.ToSingle (udpData, 4),
			BitConverter.ToSingle (udpData, 8));
		
		Quaternion cameraRotation = new Quaternion (
			BitConverter.ToSingle (udpData, 12),
			BitConverter.ToSingle (udpData, 16),
			BitConverter.ToSingle (udpData, 20),
			BitConverter.ToSingle (udpData, 24));

		string address = remoteEP.Address.ToString ();
		object[] values = new object[] { cameraPosition, cameraRotation };

		lock (devices.SyncRoot) {
			devices [address] = values;
		}

		udpReceive.BeginReceive (ReceiveCallback, udpReceive);
	}
}
