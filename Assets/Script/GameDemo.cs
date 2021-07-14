/*
https://blog.applibot.co.jp/2018/05/25/how-to-use-unity-videoplayer/
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityRawInput;
using System.Collections;
using System.Collections.Generic;
//using GigaTrax;

public class DemoBallTrack {
	public GameObject obj;
	public GameObject track;
	//GameObject objSpeed;
	public Text txtDist, txtX, txtY, txtZ;
};

public class GameDemo : MonoBehaviour {
	// Use this for initialization
	void Start(){
		int len = prefabCourt.Length;
		if( len != 0 ){
			m_nActiveCourt = 0;
			for(int i = 0; i < len; i++){
				prefabCourt[i].SetActive( (i == m_nActiveCourt)? true: false );
			}
		}
		setMenuMode(0);

		//GameObject objCamera;
		//objCamera = GameObject.FindWithTag("MainCamera");
		m_Camera = prefabMainCamera.GetComponent<Camera>();

		MainFrame.W3Ctrl_clearTouchCard();

		//m_strCmdA = MainFrame.getMachineString("CmdA");
		m_dShotTime = 0;
		m_dCourtTime = CourtInterval;
		m_nBallNumber = 0;
	}
	// Update is called once per frame
	void Update(){
		if( m_mode != 1 ){
			//if( MainFrame.Key_isAlt(RawKey.A) )
			if( RawKeyInput.IsKeyDown( RawKey.Home ) ||
				MainFrame.W3Ctrl_getTouchCard() )
			{
				setMenuMode(1);
			}
		}
		m_dCourtTime -= Time.deltaTime;
		if( m_dCourtTime <= 0 ){
			m_dCourtTime = CourtInterval;

			int len = prefabCourt.Length;
			if( len != 0 ){
				if( ++m_nActiveCourt >= len )
					m_nActiveCourt = 0;
				for(int i = 0; i < len; i++){
					prefabCourt[i].SetActive( (i == m_nActiveCourt)? true: false );
				}
			}
		}
		if( m_mode == 0 ){
			m_dShotTime -= Time.deltaTime;
			if( m_dShotTime <= 0 ){
				m_dShotTime = ShotInterval;

				float rotX = Random.Range(rotXMin, rotXMax);
				//float rotX = 0;
				float rotY = Random.Range(rotYMin, rotYMax);
				//float rotY = -10;
				float speed = Random.Range(speedMin, speedMax);

				float posX = Random.Range(posXMin, posXMax);

				Vector3 dir = new Vector3(0, 0, 1);
				dir = Quaternion.Euler( rotX, 0, 0 ) * dir;
				dir = Quaternion.Euler( 0, rotY, 0 ) * dir;

				Vector3 pos = new Vector3(posX, 2, -12.38f);

				GameObject ball = Instantiate(prefabBallHit);
				ball.transform.localPosition = pos;

				BallHit ball_hit = ball.GetComponent<BallHit>();
				ball_hit.addCallback( HitGround );

				GameObject track = Instantiate(prefabBallTrack);

				/*GameObject objText = track.transform.Find("Text").gameObject;
				Text tx = objText.GetComponent<Text>();
				tx.text = "No." + (m_nBallNumber+1).ToString("00") + ": " + ((int)speed).ToString() + "Km/h";
				m_nBallNumber++;*/

				track.transform.SetParent(prefabMainCanvas.transform, false);

				GameObject objSpeed = track.transform.Find("Value_Speed").gameObject;
				Text txtSpeed = objSpeed.GetComponent<Text>();
				txtSpeed.text = ((int)speed).ToString() + "Km/h";

				GameObject objDist = track.transform.Find("Value_Dist").gameObject;
				Text txtDist = objDist.GetComponent<Text>();
				//txtDist.text = "";

				GameObject objX = track.transform.Find("Value_X").gameObject;
				Text txtX = objX.GetComponent<Text>();
				//txtX.text = "";

				GameObject objY = track.transform.Find("Value_Y").gameObject;
				Text txtY = objY.GetComponent<Text>();
				//txtY.text = "";

				GameObject objZ = track.transform.Find("Value_Z").gameObject;
				Text txtZ = objZ.GetComponent<Text>();
				//txtZ.text = "";

				DemoBallTrack hit = new DemoBallTrack();
				hit.obj = ball;
				hit.track = track;
				hit.txtDist = txtDist;
				hit.txtX = txtX;
				hit.txtY = txtY;
				hit.txtZ = txtZ;
				m_BallArray.Add( hit );

				Rigidbody rb = ball.GetComponent<Rigidbody>();
				rb.velocity = dir * speed;

				//BallHit ball_hit = ball.GetComponent<BallHit>();
			}
		}
		int num_hit = m_BallArray.Count;
		for(int i = 0; i < num_hit; i++){
			DemoBallTrack hit = m_BallArray[i];
			Vector3 pos = hit.obj.transform.localPosition;

			hit.txtX.text = pos.x.ToString("f2");
			hit.txtY.text = pos.y.ToString("f2");
			hit.txtZ.text = pos.z.ToString("f2");

			float dist = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
			hit.txtDist.text = dist.ToString("f2");

			//hit.track.transform.localPosition = RectTransformUtility.WorldToScreenPoint(m_Camera, pos);
			//hit.track.transform.localPosition = m_Camera.WorldToScreenPoint(pos);
			hit.track.transform.position = m_Camera.WorldToScreenPoint(pos);
		}
	}
	private void setMenuMode(int mode)
	{
		int len;
		bool act1, act2;
		m_mode = mode;
		if( mode == 0 ){
			act1 = true;
			act2 = false;
		}
		else{
			act1 = false;
			act2 = true;
		}
		len = prefabMenu1.Length;
		if( len != 0 ){
			for(int i = 0; i < len; i++){
				prefabMenu1[i].SetActive( act1 );
			}
		}
		len = prefabMenu2.Length;
		if( len != 0 ){
			for(int i = 0; i < len; i++){
				prefabMenu2[i].SetActive( act2 );
			}
		}
	}
	/*public void OnServe()
	{
		//Debug.Log("Serve selected");
		MainFrame.setSysInt(strGameMode, GameMode.Serve);
		SceneManager.LoadScene(strGameScene);
	}*/
	public void OnForehand()
	{
		//Debug.Log("Forehand selected");
		MainFrame.setSysInt(strGameMode, GameMode.Forehand);
	}
	public void OnMiddlehand()
	{
		//Debug.Log("Middlehand selected");
		MainFrame.setSysInt(strGameMode, GameMode.Middlehand);
	}
	public void OnBackhand()
	{
		//Debug.Log("Backhand selected");
		MainFrame.setSysInt(strGameMode, GameMode.Backhand);
	}
	public void OnRandom()
	{
		//Debug.Log("Random selected");
		MainFrame.setSysInt(strGameMode, GameMode.Random);
	}
	public void OnStroke()
	{
		//Debug.Log("Stroke selected");
		MainFrame.setSysInt(strBounce, Bounce.Stroke);
	}
	public void OnVolley()
	{
		//Debug.Log("Volley selected");
		MainFrame.setSysInt(strBounce, Bounce.Volley);
	}
	public void OnBeginner()
	{
		//Debug.Log("Beginner selected");
		MainFrame.setSysInt(strGameLevel, GameLevel.Beginner);
		SceneManager.LoadScene(strGameScene);
	}
	public void OnAma()
	{
		//Debug.Log("Ama selected");
		MainFrame.setSysInt(strGameLevel, GameLevel.Ama);
		SceneManager.LoadScene(strGameScene);
	}
	public void OnPro()
	{
		//Debug.Log("Pro selected");
		MainFrame.setSysInt(strGameLevel, GameLevel.Pro);
		SceneManager.LoadScene(strGameScene);
	}
	public void OnServeLeft()
	{
		//Debug.Log("Left selected");
		MainFrame.setSysInt(strGameMode, GameMode.ServeLeft);
		SceneManager.LoadScene(strGameScene);
	}
	public void OnServeRight()
	{
		//Debug.Log("Right selected");
		MainFrame.setSysInt(strGameMode, GameMode.ServeRight);
		SceneManager.LoadScene(strGameScene);
	}
	public void HitGround(GameObject ball,string tag){
		if( tag == "Ground" ){
			int num_hit = m_BallArray.Count;
			for(int i = 0; i < num_hit; i++){
				DemoBallTrack hit = m_BallArray[i];
				if( hit.obj == ball ){
					m_BallArray.RemoveAt(i);
					Destroy( hit.obj, BallDestroy );
					Destroy( hit.track );
					break;
				}
			}
		}
		else if( tag == "Net" ){
			int num_hit = m_BallArray.Count;
			for(int i = 0; i < num_hit; i++){
				DemoBallTrack hit = m_BallArray[i];
				if( hit.obj == ball ){
					m_BallArray.RemoveAt(i);
					Destroy( hit.obj, BallDestroy );
					Destroy( hit.track );
					break;
				}
			}

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
	public GameObject prefabMainCamera;
	public GameObject prefabMainCanvas;
	public GameObject prefabBallHit;
	public GameObject prefabBallTrack;
	public GameObject[] prefabCourt;
	public GameObject[] prefabMenu1;
	public GameObject[] prefabMenu2;

	private const float KPH2MPS = (1000.0f / 3600.0f);
	private const float rotXMin = 1;
	private const float rotXMax = -2;
	private const float rotYMin = -8;
	private const float rotYMax = -12;
	//private const float rotXMax = -5;
	//private const float rotYMax = 30;
	private const float posXMin = 0.41f;
	private const float posXMax = 3.77f;
	//private const float speedMin = 100 * KPH2MPS;
	//private const float speedMax = 120 * KPH2MPS;
	private const float speedMin = 105 * KPH2MPS;
	private const float speedMax = 115 * KPH2MPS;
	//private const float speedMax = 120 * KPH2MPS;
	private const float BallDestroy = 0.5f;
	//private const float BallDestroy = 2.0f;
	//private const float BallDestroy = 5.0f;
	//private const float BallDestroy = 10.0f;
	//private const float ShotInterval = 3.0f;
	private const float ShotInterval = 1.0f;
	private const float CourtInterval = 5.0f;

	private const string strGameMode = "GameMode";
	private const string strBounce = "Bounce";
	private const string strGameLevel = "GameLevel";
	private const string strGameScene = "AnalyseMain2";

	private int m_mode = 0;
	//private string m_strCmdA;
	//private GameObject m_objMainCanvas;
	//private GameObject[] m_objCourt = new GameObject[3];
	private Camera m_Camera;
	private List<DemoBallTrack> m_BallArray = new List<DemoBallTrack>();
	private float m_dShotTime = 0;
	private float m_dCourtTime = 0;
	private int m_nBallNumber = 0;
	private int m_nActiveCourt = 0;
	string m_strInput;
}
