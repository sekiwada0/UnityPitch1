using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorCtrl : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		m_anim = GetComponent<Animator>();
		//setAnim( 0, 0 );
	}

	// Update is called once per frame
	void Update()
	{
	}
	public int addAnim(string name)
	{
		int index = m_AnimArray.Count;
		m_AnimArray.Add( name );
		return index;
	}
	public void setAnim(int index,float offset)
	{
		m_anim.CrossFadeInFixedTime(m_AnimArray[index], 0, -1, offset);
	}

	Animator m_anim;
	private List<string> m_AnimArray = new List<string>();
}
