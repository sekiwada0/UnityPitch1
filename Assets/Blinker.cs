
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }
	void OnEnable()
	{
		m_dTime = interval;
		m_show = true;
	}
    // Update is called once per frame
    void Update()
    {
		m_dTime -= Time.deltaTime;
		if( m_dTime <= 0 ){
			m_show = !m_show;

			GetComponent<CanvasRenderer>().SetAlpha( m_show? 1.0f: 0.0f );

			m_dTime = interval;
		}
    }
	public float interval = 1.0f;	// 点滅周期

	private float m_dTime;
	private bool m_show;
}
