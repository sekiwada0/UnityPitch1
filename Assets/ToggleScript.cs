using UnityEngine;
using System.Collections;

public class ToggleScript : MonoBehaviour {
	void Start(){
	}
	void Update(){
	}
	public void onValueChanged()
	{
		if( m_main == null ){
			GameObject objMain;
			objMain = GameObject.FindWithTag("MainCamera");
			m_main = objMain.GetComponent<Pitch_Main1>();
		}
		//m_main.onValueChanged( gameObject );
	}
	private Pitch_Main1 m_main;
}
