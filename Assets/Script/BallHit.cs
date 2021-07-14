using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class BallHit : MonoBehaviour {

	// Use this for initialization
	void Start () {
#if UNITY_EDITOR
		if( record ){
			m_Recorder = new GameObjectRecorder( gameObject );
			m_Recorder.BindComponentsOfType<Transform>(gameObject, true);
		}
#endif
	}

	// Update is called once per frame
	void Update(){

	}
#if UNITY_EDITOR
	void LateUpdate()
	{
		if( !record || clip == null )
			return;

		// Take a snapshot and record all the bindings values for this frame.
		m_Recorder.TakeSnapshot(Time.deltaTime);
	}
	void OnDisable()
	{
		if( !record || clip == null )
			return;

		if( m_Recorder.isRecording )
		{
			// Save the recorded session to the clip.
			m_Recorder.SaveToClip(clip);
		}
	}
#endif
	public void addCallback(HitGround callback){
		m_funcs += callback;
	}

	void OnCollisionEnter(Collision col)
	{
		if( m_funcs != null )
			m_funcs( gameObject, col.gameObject.tag );
	}
	public delegate void HitGround(GameObject obj,string tag);
	private HitGround m_funcs;
#if UNITY_EDITOR
	GameObjectRecorder m_Recorder;
	public AnimationClip clip;
	public bool record = false;
#endif
}
