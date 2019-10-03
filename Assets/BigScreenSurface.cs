/*
https://blog.applibot.co.jp/2018/05/25/how-to-use-unity-videoplayer/
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BigScreenSurface : MonoBehaviour {
	// Use this for initialization
	void Start(){
		GameObject obj = this.gameObject;
		m_videoPlayer = obj.GetComponent<VideoPlayer>();

		// イベント設定
		m_videoPlayer.errorReceived += OnErrorReceived;
		m_videoPlayer.prepareCompleted += OnPrepareCompleted;
		m_videoPlayer.started += OnPlayStart;
		m_videoPlayer.loopPointReached += OnPlayEnd;
		//m_videoPlayer.enabled = true;

		prepareMovie();
	}
	public void Dispose()
	{
		// イベント設定解除
		m_videoPlayer.errorReceived -= OnErrorReceived;
		m_videoPlayer.prepareCompleted -= OnPrepareCompleted;
		m_videoPlayer.started -= OnPlayStart;
		m_videoPlayer.loopPointReached -= OnPlayEnd;
	}
	// Update is called once per frame
	void Update () {
	}
	private void OnErrorReceived(VideoPlayer player, string message)
	{
	}
	private void OnPrepareCompleted(VideoPlayer player)
	{
		m_videoPlayer.Play();
	}
	private void OnPlayStart(VideoPlayer player)
	{
	}
	private void OnPlayEnd(VideoPlayer player)
	{
		if( ++m_index >= 3 )
			m_index = 0;
		prepareMovie();
	}
	private void prepareMovie()
	{
		string fname;
		switch( m_index ){
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
		string filePath = Application.streamingAssetsPath + "/" + fname + ".mp4";
		m_videoPlayer.url = filePath;
		m_videoPlayer.Prepare();
	}

	private VideoPlayer m_videoPlayer = null;
	private int m_index = 0;
}
