/*
	https://www.urablog.xyz/entry/2017/05/16/235548
*/
using UnityEngine;
using System.Collections.Generic;

public class ShootController {

	public Vector3 Shoot( Vector3 i_startPos, Vector3 i_targetPosition, float i_speed )
	{
		if( i_speed < Mathf.Epsilon )
		{
			// その位置に着地させることは不可能のようだ！
			Debug.LogWarning( "!!" );
			return Vector3.zero;
		}
		m_startPos = i_startPos;
		m_targetPosition = i_targetPosition;

		// xz平面の距離を計算。
		Vector2 startPos = new Vector2( m_startPos.x, m_startPos.z );
		Vector2 targetPos = new Vector2( m_targetPosition.x, m_targetPosition.z );
		float distance = Vector2.Distance( targetPos, startPos );

		float time  = distance / i_speed;

		return ShootFixedTime( time );
	}

	private Vector3 ShootFixedTime( float i_time )
	{
		float speedVec = ComputeVectorFromTime( i_time );
		float angle = ComputeAngleFromTime( i_time );

		if( speedVec < Mathf.Epsilon )
		{
			// その位置に着地させることは不可能のようだ！
			Debug.LogWarning( "!!" );
			return Vector3.zero;
		}
		return ConvertVectorToVector3( speedVec, angle );
	}
	private Vector3 ConvertVectorToVector3( float i_v0, float i_angle )
	{
		Vector3 startPos = m_startPos;
		Vector3 targetPos   = m_targetPosition;
		startPos.y  = 0.0f;
		targetPos.y = 0.0f;

		Vector3 dir = ( targetPos - startPos ).normalized;
		Quaternion yawRot = Quaternion.FromToRotation( Vector3.right, dir );
		Vector3 vec = i_v0 * Vector3.right;

		vec = yawRot * Quaternion.AngleAxis( i_angle, Vector3.forward ) * vec;

		return vec;
	}
	private float ComputeVectorFromTime( float i_time )
	{
		Vector2 vec = ComputeVectorXYFromTime( i_time );

		float v_x   = vec.x;
		float v_y   = vec.y;
		float v0Square  = v_x * v_x + v_y * v_y;

		// 負数を平方根計算すると虚数になってしまう。
		// 虚数はfloatでは表現できない。
		// こういう場合はこれ以上の計算は打ち切ろう。
		if( v0Square < Mathf.Epsilon )
		{
			return 0.0f;
		}

		float v0 = Mathf.Sqrt( v0Square );

		return v0;
	}
	private float ComputeAngleFromTime( float i_time )
	{
		Vector2 vec = ComputeVectorXYFromTime( i_time );

		float v_x = vec.x;
		float v_y = vec.y;

		float rad   = Mathf.Atan2( v_y, v_x );
		float angle = rad * Mathf.Rad2Deg;

		return angle;
	}
	private Vector2 ComputeVectorXYFromTime( float i_time )
	{
		// 瞬間移動はちょっと……。
		if( i_time < Mathf.Epsilon )
		{
			return Vector2.zero;
		}

		// xz平面の距離を計算。
		Vector2 startPos = new Vector2( m_startPos.x, m_startPos.z );
		Vector2 targetPos = new Vector2( m_targetPosition.x, m_targetPosition.z );
		float distance = Vector2.Distance( targetPos, startPos );

		float x = distance;
		// な、なぜ重力を反転せねばならないのだ...
		float g     = -Physics.gravity.y;
		float y0    = m_startPos.y;
		float y     = m_targetPosition.y;
		float t     = i_time;

		float v_x   = x / t;
		float v_y   = ( y - y0 ) / t + ( g * t ) / 2;

		return new Vector2( v_x, v_y );
	}
	private Vector3 m_startPos;
	private Vector3 m_targetPosition;
};

