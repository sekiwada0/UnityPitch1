
using UnityEngine;
using System.Collections.Generic;

public class HitBall {
	public HitBall(GameObject obj_){
		obj = obj_;

		//posArray = new List<Vector3>();
		//posArray.Add( obj.transform.localPosition );
	}
	public GameObject obj;
	//public List<Vector3> posArray;
	//public float dTime = 0;
	public bool ground = false;
};

