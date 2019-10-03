using UnityEngine;
using System.Collections;

public class BallHit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update(){

	}

	void OnCollisionEnter(Collision col)
	{
		//if( col.gameObject.tag == "Ground" )
        {
			if( m_main == null ){
				GameObject objMain;
				objMain = GameObject.FindWithTag("MainCamera");
				m_main = objMain.GetComponent<Pitch_Main1>();
				m_main.HitGround( gameObject );
			}
		}
	}
	/*public void setRecord(HitRecord record)
	{
		m_record = record;
	}
	public HitRecord getRecord()
	{
		return m_record;
	}*/
	private Pitch_Main1 m_main;
	//private HitRecord m_record;
}
