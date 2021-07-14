
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GigaTrax
{
	public class W3Ctrl {
		[DllImport("W3Ctrl")]
		private static extern int W3Ctrl_open();

		[DllImport("W3Ctrl")]
		private static extern void W3Ctrl_close();

		[DllImport("W3Ctrl", CharSet = CharSet.Ansi)]
		private static extern int W3Ctrl_getInt(string name);

		[DllImport("W3Ctrl", CharSet = CharSet.Ansi)]
		private static extern int W3Ctrl_setInt(string name,int value);

		[DllImport("W3Ctrl", CharSet = CharSet.Ansi)]
		private static extern int W3Ctrl_setString(string name,string value);

		[DllImport("W3Ctrl")]
		private static extern int W3Ctrl_exec(string name);

		public string CtrlPort{ set{ W3Ctrl_setString("CtrlPort", value); } }
		public string WheelPort{ set{ W3Ctrl_setString("WheelPort", value); } }
		public int Wheel1{ set{ W3Ctrl_setInt("Wheel1", value); } }
		public int Wheel2{ set{ W3Ctrl_setInt("Wheel2", value); } }
		public int Wheel3{ set{ W3Ctrl_setInt("Wheel3", value); } }
		public int LR{ set{ W3Ctrl_setInt("LR", value); } }
		public int TB{ set{ W3Ctrl_setInt("TB", value); } }
		public bool TouchCard{
			set{ W3Ctrl_setInt("TouchCard", value? 1: 0); }
			get{ return (W3Ctrl_getInt("TouchCard") != 0)? true: false; }
		}

		class ParamSet {
			public ParamSet(string name_){
				name = name_;
				Wheel1 = 0;
				Wheel2 = 0;
				Wheel3 = 0;
				LR = 0;
				TB = 0;
			}
			public string name;
			public int Wheel1, Wheel2, Wheel3;
			public int LR;
			public int TB;
		};

		class WheelParams {
			public WheelParams(string name_){
				name = name_;
			}
			public ParamSet addParamSet(string dir){
				ParamSet param = new ParamSet( dir );
				m_params.Add( param );
				return param;
			}
			public ParamSet getParamSet(string dir){
				int num = m_params.Count;
				for(int i = 0; i < num; i++){
					ParamSet param = m_params[i];
					if( param.name == dir )
						return param;
				}
				return null;
			}
			public string name;
			public List<ParamSet> m_params = new List<ParamSet>();
		};

		public W3Ctrl()
		{
			try
			{
				string i_path = Application.dataPath + "/XML/WheelParams.xml";
				//GigaTrax.Debug.Log( "path:" + i_path );
				LoadParam( i_path );
			}
			catch( System.Exception i_exception )
			{
				GigaTrax.Debug.Log( "xml load fail" );
			}
		}

		// Calls internal Serial Stream's Close() method on the internal Serial Stream.
		public void Close()
		{
			W3Ctrl_close();
		}
		public bool Open()
		{
			return (W3Ctrl_open() == 1)? true: false;
		}
		public int exec(string cmd)
		{
			return W3Ctrl_exec( cmd );
		}
		public bool setParam(string level,string dir)
		{
			//GigaTrax.Debug.Log("level:" + level);
			//GigaTrax.Debug.Log("dir:" + dir);

			int num = m_params.Count;
			for(int i = 0; i < num; i++){
				WheelParams wheel_params = m_params[i];
				if( wheel_params.name == level ){
					ParamSet param_set = wheel_params.getParamSet( dir );
					if( param_set == null )
						break;

					GigaTrax.Debug.Log("wheel:" +
						param_set.Wheel1 + " " +
						param_set.Wheel2 + " " +
						param_set.Wheel3 + " LR:" +
						param_set.LR + " TB:" +
						param_set.TB);

					//GigaTrax.Debug.Log("wheel:" + param_set.Wheel1 + "," + param_set.Wheel2 + "," + param_set.Wheel3);

					this.Wheel1 = param_set.Wheel1;
					this.Wheel2 = param_set.Wheel2;
					this.Wheel3 = param_set.Wheel3;
					this.LR = param_set.LR;
					this.TB = param_set.TB;

					return true;
				}
			}
			return false;
		}

		private void LoadParam(string i_path){
			//System.Xml.Linq.XDocument xml = System.Xml.Linq.XDocument.Parse( i_xmlText );
			System.Xml.Linq.XDocument xml = System.Xml.Linq.XDocument.Load( i_path );

			System.Xml.Linq.XElement root = xml.Root;
			if( root.Name.LocalName != "WheelParams" )
				return;
			string nodeName;
			foreach( var node0 in root.Elements() )
			{
				nodeName = node0.Name.LocalName;

				if( nodeName == "Level" ){
					string level_name = "";
					if( node0.HasAttributes ){
						foreach( var attr in node0.Attributes() )
						{
							if( attr.Name == "name" ){
								level_name = attr.Value;
								break;
							}
						}
					}
					//GigaTrax.Debug.Log("level:" + level_name);
					WheelParams wheel_params = new WheelParams( level_name );
					m_params.Add( wheel_params );

					foreach( var node1 in node0.Elements() )
					{
						nodeName = node1.Name.LocalName;

						if( nodeName == "ParamSet" ){
							ParamSet param_set = null;
							if( node1.HasAttributes ){
								foreach( var attr in node1.Attributes() )
								{
									if( attr.Name == "name" ){
										//GigaTrax.Debug.Log("paramset:" + attr.Value);
										param_set = wheel_params.addParamSet( attr.Value );
									}
									else if( attr.Name == "wheel" ){
										if( param_set != null ){
											string[] array = attr.Value.Split(' ');
											if( array.Length == 3 ){
												param_set.Wheel1 = int.Parse( array[0] );
												param_set.Wheel2 = int.Parse( array[1] );
												param_set.Wheel3 = int.Parse( array[2] );
												//GigaTrax.Debug.Log("wheel:" + param_set.Wheel1 +
												//	" " + param_set.Wheel2 +
												//	" " + param_set.Wheel3);
											}
										}
									}
									else if( attr.Name == "LR" ){
										if( param_set != null ){
											param_set.LR = int.Parse( attr.Value );
											//GigaTrax.Debug.Log("LR:" + param_set.LR);
										}
									}
									else if( attr.Name == "TB" ){
										if( param_set != null ){
											param_set.TB = int.Parse( attr.Value );
											//GigaTrax.Debug.Log("TB:" + param_set.TB);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private List<WheelParams> m_params = new List<WheelParams>();
	}
}
