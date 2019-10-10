using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GigaTrax.Ports;
using GigaTrax.PVA;
using GigaTrax;
using TMPro;
using UnityRawInput;

public class HitRecord {
	public GameObject ball;
	public int nNo;
	public float speed;
	public float distance;
	public float rotX;
	public float rotY;
};

public class HitBall {
	public HitBall(GameObject obj_){
		obj = obj_;

		posArray = new List<Vector3>();
		posArray.Add( obj.transform.localPosition );
	}
	public GameObject obj;
	public List<Vector3> posArray;
	public float dTime = 0;
	public bool ground = false;
};

public class HitInfo {
	public GameObject objInfo;
	public GameObject objDistText;
	public GameObject ball;
	public bool ground = false;
	public float distance = 0;
	public float dTime = 0;
};

public class Pitch_Main1 : MonoBehaviour {

	// Use this for initialization
	void Start(){
		m_PVACtrl = new PVACtrl();
		m_PVACtrl.init();

		m_Port = new SerialPort();

		//if( Display.displays.Length > 1 )
		//	Display.displays[1].Activate();
/*
		// Display.displays[0] は主要デフォルトディスプレイで、常に ON。
		// 追加ディスプレイが可能かを確認し、それぞれをアクティベートします。
		if( Display.displays.Length > 1 )
			Display.displays[1].Activate();
		Display.displays[0].Activate();
		//if( Display.displays.Length > 2 )
		//	Display.displays[2].Activate();
*/
		m_objMainCanvas = GameObject.FindWithTag("MainCanvas");
		m_objInfo1 = m_objMainCanvas.transform.Find("Info1").gameObject;
		m_objInfo2 = m_objMainCanvas.transform.Find("Info2").gameObject;

		GameObject objBatter = GameObject.FindWithTag("Batter");
		m_objBatter = objBatter.GetComponent<BatterAnim>();

		GameObject objPitcher = GameObject.FindWithTag("Pitcher");
		m_objPitcher = objPitcher.GetComponent<PitcherAnim>();

		m_objGage = Instantiate(prefabGage);
		m_objGage.transform.SetParent(m_objMainCanvas.transform, false);
		m_objGage.SetActive(false);

		m_objReady = Instantiate(prefabReady);
		m_objReady.transform.SetParent(m_objMainCanvas.transform, false);
		m_objReady.SetActive(false);

		initHitRecord();

		m_objCamera = GetComponent<Camera>();
		m_smoothFollow = GetComponent<SmoothFollow>();

		GameObject objTopView = m_objMainCanvas.transform.Find("TopView").gameObject;
		m_topView = objTopView.GetComponent<TrailView>();
		//m_topView.drawPoint = true;
		//m_topView.drawGrid();

		GameObject objSideView = m_objMainCanvas.transform.Find("SideView").gameObject;
		m_sideView = objSideView.GetComponent<TrailView>();

		bool workInBackground = true;
		RawKeyInput.Start(workInBackground);

		setGamePhase( GamePhase_Busy );
		setSensorPhase( SensorPhase_Busy );
		//m_objBatter.setBatterSide( BatterSide_None );
	}
	void OnDestroy(){
		m_PVACtrl.endBall();
		m_PVACtrl.term();
		RawKeyInput.Stop();

		if( m_bPortInit ){
			Port_DataSend("T"); /*Motor OFF*/
			Port_Close();
			m_bPortInit = false;
		}
	}

	// Update is called once per frame
	void Update(){
		PVAResult status = m_PVACtrl.getCamraStatus();
		if( m_status != status ){
			m_status = status;

			if( m_bPhaseLog ){
				string text;
				switch( m_status ){
				case PVAResult.OK:
					text = "OK"; break;
				case PVAResult.Uninitialize:
					text = "Uninitialize"; break;
				case PVAResult.NotConnect:
					text = "NotConnect"; break;
				case PVAResult.AlreadyRegist:
					text = "AlreadyRegist"; break;
				case PVAResult.RegistNotFound:
					text = "RegistNotFound"; break;
				case PVAResult.DongleNotFound:
					text = "DongleNotFound"; break;
				case PVAResult.NoFPS:
					text = "NoFPS"; break;
				case PVAResult.NoData:
					text = "NoData"; break;
				default:
					text = "Unknown"; break;
				}
				GigaTrax.Debug.Log( "CamraStatus:" + text );
			}
		}
		if( m_status != PVAResult.OK )
			return;

		if( !m_bPortInit ){
			Port_Open();
			Port_DataSend("Y"); /*Motor ON*/
			Port_DataSend("SU23"); /*Upper Wheel*/
			Port_DataSend("SD23"); /*Lower Wheel*/
			m_bPortInit = true;
		}
		switch( m_nSensorPhase ){
		case SensorPhase_Busy:{
			int busy;
			busy = m_PVACtrl.getSysInt("Busy");
			if( busy == 0 ){
				setSensorPhase( SensorPhase_Ready );
			}
			break;}
		case SensorPhase_Start:{
			PVAResult ret = m_PVACtrl.getDetect( ref m_detect );
			if( ret == PVAResult.OK ){
				m_PVACtrl.endBall();
				setSensorPhase(SensorPhase_Busy);

				IntPtr pData;
				if( (m_detect.flags & (int)PVAFlg.Hit) != 0 ){
					pData = m_detect.hit_path.data;

					PVADetectData first = (PVADetectData)Marshal.PtrToStructure(pData, typeof(PVADetectData));
					pData = new IntPtr(pData.ToInt64() + Marshal.SizeOf(typeof(PVADetectData)));
					PVADetectData last = (PVADetectData)Marshal.PtrToStructure(pData, typeof(PVADetectData));

					startHit( first, last );
				}
			}
			else{
				if( Input.GetMouseButtonDown(0) ){
					m_PVACtrl.endBall();
					setSensorPhase(SensorPhase_Busy);

					//GigaTrax.Debug.Log("mousePosition: " + Input.mousePosition);
					//resetView();
					startHit2();
				}
			}
			break;}
		}

		switch( m_nGamePhase ){
		case GamePhase_Busy:
			m_objPitcher.setAnim( PitcherAnim.Pitcher_Idle );
			if( m_nSensorPhase == SensorPhase_Ready ){
				setGamePhase( GamePhase_Ready );
				m_objReady.SetActive(true);
			}
			break;
		case GamePhase_Ready:
			int nBatterSide = m_PVACtrl.getSysInt("BatterSide");
			//int nBatterSide = 1;
			m_objBatter.setBatterSide( nBatterSide );

			if( RawKeyInput.IsKeyDown(RawKey.M) ){
				if( RawKeyInput.IsKeyDown(RawKey.RightControl) || RawKeyInput.IsKeyDown(RawKey.LeftControl) ){
					setGamePhase( GamePhase_CountDonw );
					m_objReady.SetActive(false);
					m_objGage.SetActive(true);
					m_bPush = false;
					m_bSensorStart = false;
					m_bPitchMotion = false;
				}
			}
			break;
		case GamePhase_CountDonw:{
			m_dTime -= Time.deltaTime;

			if( !m_bPitchMotion ){
				if( m_dTime < PitchMotionTime ){
					m_objPitcher.setAnim( PitcherAnim.Pitcher_Pitch );
					m_bPitchMotion = true;
				}
			}
			if( !m_bSensorStart ){
				if( m_dTime < SensorStartTime ){
					m_PVACtrl.startBall();
					setSensorPhase( SensorPhase_Start );
					m_bSensorStart = true;
				}
			}
			if( !m_bPush ){
				if( m_dTime < PushTime ){
					Port_DataSend("G"); /*Push*/
					m_bPush = true;
				}
			}

			float ratio = m_dTime / CountDonwDuration;
			UnityEngine.UI.Image image = m_objGage.GetComponent<UnityEngine.UI.Image>();
			if( ratio < 0 )
				ratio = 0;
			image.fillAmount = ratio;
			//prefabSpeed.text = "ratio: " + ratio.ToString("f2");

			if( ratio == 0 ){
				//m_PVACtrl.startBall();
				//setSensorPhase( SensorPhase_Start );
				setGamePhase( GamePhase_Start );
				m_objGage.SetActive(false);
			}

			break;}
		case GamePhase_Start:
			m_dTime -= Time.deltaTime;

			if( m_dTime <= 0 ){
				m_PVACtrl.endBall();
				setSensorPhase( SensorPhase_Busy );
				setGamePhase( GamePhase_Busy );
			}
			break;

		}

		int num_hit = m_HitArray.Count;
		if( num_hit != 0 ){
			m_topView.clearNewTrail();
			m_sideView.clearNewTrail();

			int i = 0;
			while( i < num_hit ){
				bool remove = false;
				HitBall hit = m_HitArray[i];
				if( hit.ground ){
					hit.dTime -= Time.deltaTime;
					if( hit.dTime <= 0 ){
						Destroy( hit.obj, 1.0f );
						remove = true;

						if( m_lastHit == hit ){
							if( m_smoothFollow )
								m_smoothFollow.setTarget(null);
							m_lastHit = null;

							//resetView();
							//setGamePhase( GamePhase_Busy );
						}
					}
				}
				else{
					Vector3 pos = hit.obj.transform.localPosition;
					hit.posArray.Add( pos );
				}
				if( remove ){
					m_topView.addOldTrail( hit.posArray );
					m_sideView.addOldTrail( hit.posArray );

					m_HitArray.RemoveAt(i);
					num_hit--;
				}
				else{
					m_topView.addNewTrail( hit.posArray );
					m_sideView.addNewTrail( hit.posArray );

					i++;
				}
			}
			if( num_hit == 0 ){
				resetView();
				setGamePhase( GamePhase_Busy );
				m_objBatter.setAnim( BatterAnim.BatterAnim_Idle );
			}
		}

		int num_info = m_HitInfoArray.Count;
		for(int i = 0; i < num_info; i++){
			HitInfo info = m_HitInfoArray[i];
			updateHitInfo( info );

			info.dTime += Time.deltaTime;
			GameObject objInfo = info.objInfo;

			float ratio = info.dTime / MainSpeedLife;
			float value = 1.0f + (MainSpeedScale - 1.0f) * ratio;
			objInfo.transform.localScale = new Vector3(value, value, value);

			Vector3 pos = objInfo.transform.localPosition;
			pos.y += 0.1f;
			objInfo.transform.localPosition = pos;
		}

		if( m_smoothFollow ){
			if( m_smoothFollow.getTarget() ){
				m_smoothFollow.update();
			}
		}
	}
	/*void LateUpdate()
	{
		if( m_smoothFollow.getTarget() ){
			m_smoothFollow.update();
		}
	}*/
	void startHit(PVADetectData first,PVADetectData last)
	{
		if( m_bPhaseLog )
			GigaTrax.Debug.Log( "startHit" );
		m_pos0 = first.pos;
		m_pos1 = last.pos;

		uint send_time = last.time - first.time;
		float t = (float)1000 / (float)send_time;

		Vector3 dir = new Vector3();
		dir.x = m_pos1.x - m_pos0.x;
		dir.y = m_pos1.y - m_pos0.y;
		dir.z = m_pos1.z - m_pos0.z;
		float speed = dir.magnitude * t;
		dir.Normalize();
		dir.z = -dir.z;
		startHit_( new Vector3(m_pos0.x, m_pos0.y, -m_pos0.z), dir, speed );
	}
	void startHit2()
	{
		if( m_bPhaseLog )
			GigaTrax.Debug.Log( "startHit2" );
		//int ispeed = 120;
		int ispeed = UnityEngine.Random.Range( 8, 13 ) * 10;
		float speed = (float)ispeed * KPH2MPS;

		Ray ray = m_objCamera.ScreenPointToRay( Input.mousePosition );
		Vector3 dir = ray.direction;
		Vector3 pos = ray.origin;
		startHit_( pos, dir, speed );
	}
	void startHit_(Vector3 pos,Vector3 dir,float speed)
	{
		HitRecord record = addHitRecord();
		record.speed = speed * MPS2KPH;

		calcBallAngleInfo( dir, record );
		setBallSpeedInfo( record.speed );
		setBallAngleInfo( record );
		//setLRAngleInfo( dir );
		//clearLRDistInfo();
		clearFlyDistInfo(); // clear 飛距離 on new ball
		updateHitRecord();

		GameObject ball = Instantiate(prefabBallHit);
		BallHit ball_hit = ball.GetComponent<BallHit>();
		//ball_hit.setRecord( record );
		record.ball = ball;

		ball.transform.localPosition = pos;

		Rigidbody rb = ball.GetComponent<Rigidbody>();
		rb.velocity = dir * speed;

		addHitInfo( ball, record.speed );

		if( m_smoothFollow ){
			m_smoothFollow.setTarget( ball );
		}
		m_lastHit = addHitBall( ball );

		setGamePhase( GamePhase_Hit );
		m_objBatter.setAnim( BatterAnim.BatterAnim_Hit );
	}
	void setSensorPhase(int phase)
	{
		m_nSensorPhase = phase;

		if( m_bPhaseLog ){
			string text;
			switch( phase ){
			case SensorPhase_Busy: text = "Busy"; break;
			case SensorPhase_Ready: text = "Ready"; break;
			case SensorPhase_Start: text = "Start"; break;
			default: text = "???"; break;
			}
			GigaTrax.Debug.Log( "Sensor:" + text );
		}
	}
	void setGamePhase(int phase)
	{
		m_nGamePhase = phase;

		switch( phase ){
		case GamePhase_CountDonw:
			m_dTime = CountDonwDuration;
			break;
		case GamePhase_Start:
			m_dTime = StartDuration;
			break;
		default:
			m_dTime = 0;
			break;
		}
		if( m_bPhaseLog ){
			string text;
			switch( phase ){
			case GamePhase_Busy: text = "Busy"; break;
			case GamePhase_Ready: text = "Ready"; break;
			case GamePhase_CountDonw: text = "CountDonw"; break;
			case GamePhase_Start: text = "Start"; break;
			case GamePhase_Hit: text = "Hit"; break;
			default: text = "???"; break;
			}
			GigaTrax.Debug.Log( "Game:" + text );
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
	public void HitGround(GameObject ball){
		Vector3 pos = ball.transform.localPosition;

		Vector3 org = new Vector3(0, 0, 0);
		float distance = Vector3.Distance( org, pos );

		BallHit ball_hit = ball.GetComponent<BallHit>();

		//HitRecord record = ball_hit.getRecord();
		int num = m_RecordArray.Count;
		for(int i = 0; i < num; i++){
			HitRecord record = m_RecordArray[i];
			if( ball == record.ball ){
				record.distance = distance;
				record.ball = null;

				int index = (m_RecordArray.Count-1) - i;
				if( index < NumHitRecord )
					setHitDistance_( distance, m_objHitRecord[index] );
				break;
			}
		}
		setFlyDistInfo( distance );
		//setLRDistInfo( pos.x );

		//if( m_nGamePhase != GamePhase_HitGround && ball == m_lastHit.obj ){
		//	setGamePhase( GamePhase_HitGround );
		//}
		//GigaTrax.Debug.Log( "Distance: " + distance.ToString("f2") + " m" );
		int num_hit = m_HitArray.Count;
		for(int i = 0; i < num_hit; i++){
			HitBall hit = m_HitArray[i];
			if( hit.obj == ball ){
				hit.ground = true;
				hit.dTime = HitGroundDuration;
				break;
			}
		}

		int num_info = m_HitInfoArray.Count;
		for(int i = 0; i < num_info; i++){
			HitInfo info = m_HitInfoArray[i];
			if( info.ball == ball ){
				info.ball = null;
				info.ground = true;
				info.distance = distance;
				updateHitInfo( info );

				Destroy( info.objInfo, HitInfoDuration );

				m_HitInfoArray.RemoveAt(i);
				break;
			}
		}

		GameObject ballEffect = Instantiate(prefabBallEffect);
		ballEffect.transform.localPosition = new Vector3(pos.x, 0.05f, pos.z);
		Destroy( ballEffect, 1.0f );
	}

	void initHitRecord(){
		m_objHitRecord = new GameObject[NumHitRecord];

		float y = HitRecordPosY;
		for(int i = 0; i < NumHitRecord; i++){
			GameObject obj = Instantiate(prefabHitRecord);
			obj.transform.localPosition = new Vector3(HitRecordPosX, y, 0);
			obj.transform.SetParent(m_objInfo2.transform, false);

			m_objHitRecord[i] = obj;
			y -= HitRecordHeight;
		}
	}
	void updateHitRecord(){
		int num = m_RecordArray.Count;
		int end = num-1;
		if( num > NumHitRecord )
			num = NumHitRecord;
		for(int i = 0; i < num; i++){
			setHitRecord_(m_RecordArray[end-i], m_objHitRecord[i]);
		}
	}
	HitRecord addHitRecord(){
		HitRecord record = new HitRecord();

		int size = m_RecordArray.Count;
		record.ball = null;
		record.nNo = size + 1;
		record.speed = -1;
		record.distance = -1;

		m_RecordArray.Add( record );
		return record;
	}
	HitBall addHitBall(GameObject obj){
		HitBall hit = new HitBall(obj);
		m_HitArray.Add( hit );
		return hit;
	}
	void setNewHitRecord_(HitRecord record,GameObject obj){
		GameObject objNo = obj.transform.Find("no").gameObject;
		Text txNo = objNo.GetComponent<Text>();
		txNo.text = record.nNo.ToString();

		GameObject objSpeed = obj.transform.Find("speed").gameObject;
		Text txSpeed = objSpeed.GetComponent<Text>();
		GameObject objSpeedUnit = obj.transform.Find("speedUnit").gameObject;
		if( record.speed >= 0 ){
			txSpeed.text = ((int)record.speed).ToString();
			objSpeedUnit.SetActive(true);
		}
		else{
			txSpeed.text = "";
			objSpeedUnit.SetActive(false);
		}

		GameObject objLaunchAngle = obj.transform.Find("launchAngle").gameObject;
		Text txLaunchAngle = objLaunchAngle.GetComponent<Text>();
		txLaunchAngle.text = ((int)record.rotX).ToString();

		GameObject objAngleUnit1 = obj.transform.Find("angleUnit_1").gameObject;
		objAngleUnit1.SetActive(true);

		GameObject objSideAngle = obj.transform.Find("sideAngle").gameObject;
		Text txSideAngle = objSideAngle.GetComponent<Text>();
		txSideAngle.text = ((int)record.rotY).ToString();

		GameObject objAngleUnit2 = obj.transform.Find("angleUnit_2").gameObject;
		objAngleUnit2.SetActive(true);

		GameObject objR = obj.transform.Find("R").gameObject;
		GameObject objL = obj.transform.Find("L").gameObject;
		objL.SetActive(false);
		objR.SetActive(false);
		if( record.rotY > 0 ){
			objR.SetActive(true);
		}
		else if( record.rotY < 0 ){
			objL.SetActive(true);
		}

		setHitDistance_(record.distance, obj);
	}
	void setHitRecord_(HitRecord record,GameObject obj){
		GameObject objNo = obj.transform.Find("no").gameObject;
		Text txNo = objNo.GetComponent<Text>();
		txNo.text = record.nNo.ToString();

		GameObject objSpeed = obj.transform.Find("speed").gameObject;
		Text txSpeed = objSpeed.GetComponent<Text>();
		if( record.speed >= 0 ){
			//txSpeed.text = record.speed.ToString("f2") + " Km/h";
			txSpeed.text = (int)record.speed + " km/h";
		}
		else{
			txSpeed.text = "";
		}
		setHitDistance_(record.distance, obj);
	}
	void setHitDistance_(float distance,GameObject obj){
		GameObject objDist = obj.transform.Find("distance").gameObject;
		Text txDist = objDist.GetComponent<Text>();
		if( distance >= 0 ){
			txDist.text= distance.ToString("f2") + " m";
		}
		else{
			txDist.text = "";
		}
	}
	void addHitInfo(GameObject ball,float speed){
		int ispeed = (int)speed;

		GameObject objInfo;
		objInfo = Instantiate(prefabHitInfo);
		objInfo.transform.SetParent(m_objMainCanvas.transform, false);

		GameObject objSpeed;
		if( ispeed >= 120 )
			objSpeed = Instantiate(prefabSpeedGold);
		else if( ispeed >= 100 )
			objSpeed = Instantiate(prefabSpeedSilver);
		else
			objSpeed = Instantiate(prefabSpeedNormal);
		objSpeed.transform.SetParent(objInfo.transform, false);

		GameObject objText = objSpeed.transform.Find("text").gameObject;
		TextMeshProUGUI tx1 = objText.GetComponent<TextMeshProUGUI>();
		tx1.text = ispeed.ToString();

		int idistance = 0;

		GameObject objDistance;
		objDistance = Instantiate(prefabDistanceNormal);
		objDistance.transform.SetParent(objInfo.transform, false);

		GameObject objDistText = objDistance.transform.Find("text").gameObject;
		TextMeshProUGUI tx2 = objDistText.GetComponent<TextMeshProUGUI>();
		tx2.text = idistance.ToString();

		//CanvasRenderer render = objSpeed.GetComponent<CanvasRenderer>();
		//render.SetAlpha(0.5f);

		HitInfo info = new HitInfo();
		info.objInfo = objInfo;
		info.objDistText = objDistText;
		info.ball = ball;
		m_HitInfoArray.Add( info );
	}
	void updateHitInfo(HitInfo info){
		TextMeshProUGUI tx = info.objDistText.GetComponent<TextMeshProUGUI>();
		if( !info.ground ){
			GameObject ball = info.ball;
			Vector3 pos = ball.transform.localPosition;
			pos.y = 0;

			Vector3 org = new Vector3(0, 0, 0);
			float distance = Vector3.Distance( org, pos );
			int idistance = (int)distance;

			tx.text = idistance.ToString();
		}
		else{
			tx.text= info.distance.ToString("f1");
		}
	}
	void setFlyDistInfo(float distance){ setDistanceInfo1("FlyDistInfo", distance); }
	void setLRDistInfo(float distance){
		string prefix = null; // added null and else if to avoid R0.0m
		if (distance < 0)
		{
			distance = -distance;
			prefix = "L";
		}
		else if (distance > 0)
		{
			prefix = "R";
		}
		setDistanceInfo2("LRDistValue", prefix, distance);
	}
	//void clearLRDistInfo(){ clearInfo("LRDistValue"); }
	void clearFlyDistInfo(){ clearInfo("FlyDistInfo"); } // clear 飛距離 on new ball
	void setBallSpeedInfo(float speed){ setSpeedInfo("BallSpeedValue", speed); }
	void calcBallAngleInfo(Vector3 dir,HitRecord record){
		float rotY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
		Vector3 vec = Quaternion.Euler( 0, -rotY, 0 ) * dir;

		float rotX = Mathf.Atan2(vec.y, dir.z) * Mathf.Rad2Deg;

		record.rotX = rotX;
		record.rotY = rotY;
	}
	void setBallAngleInfo(HitRecord record){
		setAngleInfo("LaunchAngleValue", null, record.rotX);

		string prefix = null;
		float rotY = record.rotY;
		if (rotY < 0)
		{
			rotY = -rotY;
			prefix = "L";
		}
		else if (rotY > 0)
		{
			prefix = "R";
		}
		setAngleInfo("LRAngleValue", prefix, rotY);
	}
	void clearInfo(string tag){
		GameObject objInfo = m_objInfo1.transform.Find(tag).gameObject;
		GameObject objText = objInfo.transform.Find("text").gameObject;
		Text tx = objText.GetComponent<Text>();
		tx.text = "";
	}
	void setDistanceInfo1(string tag,float distance){
		GameObject objInfo = m_objInfo1.transform.Find(tag).gameObject;
		GameObject objText = objInfo.transform.Find("text").gameObject;
		Text tx = objText.GetComponent<Text>();
		tx.text = distance.ToString("f2") + "m";
	}
	void setDistanceInfo2(string tag,string prefix,float distance){
		GameObject objInfo = m_objInfo1.transform.Find(tag).gameObject;
		GameObject objText = objInfo.transform.Find("text").gameObject;
		Text tx = objText.GetComponent<Text>();
		tx.text = prefix + distance.ToString("f1") + "m";
	}
	void setSpeedInfo(string tag,float speed){
		GameObject objInfo = m_objInfo1.transform.Find(tag).gameObject;
		GameObject objText = objInfo.transform.Find("text").gameObject;
		Text tx = objText.GetComponent<Text>();
		tx.text = (int)speed + "km/h";
	}
	void setAngleInfo(string tag,string prefix,float angle){
		GameObject objInfo = m_objInfo1.transform.Find(tag).gameObject;
		GameObject objText = objInfo.transform.Find("text").gameObject;
		Text tx = objText.GetComponent<Text>();
		tx.text = prefix + angle.ToString("f1") + "°";
	}
	GameObject addSysInfo(string name){
		name += "Info";
		GameObject obj = m_objMainCanvas.transform.Find(name).gameObject;
		GameObject objValue = obj.transform.Find("value").gameObject;
		return objValue;
	}
	void setSysInfo(GameObject obj,string value){
		obj.GetComponent<Text>().text = value;
	}
	void resetView(){
		//transform.localPosition = new Vector3(0, 1.35f, 0);
		transform.position = new Vector3(0, 1.35f, -3.5f);
		transform.eulerAngles = new Vector3(0, 0, 0);
		//m_objCanvas2.SetActive( true );
	}

	// Serial Port Open
	private void Port_Open()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("Port_Open");
		}
		if( m_bEnablePort ){
			m_Port.PortName = "COM1";
			//m_Port.PortName = "COM3";
			m_Port.BaudRate = 19200;
			//m_Port.BaudRate = 115200;
			m_Port.DataBits = 8;
			m_Port.StopBits = StopBits.One;
			m_Port.Parity = Parity.None;
			//m_Port.Handshake = Handshake.None;
			m_Port.ReadBufferSize = 4096;
			m_Port.ReadTimeout = -1;
			m_Port.WriteBufferSize = 4096;
			m_Port.WriteTimeout = -1;
			m_Port.DtrEnable = true;
			m_Port.RtsEnable = true;
			m_Port.Open();
		}
	}
	// Serial Port Close
	private void Port_Close()
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("Port_Close");
		}
		if( m_bEnablePort ){
			if( m_Port.IsOpen ){
				m_Port.Close();
			}
		}
	}
	public void Port_DataSend(string snd)
	{
		if( m_bPortLog ){
			GigaTrax.Debug.Log("Port_DataSend:" + snd);
		}
		if( m_bEnablePort ){
			if( m_Port.IsOpen ){
				m_Port.WriteString( snd + "\r\n" );
			}
		}
	}

	public GameObject prefabBallHit;
	public GameObject prefabBallEffect;
	public GameObject prefabGage;
	public GameObject prefabHitRecord;
	public GameObject prefabHitInfo;
	public GameObject prefabSpeedNormal;
	public GameObject prefabSpeedSilver;
	public GameObject prefabSpeedGold;
	public GameObject prefabDistanceNormal;
	public GameObject prefabDistanceSilver;
	public GameObject prefabDistanceGold;
	public GameObject prefabReady;

	private GameObject[] m_objHitRecord;

	private const int GamePhase_Busy        = 1;
	private const int GamePhase_Ready       = 2;
	private const int GamePhase_CountDonw   = 3;
	private const int GamePhase_Start       = 4;
	private const int GamePhase_Hit         = 5;

	private const int SensorPhase_Busy      = 0;
	private const int SensorPhase_Ready     = 1;
	private const int SensorPhase_Start     = 2;

	private PVACtrl m_PVACtrl;
	private SerialPort m_Port;

	private const int NumHitRecord = 10;

	private const float KPH2MPS = (1000.0f / 3600.0f);
	private const float MPS2KPH = (3600.0f / 1000.0f);

	private const float RAD2DEG = (180.0f / Mathf.PI);
	private const float DEG2RAD = (Mathf.PI / 180.0f);

	private const float PitchMotionTime = 3;
	//private const float PitchMotionTime = 0.5f;
	//private const float SensorStartTime = 3;
	//private const float SensorStartTime = 2;
	private const float SensorStartTime = 0.5f;
	private const float PushTime = 1.425f;
	//private const float PushTime = 1.0f;
	//private const float PushTime = 0.8f;
	//private const float PushTime = 0.175f;
	private const float CountDonwDuration = 3.0f;
	private const float StartDuration = 5.0f;
	private const float MainSpeedLife = 3;
	private const float MainSpeedScale = 1.2f;
	//private const float HitGroundDuration = 2.0f;
	private const float HitGroundDuration = 3.0f;
	//private const float HitInfoDuration = 3.0f;
	private const float HitInfoDuration = 2.5f;
	//private const float HitRecordPosX = 10.0f;
	private const float HitRecordPosX = 0;
	//private const float HitRecordPosY = -410.0f;
	private const float HitRecordPosY = -145;
	//private const float HitRecordHeight = 36.0f;
	//private const float HitRecordHeight = 24.0f;
	private const float HitRecordHeight = 48.0f;
	private const float SysInfoHeight = 25.0f;

	private DetectNotify m_detect = new DetectNotify();
	private GameObject m_objMainCanvas;
	private GameObject m_objInfo1;
	private GameObject m_objInfo2;
	private GameObject m_objGage = null;
	private GameObject m_objReady = null;
	private BatterAnim m_objBatter;
	private PitcherAnim m_objPitcher;
	private HitBall m_lastHit = null;
	private Camera m_objCamera = null;
	private SmoothFollow m_smoothFollow = null;
	private TrailView m_topView = null;
	private TrailView m_sideView = null;

	private PVAVector3D m_pos0, m_pos1;
	private List<Vector3> m_posArray = new List<Vector3>();
	private List<HitRecord> m_RecordArray = new List<HitRecord>();
	private List<HitBall> m_HitArray = new List<HitBall>();
	private List<HitInfo> m_HitInfoArray = new List<HitInfo>();

	private bool m_bPortInit = false;
	private bool m_bPortLog = true;
	private bool m_bEnablePort = true;
	//private bool m_bFollow = true;
	private bool m_bPitchMotion = false;
	private bool m_bSensorStart = false;
	private bool m_bPush = false;
	private bool m_bPhaseLog = true;
	private int m_nGamePhase;
	private int m_nSensorPhase;
	private PVAResult m_status = PVAResult.Uninitialize;
	private float m_dTime = 0;
	//private float m_posSysInfo = 0;
}
