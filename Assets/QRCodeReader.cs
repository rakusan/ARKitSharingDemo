using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class QRCodeReader : MonoBehaviour {

#if UNITY_IPHONE && !UNITY_EDITOR

	[DllImport ("__Internal")]
	private static extern void ReadQRCode(long mtlTexPtr);

	[DllImport ("__Internal")]
	private static extern void GetQRCodeBounds(out IntPtr boundsPtr);

	private static float[] GetQRCodeBounds() {
		IntPtr boundsPtr;
		GetQRCodeBounds (out boundsPtr);
		float[] bounds = new float[8];
		Marshal.Copy (boundsPtr, bounds, 0, 8);
		return bounds;
	}

#else

	private static void ReadQRCode(long mtlTexPtr) {
	}

	private static float[] GetQRCodeBounds() {
		return new float[8];
	}

#endif

	private bool done = false;
	private UnityARSessionNativeInterface arSession = null;
	private GameObject qrcodePlane;
	private GameObject plane;

	// Use this for initialization
	void Start () {
		arSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		qrcodePlane = transform.Find ("QRCodePlane").gameObject;
		plane = transform.Find ("QRCodePlane/Plane").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if (!done) {
			ARTextureHandles handles = arSession.GetARVideoTextureHandles ();
			if (handles.textureY != System.IntPtr.Zero) {
				ReadQRCode (handles.textureY.ToInt64 ());
			}
		}
	}

	void OnReadQRCode(string arg) {
		float[] bounds = GetQRCodeBounds ();

		//Debug.Log (string.Format ("QR topLeft: {0:0.######},{1:0.######}", bounds [0], bounds [1]));
		//Debug.Log (string.Format ("QR topRight: {0:0.######},{1:0.######}", bounds [2], bounds [3]));
		//Debug.Log (string.Format ("QR bottomRight: {0:0.######},{1:0.######}", bounds [4], bounds [5]));
		//Debug.Log (string.Format ("QR bottomLeft: {0:0.######},{1:0.######}", bounds [6], bounds [7]));

		var topLeft     = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [0], bounds [1]));
		var topRight    = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [2], bounds [3]));
		var bottomRight = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [4], bounds [5]));
		var bottomLeft  = Camera.main.ScreenToViewportPoint (new Vector3 (bounds [6], bounds [7]));

		HitTest (topLeft, topRight, bottomRight, bottomLeft);
	}

	private void HitTest(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) {
		Dictionary<string, List<ARHitTestResult>> results = new Dictionary<string, List<ARHitTestResult>>();
		HitTest (topLeft, results);
		HitTest (topRight, results);
		HitTest (bottomRight, results);
		HitTest (bottomLeft, results);

		foreach (var result in results) {
			List<ARHitTestResult> list = result.Value;
			if (list.Count == 4) {
				var worldTopLeft     = UnityARMatrixOps.GetPosition (list[0].worldTransform);
				//var worldTopRight    = UnityARMatrixOps.GetPosition (list[1].worldTransform);
				var worldBottomRight = UnityARMatrixOps.GetPosition (list[2].worldTransform);
				var worldBottomLeft  = UnityARMatrixOps.GetPosition (list[3].worldTransform);

				var bottomToTop = worldTopLeft - worldBottomLeft;
				var leftToRight = worldBottomRight - worldBottomLeft;
				qrcodePlane.transform.forward = bottomToTop;
				qrcodePlane.transform.position = worldBottomLeft + (bottomToTop + leftToRight) * 0.5f;
				plane.transform.localScale = new Vector3(leftToRight.magnitude, 1, bottomToTop.magnitude) * 0.1f;
				break;
			}
		}
	}

	private void HitTest(Vector3 point, Dictionary<string, List<ARHitTestResult>> results) {
		List<ARHitTestResult> hitResults = arSession.HitTest (
			new ARPoint { x = point.x, y = point.y },
			ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);

		foreach (var hitResult in hitResults) {
			string anchorIdentifier = hitResult.anchorIdentifier;
			List<ARHitTestResult> list;
			if (!results.TryGetValue (anchorIdentifier, out list)) {
				list = new List<ARHitTestResult> ();
				results.Add (anchorIdentifier, list);
			}
			list.Add (hitResult);
		}
	}

	public void OnSetAnchorClick(Text text) {
		if (done) {
			done = false;
			text.text = "Set Anchor";
		} else {
			done = true;
			text.text = "Retry Anchor";
		}
	}
}
