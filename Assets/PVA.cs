
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GigaTrax.PVA
{
	public struct PVAVector3D {
		public float x, y, z;
	};

	public struct PVAVector2D {
		public float x, y;
	};

	public struct PVADetectData {
		public uint time;
		public PVAVector3D pos;
		public PVAVector2D pos2D_0;
		public PVAVector2D pos2D_1;
	};

	public struct PVADetectPath {
		public int num;
		public IntPtr data;
		//public ref PVADetectData data;
	};

	public struct DetectNotify {
		public int flags;
		public int lost;
		public PVADetectPath pitch_path;
		public PVADetectPath hit_path;
		/* 16/12/03 added */
		public float speed;
		public float azimuth;
		public float altitude;
	};

	public enum PVAResult {
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
	public enum PVAFlg {
		Pitch  = 0x01,
		Hit    = 0x02
	};

	public class PVACtrl {
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

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern int PVA_getSysInt(string name);

		[DllImport("PVA", CharSet = CharSet.Ansi)]
		private static extern int PVA_setSysInt(string name,int value);

		[DllImport("PVA")]
		private static extern int PVA_startBall();

		[DllImport("PVA")]
		private static extern int PVA_endBall();

		public void init(){
			if( m_bPVA == false ){
				PVA_init();
				m_bPVA = true;
			}
		}
		public void term(){
			if( m_bPVA != false ){
				PVA_term();
				m_bPVA = false;
			}
		}
		public PVAResult getCamraStatus(){
			return (PVAResult)PVA_getCamraStatus();
		}
		public int getSysInt(string name){
			return PVA_getSysInt( name );
		}
		public PVAResult getDetect(ref DetectNotify detect){
			return (PVAResult)PVA_getDetect( ref detect );
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

