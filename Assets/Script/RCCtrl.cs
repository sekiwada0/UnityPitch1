using UnityEngine;
using System.Collections;

public class RCCtrl : MonoBehaviour
{
	void Start()
	{
	}
	void Update()
	{
		//Debug.Log("deltaTime: " + Time.deltaTime);
		transform.Rotate( 0, rotate_scale * Time.deltaTime, 0 );
	}
	[SerializeField]
	private float rotate_scale = 10;
}
