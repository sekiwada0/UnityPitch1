
using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	// Use this for initialization
	void Start(){
	}
	// Update is called once per frame
	void Update(){
		switch( m_move_mode ){
		case MoveMode_MoveTarget: /*setMoveTarget*/
		case MoveMode_MoveFocus:{ /*setMoveFocus*/
			// 2点間の距離を速度に反映する
			//const float EASING = 0.05f;
			const float EASING = 0.1f;
			Vector3 diff = m_target_pos - transform.position;
			Vector3 v = diff * EASING;
			transform.position += v;

			if( m_move_mode == MoveMode_MoveFocus ){ /*setMoveFocus*/
				transform.LookAt( m_look );
			}

			// 十分近づいたらアニメーション終了
			if( diff.magnitude < 0.01f )
			{
				//Debug.Log("END");
				m_move_mode = MoveMode_Node;
			}
			break;}
		case MoveMode_LookAround:{ /*setLookAround*/
			m_roty += m_rotate * Time.deltaTime;
			Vector3 vec = new Vector3( 0, 0, m_distance );
			vec = rotateX( vec, m_rotx );
			vec = rotateY( vec, m_roty );
			transform.position = m_look - vec;

			transform.eulerAngles = new Vector3(m_rotx * Mathf.Rad2Deg, m_roty * Mathf.Rad2Deg, 0);
			break;}
		case MoveMode_TopView:{
			const float EASING = 0.1f;
			bool end = false;
			float cur_dist = (m_target_pos - transform.position).magnitude;
			float diff = m_target_dist - cur_dist;
			cur_dist += diff * EASING;
			if( Mathf.Abs( diff ) < 0.01f )
				end = true;
			Quaternion rot = Quaternion.Lerp( transform.rotation, m_target_rot, EASING );
			float angle = Quaternion.Angle( transform.rotation, m_target_rot );
			if( angle < 0.1f )
				end = true;
			Vector3 pos = new Vector3( 0, 0, -cur_dist );
			pos = rot * pos;
			pos += m_target_pos;
			transform.position = pos;
			transform.rotation = rot;
			// 十分近づいたらアニメーション終了
			if( end )
			{
				m_move_mode = MoveMode_Node;
			}
			break;}
		}
		switch( m_zoom_mode ){
		case 1:{
			const float EASING = 0.1f;
			float fov = m_objCamera.fieldOfView;
			float diff = m_target_fov - fov;
			float v = diff * EASING;
			fov += v;
			m_objCamera.fieldOfView = fov;
			if( Mathf.Abs(diff) < 0.01f )
			{
				//Debug.Log("END");
				m_zoom_mode = 0;
			}
			break;}
		}
	}

	public void clear(){
		m_move_mode = MoveMode_Node;
		m_zoom_mode = 0;
	}
	public void setMoveTarget(Vector3 target){
		m_target_pos = target;
		m_move_mode = MoveMode_MoveTarget;
	}
	public void setTopView(Vector3 pos,float distance){
		m_target_rot = Quaternion.Euler( 90, 0, 0 );
		m_target_pos = pos;
		m_target_dist = distance;
		m_move_mode = MoveMode_TopView;
	}
	public void setMoveFocus(Vector3 target,Vector3 look){
		m_target_pos = target;
		m_look = look;
		m_move_mode = MoveMode_MoveFocus;
	}
	public void setLookAround(Vector3 look,float rotate){
		Vector3 vec = look - transform.position;

		float roty = -Mathf.Atan2(vec.x, vec.z);

		vec = rotateY( vec, roty );

		float rotx = Mathf.Atan2(vec.y, vec.z);

		vec = rotateX( vec, rotx );

		m_rotx = -rotx;
		m_roty = -roty;
		m_distance = vec.z;
		m_rotate = rotate * Mathf.Deg2Rad;
		m_look = look;
		m_move_mode = MoveMode_LookAround;
	}
	public void setZoom(float fov){
		if( !m_objCamera )
			m_objCamera = GetComponent<Camera>();
		if( m_objCamera ){
			m_target_fov = fov;
			m_zoom_mode = 1;
		}
	}
	public bool isProc(){
		if( m_move_mode == MoveMode_Node && m_zoom_mode == 0 )
			return false;
		return true;
	}
	private static Vector3 rotateX(Vector3 vect, float xrot)
	{
		float sn = Mathf.Sin(xrot);
		float cs = Mathf.Cos(xrot);
		float vy = vect.y * cs - vect.z * sn;
		float vz = vect.y * sn + vect.z * cs;
		return new Vector3(vect.x, vy, vz);
	}
	private static Vector3 rotateY(Vector3 vect, float yrot)
	{
		float sn = Mathf.Sin(yrot);
		float cs = Mathf.Cos(yrot);
		float vx = vect.x * cs + vect.z * sn;
		float vz = -vect.x * sn + vect.z * cs;
		return new Vector3(vx, vect.y, vz);
	}
	private const int MoveMode_Node        = 0;
	private const int MoveMode_MoveTarget  = 1;
	private const int MoveMode_TopView     = 2;
	private const int MoveMode_MoveFocus   = 3;
	private const int MoveMode_LookAround  = 4;

	private int m_move_mode = MoveMode_Node;
	private int m_zoom_mode = 0;
	private Vector3 m_target_pos;
	private Quaternion m_target_rot;
	private float m_target_dist;
	private Vector3 m_look;
	private float m_rotate;
	private float m_distance;
	private float m_rotx, m_roty;
	private float m_target_fov;
	private Camera m_objCamera = null;
}
