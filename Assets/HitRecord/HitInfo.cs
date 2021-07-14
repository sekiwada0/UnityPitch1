
using UnityEngine;
using System.Collections;

public class HitInfo : MonoBehaviour {
	// Use this for initialization
	void Start(){
	}
	// Update is called once per frame
	void Update(){
		m_dTime += Time.deltaTime;

		float ratio = m_dTime / SpeedLife;
		float value = 1.0f + (SpeedScale - 1.0f) * ratio;
		transform.localScale = new Vector3(value, value, value);

		Vector3 pos = transform.localPosition;
		pos.y += 0.1f;
		transform.localPosition = pos;
	}
	private float m_dTime = 0;
	private const float SpeedLife = 3;
	private const float SpeedScale = 1.2f;
}
