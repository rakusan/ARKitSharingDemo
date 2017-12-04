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
	private static extern void GetQRCodeCorners(out IntPtr cornersPtr);

	private static float[] GetQRCodeCorners() {
		IntPtr cornersPtr;
		GetQRCodeCorners (out cornersPtr);
		float[] corners = new float[8];
		Marshal.Copy (cornersPtr, corners, 0, 8);
		return corners;
	}

#else

	private static void ReadQRCode(long mtlTexPtr) {
	}

	private static float[] GetQRCodeCorners() {
		return new float[8];
	}

#endif

	private bool done = false;
	private UnityARSessionNativeInterface arSession = null;
	private Matrix4x4 displayTransformInverse;
	private GameObject qrcodePlane;
	private GameObject plane;

	// Use this for initialization
	void Start () {
		arSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
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

	private void ARFrameUpdated(UnityARCamera camera) {
		Matrix4x4 tmp = new Matrix4x4 (
			camera.displayTransform.column0,
			camera.displayTransform.column1,
			camera.displayTransform.column2,
			camera.displayTransform.column3
		);
		displayTransformInverse = tmp.inverse;
	}

	private Vector2 VideoTextureToViewportPoint(Vector2 videoTexturePoint) {
		Vector4 column0 = displayTransformInverse.GetColumn(0);
		Vector4 column1 = displayTransformInverse.GetColumn(1);
		float x = column0.x * videoTexturePoint.x + column0.y * videoTexturePoint.y + column0.z;
		float y = column1.x * videoTexturePoint.x + column1.y * videoTexturePoint.y + column1.z;
		return new Vector2 (x, y);
	}

	void OnReadQRCode(string arg) {
		float[] corners = GetQRCodeCorners ();

//		Debug.Log (string.Format ("QR topLeft: {0:0.######},{1:0.######}", corners [0], corners [1]));
//		Debug.Log (string.Format ("QR topRight: {0:0.######},{1:0.######}", corners [2], corners [3]));
//		Debug.Log (string.Format ("QR bottomLeft: {0:0.######},{1:0.######}", corners [4], corners [5]));
//		Debug.Log (string.Format ("QR bottomRight: {0:0.######},{1:0.######}", corners [6], corners [7]));

		var topLeft     = VideoTextureToViewportPoint(new Vector2 (corners [0], corners [1]));
		var topRight    = VideoTextureToViewportPoint(new Vector2 (corners [2], corners [3]));
		var bottomLeft  = VideoTextureToViewportPoint(new Vector2 (corners [4], corners [5]));
		var bottomRight = VideoTextureToViewportPoint(new Vector2 (corners [6], corners [7]));

		HitTest (topLeft, topRight, bottomLeft, bottomRight);
	}

	private void HitTest(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) {
		Dictionary<string, List<ARHitTestResult>> results = new Dictionary<string, List<ARHitTestResult>>();
		HitTest (topLeft, results);
		HitTest (topRight, results);
		HitTest (bottomLeft, results);
		HitTest (bottomRight, results);

		foreach (var result in results) {
			List<ARHitTestResult> list = result.Value;
			if (list.Count == 4) {
				var worldTopLeft     = UnityARMatrixOps.GetPosition (list[0].worldTransform);
				//var worldTopRight    = UnityARMatrixOps.GetPosition (list[1].worldTransform);
				var worldBottomLeft  = UnityARMatrixOps.GetPosition (list[2].worldTransform);
				var worldBottomRight = UnityARMatrixOps.GetPosition (list[3].worldTransform);

				var bottomToTop = worldTopLeft - worldBottomLeft;
				var leftToRight = worldBottomRight - worldBottomLeft;
				qrcodePlane.transform.forward = bottomToTop;
				qrcodePlane.transform.position = worldBottomLeft + (bottomToTop + leftToRight) * 0.5f;
				plane.transform.localScale = new Vector3(leftToRight.magnitude, 1, bottomToTop.magnitude) * 0.1f;
				break;
			}
		}
	}

	private void HitTest(Vector2 point, Dictionary<string, List<ARHitTestResult>> results) {
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
