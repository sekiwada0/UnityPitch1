
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GigaTrax.PVAdmin
{
	public enum PlayResult {
		Strike  = 0,
		Ball    = 1,
		Foul    = 2,
		Hit     = 3,
		Homerun = 4
	};

	public class Ctrl {
		[DllImport("PVAdmin")]
		private static extern void PVAdmin_init();

		[DllImport("PVAdmin")]
		private static extern void PVAdmin_term();

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern int PVAdmin_getInt(string name);

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern int PVAdmin_setInt(string name,int value);

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern IntPtr PVAdmin_getString(string name);

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern int PVAdmin_setString(string name,string value);

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern int PVAdmin_exec(string name);

		[DllImport("PVAdmin", CharSet = CharSet.Ansi)]
		private static extern int PVAdmin_startPlayInfo(string player_name);

		[DllImport("PVAdmin")]
		private static extern int PVAdmin_addPlayInfo(int result,float speed,float distance);

		[DllImport("PVAdmin")]
		private static extern int PVAdmin_endPlayInfo();

		public void init(){
			if( m_bInit == false ){
				PVAdmin_init();
				m_bInit = true;
			}
		}
		public void term(){
			if( m_bInit != false ){
				PVAdmin_term();
				m_bInit = false;
			}
		}
		public int exec(string name){ return PVAdmin_exec( name ); }
		public int setInt(string name,int value){ return PVAdmin_setInt( name, value ); }
		public int getInt(string name){ return PVAdmin_getInt( name ); }
		public int setString(string name,string value){ return PVAdmin_setString( name, value ); }
		public string getString(string name){
			IntPtr str1 = PVAdmin_getString( name );
			String str2;
			if( str1 == null )
				str2 = "";
			else
				str2 = Marshal.PtrToStringAnsi( str1 );
			return str2;
		}
		public bool startPlayInfo(string player_name){
			int ret = PVAdmin_startPlayInfo( player_name );
			if( ret == 0 )
				return false;
			return true;
		}
		public bool addPlayInfo(PlayResult result,float speed,float distance){
			int ret = PVAdmin_addPlayInfo( (int)result, speed, distance );
			if( ret == 0 )
				return false;
			return true;
		}
		public bool endPlayInfo(){
			int ret = PVAdmin_endPlayInfo();
			if( ret == 0 )
				return false;
			return true;
		}

		private bool m_bInit = false;
	}
}

