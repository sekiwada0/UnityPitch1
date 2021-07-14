using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitcherAnim : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		m_anim = GetComponent<Animator>();
		setAnim( Pitcher_Idle, 0 );
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void setAnim(int index,float offset)
	{
		if( index == Pitcher_Idle )
			m_anim.CrossFadeInFixedTime("pitcher_IDLE",0,-1,offset);
		else if( index == Pitcher_Pitch )
			m_anim.CrossFadeInFixedTime("pitcher_WINDUP",0,-1,offset);
	}
	public const int Pitcher_Idle  = 0;
	public const int Pitcher_Pitch = 1;

	Animator m_anim;
}
