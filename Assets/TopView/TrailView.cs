
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class TrailView : MonoBehaviour {

	[SerializeField]
	private int width = 256;

	[SerializeField]
	private int height = 256;

	[SerializeField]
	private Vector2 viewOffset = new Vector2(0, 0);

	[SerializeField]
	private float viewZoom = 1;

	[SerializeField]
	private bool viewTop = true;

	[SerializeField]
	private float widthNew = 5;

	[SerializeField]
	private float widthOld = 2.5f;

	[SerializeField]
	private Color colorNew = new Color(1, 0, 0, 1);

	[SerializeField]
	private Color colorOld = new Color(0, 1, 1, 1);

#if UNITY_EDITOR
	private int m_width;
	private int m_height;
	private float m_viewZoom;
	private Vector2 m_viewOffset;
#endif
	private View2D m_NewTrail;
	private View2D m_OldTrail;

	void Start()
	{
		GameObject objNewTrail = findChild("NewTrail");
		m_NewTrail = objNewTrail.GetComponent<View2D>();

		GameObject objOldTrail = findChild("OldTrail");
		m_OldTrail = objOldTrail.GetComponent<View2D>();

#if UNITY_EDITOR
		m_width = width;
		m_height = height;
		m_viewZoom = viewZoom;
		m_viewOffset = viewOffset;
#endif
		m_NewTrail.setSize( width, height );
		m_OldTrail.setSize( width, height );

		m_NewTrail.setViewZoom( viewZoom );
		m_OldTrail.setViewZoom( viewZoom );

		m_NewTrail.setViewOffset( viewOffset );
		m_OldTrail.setViewOffset( viewOffset );

		m_NewTrail.setColor( colorNew );
		m_OldTrail.setColor( colorOld );

		m_NewTrail.setWidth( widthNew );
		m_OldTrail.setWidth( widthOld );

		if( viewTop )
			drawTopGrid();
		else
			drawSideGrid();
	}
	void Update()
	{
#if UNITY_EDITOR
		if( m_width != width || m_height != height ){
			m_width = width;
			m_height = height;
			m_NewTrail.setSize( width, height );
			m_OldTrail.setSize( width, height );
		}
		if( m_viewZoom != viewZoom ){
			//Debug.Log( "change viewZoom: " + viewZoom.ToString("f2") + " " + viewZoom.ToString("f2") );
			m_viewZoom = viewZoom;
			m_NewTrail.setViewZoom( viewZoom );
			m_OldTrail.setViewZoom( viewZoom );
		}
		if( m_viewOffset.x != viewOffset.x || m_viewOffset.y != viewOffset.y ){
			//Debug.Log( "change viewOffset: " + viewOffset.x.ToString("f2") + " " + viewOffset.y.ToString("f2") );
			m_viewOffset = viewOffset;
			m_NewTrail.setViewOffset( viewOffset );
			m_OldTrail.setViewOffset( viewOffset );
		}
#endif
	}
	public void drawTopGrid(){
		PVADraw draw = m_OldTrail.getDraw();

		Vector2 pos1 = draw.ScreenToMarker( new Vector2(0, 0) );
		Vector2 pos2 = draw.ScreenToMarker( new Vector2(12, 0) );
		float w1 = pos2.x - pos1.x;

		pos2 = draw.ScreenToMarker( new Vector2(6, 0) );
		float w2 = pos2.x - pos1.x;

		Vector2 max = draw.ScreenToMarker( new Vector2(0, 8) );
		//Debug.Log( "max:" + max.y );

		draw.begin();
		draw.setWidth(1);
		draw.setColor( new Color(1,1,1,1) );
		draw.moveTo( new Vector2(0, 0) );
		draw.lineTo( new Vector2(0, max.y) );
		draw.end();

		int[] array = new int[]{-25, 25};
		float z = 0;
		for(int i = 0; i < 100; i++){
			draw.begin();
			draw.moveTo( new Vector2(-w1, z) );
			draw.lineTo( new Vector2( w1, z) );
			draw.end();

			int num = array.Length;
			for(int j = 0; j < num; j++){
				float x = (float)array[j];
				draw.begin();
				draw.moveTo( new Vector2(x-w2, z) );
				draw.lineTo( new Vector2(x+w2, z) );
				draw.end();
			}

			z += 25;
			if( z > max.y )
				break;
		}
	}
	public void drawSideGrid(){
		PVADraw draw = m_OldTrail.getDraw();

		//Debug.Log( "width:" + width + " height:" + height );

		Vector2 min = draw.ScreenToMarker( new Vector2(60, height) );
		Vector2 max = draw.ScreenToMarker( new Vector2(width-30, 0) );

		//Debug.Log( "min:" + min.x + " " + min.y );
		//Debug.Log( "max:" + max.x + " " + max.y );

		float y = 0;
		for(int i = 0; i < 5; i++){
			draw.begin();
			draw.setWidth(1);
			draw.setColor( new Color(1,1,1,1) );
			draw.moveTo( new Vector2(min.x, y) );
			draw.lineTo( new Vector2(max.x, y) );
			draw.end();

			//y += 50;
			//y += 25;
			y += 10;
			if( y >= max.y )
				break;
		}
		for(int i = 0; i <= 20; i += 10){
			GameObject obj = findChild( "y" + i + "m" );
			Vector2 pos1 = draw.MarkerToScreen( new Vector2(0, (float)i) );

			Vector3 pos2 = obj.transform.localPosition;
			pos2.y = -pos1.y;
			obj.transform.localPosition = pos2;
			//Debug.Log( "y" + i + "m = " + pos.y);
		}
		for(int i = 25; i <= 100; i += 25){
			GameObject obj = findChild( "z" + i + "m" );
			Vector2 pos1 = draw.MarkerToScreen( new Vector2((float)i, 0) );

			Vector3 pos2 = obj.transform.localPosition;
			pos2.x = pos1.x;
			obj.transform.localPosition = pos2;
			//Debug.Log( "z" + i + "m = " + pos.x);
		}
	}
	public void clearNewTrail()
	{
		m_NewTrail.clear();
	}
	public void addNewTrail(List<Vector3> posArray)
	{
		m_NewTrail.drawFadeArray( posArray, viewTop );
	}
	public void addOldTrail(List<Vector3> posArray)
	{
		m_OldTrail.drawArray( posArray, viewTop );
	}
	private GameObject findChild(string name)
	{
		return transform.Find(name).gameObject;
	}
}
