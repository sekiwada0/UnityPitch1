using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using GigaTrax.Ports;
//using GigaTrax.PVA;
using GigaTrax;
using TMPro;
using UnityRawInput;

public class Analyse_Main : MonoBehaviour {

	// Use this for initialization
	void Start(){
/*
		// Display.displays[0] は主要デフォルトディスプレイで、常に ON。
		// 追加ディスプレイが可能かを確認し、それぞれをアクティベートします。
		if( Display.displays.Length > 1 )
			Display.displays[1].Activate();
		Display.displays[0].Activate();
		//if( Display.displays.Length > 2 )
		//	Display.displays[2].Activate();
*/
		m_objInfo2 = prefabMainCanvas.transform.Find("Info2").gameObject;

		m_objGage = prefabMainCanvas.transform.Find("Gage").gameObject;
		m_objGage.SetActive(false);

		m_objReady = prefabMainCanvas.transform.Find("Ready").gameObject;
		m_objResult = prefabMainCanvas.transform.Find("Result").gameObject;

		m_Camera = prefabMainCamera.GetComponent<Camera>();
		m_ctlCamera = prefabMainCamera.GetComponent<CameraControl>();

		//m_nGameMode = GameMode.ServeLeft;
		m_nGameMode = MainFrame.getSysInt(strGameMode);
		switch( m_nGameMode ){
		case GameMode.ServeLeft:
		case GameMode.ServeRight:
			m_bServe = true;
			break;
		default:
			m_bServe = false;
			break;
		}

		string strSpeedUnit = MainFrame.getSysString("SpeedUnit");
		if( strSpeedUnit == "Mile" )
			m_nSpeedUnit = SpeedUnit_MPH;
		else
			m_nSpeedUnit = SpeedUnit_KMPH;

		/* Show UI */
		GameObject objRest = prefabMainCanvas.transform.Find("Rest").gameObject;
		//objRest.SetActive(true);
		GameObject objRestName = objRest.transform.Find("name").gameObject;
		Text txtRestName = objRestName.GetComponent<Text>();

		m_objRestValue = objRest.transform.Find("value").gameObject;

		string strGameLimit = MainFrame.getSysString("GameLimit");
		if( strGameLimit == "Time" ){
			txtRestName.text = "RestTime";
			m_nGameLimit = GameLimit_Time;
			m_fRestTime = (float)MainFrame.getSysInt("TimeLimit");
			m_nRestTime = 0;
			m_nSecondSwitching = MainFrame.getSysInt("SecondSwitching");
		}
		else if( strGameLimit == "Ball" ){
			txtRestName.text = "RestBall";
			m_nGameLimit = GameLimit_Ball;
			m_nBallLimit = MainFrame.getSysInt("BallLimit");
			m_nRestBall = m_nBallLimit;
		}
		updateRest();
		initRest();

		m_objWarning = prefabMainCanvas.transform.Find("Warning").gameObject;
		m_objWarning.SetActive(false);

		int modify = MainFrame.PVA_getInt("Modify");
		if( !m_bServe ){
			float fPitch_Timeout = MainFrame.getSysFloat("Pitch_Timeout");
			MainFrame.PVA_setInt("Pitch_Timeout", (int)(fPitch_Timeout * 1000.0f));
		}
		else{
			MainFrame.PVA_setInt("CalcDuration", 0);
			MainFrame.PVA_setInt("Pitch_Timeout", 0);
		}
		MainFrame.PVA_setFloat("XRange", MainFrame.getSysFloat("XRange"));
		MainFrame.PVA_setFloat("ZRange", MainFrame.getSysFloat("ZRange"));
		MainFrame.PVA_setInt("StrikeZone", 0);
		MainFrame.PVA_setInt("PitchEndLimit", 0);
		MainFrame.PVA_setInt("BatterSide", 0);
		MainFrame.PVA_setInt("StopFloorBound", 0);
		MainFrame.PVA_setInt("NeedPitch", m_bServe? 0: 1);
		MainFrame.PVA_setInt("SkidSearch", 1);
		MainFrame.PVA_setInt("Modify", modify);

		m_dInit = MainFrame.getSysFloat("InitDuration");
		if( !m_bServe ){
			m_dStart = (float)MainFrame.PVA_getInt("CalcDuration");
			if( m_dStart <= 0 )
				m_dStart = StartDuration;
		}
		else{
			m_dStart = MainFrame.getSysFloat("ServeTime");
		}
		m_dCountDown = MainFrame.getSysFloat("CountDown");
		m_dResultDuration = MainFrame.getSysFloat("ResultDuration");
		m_dEnemyServeTime = MainFrame.getSysFloat("EnemyServeTime");
		m_dBallDestroy = MainFrame.getSysFloat("BallDestroy");

		m_dPitchONTime = MainFrame.getMachineFloat("PitchONTime");

		if( m_nGameMode != GameMode.Random ){
			m_dShootInterval = MainFrame.getSysFloat("ShootInterval");
		}
		else{
			m_dShootInterval = MainFrame.getSysFloat("ShootInterval_Random");
		}
		if( !m_bServe ){
			m_fCameraPosY = MainFrame.getSysFloat("Receive_CameraPosY");
			m_fCameraPosZ = MainFrame.getSysFloat("Receive_CameraPosZ");
			m_fCameraRotX = MainFrame.getSysFloat("Receive_CameraRotX");
			m_fCameraRotY = 0;
			m_fCameraFOV = MainFrame.getSysFloat("Receive_CameraFOV");
			m_fShootPosZ = MainFrame.getSysFloat("Receive_ShootPosZ");
			m_bUseDizzView = false;
		}
		else{
			if( m_bUseDizzView ){
				transform.localPosition = new Vector3(0, -10, 0);
				transform.eulerAngles = new Vector3(90, 0, 0);

				GameObject objDizzView = prefabDizzView.gameObject;
				objDizzView.SetActive( true );

				GameObject objDizzCanvas = prefabDizzCanvas.gameObject;
				objDizzCanvas.SetActive( true );
			}
			else{
				m_fCameraPosY = MainFrame.getSysFloat("Serve_CameraPosY");
				m_fCameraPosZ = MainFrame.getSysFloat("Serve_CameraPosZ");
				m_fCameraRotX = MainFrame.getSysFloat("Serve_CameraRotX");
				m_fCameraRotY = 0;
				m_fCameraFOV = MainFrame.getSysFloat("Serve_CameraFOV");
				m_fShootPosX = 0;
				m_fShootPosZ = MainFrame.getSysFloat("Serve_ShootPosZ");
			}
		}

		m_fHawkEyeDist = MainFrame.getSysFloat("HawkEyeDist");

		GameObject objEnemyPos = prefabEnemyPos.gameObject;
		m_enemyPos = objEnemyPos.transform.position;

		GameObject objMachine = prefabMachine.gameObject;
		objMachine.SetActive( m_bServe? false: true );

		if( !m_bServe ){
			m_fMinX = -CourtLimitX;
			m_fMaxX = CourtLimitX;

			m_fMinZ = 0;
			m_fMaxZ = CourtLimitZ2;
		}
		else{
			if( m_nGameMode == GameMode.ServeRight ){
				m_fMinX = -CourtLimitX;
				m_fMaxX = 0;
			}
			else{
				m_fMinX = 0;
				m_fMaxX = CourtLimitX;
			}
			m_fMinZ = 0;
			m_fMaxZ = CourtLimitZ1;
		}

		MainFrame.PlayInfo_start( null );

		initHitRecord();
		//initStrikeMark();

		switch( m_nGameMode ){
		case GameMode.ServeLeft:
		case GameMode.ServeRight:
			m_viewPos = 2;
			break;
		case GameMode.Forehand:
			m_viewPos = 2;
			break;
		case GameMode.Middlehand:
			m_viewPos = 1;
			break;
		case GameMode.Backhand:
			m_viewPos = 0;
			break;
		case GameMode.Random:
			m_viewPos = 1;
			break;
		}
		resetView();
		//setSceneIdle();

		setPromp( Prompt_Init );
		setGamePhase( GamePhase_Init );
		setSensorPhase( SensorPhase_Busy );
	}
	void OnDestroy(){
		if( m_bCtrlPower ){
			MainFrame.W3Ctrl_PowerOFF();
			m_bCtrlPower = false;
		}
		MainFrame.PlayInfo_end();
		//m_PVACtrl.endBall();
		//m_PVACtrl.term();
	}

	// Update is called once per frame
	void Update(){
		GigaTrax.PVA.APIResult status = MainFrame.PVA_getCamraStatus();
		if( status != GigaTrax.PVA.APIResult.OK ){
			//GigaTrax.Debug.Log( "CamraStatus" + status );
			return;
		}
		if( !m_bCtrlPower ){
			if( m_bServe ){
				m_strDir = strCenter;
				m_strLevel = "Serve";
			}
			else{
				switch( m_nGameMode ){
				case GameMode.Forehand:
					m_strDir = strLeft;
					break;
				case GameMode.Middlehand:
					m_strDir = strCenter;
					break;
				case GameMode.Backhand:
					m_strDir = strRight;
					break;
				}
				int nBounce = MainFrame.getSysInt(strBounce);
				switch( nBounce ){
				case Bounce.Stroke:
					m_strBounce = "Stroke";
					break;
				case Bounce.Volley:
					m_strBounce = "Volley";
					break;
				}
				int nGameLevel = MainFrame.getSysInt(strGameLevel);
				switch( nGameLevel ){
				case GameLevel.Beginner:
					m_strLevel = "Beginner";
					break;
				case GameLevel.Ama:
					m_strLevel = "Ama";
					break;
				case GameLevel.Pro:
					m_strLevel = "Pro";
					break;
				}
			}
			MainFrame.W3Ctrl_setParam( m_strLevel, m_strBounce + ":" + m_strDir );

			//MainFrame.W3Ctrl_Open();
			MainFrame.W3Ctrl_PowerON();
			m_bCtrlPower = true;
		}

		switch( m_nSensorPhase ){
		case SensorPhase_Busy:{
			int busy;
			busy = MainFrame.PVA_getInt("Busy");
			//GigaTrax.Debug.Log("Busy:" + busy);
			if( busy == 0 ){
				setSensorPhase( SensorPhase_Ready );
			}
			break;}
		case SensorPhase_Ready:
			break;
		//case SensorPhase_StartWait:
		//	break;
		case SensorPhase_Start:{
			GigaTrax.PVA.APIResult ret = MainFrame.PVA_getDetect( ref m_detect );
			if( ret == GigaTrax.PVA.APIResult.OK ){
				m_objGage.SetActive(false);
				MainFrame.PVA_endBall();
				setSensorPhase(SensorPhase_Busy);
				//setGamePhase( GamePhase_Hit );

				HitRecord record = addHitRecord();

				IntPtr pData;
				if( (m_detect.flags & (int)GigaTrax.PVA.DetectFlg.Hit) != 0 ){
					pData = m_detect.hit_path.data;

					GigaTrax.PVA.DetectData first = (GigaTrax.PVA.DetectData)Marshal.PtrToStructure(pData, typeof(GigaTrax.PVA.DetectData));
					pData = new IntPtr(pData.ToInt64() + Marshal.SizeOf(typeof(GigaTrax.PVA.DetectData)));
					GigaTrax.PVA.DetectData last = (GigaTrax.PVA.DetectData)Marshal.PtrToStructure(pData, typeof(GigaTrax.PVA.DetectData));

					uint send_time = last.time - first.time;
					float t = (float)1000 / (float)send_time;

					Vector3 dir = new Vector3();
					dir.x = last.pos.x - first.pos.x;
					dir.y = last.pos.y - first.pos.y;
					dir.z = last.pos.z - first.pos.z;
					float speed = dir.magnitude * t;
					dir.Normalize();
					dir.z = -dir.z;
					dir = Quaternion.Euler( 0, m_fCameraRotY, 0 ) * dir;

					Vector3 pos = new Vector3(
						m_fShootPosX + first.pos.x,
						first.pos.y,
						m_fShootPosZ - first.pos.z
					);
					startHit( record, pos, dir, speed );

					setGamePhase( GamePhase_Hit );

					if( m_nGameLimit == GameLimit_Ball ){
						m_nRestBall--;
						updateRest();
						if( m_nRestBall <= 0 )
							m_bGameEnd = true;
					}
				}
				else{
					record.result = Result_Fault;
					//MainFrame.PlayInfo_add( record.result, 0 );

					setGamePhase( GamePhase_Idle );
					resetView();
					//setSceneIdle();
				}
				updateHitRecord();
			}
			break;}
		}

		if( m_nGameLimit == GameLimit_Time ){
			switch( m_nGamePhase ){
			case GamePhase_Init:
			case GamePhase_Result:
				break;
			default:
				if( m_fRestTime > 0 ){
					m_fRestTime -= Time.deltaTime;
					if( m_fRestTime < 0 )
						m_fRestTime = 0;
					if( m_fRestTime == 0 )
						m_bGameEnd = true;
					updateRest();
				}
				if( !m_bGameEnd ){
					if( MainFrame.Key_isAlt(RawKey.C) ){
						m_bGameEnd = true;
					}
				}
				break;
			}
		}
		switch( m_nGamePhase ){
		case GamePhase_Init:
			m_dTime -= Time.deltaTime;
			//chkPitchON();
			if( m_dTime <= 0 ){
				setGamePhase( GamePhase_Idle );
			}
			break;
		case GamePhase_Idle:
			if( m_bGameEnd ){
				setGamePhase( GamePhase_Result );
				break;
			}
			/*if( MainFrame.Key_isAlt(RawKey.B) ){
				setGamePhase( GamePhase_CountDown );
				break;
			}*/
			if( m_nSensorPhase == SensorPhase_Ready ){
				if( !m_bServe ){
					m_dTime -= Time.deltaTime;
					chkPitchON();
					if( m_dTime <= 0 )
						setGamePhase( GamePhase_CountDown );
				}
				else{
					StartCoroutine("ShowWarning");
					MainFrame.W3Ctrl_PitchON();

					MainFrame.PVA_startBall();
					setSensorPhase( SensorPhase_Start );

					setGamePhase( GamePhase_Start );
					setPromp( Prompt_Serve );
				}
			}
			break;
		case GamePhase_CountDown:{
			m_dTime -= Time.deltaTime;
			chkPitchON();

			if( !m_bEnemyServe ){
				if( m_dTime < m_dEnemyServeTime ){
					startEnemyServe();
					m_bEnemyServe = true;
				}
			}
			if( !m_bSensorStart ){
				if( m_dTime < SensorStartTime ){
					MainFrame.PVA_startBall();
					setSensorPhase( SensorPhase_Start );
					m_bSensorStart = true;
				}
			}

			float ratio = m_dTime / m_dCountDown;
			UnityEngine.UI.Image image = m_objGage.GetComponent<UnityEngine.UI.Image>();
			if( ratio < 0 )
				ratio = 0;
			image.fillAmount = ratio;
			//prefabSpeed.text = "ratio: " + ratio.ToString("f2");

			if( ratio == 0 ){
				setGamePhase( GamePhase_Start );
				m_objGage.SetActive(false);
				setPromp( Prompt_None );
			}
			break;}

		case GamePhase_Start:{
			bool end = false;
			if( MainFrame.Key_isCtrl(RawKey.S) ){
				MainFrame.PVA_setInt("SaveBin", 1);
				end = true;
			}
			else{
				m_dTime -= Time.deltaTime;
				if( m_dTime <= 0 ){
					end = true;
				}
			}
			if( end ){
				MainFrame.PVA_endBall();
				setSensorPhase( SensorPhase_Busy );
				setGamePhase( GamePhase_Idle );
				resetView();
				//setSceneIdle();
			}
			break;}

		case GamePhase_Hit:
			m_dTime -= Time.deltaTime;
			if( !m_bServe ){
				chkPitchON();
			}
			if( m_dTime <= 0 ){
				setGamePhase( GamePhase_Idle );
				resetView();
			}
			break;
		case GamePhase_Result:
			m_dTime -= Time.deltaTime;
			if( m_dTime <= 0 ){
				SceneManager.LoadScene(strGameOver);
			}
			break;
		}

		if( m_nLookAt == 1 ){
			if( !m_ctlCamera.isProc() ){
				//m_ctlCamera.setLookAround( m_posLookAt, 10.0f );
				m_ctlCamera.setTopView( m_posLookAt, 24.0f );
				m_ctlCamera.setZoom(14);
				m_nLookAt = 2;
			}
		}
	}
	void startEnemyServe()
	{
		//Vector3 shootPos = new Vector3(0, 0.615f, 12.111f);
		Vector3 shootPos = m_enemyPos;
		//Vector3 targetPos = transform.position;
		Vector3 targetPos = new Vector3(m_fShootPosX, 0, m_fShootPosZ);
		float speed = 80.0f * KPH2MPS;

		GameObject ballEffect = Instantiate(prefabBallEffect);
		ballEffect.transform.localPosition = shootPos;
		ballEffect.transform.eulerAngles = new Vector3(-90, 0, 0);
		Destroy( ballEffect, 1.0f );

		float vx = targetPos.x - shootPos.x;
		float vz = targetPos.z - shootPos.z;
		float div = shootPos.z / Mathf.Abs( vz );
		float vy = 1.1f; /*shoot_hole_height*/
		targetPos.x = vx * div + shootPos.x;
		targetPos.y = vy;
		targetPos.z = 0;
		//GigaTrax.Debug.Log( "targetPos:" + targetPos );

		Vector3 vec = m_ShootController.Shoot( shootPos, targetPos, speed );

		GameObject ball = Instantiate(prefabBallPitch);
		//BallHit ball_hit = ball.GetComponent<BallHit>();
		ball.transform.localPosition = shootPos;

		Rigidbody rb = ball.GetComponent<Rigidbody>();
		//rb.velocity = vec;

		Vector3 force = vec * rb.mass;
		rb.AddForce( force, ForceMode.Impulse );

		//Destroy( ball, EnemyServeTime );
		Destroy( ball, 1.0f );
	}
	void startHit(HitRecord record,Vector3 pos,Vector3 dir,float speed)
	{
		if( m_bPhaseLog )
			GigaTrax.Debug.Log( "startHit" );

		GameObject ball = Instantiate(prefabBallHit);
		BallHit ball_hit = ball.GetComponent<BallHit>();
		ball_hit.addCallback( HitGround );

		ball.transform.localPosition = pos;

		Rigidbody rb = ball.GetComponent<Rigidbody>();
		rb.velocity = dir * speed;

		addHitInfo( ball, speed * MPS2KPH );

		record.speed = speed * MPS2KPH;
		record.ball = ball;
	}
	void setSensorPhase(int phase)
	{
		m_nSensorPhase = phase;

		if( m_bPhaseLog ){
			string text;
			switch( phase ){
			case SensorPhase_Busy: text = "Busy"; break;
			case SensorPhase_Ready: text = "Ready"; break;
			case SensorPhase_StartWait: text = "StartWait"; break;
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
		case GamePhase_Init:
			m_dTime = m_dInit;
			break;
		case GamePhase_Idle:
			m_dTime = m_dShootInterval;
			break;
		case GamePhase_CountDown:
			m_dTime = m_dCountDown;

			setSensorPhase( SensorPhase_StartWait );
			if( m_nGameMode == GameMode.Random ){
				int viewPos = UnityEngine.Random.Range(0, 2);
				if( viewPos != m_viewPos ){
					m_viewPos = viewPos;

					switch( m_viewPos ){
					case 2:
						m_strDir = strLeft;
						break;
					case 1:
						m_strDir = strCenter;
						break;
					case 0:
						m_strDir = strRight;
						break;
					}
					MainFrame.W3Ctrl_setParam( m_strLevel, m_strBounce + ":" + m_strDir );
				}
			}
			resetView();

			setPromp( Prompt_CountDown );
			m_objGage.SetActive(true);
			m_bSensorStart = false;
			m_bEnemyServe = false;
			break;
		case GamePhase_Start:
			m_bPitchON = false;
			m_dTime = m_dStart;
			break;
		case GamePhase_Hit:
			setPromp( Prompt_None );
			m_dTime = HitDuration;
			break;
		case GamePhase_Result:
			MainFrame.W3Ctrl_setParam( "System", "Origin" );

			setPromp( Prompt_Result );
			m_dTime = m_dResultDuration;
			break;
		default:
			m_dTime = 0;
			break;
		}
		if( m_bPhaseLog ){
			string text;
			switch( phase ){
			case GamePhase_Init: text = "Init"; break;
			case GamePhase_Idle: text = "Idle"; break;
			case GamePhase_CountDown: text = "CountDown"; break;
			case GamePhase_Start: text = "Start"; break;
			case GamePhase_Hit: text = "Hit"; break;
			case GamePhase_Result: text = "Result"; break;
			default: text = "???"; break;
			}
			GigaTrax.Debug.Log( "Game:" + text );
		}
	}
	void chkPitchON()
	{
		if( m_nSensorPhase != SensorPhase_Ready )
			return;

		if( !m_bPitchON ){
			float time = m_dTime;
			switch( m_nGamePhase ){
			case GamePhase_Start:
				time += (m_dShootInterval + m_dCountDown);
				break;
			case GamePhase_Hit:
				time = m_dShootInterval + m_dCountDown;
				break;
			case GamePhase_Idle:
				time += m_dCountDown;
				break;
			}
			if( time < m_dPitchONTime ){
				if( m_nGamePhase == GamePhase_Hit )
					m_dTime = 0;

				StartCoroutine("ShowWarning");
				MainFrame.W3Ctrl_PitchON();
				m_bPitchON = true;
			}
		}
	}
	public void HitGround(GameObject ball,string tag){
		if( tag == "Ground" ){
			Vector3 pos = ball.transform.localPosition;

			bool found = false;
			int num = m_RecordArray.Count;
			for(int i = 0; i < num; i++){
				HitRecord record = m_RecordArray[i];
				if( ball == record.ball ){
					found = true;

					float timeDestroy = m_dBallDestroy;
					if( m_nGamePhase == GamePhase_Hit )
						m_dTime = HitGroundDuration;

					/*need_hawkeye? */
					if( isNeedHawkEye( pos ) ){
						timeDestroy += HitGroundDuration;

						if( m_bServe ){
							m_ctlCamera.setMoveTarget( new Vector3(pos.x, m_fCameraPosY, pos.z) );
							//m_ctlCamera.setMoveFocus( new Vector3(pos.x, m_fCameraPosZ, pos.z), pos );
							//m_ctlCamera.setTopView( pos, m_fCameraPosZ );
							//m_ctlCamera.setZoom(14);
							m_nLookAt = 2;

						}
						else{
							m_ctlCamera.setMoveFocus( new Vector3(0, 3.43f, 0), pos );
							m_posLookAt = pos;
							m_nLookAt = 1;
						}
					}
					GameObject ballStamp = Instantiate(prefabBallStamp);
					ballStamp.transform.localPosition = new Vector3(pos.x, 0.0087f, pos.z);
					Destroy( ballStamp, timeDestroy );

					Destroy( ball, timeDestroy );

					/* check IN/OUT */
					if( pos.x > m_fMinX && pos.x < m_fMaxX && pos.z > m_fMinZ && pos.z < m_fMaxZ ){
						record.result = Result_In;
					}
					else{
						record.result = Result_Out;
					}

					int index = (m_RecordArray.Count-1) - i;
					if( index < NumHitRecord )
						setHitResult_( record.result, m_objHitRecord[index] );

					if( m_HitInfo != null )
						updateHitInfo( record.result );

					//GameObject ballEffect = Instantiate(prefabBallEffect);
					//ballEffect.transform.localPosition = new Vector3(pos.x, 0.05f, pos.z);
					//Destroy( ballEffect, 1.0f );

					record.ball = null;
					record.bGround = true;
					break;
				}
			}
			if( !found )
				return;

			if( m_HitInfo != null ){
				Destroy( m_HitInfo, HitInfoDuration );
				m_HitInfo = null;
			}
		}
		else if( tag == "Net" ){
			Rigidbody rb = ball.GetComponent<Rigidbody>();
			//rb.velocity = new Vector3(0, 0, 0);
			rb.velocity *= 0.1f;
			const float min = 0.05f;
			//float z = Mathf.Abs( rb.velocity.z );
			//if( z < min )
			if( rb.velocity.magnitude < min )
			{
				rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, -min);
				//rb.velocity.z = -min;
	 		}
 		}
	}

	/*
		点と線分の距離を求める(二乗)
		a-b 線分
		pos 点
	*/
	static float sqdistance_ls_p(Vector2 a,Vector2 b,Vector2 pos)
	{
		const float EPS = 0.0000001f;

		/* 線分Ａと線分ＢのベクトルＡＢを作成 */
		Vector2 ab = b - a;
		float sqlen = ab.sqrMagnitude;
		if( sqlen < EPS )
			return (pos - a).sqrMagnitude;

		/* 線分Ａとある点ＰのベクトルＡＰを作成 */
		Vector2 ap = pos - a;

		/* ベクトルＡＢ、ＡＰの内積より媒介変数ｔを求め、線分内にあるか調べる */
		float t = Vector2.Dot(ab, ap) / sqlen;

		/* ある点Ｐから下ろした垂線が線分上にない */
		if( t <= 0 )
			return (pos - a).sqrMagnitude;
		if( t >= 1 )
			return (pos - b).sqrMagnitude;

		/* 線分Ａと交点ＱのベクトルＡＱを作成 */
		Vector2 aq = ab * t;

		/* ある点Ｐと交点ＱのベクトルＰＱを作成 */
		return ( aq - ap ).sqrMagnitude;
	}
	bool isNeedHawkEye(Vector3 pos)
	{
		if( m_bUseDizzView )
			return false;

		Vector2 pos2D = new Vector2(pos.x, pos.z);
		Vector2 p0 = new Vector2( m_fMinX, m_fMinZ );
		Vector2 p1 = new Vector2( m_fMinX, m_fMaxZ );
		Vector2 p2 = new Vector2( m_fMaxX, m_fMaxZ );
		Vector2 p3 = new Vector2( m_fMaxX, m_fMinZ );

		float sq_dist = m_fHawkEyeDist * m_fHawkEyeDist;

		float dist1 = sqdistance_ls_p( p0, p1, pos2D );
		float dist2 = sqdistance_ls_p( p1, p2, pos2D );
		float dist3 = sqdistance_ls_p( p2, p3, pos2D );
		//GigaTrax.Debug.Log( "dist1:" + Mathf.Sqrt(dist1) );
		//GigaTrax.Debug.Log( "dist2:" + Mathf.Sqrt(dist2) );
		//GigaTrax.Debug.Log( "dist3:" + Mathf.Sqrt(dist3) );

		if( dist1 < sq_dist )
			return true;
		if( dist2 < sq_dist )
			return true;
		if( dist3 < sq_dist )
			return true;

		if( m_bServe ){
			pos = m_Camera.WorldToScreenPoint( pos );

			GigaTrax.Debug.Log( "pos:" + pos );

			if( pos.x < 10 || pos.y < 10 || pos.x > (Screen.width-10) || pos.y > (Screen.height-10) )
				return true;
		}

		return false;
	}

/*HitRecord*/
	void initHitRecord(){
		m_objHitRecord = new GameObject[NumHitRecord];

		float y = HitRecordPosY;
		for(int i = 0; i < NumHitRecord; i++){
			GameObject obj = Instantiate(prefabHitRecord);
			obj.transform.localPosition = new Vector3(HitRecordPosX, y, 0);
			obj.transform.SetParent(m_objInfo2.transform, false);
			obj.SetActive(false);

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
			HitRecord record = m_RecordArray[end-i];
			GameObject obj = m_objHitRecord[i];
			obj.SetActive(true);

			GameObject objNo = obj.transform.Find("no").gameObject;
			Text txNo = objNo.GetComponent<Text>();
			txNo.text = record.nNo.ToString();

			setHitSpeed_(record, obj);
			setHitResult_( record.result, obj );
		}
	}
	HitRecord addHitRecord(){
		HitRecord record = new HitRecord();

		int size = m_RecordArray.Count;
		record.ball = null;
		record.nNo = size + 1;
		record.result = Result_None;
		record.speed = -1;
		record.bGround = false;

		m_RecordArray.Add( record );
		return record;
	}
	void setHitSpeed_(HitRecord record,GameObject obj){
		GameObject objSpeed = obj.transform.Find("speed").gameObject;
		GameObject objSpeedUnit = obj.transform.Find("speed_unit").gameObject;

		string text = "";
		if( record.speed >= 0 ){
			text = ((int)record.speed).ToString();
		}
		objSpeed.SetActive( true );
		objSpeedUnit.SetActive( true );

		Text txSpeed = objSpeed.GetComponent<Text>();
		txSpeed.text = text;
	}
	void setHitResult_(int result,GameObject obj){
		GameObject objResult = obj.transform.Find("result").gameObject;

		Text txResult = objResult.GetComponent<Text>();
		txResult.text = getHitResult_( result );
	}
	void addHitInfo(GameObject ball,float speed){
		int ispeed = (int)speed;

		GameObject objInfo;
		objInfo = Instantiate(prefabHitInfo);
		objInfo.transform.SetParent(prefabMainCanvas.transform, false);

		GameObject objText = objInfo.transform.Find("speed").gameObject;
		TextMeshProUGUI tx1 = objText.GetComponent<TextMeshProUGUI>();
		tx1.text = ispeed.ToString();

		m_HitInfo = objInfo;
	}
	void updateHitInfo(int result){
		GameObject objText = m_HitInfo.transform.Find("result").gameObject;
		TextMeshProUGUI tx1 = objText.GetComponent<TextMeshProUGUI>();
		tx1.text = getHitResult_( result );
	}
	static string getHitResult_(int result){
		string text = "";
		if( result == Result_Fault ){
			text = "FAULT";
		}
		else if( result == Result_Net ){
			text = "NET";
		}
		else if( result == Result_In ){
			text = "IN";
		}
		else if( result == Result_Out ){
			text = "OUT";
		}
		return text;
	}
	void resetView(){
		if( m_bUseDizzView )
			return;
		m_ctlCamera.clear();

		Vector3 pos = new Vector3(0, m_fCameraPosY, m_fCameraPosZ);

		if( !m_bServe ){
			float x;
			if( m_viewPos == 0 ){
				x = -4;
			}
			else if( m_viewPos == 1 ){
				x = 0;
			}
			else{
				x = 4;
			}
			m_fShootPosX = x;

			/* viewのY軸回転を求める */
			float vx = m_enemyPos.x - m_fShootPosX;
			float vz = m_enemyPos.z - m_fShootPosZ;

			float rad = Mathf.Atan2(vx, vz);
			m_fCameraRotY = rad * Mathf.Rad2Deg;

			pos.z -= m_fShootPosZ;
			pos = Quaternion.Euler( 0, m_fCameraRotY, 0 ) * pos;
			pos.x += m_fShootPosX;
			pos.z += m_fShootPosZ;
		}

		m_ctlCamera.setMoveTarget( pos );
		transform.eulerAngles = new Vector3(m_fCameraRotX, m_fCameraRotY, 0);
		m_Camera.fieldOfView = m_fCameraFOV;
		m_nLookAt = 0;
	}
/*Rest*/
	void initRest(){
		StartCoroutine("FlashRest");
	}
	void updateRest(){
		if( m_nGameLimit == GameLimit_Time ){
			int nRestTime = (int)m_fRestTime;
			if( nRestTime != m_nRestTime ){
				m_nRestTime = nRestTime;

				int min = 0;
				int sec = 0;
				if( m_nRestTime < m_nSecondSwitching ){
					min = m_nRestTime / 60;
					sec = m_nRestTime % 60;
				}
				else{
					min = (m_nRestTime + 30) / 60;
					sec = 0;
				}
				Text txt = m_objRestValue.GetComponent<Text>();
				txt.text = min.ToString() + ":" + sec.ToString("00");
				//txt.text = min.ToString() + ":" + sec.ToString("00") + " (" + m_nRestTime.ToString() + ")";
			}
		}
		else if( m_nGameLimit == GameLimit_Ball ){
			Text txt = m_objRestValue.GetComponent<Text>();
			txt.text = m_nRestBall.ToString() + "/" + m_nBallLimit.ToString();
		}
	}
	// 呼び出されたらRestBlink秒待ってBlinkerを解除する
	IEnumerator FlashRest()
	{
		int nRestBlink = MainFrame.getSysInt("RestBlink");

		yield return new WaitForSeconds( (float)nRestBlink );

		Blinker blinker = m_objRestValue.GetComponent<Blinker>();
		blinker.enabled = false;
	}
	// 呼び出されたらActiveにして ShowWarningTime秒待って 非Activeにする
	IEnumerator ShowWarning(){
		m_objWarning.SetActive(true);

		yield return new WaitForSeconds( ShowWarningTime );

		m_objWarning.SetActive(false);
	}
	void setPromp(int value)
	{
		m_nPrompt = value;
		switch( value ){
		case Prompt_Init:{
			m_objReady.SetActive(true);
			Text txReady = m_objReady.GetComponent<Text>();
			txReady.text = "STARTING...";

			m_objResult.SetActive(false);
			break;}
		case Prompt_None:
			m_objReady.SetActive(false);
			break;
		case Prompt_Result:{
			m_objReady.SetActive(false);
			m_objResult.SetActive(true);

			GameObject objSuccess = m_objResult.transform.Find("Success").gameObject;
			GameObject objAveSpeed = m_objResult.transform.Find("AveSpeed").gameObject;
			GameObject objMaxSpeed = m_objResult.transform.Find("MaxSpeed").gameObject;

			int num_in = 0;
			float max_speed = 0;
			float sum_speed = 0;
			int num_speed = 0;
			int num = m_RecordArray.Count;
			for(int i = 0; i < num; i++){
				HitRecord record = m_RecordArray[i];
				if( record.result == Result_In )
					num_in++;
				if( record.speed > 0 ){
					if( record.speed > max_speed )
						max_speed = record.speed;
					sum_speed += record.speed;
					num_speed++;
				}
			}
			float ave_speed = 0;
			float success = 0;
			if( num_speed > 0 ){
				ave_speed = sum_speed / (float)num_speed;
			}
			if( num > 0 ){
				success = (float)num_in / (float)num * 100.0f;
			}
			Text txSuccess = objSuccess.GetComponent<Text>();
			txSuccess.text = success.ToString("f1") + " %";

			Text txAveSpeed = objAveSpeed.GetComponent<Text>();
			txAveSpeed.text = ave_speed.ToString("f1") + " km/h";

			Text txMaxSpeed = objMaxSpeed.GetComponent<Text>();
			txMaxSpeed.text = max_speed.ToString("f1") + " km/h";

			break;}
		default:{
			string text = "";
			switch( value ){
			case Prompt_CountDown:
				text = "READY !";
				break;
			case Prompt_Serve:
				text = "SERVE PLEASE";
				break;
			case Prompt_In:
				text = "IN !";
				break;
			case Prompt_Out:
				text = "OUT !";
				break;
			}
			m_objReady.SetActive(true);

			Text txReady = m_objReady.GetComponent<Text>();
			txReady.text = text;
			break;}
		}
	}

	public GameObject prefabMainCamera;
	public GameObject prefabMainCanvas;
	public GameObject prefabBallPitch;
	public GameObject prefabBallHit;
	public GameObject prefabBallEffect;
	public GameObject prefabBallStamp;
	public GameObject prefabHitRecord;
	public GameObject prefabHitInfo;
	public GameObject prefabEnemyPos;
	public GameObject prefabMachine;
	public GameObject prefabDizzView;
	public GameObject prefabDizzCanvas;

	private string m_strPush;

	private const int SpeedUnit_MPH = 0;
	private const int SpeedUnit_KMPH = 1;
	private int m_nSpeedUnit;

/*Rest*/
	private const int GameLimit_Unknown = -1;
	private const int GameLimit_Time = 0;
	private const int GameLimit_Ball = 1;
	private int m_nGameLimit = GameLimit_Unknown;
	private float m_fRestTime = 0;
	private int m_nRestTime = 0;
	private int m_nSecondSwitching = 0;
	private int m_nBallLimit = 0;
	private int m_nRestBall = 0;

	private GameObject[] m_objHitRecord;

	private const int GamePhase_Init        = 0;
	private const int GamePhase_Idle        = 1;
	private const int GamePhase_CountDown   = 2;
	private const int GamePhase_Start       = 3;
	private const int GamePhase_Hit         = 4;
	private const int GamePhase_Result      = 5;

	private const int SensorPhase_Busy      = 0;
	private const int SensorPhase_Ready     = 1;
	private const int SensorPhase_StartWait = 2;
	private const int SensorPhase_Start     = 3;

	private const int NumHitRecord = 20;

	private const int Result_None = 0;
	private const int Result_Fault = 1;
	private const int Result_Net = 2;
	private const int Result_In = 3;
	private const int Result_Out = 4;

	private const int Prompt_Init = 0;
	private const int Prompt_CountDown = 1;
	private const int Prompt_Serve = 2;
	private const int Prompt_None = 3;
	private const int Prompt_In = 4;
	private const int Prompt_Out = 5;
	private const int Prompt_Result = 6;

	private const float KPH2MPS = (1000.0f / 3600.0f);
	private const float MPS2KPH = (3600.0f / 1000.0f);

	private const float RAD2DEG = (180.0f / Mathf.PI);
	private const float DEG2RAD = (Mathf.PI / 180.0f);

	private float m_dEnemyServeTime = 1;
	private float m_dBallDestroy = 1;
	private float m_dInit = 0;
	private float m_dShootInterval = 2;
	//private const float EnemyServeTime = 0.5f;
	//private const float SensorStartTime = 3;
	//private const float SensorStartTime = 2;
	private const float SensorStartTime = 0.5f;
	private float m_dPitchONTime = 0.5f;
	private const float StartDuration = 5.0f;
	private float m_dResultDuration = 10;
	//private const float ShowWarningTime = 2.0f;
	private const float ShowWarningTime = 1.0f;
	//private const float MainSpeedLife = 3;
	//private const float MainSpeedScale = 1.2f;
	//private const float HitGroundDuration = 2.0f;
	private const float HitGroundDuration = 3.0f;
	private const float HitDuration = 5.0f;
	//private const float HitInfoDuration = 3.0f;
	private const float HitInfoDuration = 2.5f;
	//private const float HitRecordPosX = 10.0f;
	private const float HitRecordPosX = 0;
	//private const float HitRecordPosY = -410.0f;
	//private const float HitRecordPosY = -145;
	private const float HitRecordPosY = -51;
	//private const float HitRecordHeight = 36.0f;
	//private const float HitRecordHeight = 24.0f;
	//private const float HitRecordHeight = 48.0f;
	private const float HitRecordHeight = 42.5f;
	//private const float SysInfoHeight = 25.0f;

	private const float CourtLimitX = 4.1364f;
	private const float CourtLimitZ2 = 11.9154f;
	private const float CourtLimitZ1 = 6.4225f;
	private float m_fHawkEyeDist = 1;

	private const string strGameMode = "GameMode";
	private const string strBounce = "Bounce";
	private const string strGameLevel = "GameLevel";
	private const string strGameOver = "GameOver";

	//private const string strServe = "Serve";
	private const string strCenter = "Center";
	private const string strLeft = "Left";
	private const string strRight = "Right";

	private string m_strDir;
	private string m_strBounce;
	private string m_strLevel;

	private GigaTrax.PVA.DetectNotify m_detect = new GigaTrax.PVA.DetectNotify();
	private GameObject m_objInfo2;
	private GameObject m_objGage = null;
	private GameObject m_objReady = null;
	private GameObject m_objResult = null;
	private GameObject m_objRestValue = null;
	private GameObject m_objWarning = null;
	//private PitcherAnim m_objPitcher;
	private Camera m_Camera = null;
	private CameraControl m_ctlCamera = null;
	private int m_nGameMode = 0;
	private int m_curflags = 0;

	private float m_fCameraPosY = 1.7f;
	private float m_fCameraPosZ = -12.41f;
	private float m_fCameraRotX = 0;
	private float m_fCameraRotY = 0;
	private float m_fCameraFOV = 40.0f;
	private float m_fShootPosX = 0;
	private float m_fShootPosZ = -16.98f;
	private Vector3 m_enemyPos;
	private float m_fMinX, m_fMaxX;
	private float m_fMinZ, m_fMaxZ;

	private Vector3 m_posLookAt;
	private int m_nLookAt = 0;
	private int m_viewPos = 1;
	private ShootController m_ShootController = new ShootController();

	private List<HitRecord> m_RecordArray = new List<HitRecord>();
	private GameObject m_HitInfo = null;

	private bool m_bGameEnd = false;
	private bool m_bEnemyServe = false;
	private bool m_bSensorStart = false;
	private bool m_bPitchON = false;
	private bool m_bPhaseLog = true;
	private int m_nGamePhase;
	private int m_nSensorPhase;
	private int m_nPrompt;
	private float m_dTime = 0;
	private float m_dCountDown = 0;
	private float m_dStart = 0;
	private bool m_bCtrlPower = false;
	private bool m_bUseDizzView = false;
	private bool m_bServe = true;
	//private float m_posSysInfo = 0;
}
