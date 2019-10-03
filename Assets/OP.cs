/*
https://blog.applibot.co.jp/2018/05/25/how-to-use-unity-videoplayer/
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OP : MonoBehaviour {
	// Use this for initialization
	void Start(){
		if( Display.displays.Length > 1 )
			Display.displays[1].Activate();

		Camera mainCamera = Camera.main;

		// シーンを横断する場合はDontDestroyOnLoadにする
		//m_objMovie = new GameObject("Movie_");

		GameObject obj = this.gameObject;
		m_audioSource = obj.AddComponent<AudioSource>();
		m_videoPlayer = obj.AddComponent<VideoPlayer>();

		// 表示設定
		m_videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
		m_videoPlayer.targetCamera = mainCamera;
		m_videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		m_videoPlayer.playOnAwake = false;

		// サウンド設定
		//TODO: エディタだと音声が出力されない(Unity 2017.2.1f1)
		//	  PrepareCompleted後に設定すると2回目以降は再生される…
		m_audioSource.playOnAwake = false;
		m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		m_videoPlayer.controlledAudioTrackCount = 1;
		m_videoPlayer.EnableAudioTrack(0, true);
		m_videoPlayer.SetTargetAudioSource(0, m_audioSource);

		// イベント設定
		m_videoPlayer.errorReceived += OnErrorReceived;
		m_videoPlayer.prepareCompleted += OnPrepareCompleted;
		m_videoPlayer.started += OnPlayStart;
		m_videoPlayer.loopPointReached += OnPlayEnd;

		string fname;
		int movie_index = Random.Range( 0, 3 );
		switch( movie_index ){
		case 0:
			fname = "CDI_movie1";
			break;
		case 1:
			fname = "CDI_movie2";
			break;
		default:
			fname = "CDI_movie3";
			break;
		}
		m_videoPlayer.enabled = true;
		string filePath = Application.streamingAssetsPath + "/" + fname + ".mp4";
		//obj.name = "Movie_" + System.IO.Path.GetFileName( filePath );

		m_videoPlayer.url = filePath;
		m_videoPlayer.Prepare();
	}
	public void Dispose()
	{
		// イベント設定解除
		m_videoPlayer.errorReceived -= OnErrorReceived;
		m_videoPlayer.prepareCompleted -= OnPrepareCompleted;
		m_videoPlayer.started -= OnPlayStart;
		m_videoPlayer.loopPointReached -= OnPlayEnd;
		//GameObject.Destroy(m_objMovie);
	}
	// Update is called once per frame
	void Update () {
	}
	private void OnErrorReceived(VideoPlayer player, string message)
	{
	}
	private void OnPrepareCompleted(VideoPlayer player)
	{
		m_videoPlayer.isLooping = false;
		m_videoPlayer.Play();
	}
	private void OnPlayStart(VideoPlayer player)
	{
	}
	private void OnPlayEnd(VideoPlayer player)
	{
		SceneManager.LoadScene ("test5");
	}

	private VideoPlayer m_videoPlayer = null;
	private AudioSource m_audioSource = null;
}
