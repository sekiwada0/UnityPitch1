
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GigaTrax.Ports
{
	public enum Parity {
		None,
		Odd,
		Even,
		Mark,
		Space
	}
	public enum StopBits {
		One,
		OnePointFive,
		Two
	}

	public class SerialPort {
		[DllImport("Ports")]
		private static extern int Ports_open();

		[DllImport("Ports")]
		private static extern void Ports_close();

		[DllImport("Ports", CharSet = CharSet.Ansi)]
		private static extern int Ports_getInt(string name);

		[DllImport("Ports", CharSet = CharSet.Ansi)]
		private static extern int Ports_setInt(string name,int value);

		[DllImport("Ports", CharSet = CharSet.Ansi)]
		private static extern int Ports_setString(string name,string value);

		[DllImport("Ports")]
		private static extern int Ports_readKeyword(string name);

		[DllImport("Ports")]
		private static extern int Ports_write(byte[] src,int size);

		public bool IsOpen
		{
			get {
				return (Ports_getInt("Open") != 0)? true: false;
			}
		}

		public string PortName{ set{ Ports_setString("PortName", value); } }
		public int BaudRate{ set{ Ports_setInt("BaudRate", value); } }
		public int DataBits{ set{ Ports_setInt("DataBits", value); } }
		public StopBits StopBits{ set{ Ports_setInt("StopBits", (int)value); } }
		public Parity Parity{ set{ Ports_setInt("Parity", (int)value); } }
		public int ReadBufferSize{ set{ Ports_setInt("ReadBufferSize", value); } }
		public int ReadTimeout{ set{ Ports_setInt("ReadTimeout", value); } }
		public int WriteBufferSize{ set{ Ports_setInt("WriteBufferSize", value); } }
		public int WriteTimeout{ set{ Ports_setInt("WriteTimeout", value); } }
		public bool DtrEnable{ set{ Ports_setInt("DtrEnable", value? 1: 0); } }
		public bool RtsEnable{ set{ Ports_setInt("RtsEnable", value? 1: 0); } }
		public int Interval{ set{ Ports_setInt("Interval", value); } }

		public SerialPort()
		{
		}

		// Calls internal Serial Stream's Close() method on the internal Serial Stream.
		public void Close()
		{
			if( IsOpen ){
				Ports_close();
			}
		}

		public bool Open()
		{
			if( IsOpen )
				return false;

			int ret = Ports_open();
			if( ret == 0 )
				return false;
			return true;
		}

		public bool ReadKeyword(string text)
		{
			int ret = Ports_readKeyword( text );
			return ret != 0;
		}

		// Writes string to output, no matter string's length.
		public int WriteString(string text)
		{
			byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
			return Ports_write( data, data.Length );
		}
	}
}
