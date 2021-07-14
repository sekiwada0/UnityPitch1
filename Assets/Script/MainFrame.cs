
using UnityEngine;
using GigaTrax.Ports;
using GigaTrax;
//using GigaTrax.PVA;
//using GigaTrax.PVAdmin;
using UnityRawInput;
using System.Collections.Generic;

class MachineParam {
	public MachineParam(string name_){
		name = name_;
		tags = new Dictionary<string,string>();
	}
	public string name;
	public Dictionary<string,string> tags;
};

public class MainFrame : MonoBehaviour {
	void Awake()
	{
		if( instance == null ){
			instance = this;
			DontDestroyOnLoad( this );
		}
		else{
			Destroy( this );
		}
	}

	// Use this for initialization
	void Start(){
		try
		{
			string i_path = Application.dataPath + "/XML/Settings.xml"; /*NG*/
			//GigaTrax.Debug.Log( "path:" + i_path );
			LoadParam( i_path );
		}
		catch( System.Exception i_exception )
		{
			GigaTrax.Debug.Log( "xml load fail" );
		}
		m_strMachine = getSysString("PitchingMachine");
		//m_strLanguage = getSysString("Language");
		//m_strOPMovie = getSysString("OPMovie");

		m_PVACtrl = new GigaTrax.PVA.Ctrl();
		m_PVACtrl.init();

		m_PVAdmin = new GigaTrax.PVAdmin.Ctrl();
		m_PVAdmin.init();
		m_nAdmin = Admin_Init;
		m_nReplyCode = 0;
		//m_bAuthExec = false;
		//m_bAuthResult = false;

		//m_Port = new SerialPort();
		m_W3Ctrl = new W3Ctrl();
		W3Ctrl_Open_();

		bool workInBackground = true;
		RawKeyInput.Start(workInBackground);
		m_bRawKeyInit = true;
	}

	// Update is called once per frame
	void Update(){
		if( m_PVACtrl == null || m_PVAdmin == null )
			return;
		switch( m_nAdmin ){
		case Admin_Init:
			m_PVAdmin.exec("Authentication");
			m_nAdmin = Admin_Exec;
			break;
		case Admin_Exec:{
			int busy = m_PVAdmin.getInt("Busy");
			if( busy == 0 ){
				m_strVendorID = m_PVAdmin.getString("VendorID");
				m_strDeviceID = m_PVAdmin.getString("DeviceID");
//GigaTrax.Debug.Log( "VendorID:" + m_strVendorID);
//GigaTrax.Debug.Log( "DeviceID:" + m_strDeviceID);

				int bAuthentication = m_PVAdmin.getInt("Authentication");
//GigaTrax.Debug.Log( "Authentication:" + bAuthentication);
				if( bAuthentication != 0 ){
					m_nAdmin = Admin_OK;
					//m_bAuthResult = true;
				}
				else{
					m_nAdmin = Admin_Failed;
					m_nReplyCode = m_PVAdmin.getInt("ReplyCode");
					m_strReplyMessage = m_PVAdmin.getString("ReplyMessage");
//GigaTrax.Debug.Log( "ReplyCode:" + m_nReplyCode);
//GigaTrax.Debug.Log( "ReplyMessage:" + m_strReplyMessage);
				}
			}
			break;}
		case Admin_OK:{
			GigaTrax.PVA.APIResult status = m_PVACtrl.getCamraStatus();
			if( m_status != status ){
				m_status = status;

				string text;
				switch( m_status ){
				case GigaTrax.PVA.APIResult.OK:
					text = "OK"; break;
				case GigaTrax.PVA.APIResult.Uninitialize:
					text = "Uninitialize"; break;
				case GigaTrax.PVA.APIResult.NotConnect:
					text = "NotConnect"; break;
				case GigaTrax.PVA.APIResult.AlreadyRegist:
					text = "AlreadyRegist"; break;
				case GigaTrax.PVA.APIResult.RegistNotFound:
					text = "RegistNotFound"; break;
				case GigaTrax.PVA.APIResult.DongleNotFound:
					text = "DongleNotFound"; break;
				case GigaTrax.PVA.APIResult.NoFPS:
					text = "NoFPS"; break;
				case GigaTrax.PVA.APIResult.NoData:
					text = "NoData"; break;
				default:
					text = "Unknown"; break;
				}
				GigaTrax.Debug.Log( "CamraStatus:" + text );
			}
			if( m_status != GigaTrax.PVA.APIResult.OK )
				return;

			break;}
		}
	}
	void OnDestroy(){
		if( m_PVACtrl != null ){
			m_PVACtrl.endBall();
			m_PVACtrl.term();
		}
		if( m_PVAdmin != null ){
			m_PVAdmin.term();
		}
		if( m_bRawKeyInit ){
			RawKeyInput.Stop();
			m_bRawKeyInit = false;
		}
		if( m_W3Ctrl != null ){
			W3Ctrl_Close_();
		}
	}
	void OnGUI()
	{
		Event e = Event.current;
		if( e.isKey ){
			if( e.character == 'c' || e.character == 'C' ){
				m_PVACtrl.config();
			}
		}
	}
	public static GigaTrax.PVA.APIResult PVA_getCamraStatus(){
		if( instance == null )
			return GigaTrax.PVA.APIResult.Uninitialize;
		return instance.m_status;
	}
	public static int PVA_getInt(string name){
		if( instance == null )
			return -1;
		return instance.m_PVACtrl.getSysInt(name);
	}
	public static float PVA_getFloat(string name){
		if( instance == null )
			return -1;
		return instance.m_PVACtrl.getSysFloat(name);
	}
	public static int PVA_setInt(string name,int value){
		if( instance == null )
			return -1;
		return instance.m_PVACtrl.setSysInt(name, value);
	}
	public static int PVA_setFloat(string name,float value){
		if( instance == null )
			return -1;
		return instance.m_PVACtrl.setSysFloat(name, value);
	}
	public static GigaTrax.PVA.APIResult PVA_getDetect(ref GigaTrax.PVA.DetectNotify detect){
		if( instance == null )
			return GigaTrax.PVA.APIResult.Uninitialize;
		return (GigaTrax.PVA.APIResult)instance.m_PVACtrl.getDetect( ref detect );
	}
	public static void PVA_startBall(){
		if( instance == null )
			return;
		instance.m_PVACtrl.startBall();
	}
	public static void PVA_endBall(){
		if( instance == null )
			return;
		instance.m_PVACtrl.endBall();
	}

	// Serial Port Open
	private void W3Ctrl_Open_()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("W3Ctrl_Open");
		}
		m_W3Ctrl.CtrlPort = getMachineString_("CtrlPort");
		m_W3Ctrl.WheelPort = getMachineString_("WheelPort");
		m_W3Ctrl.Wheel1 = 1000;
		m_W3Ctrl.Wheel2 = 1000;
		m_W3Ctrl.Wheel3 = 1000;
		m_W3Ctrl.LR = 0;
		m_W3Ctrl.TB = 0;
		m_W3Ctrl.Open();
	}
/*W3Ctrl*/
	// Serial Port Close
	private void W3Ctrl_Close_()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("W3Ctrl_Close");
		}
		m_W3Ctrl.Close();
	}
	private void W3Ctrl_PowerON_()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("W3Ctrl_PowerON");
		}
		m_W3Ctrl.exec("PowerON");
	}
	private void W3Ctrl_PowerOFF_()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("W3Ctrl_PowerOFF");
		}
		m_W3Ctrl.exec("PowerOFF");
	}
	private void W3Ctrl_PitchON_()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("W3Ctrl_PitchON");
		}
		m_W3Ctrl.exec("PitchON");
	}
	private bool W3Ctrl_getTouchCard_(){ return m_W3Ctrl.TouchCard; }
	private void W3Ctrl_clearTouchCard_(){ m_W3Ctrl.TouchCard = false; }
	/*private void W3Ctrl_Speed_(int v1,int v2,int v3){
		m_W3Ctrl.Wheel1 = v1;
		m_W3Ctrl.Wheel2 = v2;
		m_W3Ctrl.Wheel3 = v3;
	}
	private void W3Ctrl_Dir_(int v1,int v2){
		m_W3Ctrl.LR = v1;
		m_W3Ctrl.TB = v2;
	}*/
	private bool W3Ctrl_setParam_(string level,string dir){
		return m_W3Ctrl.setParam( level, dir );
	}

	private string getSysString_(string name){
		Dictionary<string,string> tags = m_systags;
		if( tags == null || !tags.ContainsKey(name) )
			return "";
		return tags[name];
	}
	public static string getSysString(string name){
//GigaTrax.Debug.Log( "getSysString:" + name );
		if( instance == null )
			return "";
		return instance.getSysString_(name);
	}

	private int getSysInt_(string name){
		Dictionary<string,string> tags = m_systags;
		if( tags == null || !tags.ContainsKey(name) )
			return 0;
		string str = tags[name];
		if( string.IsNullOrEmpty(str) )
			return 0;
		return int.Parse( str );
	}
	public static int getSysInt(string name){
		if( instance == null )
			return 0;
		return instance.getSysInt_(name);
	}
	private int setSysInt_(string name,int value){
		Dictionary<string,string> tags = m_systags;
		if( tags == null )
			return 0;
		string str = value.ToString();
		if( tags.ContainsKey(name) ){
			tags[name] = str;
		}
		else{
			m_systags.Add(name, str);
		}
		return 1;
	}
	public static int setSysInt(string name,int value){
		if( instance == null )
			return 0;
		return instance.setSysInt_(name, value);
	}

	private float getSysFloat_(string name){
		Dictionary<string,string> tags = m_systags;
		if( tags == null || !tags.ContainsKey(name) )
			return 0;
		string str = tags[name];
		if( string.IsNullOrEmpty(str) )
			return 0;
		return float.Parse( str );
	}
	public static float getSysFloat(string name){
		if( instance == null )
			return 0;
		return instance.getSysFloat_(name);
	}

	private int getAdminInt_(string name){
		switch( name ){
		case "AuthDone":
			if( m_nAdmin >= Admin_OK )
				return 1;
			break;
		case "AuthResult":
			if( m_nAdmin == Admin_OK )
				return 1;
			break;
		}
		return 0;
	}
	public static int getAdminInt(string name){
		if( instance == null )
			return 0;
		return instance.getAdminInt_(name);
	}
	private string getAdminString_(string name){
		switch( name ){
		case "VendorID":
			return m_strVendorID;
		case "DeviceID":
			return m_strDeviceID;
		case "ReplyMessage":
			return m_strReplyMessage;
		}
		return "";
	}
	public static string getAdminString(string name){
		if( instance == null )
			return "";
		return instance.getAdminString_(name);
	}

	private MachineParam addMachineParam_(string name){
		MachineParam param = new MachineParam( name );
		m_MachineParams.Add( param );
		return param;
	}
	private MachineParam getMachineParam_(string name){
		int num = m_MachineParams.Count;
		for(int i = 0; i < num; i++){
			MachineParam param = m_MachineParams[i];
			if( param.name == name )
				return param;
		}
		return null;
	}
	private string getMachineString_(string name){
		MachineParam param = getMachineParam_( m_strMachine );
		if( param == null )
			return "";
		Dictionary<string,string> tags = param.tags;
		if( !tags.ContainsKey(name) )
			return "";
		return tags[name];
	}
	private int getMachineInt_(string name){
		string str = getMachineString_(name);
		if( string.IsNullOrEmpty(str) )
			return 0;
		return int.Parse( str );
	}
	private float getMachineFloat_(string name){
		string str = getMachineString_(name);
		if( string.IsNullOrEmpty(str) )
			return 0;
		return float.Parse( str );
	}
	public static string getMachineString(string name){
//GigaTrax.Debug.Log( "getMachineString:" + name );
		if( instance == null )
			return "";
		return instance.getMachineString_(name);
	}
	public static int getMachineInt(string name){
//GigaTrax.Debug.Log( "getMachineInt:" + name );
		if( instance == null )
			return 0;
		return instance.getMachineInt_(name);
	}
	public static float getMachineFloat(string name){
//GigaTrax.Debug.Log( "getMachineFloat:" + name );
		if( instance == null )
			return 0;
		return instance.getMachineFloat_(name);
	}

/*W3Ctrl*/
	public static bool W3Ctrl_Open(){
		if( instance == null )
			return false;
		instance.W3Ctrl_Open_();
		return true;
	}
	public static bool W3Ctrl_Close(){
		if( instance == null )
			return false;
		instance.W3Ctrl_Close_();
		return true;
	}
	public static bool W3Ctrl_PowerON(){
		if( instance == null )
			return false;
		instance.W3Ctrl_PowerON_();
		return true;
	}
	public static bool W3Ctrl_PowerOFF(){
		if( instance == null )
			return false;
		instance.W3Ctrl_PowerOFF_();
		return true;
	}
	public static bool W3Ctrl_PitchON(){
		if( instance == null )
			return false;
		instance.W3Ctrl_PitchON_();
		return true;
	}
	/*public static bool W3Ctrl_Speed(int v1,int v2,int v3){
		if( instance == null )
			return false;
		instance.W3Ctrl_Speed_( v1, v2, v3 );
		return true;
	}
	public static bool W3Ctrl_Dir(int v1,int v2){
		if( instance == null )
			return false;
		instance.W3Ctrl_Dir_( v1, v2 );
		return true;
	}*/
	public static bool W3Ctrl_setParam(string level,string dir){
		if( instance == null )
			return false;
		return instance.W3Ctrl_setParam_( level, dir );
	}
	public static bool W3Ctrl_getTouchCard(){
		if( instance == null )
			return false;
		return instance.W3Ctrl_getTouchCard_();
	}
	public static void W3Ctrl_clearTouchCard(){
		if( instance == null )
			return;
		instance.W3Ctrl_clearTouchCard_();
	}

/*PlayInfo*/
	public static bool PlayInfo_start(string player_name){
		if( instance == null )
			return false;
		return instance.m_PVAdmin.startPlayInfo( player_name );
	}
	public static bool PlayInfo_add(GigaTrax.PVAdmin.PlayResult result,float speed){
		if( instance == null )
			return false;
		return instance.m_PVAdmin.addPlayInfo( result, speed, 0 );
	}
	public static bool PlayInfo_end(){
		if( instance == null )
			return false;
		return instance.m_PVAdmin.endPlayInfo();
	}

	public static bool Key_isCtrl(RawKey code){
		if( RawKeyInput.IsKeyDown( code ) ){
			if( RawKeyInput.IsKeyDown(RawKey.RightControl) || RawKeyInput.IsKeyDown(RawKey.LeftControl) ){
				return true;
			}
		}
		return false;
	}
	public static bool Key_isAlt(RawKey code){
		if( RawKeyInput.IsKeyDown( code ) ){
			if( RawKeyInput.IsKeyDown(RawKey.RightMenu) || RawKeyInput.IsKeyDown(RawKey.LeftMenu) ){
				return true;
			}
		}
		return false;
	}

	private void LoadParam(string i_path){
		//System.Xml.Linq.XDocument xml = System.Xml.Linq.XDocument.Parse( i_xmlText );
		System.Xml.Linq.XDocument xml = System.Xml.Linq.XDocument.Load( i_path );

		System.Xml.Linq.XElement root = xml.Root;
		if( root.Name.LocalName != "params" )
			return;
		string nodeName;
		foreach( var node0 in root.Elements() )
		{
			nodeName = node0.Name.LocalName;
//GigaTrax.Debug.Log( "node[0]:" + nodeName );
			if( nodeName == "system" ){
				m_systags = new Dictionary<string,string>();
				foreach( var node1 in node0.Elements() )
				{
//GigaTrax.Debug.Log( "node[1]:" + node1.Name.LocalName + "=" + node1.Value);
					m_systags.Add(node1.Name.LocalName, node1.Value);
				}
			}
			else if( nodeName == "PitchingMachine" ){
				string param_name = "";
				if( node0.HasAttributes ){
					foreach( var attr in node0.Attributes() )
					{
						//GigaTrax.Debug.Log("attr:" + attr.Name + "=" + attr.Value);
						if( attr.Name == "name" ){
							param_name = attr.Value;
							break;
						}
					}
				}
//GigaTrax.Debug.Log("param_name:" + param_name);
				MachineParam param = addMachineParam_( param_name );
				foreach( var node1 in node0.Elements() )
				{
//GigaTrax.Debug.Log("  " + node1.Name.LocalName + "=" + node1.Value);
					param.tags.Add(node1.Name.LocalName, node1.Value);
				}
			}
		}
	}
	/*private void ShowData( System.Xml.Linq.XElement node )
	{
		string nodeName = node.Name.LocalName;
		string nodeValue = node.Value;
		GigaTrax.Debug.Log("node:" + nodeName + "=" + nodeValue);

		if( node.HasAttributes ){
			foreach( var attr in node.Attributes() )
			{
				GigaTrax.Debug.Log("attr:" + attr.Name + "=" + attr.Value);
			}
		}

		foreach( var child in node.Elements() )
		{
			ShowData( child );
		}
	}*/

	static public MainFrame instance;
	private GigaTrax.PVA.APIResult m_status = GigaTrax.PVA.APIResult.Uninitialize;
	private GigaTrax.PVA.Ctrl m_PVACtrl;
	private GigaTrax.PVAdmin.Ctrl m_PVAdmin;
	private W3Ctrl m_W3Ctrl;
	private bool m_bPortLog = true;
	private bool m_bRawKeyInit = false;
	private bool m_bExec = false;

/*Admin*/
	private const int Admin_Init = 0;
	private const int Admin_Exec = 1;
	private const int Admin_OK   = 2;
	private const int Admin_Failed = 3;
	private int m_nAdmin;
	private int m_nReplyCode;
	//private bool m_bAuthExec;
	//private bool m_bAuthResult;
	private string m_strVendorID;
	private string m_strDeviceID;
	private string m_strReplyMessage;

	//private const string m_strLee1701 = "Lee1701";
	private string m_strMachine;
	//private string m_strLanguage = "ja";
	//private string m_strOPMovie = "CDILogo1.mp4";
	private Dictionary<string,string> m_systags = null;
	private List<MachineParam> m_MachineParams = new List<MachineParam>();
}
