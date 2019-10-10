using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitcherAnim : MonoBehaviour
{
	/*void Awake () {
		m_anim = GetComponent<Animator>();
	}*/

	// Start is called before the first frame update
	void Start()
	{
		m_anim = GetComponent<Animator>();
		setAnim( Pitcher_Idle );

		//m_anim.CrossFade("Take 001",0); /*Pitcher_Full_Throws: 投げる -> 腕ストレッチ*/
		//m_anim.CrossFade("Take 001",0); /*Pitcher_handTheCap: 帽子いじる*/
		//m_anim.CrossFade("pitcher_IDLE",0); /*Pitcher_IDLE: 待機*/
		//m_anim.CrossFade("pitcher_PASS",0); /*Pitcher_PASS: グローブ開いてパス待ち*/
		//m_anim.CrossFade("pitcher_WINDUP",0); /*Pitcher_WINDUP: 投げる*/
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void setAnim(int index)
	{
		if( index == Pitcher_Idle )
			m_anim.CrossFade("pitcher_IDLE",0);
		else if( index == Pitcher_Pitch )
			m_anim.CrossFade("pitcher_WINDUP",0);
	}
	public const int Pitcher_Idle  = 0;
	public const int Pitcher_Pitch = 1;

	Animator m_anim;
}
