using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class QRCodeReaderTango : MonoBehaviour, ITangoVideoOverlay {

	private const double MARKER_SIZE = 0.0945;

	private TangoApplication tangoApp;

	private bool done = false;
	private GameObject qrcodePlane;
	private GameObject plane;

	private List<TangoSupport.Marker> markerList;


	void Start () {
		tangoApp = FindObjectOfType<TangoApplication> ();
		if (tangoApp != null) {
			tangoApp.Register (this);
		} else {
			Debug.Log("No Tango Manager found in scene.");
		}

		qrcodePlane = transform.Find ("QRCodePlane").gameObject;
		plane = transform.Find ("QRCodePlane/Plane").gameObject;

		markerList = new List<TangoSupport.Marker>();
	}
	
	public void OnTangoImageAvailableEventHandler(Tango.TangoEnums.TangoCameraId cameraId, 
		Tango.TangoUnityImageData imageBuffer) {

		TangoSupport.DetectMarkers(imageBuffer, cameraId,
			TangoSupport.MarkerType.QRCODE, MARKER_SIZE, markerList);

		if (markerList.Count > 0) {
			TangoSupport.Marker marker = markerList[0];

			qrcodePlane.transform.position = marker.m_translation;
			qrcodePlane.transform.rotation = marker.m_orientation;

			var bottomToTop = marker.m_corner3DP3 - marker.m_corner3DP0;
			var leftToRight = marker.m_corner3DP1 - marker.m_corner3DP0;
			plane.transform.localScale = new Vector3(leftToRight.magnitude, 1, bottomToTop.magnitude) * 0.1f;
		}
	}

	public void OnSetAnchorClick(Text text) {
		if (done) {
			done = false;
			tangoApp.Register (this);
			text.text = "Set Anchor";
		} else {
			done = true;
			tangoApp.Unregister (this);
			text.text = "Retry Anchor";
		}
	}
}
