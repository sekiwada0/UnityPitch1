
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GigaTrax.PVA
{
	public struct Vector3D {
		public float x, y, z;
	};

	public struct Vector2D {
		public float x, y;
	};

	public struct DetectData {
		public uint time;
		public Vector3D pos;
		public Vector2D pos2D_0;
		public Vector2D pos2D_1;
	};

	public struct DetectPath {
		public int num;
		public IntPtr data;
		//public ref DetectData data;
	};

	public struct DetectNotify {
		public int flags;
		public int lost;
		public DetectPath pitch_path;
		public DetectPath hit_path;
		/* 16/12/03 added */
		public float speed;
		public float azimuth;
		public float altitude;
		/* 19/11/16 added */
		public Vector2D strike_pos;
	};

	public enum APIResult {
		OK             = 0,
		Uninitialize   = -1,
		NotConnect     = -2,
		AlreadyRegist  = -3,
		RegistNotFound = -4,
		DongleNotFound = -5,
		NoFPS          = -6,
		NoData         = -7,
		Unknown        = -100
	};
	public enum DetectFlg {
		Pitch  = 0x01,
		Hit    = 0x02
	};

	public class Ctrl {
		[DllImport("PVA")]
		private static extern void PVA_init();

		[DllImport("PVA")]
		private static extern void PVA_term();

		[DllImport("PVA")]
		private static extern int PVA_getCamraStatus();

		[DllImport("PVA")]
		private static extern int PVA_config(IntPtr hWnd);

		[DllImport("PVA")]
		private static extern int PVA_startDetect(IntPtr hWnd, uint msgid);

		[DllImport("PVA")]
		private static extern int PVA_endDetect(IntPtr hWnd);

		[DllImport("PVA")]
		private static extern int PVA_getDetect(ref DetectNotify detect);

		[DllImport("PVA")]
		private static extern int PVA_startBall();

		[DllImport("PVA")]
		private static extern int PVA_endBall();

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern int PVA_getSysInt(string name);

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern int PVA_setSysInt(string name,int value);

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern float PVA_getSysFloat(string name);

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern int PVA_setSysFloat(string name,float value);

		public void init(){
			if( m_bPVA == false ){
				//GigaTrax.Debug.Log( "PVA_init" );
				PVA_init();
				m_bPVA = true;
			}
		}
		public void term(){
			if( m_bPVA != false ){
				//GigaTrax.Debug.Log( "PVA_term" );
				PVA_term();
				m_bPVA = false;
			}
		}
		public APIResult getCamraStatus(){
			int ret = PVA_getCamraStatus();
			//GigaTrax.Debug.Log( "PVA_getCamraStatus=" + ret );
			return (APIResult)ret;
		}
		public int getSysInt(string name){
			int ret = PVA_getSysInt( name );
			//GigaTrax.Debug.Log( "PVA_getSysInt(" + name + ")=" + ret );
			return ret;
		}
		public float getSysFloat(string name){ return PVA_getSysFloat( name ); }
		public int setSysInt(string name,int value){ return PVA_setSysInt( name, value ); }
		public int setSysFloat(string name,float value){ return PVA_setSysFloat( name, value ); }
		public APIResult getDetect(ref DetectNotify detect){
			return (APIResult)PVA_getDetect( ref detect );
		}
		public void startBall(){
			if( m_bStart == false )
			{
				PVA_startBall();
				m_bStart = true;
			}
		}
		public void endBall()
		{
			if( m_bStart != false )
			{
				PVA_endBall();
				m_bStart = false;
			}
		}
		public void config(){
			PVA_config( IntPtr.Zero );
		}
		private bool m_bPVA = false;
		private bool m_bStart = false;
	}
}

