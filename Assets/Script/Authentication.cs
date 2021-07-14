
using UnityEngine;
using UnityEngine.UI;

public class Authentication : MonoBehaviour {
	// Use this for initialization
	void Start(){
		GameObject objMainCanvas = GameObject.FindWithTag("MainCanvas");
		GameObject objVendorID = objMainCanvas.transform.Find("VendorID_value").gameObject;
		GameObject objDeviceID = objMainCanvas.transform.Find("DeviceID_value").gameObject;

		Text txVendorID = objVendorID.GetComponent<Text>();
		txVendorID.text = MainFrame.getAdminString("VendorID");

		Text txDeviceID = objDeviceID.GetComponent<Text>();
		txDeviceID.text = MainFrame.getAdminString("DeviceID");
	}
	// Update is called once per frame
	void Update(){
	}
}
