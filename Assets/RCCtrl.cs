using UnityEngine;
using System.Collections;

public class RCCtrl : MonoBehaviour
{
	//private float m_rotate_scale = 1;
	private float m_rotate_scale = 10;

	void Start()
	{
	}

	void Update()
	{
		//Debug.Log("deltaTime: " + Time.deltaTime);
		transform.Rotate( 0, m_rotate_scale * Time.deltaTime, 0 );
	}
}
