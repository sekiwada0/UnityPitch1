using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterAnim : MonoBehaviour
{
	/*void Awake () {
		m_anim = GetComponent<Animator>();
	}*/

	// Start is called before the first frame update
	void Start()
	{
		m_anim = GetComponent<Animator>();
		setAnim( BatterAnim_Idle );
		//gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void setAnim(int index)
	{
		if( index == BatterAnim_Idle )
			m_anim.CrossFade("Batter_IDLE",0);
		else if( index == BatterAnim_Hit )
			m_anim.CrossFade("Batter_BAT",0);
	}
	public void setBatterSide(int index)
	{
		if( index == m_nBatterSide )
			return;
		m_nBatterSide = index;

		if( m_nBatterSide == BatterSide_None ){
			gameObject.SetActive(false);
		}
		else{
			gameObject.SetActive(true);

			Vector3 pos = new Vector3();
			Vector3 rotate = new Vector3();
			Vector3 scale = new Vector3();
			float size = 1.25f;
			if( m_nBatterSide == BatterSide_Left ){
				pos.x = -1.15f;
				rotate.y = 90;
				scale.x = size;
			}
			else{ /*BatterSide_Right*/
				pos.x = 1.15f;
				rotate.y = -90;
				scale.x = -size;
			}
			pos.y = -0.02f;
			scale.y = size;
			scale.z = size;
			transform.localPosition = pos;
			transform.eulerAngles = rotate;
			transform.localScale = scale;
		}
	}
	public const int BatterAnim_Idle  = 0;
	public const int BatterAnim_Hit   = 1;

	public const int BatterSide_None  = 0;
	public const int BatterSide_Left  = 1;
	public const int BatterSide_Right = 2;

	//private int m_nBatterSide = BatterSide_None;
	private int m_nBatterSide = -1;

	Animator m_anim;
}
